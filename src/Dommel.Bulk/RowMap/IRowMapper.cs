using System.Reflection;
using Dapper;
using Dommel.Bulk.DatabaseAdapters;

namespace Dommel.Bulk.RowMap;

public interface IRowMapper
{
    void MapRows<T>(IEnumerable<T> entities, ISqlBuilder sqlBuilder, IDatabaseAdapter databaseAdapter, TextWriter textWriter, DynamicParameters parameters, IReadOnlyCollection<PropertyInfo> properties, string columnSeparator, string rowSeparator, string rowStart, string rowEnd);
}