using Dapper;

namespace Dommel.Bulk;

public class SqlQuery
{
    public SqlQuery(string query, DynamicParameters parameters)
    {
        Query = query;
        Parameters = parameters;
    }

    public string Query { get; }

    public DynamicParameters Parameters { get; }
}