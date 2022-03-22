using System.Data;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Dommel.Bulk.Tests.Common;

namespace Dommel.Bulk.Benchmarks;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public abstract class DatabaseBenchmarksBase : SqlBuilderBenchmarks
{
    private IDbConnection _connection;

    protected DatabaseBenchmarksBase(IDbConnection connection)
    {
        _connection = connection;
    }

    [Params(10_000)]
    public override int DataSize { get; set; }

    public override void Setup()
    {
        SetupDatabase(_connection);

        base.Setup();
    }

    [Benchmark(Baseline = true), BenchmarkCategory("simple")]
    public async Task BulkInsertBenchmarkAsync()
    {
        await _connection.BulkInsertAsync(data);
    }

    [Benchmark(Baseline = true), BenchmarkCategory("parameters")]
    public async Task BulkInsertParametersBenchmarkAsync()
    {
        await _connection.BulkInsertParametersAsync(data);
    }

    [Benchmark]
    public async Task InsertAllBenchmarkAsync()
    {
        await _connection.InsertAllAsync(data);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _connection.DeleteAll<Person>();
    }

    protected abstract void SetupDatabase(IDbConnection connection);
}