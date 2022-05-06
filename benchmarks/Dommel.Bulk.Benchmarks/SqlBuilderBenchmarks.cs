using BenchmarkDotNet.Attributes;
using Dommel.Bulk.DatabaseAdapters;
using Dommel.Bulk.RowMap;

namespace Dommel.Bulk.Benchmarks;

public class SqlBuilderBenchmarks : BenchmarksBase
{
    private readonly ISqlBuilder _sqlBuilder = new MySqlSqlBuilder();
    private readonly IDatabaseAdapter _databaseAdapter = new MySqlDatabaseAdapter();

    [Benchmark, BenchmarkCategory("simple")]
    public void SqlBuilderBenchmark()
    {
        IRowMapper rowMapper = new ExpressionRowMapper();
        _databaseAdapter.BuildBulkInsertQuery(_sqlBuilder, rowMapper, data, ExecutionFlags.None, Array.Empty<string>(), null);
    }

    [Benchmark, BenchmarkCategory("parameters")]
    public void SqlBuilderParametersBenchmark()
    {
        IRowMapper rowMapper = new ParametersRowMapper();
        _databaseAdapter.BuildBulkInsertQuery(_sqlBuilder, rowMapper, data, ExecutionFlags.None, Array.Empty<string>(), null);
    }
}