using BenchmarkDotNet.Attributes;

namespace Dommel.Bulk.Benchmarks;

[MemoryDiagnoser()]
public class SqlBuilderBenchmarks : BenchmarksBase
{
    private readonly ISqlBuilder _sqlBuilder = new MySqlSqlBuilder();

    [Benchmark]
    public void SqlBuilderReflectionBenchmark()
    {
        DommelBulkMapper.BuildInsertQueryReflection(_sqlBuilder, data);
    }

    [Benchmark]
    public void SqlBuilderExpressionBenchmark()
    {
        DommelBulkMapper.BuildInsertQueryExpression(_sqlBuilder, data);
    }

    [Benchmark]
    public void SqlBuilderParametersBenchmark()
    {
        DommelBulkMapper.BuildInsertParametersQuery(_sqlBuilder, data);
    }
}