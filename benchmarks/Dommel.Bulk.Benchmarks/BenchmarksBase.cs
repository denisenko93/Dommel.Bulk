using BenchmarkDotNet.Attributes;
using Bogus;

namespace Dommel.Bulk.Benchmarks;

public class BenchmarksBase
{
    protected IReadOnlyCollection<TestData.Person> data;

    [Params(100_000)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public virtual void Setup()
    {
        var personGenerator = new Faker<TestData.Person>()
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.Age, f => f.Person.Random.Number(100))
            .RuleFor(x => x.Gender, f => f.Person.Gender)
            .RuleFor(x => x.BirthDay, f => f.Person.DateOfBirth);

        data = Enumerable.Range(0, DataSize).Select(x => personGenerator.Generate()).ToArray();
    }
}