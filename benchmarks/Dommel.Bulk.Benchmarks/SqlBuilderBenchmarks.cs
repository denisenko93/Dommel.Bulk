using BenchmarkDotNet.Attributes;
using Dommel.Bulk.DatabaseAdapters;

namespace Dommel.Bulk.Benchmarks;

public class SqlBuilderBenchmarks : BenchmarksBase
{
    private readonly ISqlBuilder _sqlBuilder = new MySqlSqlBuilder();
    private readonly IDatabaseAdapter _databaseAdapter = new MySqlDatabaseAdapter();

    [Benchmark, BenchmarkCategory("simple")]
    public void SqlBuilderBenchmark()
    {
        DommelBulkMapper.BuildInsertQuery(_sqlBuilder, _databaseAdapter, data);
    }

    [Benchmark, BenchmarkCategory("parameters")]
    public void SqlBuilderParametersBenchmark()
    {
        DommelBulkMapper.BuildInsertParametersQuery(_sqlBuilder, data);
    }
}