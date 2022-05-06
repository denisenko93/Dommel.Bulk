using System.Data;
using Dapper;
using Dommel.Bulk.DatabaseAdapters;
using Dommel.Bulk.RowMap;

namespace Dommel.Bulk;

public static partial class DommelBulkMapper
{
    private static ExpressionRowMapper _expressionRowMapper = new ExpressionRowMapper();

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
    public static int BulkInsert<TEntity>(
        this IDbConnection connection,
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        IRowMapper? rowMapper = null,
        ExecutionFlags flags = ExecutionFlags.None,
        string constraintName = null,
        params string[] propertiesToUpdate)
        where TEntity : class
    {
        IDatabaseAdapter databaseAdapter = GetDatabaseAdapter(connection);
        ISqlBuilder sqlBuilder = DommelMapper.GetSqlBuilder(connection);

        SqlQuery query = databaseAdapter.BuildBulkInsertQuery(
            sqlBuilder,
            rowMapper ?? _expressionRowMapper,
            entities,
            flags,
            propertiesToUpdate,
            constraintName);

        LogQuery<TEntity>(query.Query);
        return connection.Execute(query.Query, query.Parameters, transaction);
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
    public static Task<int> BulkInsertAsync<TEntity>(
        this IDbConnection connection,
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default,
        IRowMapper? rowMapper = null,
        ExecutionFlags flags = ExecutionFlags.None,
        string constraintName = null,
        params string[] propertiesToUpdate)
        where TEntity : class
    {
        IDatabaseAdapter databaseAdapter = GetDatabaseAdapter(connection);
        ISqlBuilder sqlBuilder = DommelMapper.GetSqlBuilder(connection);

        SqlQuery query = databaseAdapter.BuildBulkInsertQuery(
            sqlBuilder,
            rowMapper ?? _expressionRowMapper,
            entities,
            flags,
            propertiesToUpdate,
            constraintName);

        LogQuery<TEntity>(query.Query);
        return connection.ExecuteAsync(new CommandDefinition(query.Query, query.Parameters, transaction: transaction, cancellationToken: cancellationToken));
    }
}