using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;
using Dommel.Bulk.Extensions;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk;

public static partial class DommelBulkMapper
{
    private static readonly ITypeMapper EnumTypeMapper = new FormatTypeMapper<Enum>("d");
    private static Dictionary<Type, ITypeMapper> TypeMappers { get; } = new Dictionary<Type, ITypeMapper>
    {
        [typeof(bool)] = new BoolTypeMapper(),
        [typeof(byte)] = new FormatTypeMapper<byte>("D"),
        [typeof(char)] = new GenericTypeMapper<char>(x => $"'{x.ToString().Escape()}'"),
        [typeof(double)] = new FormatTypeMapper<double>("G17"),
        [typeof(float)] = new FormatTypeMapper<float>("G9"),
        [typeof(int)] = new FormatTypeMapper<int>("D"),
        [typeof(long)] = new FormatTypeMapper<long>("D"),
        [typeof(sbyte)] = new FormatTypeMapper<sbyte>("D"),
        [typeof(short)] = new FormatTypeMapper<short>("D"),
        [typeof(uint)] = new FormatTypeMapper<uint>("D"),
        [typeof(ulong)] = new FormatTypeMapper<ulong>("D"),
        [typeof(ushort)] = new FormatTypeMapper<ushort>("D"),
        [typeof(decimal)] = new FormatTypeMapper<decimal>("G"),
        [typeof(DateTime)] = new FormatTypeMapper<DateTime>("yyyy-MM-dd HH:mm:ss.ffffff", quote: "'"),
        [typeof(DateTimeOffset)] = new FormatTypeMapper<DateTimeOffset>("yyyy-MM-dd HH:mm:ss.ffffffK", quote: "'"),
        [typeof(Guid)] = new FormatTypeMapper<Guid>("D", quote: "'"),
        [typeof(string)] = new GenericTypeMapper<string>(x => $"'{x.Escape()}'"),
        [typeof(TimeSpan)] = new TimeSpanTimeMapper(),
        [typeof(ArraySegment<byte>)] = new ByteArraySegmentTypeMapper("0x{0}"),
        [typeof(byte[])] = new ByteArrayTypeMapper("0x{0}"),
#if NET6_0_OR_GREATER
        [typeof(DateOnly)] = new FormatTypeMapper<DateOnly>("yyyy-MM-dd", quote:"'"),
        [typeof(TimeOnly)] = new FormatTypeMapper<TimeOnly>("HH:mm:ss.ffffff", quote:"'"),
#endif
    };

    /// <summary>
    /// Adds a custom implementation of <see cref="ITypeMapper"/>
    /// for the specified type
    /// </summary>
    /// <param name="type">type to the map</param>
    /// <param name="typeMapper">An implementation of the <see cref="ITypeMapper"/> interface.</param>
    public static void AddTypeMapper(Type type, ITypeMapper typeMapper)
    {
        TypeMappers[type] = typeMapper;
    }

    /// <summary>
    /// Bulk inserts the specified collection of entities into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="connection">The connection to the database. This can either be open or closed.</param>
    /// <param name="entities">The entities to be inserted.</param>
    /// <param name="transaction">Optional transaction for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int BulkInsert<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction = null)
        where TEntity : class
    {
        var sql = BuildInsertQueryReflection(DommelMapper.GetSqlBuilder(connection), entities);
        LogQuery<TEntity>(sql);
        return connection.Execute(sql, transaction);
    }

