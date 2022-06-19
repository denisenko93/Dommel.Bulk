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
    private readonly IReadOnlyCollection<Person> _people;
    private readonly IReadOnlyCollection<UserLog> _userLog;

    protected BulkInsertTestsBase()
    {
        _people = Enumerable.Range(0, 100)
            .Select(_ => FakeGenerators.PersonFaker.Generate())
            .ToArray();

        _userLog = Enumerable.Range(0, 100_000)
            .Select(_ => FakeGenerators.UserLogFaker.Generate())
            .ToArray();
    }

    protected abstract IEnumerable<IDbConnection> GetConnections();

    protected abstract IReadOnlyCollection<TAllTypesEntity> GetAllFakeTypes();

    protected abstract void AssertAllTypesEqual(TAllTypesEntity expected, TAllTypesEntity actual);

    protected IEnumerable<IDbConnection> GetOpenConnections()
    {
        IEnumerable<IDbConnection> connections = GetConnections();

        foreach (IDbConnection connection in connections)
        {
            connection.Open();
            yield return connection;
        }
    }

    [Fact]
    public async Task BulkInsertTestAsync()
    {
        IReadOnlyCollection<TAllTypesEntity> types = GetAllFakeTypes();

        foreach (IDbConnection connection in GetOpenConnections())
        {
            await connection.DeleteAllAsync<TAllTypesEntity>();

            await connection.BulkInsertAsync(types);

            TAllTypesEntity[] allTypesFromDb = (await connection.GetAllAsync<TAllTypesEntity>()).ToArray();

            connection.Dispose();

            foreach (TAllTypesEntity allTypesEntity in types)
            {
                TAllTypesEntity allTypeFromDb = allTypesFromDb.First(x => x.Id == allTypesEntity.Id);

                AssertAllTypesEqual(allTypesEntity, allTypeFromDb);
            }
        }
    }

    [Fact]
    public void BulkInsertTest()
    {
        foreach (IDbConnection connection in GetOpenConnections())
        {
            connection.DeleteAll<Person>();

            connection.BulkInsert(_people);

            Person[] peopleFromDb = connection.GetAll<Person>().ToArray();

            connection.Dispose();

            foreach (Person person in _people)
            {
                Person personFromDb = peopleFromDb.First(x => x.Ref == person.Ref);

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

    [Fact]
    public void BigDataBulkInsertTest()
    {
        foreach (IDbConnection connection in GetOpenConnections())
        {
            connection.DeleteAll<UserLog>();

            connection.BulkInsert(_userLog);

            UserLog[] userLogFromDb = connection.GetAll<UserLog>().ToArray();

            Assert.Equal(userLogFromDb.Length, _userLog.Count);

            connection.Dispose();
        }
    }

    [Fact]
    public async Task BulkInsertParametersTestAsync()
    {
        IReadOnlyCollection<TAllTypesEntity> allTypes = GetAllFakeTypes();

        foreach (IDbConnection connection in GetOpenConnections())
        {
            await connection.DeleteAllAsync<TAllTypesEntity>();

            await connection.BulkInsertParametersAsync(allTypes);

            TAllTypesEntity[] allTypesFromDb = (await connection.GetAllAsync<TAllTypesEntity>()).ToArray();

            connection.Dispose();

            foreach (TAllTypesEntity allTypesEntity in allTypes)
            {
                TAllTypesEntity allTypeFromDb = allTypesFromDb.First(x => x.Id == allTypesEntity.Id);

                AssertAllTypesEqual(allTypesEntity, allTypeFromDb);
            }
        }
    }

    [Fact]
    public void BulkInsertParametersTest()
    {
        foreach (IDbConnection connection in GetOpenConnections())
        {
            connection.DeleteAll<Person>();

            connection.BulkInsertParameters(_people);

            Person[] peopleFromDb = connection.GetAll<Person>().ToArray();

            connection.Dispose();

            foreach (Person person in _people)
            {
                Person personFromDb = peopleFromDb.First(x => x.Ref == person.Ref);

                AssertPersonEqual(personFromDb, person);
            }
        }
    }

    [Fact]
    public async Task UniqueErrorTest()
    {
        foreach (IDbConnection connection in GetOpenConnections())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

            persons[0].FirstName = "Hello";
            persons[0].LastName = "world";

            persons[1].FirstName = "Hello";
            persons[1].LastName = "world";

            await Assert.ThrowsAnyAsync<Exception>(() => connection.BulkInsertAsync(persons));

            await connection.DeleteAllAsync<Person>();

            connection.Dispose();
        }
    }

    [Fact]
    public async Task UniqueIgnoreErrorsTest()
    {
        foreach (IDbConnection connection in GetOpenConnections())
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

            connection.Dispose();
        }
    }

    [Fact]
    public async Task PrimaryKeyAndUniqueUpdateIfExistsErrorTest()
    {
        foreach (IDbConnection connection in GetOpenConnections())
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

            connection.Dispose();
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