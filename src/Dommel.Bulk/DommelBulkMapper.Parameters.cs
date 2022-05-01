using System.Data;
using Dommel.Bulk.RowMap;

namespace Dommel.Bulk;

public static partial class DommelBulkMapper
{
    private static ParametersRowMapper _parametersRowMapper = new ParametersRowMapper();

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
    public static int BulkInsertParameters<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction = null, ExecutionFlags flags = ExecutionFlags.None, params string[] propertiesToUpdate)
        where TEntity : class
    {
        return BulkInsert(connection, entities, transaction, _parametersRowMapper, flags, propertiesToUpdate);
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
    public static Task<int> BulkInsertParametersAsync<TEntity>(this IDbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction = null, CancellationToken cancellationToken = default, ExecutionFlags flags = ExecutionFlags.None, params string[] propertiesToUpdate)
        where TEntity : class
    {
        return BulkInsertAsync(connection, entities, transaction, cancellationToken, _parametersRowMapper, flags, propertiesToUpdate);
    }


}