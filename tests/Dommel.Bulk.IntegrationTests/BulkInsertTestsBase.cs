using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dommel.Bulk.Tests.Common;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public abstract class BulkInsertTestsBase<TAllTypesEntity>
where TAllTypesEntity : class, IEntity
{
    private IReadOnlyCollection<Person> _people;

    public BulkInsertTestsBase()
    {
        _people = Enumerable.Range(0, 100)
            .Select(_ => FakeGenerators.PersonFaker.Generate())
            .ToArray();
    }

    protected abstract IDbConnection GetConnection();

    protected abstract IReadOnlyCollection<TAllTypesEntity> GetAllFakeTypes();

    protected abstract void AssertAllTypesEqual(TAllTypesEntity expected, TAllTypesEntity actual);

    protected IDbConnection GetOpenConnection()
    {
        var connection = GetConnection();
        connection.Open();
        return connection;
    }

    [Fact]
    public async Task BulkInsertTestAsync()
    {
        TAllTypesEntity[] allTypesFromDb;

        IReadOnlyCollection<TAllTypesEntity> types = GetAllFakeTypes();

        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<TAllTypesEntity>();

            await connection.BulkInsertAsync(types);

            allTypesFromDb = (await connection.GetAllAsync<TAllTypesEntity>()).ToArray();
        }

        foreach (TAllTypesEntity allTypesEntity in types)
        {
            var allTypeFromDb = allTypesFromDb.First(x => x.Id == allTypesEntity.Id);

            AssertAllTypesEqual(allTypesEntity, allTypeFromDb);
        }
    }

    [Fact]
    public void BulkInsertTest()
    {
        Person[] peopleFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            connection.DeleteAll<Person>();

            connection.BulkInsert(_people);

            peopleFromDb = connection.GetAll<Person>().ToArray();
        }

        foreach (Person person in _people)
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
        TAllTypesEntity[] allTypesFromDb;

        IReadOnlyCollection<TAllTypesEntity> allTypes = GetAllFakeTypes();

        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<TAllTypesEntity>();

            await connection.BulkInsertParametersAsync(allTypes);

            allTypesFromDb = (await connection.GetAllAsync<TAllTypesEntity>()).ToArray();
        }

        foreach (TAllTypesEntity allTypesEntity in allTypes)
        {
            var allTypeFromDb = allTypesFromDb.First(x => x.Id == allTypesEntity.Id);

            AssertAllTypesEqual(allTypesEntity, allTypeFromDb);
        }
    }

    [Fact]
    public void BulkInsertParametersTest()
    {
        Person[] peopleFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            connection.DeleteAll<Person>();

            connection.BulkInsertParameters(_people);

            peopleFromDb = connection.GetAll<Person>().ToArray();
        }

        foreach (Person person in _people)
        {
            var personFromDb = peopleFromDb.First(x => x.Ref == person.Ref);

            AssertPersonEqual(personFromDb, person);
        }
    }

    [Fact]
    public async Task UniqueErrorTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

            persons[0].FirstName = "Hello";
            persons[0].LastName = "world";

            persons[1].FirstName = "Hello";
            persons[1].LastName = "world";

            await Assert.ThrowsAnyAsync<Exception>(() => connection.BulkInsertAsync(persons));

            await connection.DeleteAllAsync<Person>();
        }
    }

    [Fact]
    public async Task UniqueIgnoreErrorsTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

            persons[0].FirstName = "Hello";
            persons[0].LastName = "world";

            persons[1].FirstName = "Hello";
            persons[1].LastName = "world";

            await connection.BulkInsertAsync(persons, flags: ExecutionFlags.IgnoreErrors);

            IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

            Assert.Single(peopleFromDb);

            AssertPersonEqual(peopleFromDb.First(), persons[0]);

            await connection.DeleteAllAsync<Person>();
        }
    }

    [Fact(Skip = "future")]
    public async Task UniqueUpdateIfExistsTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

            persons[0].FirstName = "Hello";
            persons[0].LastName = "world";

            persons[1].FirstName = "Hello";
            persons[1].LastName = "world";

            await connection.BulkInsertAsync(persons, flags: ExecutionFlags.UpdateIfExists);

            IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

            Assert.Single(peopleFromDb);

            AssertPersonEqual(peopleFromDb.First(), persons[1]);

            await connection.DeleteAllAsync<Person>();
        }
    }

    [Fact]
    public async Task PrimaryKeyAndUniqueUpdateIfExistsErrorTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(3).ToArray();

            persons[0].Id = 1;
            persons[0].FirstName = "Hello";
            persons[0].LastName = "world";

            persons[1].Id = 2;

            persons[2].Id = 2;
            persons[2].FirstName = "Hello";
            persons[2].LastName = "world";

            await Assert.ThrowsAnyAsync<Exception>(() => connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys | ExecutionFlags.UpdateIfExists));

            await connection.DeleteAllAsync<Person>();
        }
    }

    protected static void AssertPersonEqual(Person personFromDb, Person person)
    {
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