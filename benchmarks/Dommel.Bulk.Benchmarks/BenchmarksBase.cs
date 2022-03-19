using BenchmarkDotNet.Attributes;
using Dommel.Bulk.Tests.Common;

namespace Dommel.Bulk.Benchmarks;

public class BenchmarksBase
{
    protected IReadOnlyCollection<AllTypesEntity> data;

    [Params(10_000)]
    public virtual int DataSize { get; set; }

    [GlobalSetup]
    public virtual void Setup()
    {
        data = Enumerable.Range(0, DataSize).Select(x => FakeGenerators.AllTypesFaker.Generate()).ToArray();
    }
}