using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Dapper;
using Dommel.Bulk.DatabaseAdapters;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk;

public static partial class DommelBulkMapper
{
    private static readonly ConcurrentDictionary<SqlBuilderCacheKey, object> StringBuilderFuncCache = new ConcurrentDictionary<SqlBuilderCacheKey, object>();

    /// <summary>
    /// Bulk inserts the specified collection of entities into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="connection">The connection to the database. This can either be open or closed.</param>
    /// <param name="entities">The entities to be inserted.</param>
    /// <param name="transaction">Optional transaction for the command.</param>
    /// <param name="flags">flags enables extended behaviours</param>
    /// <param name="propertiesToUpdate">list of properties to update</param>
    /// <returns>The number of rows affected.</returns>
    public static int BulkInsert<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null, ExecutionFlags flags = ExecutionFlags.None, params string[] propertiesToUpdate)
        where TEntity : class
    {
        var sql = BuildInsertQuery(DommelMapper.GetSqlBuilder(connection), GetDatabaseAdapter(connection), entities);
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
    /// <param name="flags">flags enables extended behaviours</param>
    /// <param name="propertiesToUpdate">list of properties to update</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> BulkInsertAsync<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null, CancellationToken cancellationToken = default, ExecutionFlags flags = ExecutionFlags.None, params string[] propertiesToUpdate)
        where TEntity : class
    {
        var sql = BuildInsertQuery(DommelMapper.GetSqlBuilder(connection), GetDatabaseAdapter(connection), entities);
        LogQuery<TEntity>(sql);
        return connection.ExecuteAsync(new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));
    }

    internal static string? BuildInsertQuery<T>(ISqlBuilder sqlBuilder, IDatabaseAdapter databaseAdapter, IEnumerable<T> entities)
    {
        Action<T, TextWriter> mapEntityFunc;

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

        if (StringBuilderFuncCache.TryGetValue(cacheKey, out object? func))
        {
            mapEntityFunc = (Action<T, TextWriter>) func;
        }
        else
        {
            mapEntityFunc = GenerateStringBuilderMapFunc<T>(typeProperties, databaseAdapter, ", ");

            StringBuilderFuncCache[cacheKey] = mapEntityFunc;
        }

        TextWriter textWriter = new StringWriter();

        textWriter.Write($"INSERT INTO {tableName} (");

        var columnNames = typeProperties.Select(p => Resolvers.Column(p, sqlBuilder, false));

        bool isFirst = true;
        foreach (string columnName in columnNames)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                textWriter.Write(", ");
            }

            textWriter.Write(columnName);
        }
        textWriter.WriteLine(") VALUES");

        isFirst = true;
        foreach (T entity in entities)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                textWriter.WriteLine(",");
            }

            textWriter.Write("(");

            mapEntityFunc(entity, textWriter);

            textWriter.Write(")");
        }
        textWriter.Write(";");

        return textWriter.ToString();
    }

    private static readonly MethodInfo TextWriterWriteMethod = typeof(TextWriter).GetMethod("Write", new []{typeof(string)})!;

    private static Action<T, TextWriter> GenerateStringBuilderMapFunc<T>(IEnumerable<PropertyInfo> typeProperties, IDatabaseAdapter databaseAdapter, string separator)
    {
        ParameterExpression entityParameter = Expression.Parameter(typeof(T));
        ParameterExpression textWriterParameter = Expression.Parameter(typeof(TextWriter));

        Expression writeNull = Expression.Call(textWriterParameter, TextWriterWriteMethod,
            Expression.Constant(databaseAdapter.GetNullStr()));

        bool firstProperty = true;

        List<Expression> expressions = new List<Expression>();

        foreach (PropertyInfo typeProperty in typeProperties)
        {
            ITypeMapper typeMapper = databaseAdapter.GetTypeMapper(typeProperty.PropertyType);

            LambdaExpression typePropertyExpression = typeMapper.GetExpression();

            Type typeMapperParameterType = typePropertyExpression.Parameters[0].Type;

            Expression property;

            if (!typeProperty.PropertyType.IsValueType || (typeProperty.PropertyType.IsGenericType
                && typeProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                property = Expression.Property(entityParameter, typeProperty);

                ParameterExpression propertyTypeParameter = Expression.Parameter(typeProperty.PropertyType, "property");
                Expression typeMapperParameter = typeProperty.PropertyType == typeMapperParameterType
                    ? propertyTypeParameter
                    : Expression.Convert(propertyTypeParameter, typeMapperParameterType);

                typePropertyExpression = Expression.Lambda(
                    Expression.Condition(
                        Expression.Equal(propertyTypeParameter, Expression.Constant(null)),
                        writeNull,
                        Expression.Invoke(typeMapper.GetExpression(), typeMapperParameter, textWriterParameter)),
                    propertyTypeParameter,
                    textWriterParameter);
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
                expressions.Add(Expression.Call(textWriterParameter, TextWriterWriteMethod, Expression.Constant(separator)));
            }

            expressions.Add(Expression.Invoke(typePropertyExpression, property, textWriterParameter));
        }

        var lambdaExpression = Expression.Lambda<Action<T, TextWriter>>(
            Expression.Block(expressions),
            entityParameter,
            textWriterParameter);

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