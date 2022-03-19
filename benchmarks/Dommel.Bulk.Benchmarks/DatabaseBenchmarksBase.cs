using System.Data;
using BenchmarkDotNet.Attributes;
using Bogus;

namespace Dommel.Bulk.Benchmarks;

public abstract class DatabaseBenchmarksBase : BenchmarksBase
{
    private IDbConnection _connection;

    protected DatabaseBenchmarksBase(IDbConnection connection)
    {
        _connection = connection;
    }

    [Params(1000)]
    public override int DataSize { get; set; }

    public override void Setup()
    {
        SetupDatabase(_connection);

        base.Setup();
    }

    [Benchmark]
    public async Task BulkInsertBenchmarkAsync()
    {
        await _connection.BulkInsertAsync(data);
    }

    [Benchmark]
    public async Task BulkInsertParametersBenchmarkAsync()
    {
        await _connection.BulkInsertParametersAsync(data);
    }

    [Benchmark(Baseline = true)]
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