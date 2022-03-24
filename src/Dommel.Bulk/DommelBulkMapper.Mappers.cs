using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<SqlBuilderCacheKey, object> StringBuilderFuncCache = new ConcurrentDictionary<SqlBuilderCacheKey, object>();

    private static Dictionary<Type, ITypeMapper> TypeMappers { get; } = new Dictionary<Type, ITypeMapper>
    {
        [typeof(bool)] = new GenericTypeMapper<bool>(x => x ? "1" : "0"),
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
        [typeof(DateTimeOffset)] = new GenericTypeMapper<DateTimeOffset>(x=> $"'{x.UtcDateTime:yyyy-MM-dd HH:mm:ss.ffffff}{x:zzz}'"),
        [typeof(Guid)] = new FormatTypeMapper<Guid>("D", quote: "'"),
        [typeof(string)] = new GenericTypeMapper<string>(x => $"'{x.Escape()}'"),
        [typeof(TimeSpan)] = new GenericTypeMapper<TimeSpan>(x => $"'{(int) x.TotalHours}{x:\\:mm\\:ss\\.ffffff}'"),
        [typeof(ArraySegment<byte>)] = new GenericTypeMapper<ArraySegment<byte>>(x => $"0x{x.ToHexString()}"),
        [typeof(byte[])] = new GenericTypeMapper<byte[]>(x => $"0x{x.ToHexString()}"),
#if NET6_0_OR_GREATER
        [typeof(DateOnly)] = new FormatTypeMapper<DateOnly>("yyyy-MM-dd", quote:"'"),
        [typeof(TimeOnly)] = new FormatTypeMapper<TimeOnly>("HH:mm:ss.ffffff", quote:"'"),
#endif
    };

    /// <summary>
    /// Adds a custom type mapper for the specific <see cref="type"/>. Must be implementation of <see cref="ITypeMapper"/>
    /// </summary>
    /// <param name="type">type to the map</param>
    /// <param name="typeMapper">An implementation of the <see cref="ITypeMapper"/> interface.</param>
    public static void AddTypeMapper(Type type, ITypeMapper typeMapper)
    {
        TypeMappers[type] = typeMapper;
    }

    /// <summary>
    /// Add custom type mapper for the generic <see cref="T"/>. Must be implementation of <see cref="GenericTypeMapper{T}"/>
    /// </summary>
    /// <param name="genericTypeMapper">Custom implementation of <see cref="GenericTypeMapper{T}"/></param>
    /// <typeparam name="T">Type to map</typeparam>
    public static void AddTypeMapper<T>(GenericTypeMapper<T> genericTypeMapper)
    {
        TypeMappers[typeof(T)] = genericTypeMapper;
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
        var sql = BuildInsertQuery(DommelMapper.GetSqlBuilder(connection), entities);
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
        var sql = BuildInsertQuery(DommelMapper.GetSqlBuilder(connection), entities);
        LogQuery<TEntity>(sql);
        return connection.ExecuteAsync(new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));
    }

    internal static string BuildInsertQuery<T>(ISqlBuilder sqlBuilder, IEnumerable<T> entities)
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

        if (TypeMappers.TryGetValue(type, out ITypeMapper? typeMapper))
        {
            return typeMapper;
        }
        else if (type.IsEnum && TypeMappers.TryGetValue(Enum.GetUnderlyingType(type), out ITypeMapper? enumTypeMapper))
        {
            return enumTypeMapper;
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