    /// <summary>
    /// Bulk inserts the specified collection of entities into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="connection">The connection to the database. This can either be open or closed.</param>
    /// <param name="entities">The entities to be inserted.</param>
    /// <param name="transaction">Optional transaction for the command.</param>
    /// <param name="cancellationToken">Optional cancellation token for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> BulkInsertAsync<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var sql = BuildInsertQueryReflection(DommelMapper.GetSqlBuilder(connection), entities);
        LogQuery<TEntity>(sql);
        return connection.ExecuteAsync(new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));
    }

    internal static string BuildInsertQueryReflection<T>(ISqlBuilder sqlBuilder, IEnumerable<T> entities)
    {
        Type type = typeof(T);

        var tableName = Resolvers.Table(type, sqlBuilder);

        // Use all non-key and non-generated properties for inserts
        var keyProperties = Resolvers.KeyProperties(type);
        var typeProperties = Resolvers.Properties(type)
            .Where(x => !x.IsGenerated)
            .Select(x => x.Property)
            .Except(keyProperties.Where(p => p.IsGenerated).Select(p => p.Property))
            .ToArray();

        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("INSERT INTO {0} (", tableName);

        var columnNames = typeProperties.Select(p => Resolvers.Column(p, sqlBuilder, false));

        sb.AppendJoin(", ", columnNames);
        sb.AppendLine(") VALUES");

        int line = 1;

        foreach (T entity in entities)
        {
            sb.Append('(');

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                object value = typeProperty.GetValue(entity);

                if (value == null)
                {
                    sb.Append($"{Constants.NullStr}, ");
                }
                else
                {
                    ITypeMapper typeMapper = GetTypeMapper(typeProperty.PropertyType);

                    sb.Append($"{typeMapper.Map(typeProperty.GetValue(entity))}, ");
                }
            }

            sb.Remove(sb.Length - 2, 2);

            sb.AppendLine("),");
            line++;
        }

        sb.Remove(sb.Length - 3, 3);
        sb.Append(';');

        return sb.ToString();
    }

    private static readonly ConcurrentDictionary<SqlBuilderCacheKey, object> StringFuncCache = new ConcurrentDictionary<SqlBuilderCacheKey, object>();

    internal static string BuildInsertQueryExpression<T>(ISqlBuilder sqlBuilder, IEnumerable<T> entities)
    {
        Func<T, string> mapEntityFunc;

        Type type = typeof(T);

        SqlBuilderCacheKey cacheKey = new SqlBuilderCacheKey(sqlBuilder.GetType().TypeHandle, type.TypeHandle);

        var tableName = Resolvers.Table(type, sqlBuilder);

        // Use all non-key and non-generated properties for inserts
        var keyProperties = Resolvers.KeyProperties(type);
        var typeProperties = Resolvers.Properties(type)
            .Where(x => !x.IsGenerated)
            .Select(x => x.Property)
            .Except(keyProperties.Where(p => p.IsGenerated).Select(p => p.Property))
            .ToArray();

        if (StringFuncCache.TryGetValue(cacheKey, out object? func))
        {
            mapEntityFunc = (Func<T, string>) func;
        }
        else
        {
            mapEntityFunc = GenerateStringMapFunc<T>(typeProperties);

            StringFuncCache[cacheKey] = mapEntityFunc;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("INSERT INTO {0} (", tableName);

        var columnNames = typeProperties.Select(p => Resolvers.Column(p, sqlBuilder, false));

        sb.AppendJoin(", ", columnNames);
        sb.AppendLine(") VALUES");

        foreach (T entity in entities)
        {
            sb.Append(mapEntityFunc(entity));

            sb.AppendLine(",");
        }

        sb.Remove(sb.Length - 3, 3);
        sb.Append(';');

        return sb.ToString();
    }

    private static readonly ConcurrentDictionary<SqlBuilderCacheKey, object> StringBuilderFuncCache = new ConcurrentDictionary<SqlBuilderCacheKey, object>();

    internal static string BuildInsertQueryExpressionStringBuilder<T>(ISqlBuilder sqlBuilder, IEnumerable<T> entities)
    {
        Func<T, StringBuilder, StringBuilder> mapEntityFunc;

        Type type = typeof(T);

        SqlBuilderCacheKey cacheKey = new SqlBuilderCacheKey(sqlBuilder.GetType().TypeHandle, type.TypeHandle);

        var tableName = Resolvers.Table(type, sqlBuilder);

        // Use all non-key and non-generated properties for inserts
        var keyProperties = Resolvers.KeyProperties(type);
        var typeProperties = Resolvers.Properties(type)
            .Where(x => !x.IsGenerated)
            .Select(x => x.Property)
            .Except(keyProperties.Where(p => p.IsGenerated).Select(p => p.Property))
            .ToArray();

        if (StringBuilderFuncCache.TryGetValue(cacheKey, out object func))
        {
            mapEntityFunc = (Func<T, StringBuilder, StringBuilder>) func;
        }
        else
        {
            mapEntityFunc = GenerateStringBuilderMapFunc<T>(typeProperties);

            StringBuilderFuncCache[cacheKey] = mapEntityFunc;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("INSERT INTO {0} (", tableName);

        var columnNames = typeProperties.Select(p => Resolvers.Column(p, sqlBuilder, false));

        sb.AppendJoin(", ", columnNames);
        sb.AppendLine(") VALUES");

        foreach (T entity in entities)
        {
            mapEntityFunc(entity, sb);

            sb.AppendLine(",");
        }

        sb.Remove(sb.Length - 3, 3);
        sb.Append(';');

        return sb.ToString();
    }

    public static ITypeMapper GetTypeMapper(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (TypeMappers.TryGetValue(type, out ITypeMapper typeMapper))
        {
            return typeMapper;
        }
        else if (type.IsEnum)
        {
            return EnumTypeMapper;
        }

        throw new NotSupportedException($"Not found type mapper for type {type}");
    }

#if NET6_0_OR_GREATER
    private static MethodInfo? stringBuilderAppendMethod = typeof(StringBuilder).GetMethod("Append", new []{typeof(StringBuilder.AppendInterpolatedStringHandler)});
#else
    private static MethodInfo stringBuilderAppendMethod = typeof(StringBuilder).GetMethod("Append", new []{typeof(string)});
#endif

    private static Func<T, StringBuilder, StringBuilder> GenerateStringBuilderMapFunc<T>(IEnumerable<PropertyInfo> typeProperties)
    {
        ParameterExpression entityParameter = Expression.Parameter(typeof(T));
        ParameterExpression stringBuilderParameter = Expression.Parameter(typeof(StringBuilder));

        bool firstProperty = true;

        Expression expression = Expression.Call(stringBuilderParameter, stringBuilderAppendMethod, Expression.Constant("("));;

        foreach (PropertyInfo typeProperty in typeProperties)
        {
            ITypeMapper typeMapper = GetTypeMapper(typeProperty.PropertyType);

            LambdaExpression typePropertyExpression = typeMapper.GetExpression();

            Type typeMapperParameterType = typePropertyExpression.Parameters[0].Type;

            Expression property;

            if (!typeProperty.PropertyType.IsValueType || (typeProperty.PropertyType.IsGenericType && typeProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                property = Expression.Property(entityParameter, typeProperty);

                ParameterExpression propertyTypeParameter = Expression.Parameter(typeProperty.PropertyType, "property");
                Expression typeMapperParameter = typeProperty.PropertyType == typeMapperParameterType
                    ? propertyTypeParameter
                    : Expression.Convert(propertyTypeParameter, typeMapperParameterType);

                typePropertyExpression = Expression.Lambda(
                    Expression.Condition(
                        Expression.Equal(propertyTypeParameter, Expression.Constant(null)),
                        Expression.Constant(Constants.NullStr),
                        Expression.Invoke(typeMapper.GetExpression(), typeMapperParameter)),
                    propertyTypeParameter);
            }
            else
            {
                property = typeProperty.PropertyType == typeMapperParameterType
                    ? Expression.Property(entityParameter, typeProperty)
                    : Expression.Convert(Expression.Property(entityParameter, typeProperty), typeMapperParameterType);
            }

            if (firstProperty)
            {
                firstProperty = false;
            }
            else
            {
                expression = Expression.Call(expression, stringBuilderAppendMethod, Expression.Constant(", "));
            }

            expression = Expression.Call(
                expression,
                stringBuilderAppendMethod,
                Expression.Invoke(typePropertyExpression, property));
        }

        expression = Expression.Call(
                expression,
                stringBuilderAppendMethod,
                Expression.Constant(")"));

        var lambdaExpression = Expression.Lambda<Func<T, StringBuilder, StringBuilder>>(
            expression,
            entityParameter,
            stringBuilderParameter);

        return lambdaExpression.Compile();
    }


    private static MethodInfo? stringFormatMethod = typeof(string).GetMethod("Format", new []{typeof(string), typeof(object[])});

    private static Func<T, string> GenerateStringMapFunc<T>(IReadOnlyCollection<PropertyInfo> typeProperties)
    {
        string format = $"({string.Join(", ", Enumerable.Range(0, typeProperties.Count).Select(x => $"{{{x}}}"))})";

        ParameterExpression entityParameter = Expression.Parameter(typeof(T));

        IEnumerable<Expression> expressions = typeProperties
            .Select(x =>
            {
                ITypeMapper typeMapper = GetTypeMapper(x.PropertyType);

                LambdaExpression expression = typeMapper.GetExpression();

                Type typeMapperParameterType = expression.Parameters[0].Type;

                Expression property;

                if (!x.PropertyType.IsValueType || (x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    property = Expression.Property(entityParameter, x);

                    ParameterExpression propertyTypeParameter = Expression.Parameter(x.PropertyType, "property");
                    Expression typeMapperParameter = x.PropertyType == typeMapperParameterType
                        ? propertyTypeParameter
                        : Expression.Convert(propertyTypeParameter, typeMapperParameterType);

                    expression = Expression.Lambda(
                        Expression.Condition(
                            Expression.Equal(propertyTypeParameter, Expression.Constant(null)),
                            Expression.Constant(Constants.NullStr),
                            Expression.Invoke(typeMapper.GetExpression(), typeMapperParameter)),
                        propertyTypeParameter);
                }
                else
                {
                    property = x.PropertyType == typeMapperParameterType
                        ? Expression.Property(entityParameter, x)
                        : Expression.Convert(Expression.Property(entityParameter, x), typeMapperParameterType);
                }

                return Expression.Invoke(expression, property);
            });

        var lambdaExpression = Expression.Lambda<Func<T, string>>(
            Expression.Call(stringFormatMethod, Expression.Constant(format), Expression.NewArrayInit(typeof(string), expressions)),
            entityParameter);

        return lambdaExpression.Compile();
    }
}

internal struct SqlBuilderCacheKey : IEquatable<SqlBuilderCacheKey>
{
    public SqlBuilderCacheKey(RuntimeTypeHandle sqlBuilderType, RuntimeTypeHandle entityType)
    {
        SqlBuilderType = sqlBuilderType;
        EntityType = entityType;
    }

    public RuntimeTypeHandle SqlBuilderType { get; }

    public RuntimeTypeHandle EntityType { get; }

    public bool Equals(SqlBuilderCacheKey other)
    {
        return SqlBuilderType.Equals(other.SqlBuilderType) && EntityType.Equals(other.EntityType);
    }

    public override bool Equals(object? obj)
    {
        return obj is SqlBuilderCacheKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SqlBuilderType, EntityType);
    }
}