using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dommel.Bulk.Tests.Common;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public abstract class BulkInsertTestsBase
{
    protected const string DatabaseName = "dommel_bulk_test";

    protected abstract IDbConnection GetConnection();

    protected IDbConnection GetOpenConnection()
    {
        var connection = GetConnection();
        connection.Open();
        connection.ChangeDatabase(DatabaseName);
        return connection;
    }

    [Fact]
    public async Task BulkInsertTestAsync()
    {
        var people = Enumerable.Range(0, 20)
            .Select(_ => FakeGenerators.PersonFaker.Generate())
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
            var personFromDb = peopleFromDb.First(x => x.Ref == person.Ref);

            Assert.NotNull(personFromDb);

            Assert.NotEqual(0, personFromDb.Id);
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

    [Fact]
    public void BulkInsertTest()
    {
        var people = Enumerable.Range(0, 20)
            .Select(_ => FakeGenerators.PersonFaker.Generate())
            .ToArray();

        Person[] peopleFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            connection.DeleteAll<Person>();

            connection.BulkInsert(people);

            peopleFromDb = connection.GetAll<Person>().ToArray();
        }

        foreach (Person person in people)
        {
            var personFromDb = peopleFromDb.First(x => x.Ref == person.Ref);

            Assert.NotNull(personFromDb);

            Assert.NotEqual(0, personFromDb.Id);
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

    [Fact]
    public async Task BulkInsertParametersTestAsync()
    {
        var people = Enumerable.Range(0, 20)
            .Select(_ => FakeGenerators.PersonFaker.Generate())
            .ToArray();

        Person[] peopleFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            await connection.BulkInsertParametersAsync(people);

            peopleFromDb = (await connection.GetAllAsync<Person>()).ToArray();
        }

        foreach (Person person in people)
        {
            var personFromDb = peopleFromDb.First(x => x.Ref == person.Ref);

            Assert.NotNull(personFromDb);

            Assert.NotEqual(0, personFromDb.Id);
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

    [Fact]
    public void BulkInsertParametersTest()
    {
        var people = Enumerable.Range(0, 20)
            .Select(_ => FakeGenerators.PersonFaker.Generate())
            .ToArray();

        Person[] peopleFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            connection.DeleteAll<Person>();

            connection.BulkInsertParameters(people);

            peopleFromDb = connection.GetAll<Person>().ToArray();
        }

        foreach (Person person in people)
        {
            var personFromDb = peopleFromDb.First(x => x.Ref == person.Ref);

            Assert.NotNull(personFromDb);

            Assert.NotEqual(0, personFromDb.Id);
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