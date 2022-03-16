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

   // [Params(10, 100, 1_000, 10_000, 20_000, 50_000, 100_000, 500_000, 1_000_000)]
    [Params(10_000)]
    public int ChunkSize { get; set; }

    public override void Setup()
    {
        SetupDatabase(_connection);

        base.Setup();
    }

    [Benchmark]
    public async Task BulkInsertBenchmarkAsync()
    {
        foreach (var dataChunk in data.Chunk(ChunkSize))
        {
            await _connection.BulkInsertAsync(dataChunk);
        }
    }

    [Benchmark]
    public void BulkInsertBenchmark()
    {
        foreach (var dataChunk in data.Chunk(ChunkSize))
        {
             _connection.BulkInsert(dataChunk);
        }
    }

    [Benchmark(Baseline = true)]
    public async Task BulkInsertParametersBenchmarkAsync()
    {
        foreach (var dataChunk in data.Chunk(ChunkSize))
        {
            await _connection.BulkInsertParametersAsync(dataChunk);
        }
    }

    [Benchmark]
    public void BulkInsertParametersBenchmark()
    {
        foreach (var dataChunk in data.Chunk(ChunkSize))
        {
            _connection.BulkInsertParameters(dataChunk);
        }
    }

    //[Benchmark]
    public async Task InsertBenchmarkAsync()
    {
        foreach (var dataChunk in data.Chunk(ChunkSize))
        {
            await _connection.InsertAllAsync(dataChunk);
        }
    }

    //[Benchmark]
    public void InsertBenchmark()
    {
        foreach (var dataChunk in data.Chunk(ChunkSize))
        {
            _connection.InsertAll(dataChunk);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _connection.DeleteAll<Person>();
    }

    protected abstract void SetupDatabase(IDbConnection connection);
}