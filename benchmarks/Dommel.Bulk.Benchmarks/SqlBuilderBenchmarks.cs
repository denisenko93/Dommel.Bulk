using BenchmarkDotNet.Attributes;

namespace Dommel.Bulk.Benchmarks;

public class SqlBuilderBenchmarks : BenchmarksBase
{
    private readonly ISqlBuilder _sqlBuilder = new MySqlSqlBuilder();

    [Benchmark, BenchmarkCategory("simple")]
    public void SqlBuilderBenchmark()
    {
        DommelBulkMapper.BuildInsertQuery(_sqlBuilder, data);
    }

    [Benchmark, BenchmarkCategory("parameters")]
    public void SqlBuilderParametersBenchmark()
    {
        DommelBulkMapper.BuildInsertParametersQuery(_sqlBuilder, data);
    }
}