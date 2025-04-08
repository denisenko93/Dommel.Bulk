using System.Data;
using Dapper;
using Dommel.Bulk.DatabaseAdapters;
using Dommel.Bulk.Extensions;
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
    /// <param name="constraintName">name of database unique index constraint</param>
    /// <param name="chunkSize">max count of items allowed to insert in same query. 0 or null is disabled</param>
    /// <param name="propertiesToUpdate">list of properties to update</param>
    /// <returns>The number of rows affected.</returns>
    public static int BulkInsert<TEntity>(
        this IDbConnection connection,
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        IRowMapper? rowMapper = null,
        ExecutionFlags flags = ExecutionFlags.None,
        string constraintName = null,
        int? chunkSize = null,
        params string[] propertiesToUpdate)
        where TEntity : class
    {
        if (entities?.Any() != true)
        {
            return 0;
        }

        IDatabaseAdapter databaseAdapter = GetDatabaseAdapter(connection);
        ISqlBuilder sqlBuilder = DommelMapper.GetSqlBuilder(connection);

        if (chunkSize is null or < 1)
        {
            return Execute(connection, databaseAdapter, sqlBuilder, entities, transaction, rowMapper, flags, constraintName, propertiesToUpdate);
        }
        else
        {
            int affected = 0;
            foreach (var entitiesChunk in entities.Chunk(chunkSize.Value))
            {
                affected += Execute(connection, databaseAdapter, sqlBuilder, entitiesChunk, transaction, rowMapper, flags, constraintName, propertiesToUpdate);
            }

            return affected;
        }

        static int Execute(
            IDbConnection connection,
            IDatabaseAdapter databaseAdapter,
            ISqlBuilder sqlBuilder,
            IEnumerable<TEntity> entities,
            IDbTransaction? transaction,
            IRowMapper? rowMapper,
            ExecutionFlags flags,
            string constraintName,
            string[] propertiesToUpdate)
        {
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
    /// <param name="constraintName">name of database unique index constraint</param>
    /// <param name="chunkSize">max count of items allowed to insert in same query. 0 or null is disabled</param>
    /// <param name="propertiesToUpdate">list of properties to update</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<int> BulkInsertAsync<TEntity>(
        this IDbConnection connection,
        IEnumerable<TEntity> entities,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default,
        IRowMapper? rowMapper = null,
        ExecutionFlags flags = ExecutionFlags.None,
        string constraintName = null,
        int? chunkSize = null,
        params string[] propertiesToUpdate)
        where TEntity : class
    {
        if (entities?.Any() != true)
        {
            return 0;
        }
        IDatabaseAdapter databaseAdapter = GetDatabaseAdapter(connection);
        ISqlBuilder sqlBuilder = DommelMapper.GetSqlBuilder(connection);

        if (chunkSize is null or < 1)
        {
            return await ExecuteAsync(connection, databaseAdapter, sqlBuilder, entities, transaction, cancellationToken, rowMapper, flags, constraintName, propertiesToUpdate);
        }
        else
        {
            int affected = 0;
            foreach (var entitiesChunk in entities.Chunk(chunkSize.Value))
            {
                affected += await ExecuteAsync(connection, databaseAdapter, sqlBuilder, entitiesChunk, transaction, cancellationToken, rowMapper, flags, constraintName, propertiesToUpdate);
            }

            return affected;
        }

        static Task<int> ExecuteAsync(
            IDbConnection connection,
            IDatabaseAdapter databaseAdapter,
            ISqlBuilder sqlBuilder,
            IEnumerable<TEntity> entities,
            IDbTransaction? transaction,
            CancellationToken cancellationToken,
            IRowMapper? rowMapper,
            ExecutionFlags flags,
            string constraintName,
            string[] propertiesToUpdate)
        {
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
}