using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Dapper;
using Dommel.Bulk.Extensions;

namespace Dommel.Bulk;

public static class DommelBulkMapper
{
    private static ConcurrentDictionary<string, string> QueryCache { get; } = new ConcurrentDictionary<string, string>();

    /// <summary>
    /// Bulk inserts the specified collection of entities into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="connection">The connection to the database. This can either be open or closed.</param>
    /// <param name="entities">The entities to be inserted.</param>
    /// <param name="transaction">Optional transaction for the command.</param>
    /// <returns>The number of rows affected.</returns>
    public static int Insert<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction = null)
        where TEntity : class
    {
        var sql = BuildInsertQuery(DommelMapper.GetSqlBuilder(connection), entities);
        LogQuery<TEntity>(sql.Query);
        return connection.Execute(sql.Query, sql.Parameters, transaction);
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
        LogQuery<TEntity>(sql.Query);
        return connection.ExecuteAsync(new CommandDefinition(sql.Query, sql.Parameters, transaction: transaction, cancellationToken: cancellationToken));
    }

    private static void LogQuery<T>(string query, [CallerMemberName] string? method = null)
        => DommelMapper.LogReceived?.Invoke(method != null ? $"{method}<{typeof(T).Name}>: {query}" : query);

    internal static SqlQuery BuildInsertQuery<T>(ISqlBuilder sqlBuilder, IEnumerable<T> entities)
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
        DynamicParameters parameters = new DynamicParameters();

        sb.AppendFormat("INSERT INTO {0} (", tableName);

        var columnNames = typeProperties.Select(p => Resolvers.Column(p, sqlBuilder, false));

        sb.AppendJoin(", ", columnNames);
        sb.AppendLine(") VALUES");

        int line = 1;

        foreach (T entity in entities)
        {
            sb.Append("(");

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                string parameterName = sqlBuilder.PrefixParameter($"{typeProperty.Name}_{line}");

                parameters.Add(parameterName, typeProperty.GetValue(entity));

                sb.AppendFormat("{0}, ", parameterName);
            }

            sb.Remove(sb.Length - 2, 2);

            sb.AppendLine("),");
            line++;
        }

        sb.Remove(sb.Length - 3, 3);
        sb.Append(";");

        return new SqlQuery(sb.ToString(), parameters);
    }

    internal class SqlQuery
    {
        public SqlQuery(string query, DynamicParameters parameters)
        {
            Query = query;
            Parameters = parameters;
        }

        public string Query { get; }

        public DynamicParameters Parameters { get; }
    }
}