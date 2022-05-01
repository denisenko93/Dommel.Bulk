using Dommel.Bulk.RowMap;
using Dommel.Bulk.TypeMap;

namespace Dommel.Bulk.DatabaseAdapters;

/// <summary>
/// Represents a database implementation.
/// Custom implementations can be registered with <see cref="DommelBulkMapper.AddDatabaseAdapter(System.Type,Dommel.Bulk.DatabaseAdapters.IDatabaseAdapter)"/>.
/// </summary>
public interface IDatabaseAdapter
{
    /// <summary>
    /// Return type mapper from <see cref="Type"/>
    /// </summary>
    /// <param name="type">Type to map</param>
    /// <returns>Implementation of <see cref="ITypeMapper"/></returns>
    /// <exception cref="NotSupportedException">If mapper of <see cref="Type"/> is not found</exception>
    ITypeMapper GetTypeMapper(Type type);

    /// <summary>
    /// Add custom type mapper for the generic <see cref="T"/>. Must be implementation of <see cref="GenericTypeMapper{T}"/>
    /// </summary>
    /// <param name="genericTypeMapper">Custom implementation of <see cref="GenericTypeMapper{T}"/></param>
    /// <typeparam name="T">Type to map</typeparam>
    void AddTypeMapper<T>(GenericTypeMapper<T> genericTypeMapper);

    /// <summary>
    /// Returns NULL text
    /// </summary>
    /// <returns>NULL text</returns>
    public string GetNullStr();

    SqlQuery BuildBulkInsertQuery<T>(ISqlBuilder sqlBuilder, IRowMapper rowMapper, IEnumerable<T> entities, ExecutionFlags flags, string[] propertiesToUpdate);
}