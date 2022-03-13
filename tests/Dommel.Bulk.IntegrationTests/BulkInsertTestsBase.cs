using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public abstract class BulkInsertTestsBase
{
    protected abstract IDbConnection GetConnection();

    protected IDbConnection GetOpenConnection()
    {
        var connection = GetConnection();
        connection.Open();
        return connection;
    }

    [Fact]
    public async Task InsertSampleDataTestAsync()
    {
        var personGenerator = new Faker<Person>()
            .RuleFor(x => x.Ref, () => Guid.NewGuid())
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.Age, f => f.Person.Random.Number(100))
            .RuleFor(x => x.Gender, f => f.Person.Gender)
            .RuleFor(x => x.BirthDay, f => f.Person.DateOfBirth);

        var people = Enumerable.Range(0, 20)
            .Select(x => personGenerator.Generate())
            .ToArray();

        Person[] peopleFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            await connection.BulkInsertAsync(people);

            peopleFromDb = (await connection.GetAllAsync<Person>()).ToArray();
        }

        foreach (Person person in people)
        {
            var personFromDb = peopleFromDb.FirstOrDefault(x => x.Ref == person.Ref);

            Assert.NotNull(personFromDb);

            Assert.Equal(person.Ref, personFromDb.Ref);
            Assert.Equal(person.FirstName, personFromDb.FirstName);
            Assert.Equal(person.LastName, personFromDb.LastName);
            Assert.Equal(person.Age, personFromDb.Age);
            Assert.Equal(person.Gender, personFromDb.Gender);
            Assert.Equal(person.BirthDay, personFromDb.BirthDay, TimeSpan.FromSeconds(2));
            Assert.NotNull(personFromDb.CreatedOn);
            Assert.NotNull(personFromDb.FullName);
        }
    }
}

[Table("people")]
public class Person
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid? Ref { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; }

    [Column("last_name")]
    public string LastName { get; set; }

    [Column("gender")]
    public Name.Gender Gender { get; set; }

    [Column("age")]
    public int Age { get; set; }

    [Column("birth_day")]
    public DateTime BirthDay { get; set; }

    [Column("created_on")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime? CreatedOn { get; set; }

    [Column("full_name")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string FullName { get; set; }
}