using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dommel.Bulk.Tests.Common;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public abstract class BulkInsertTestsBase
{
    protected const string DatabaseName = "dommel_bulk_test";

    private IReadOnlyCollection<Person> _people;
    private IReadOnlyCollection<AllTypesEntity> _allTypes;

    public BulkInsertTestsBase()
    {
        _people = Enumerable.Range(0, 100)
            .Select(_ => FakeGenerators.PersonFaker.Generate())
            .ToArray();

        _allTypes = Enumerable.Range(0, 1000)
            .Select(_ => FakeGenerators.AllTypesFaker.Generate())
            .ToArray();
    }

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
        AllTypesEntity[] allTypesFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<AllTypesEntity>();

            await connection.BulkInsertAsync(_allTypes);

            allTypesFromDb = (await connection.GetAllAsync<AllTypesEntity>()).ToArray();
        }

        foreach (AllTypesEntity allTypesEntity in _allTypes)
        {
            var allTypeFromDb = allTypesFromDb.First(x => x.Id == allTypesEntity.Id);

            Assert.NotNull(allTypeFromDb);

            Assert.Equal(allTypesEntity.Id, allTypeFromDb.Id);
            Assert.Equal(allTypesEntity.Ref, allTypeFromDb.Ref);
            Assert.Equal(allTypesEntity.Short, allTypeFromDb.Short);
            Assert.Equal(allTypesEntity.ShortNull, allTypeFromDb.ShortNull);
            Assert.Equal(allTypesEntity.UShort, allTypeFromDb.UShort);
            Assert.Equal(allTypesEntity.UShortNull, allTypeFromDb.UShortNull);
            Assert.Equal(allTypesEntity.Int, allTypeFromDb.Int);
            Assert.Equal(allTypesEntity.IntNull, allTypeFromDb.IntNull);
            Assert.Equal(allTypesEntity.UInt, allTypeFromDb.UInt);
            Assert.Equal(allTypesEntity.UIntNull, allTypeFromDb.UIntNull);
            Assert.Equal(allTypesEntity.Long, allTypeFromDb.Long);
            Assert.Equal(allTypesEntity.LongNull, allTypeFromDb.LongNull);
            Assert.Equal(allTypesEntity.ULong, allTypeFromDb.ULong);
            Assert.Equal(allTypesEntity.ULongNull, allTypeFromDb.ULongNull);
            Assert.Equal(allTypesEntity.Decimal, allTypeFromDb.Decimal);
            Assert.Equal(allTypesEntity.DecimalNull, allTypeFromDb.DecimalNull);
            Assert.True(Math.Abs(allTypesEntity.Float - allTypeFromDb.Float) < 0.000001, $"{Math.Abs(allTypesEntity.Float - allTypeFromDb.Float)} must be less then {0.000001}");
            if (allTypesEntity.FloatNull == null)
            {
                Assert.Null(allTypeFromDb.FloatNull);
            }
            else
            {
                Assert.True(Math.Abs(allTypesEntity.FloatNull - allTypeFromDb.FloatNull ?? 0) < 0.000001, $"{Math.Abs(allTypesEntity.FloatNull - allTypeFromDb.FloatNull ?? 0)} must be less then {0.000001}");
            }
            Assert.True(Math.Abs(allTypesEntity.Double - allTypeFromDb.Double) < 0.000_000_000_000_001, $"{Math.Abs(allTypesEntity.Double - allTypeFromDb.Double)} must be less then {0.000001}");
            if (allTypesEntity.DoubleNull == null)
            {
                Assert.Null(allTypeFromDb.DoubleNull);
            }
            else
            {
                Assert.True(Math.Abs(allTypesEntity.DoubleNull - allTypeFromDb.DoubleNull ?? 0) < 0.000_000_000_000_001, $"{Math.Abs(allTypesEntity.DoubleNull - allTypeFromDb.DoubleNull ?? 0)} must be less then {0.000001}");
            }
            Assert.Equal(allTypesEntity.Byte, allTypeFromDb.Byte);
            Assert.Equal(allTypesEntity.ByteNull, allTypeFromDb.ByteNull);
            Assert.Equal(allTypesEntity.SByte, allTypeFromDb.SByte);
            Assert.Equal(allTypesEntity.SByteNull, allTypeFromDb.SByteNull);
            Assert.Equal(allTypesEntity.Bool, allTypeFromDb.Bool);
            Assert.Equal(allTypesEntity.BoolNull, allTypeFromDb.BoolNull);
            Assert.Equal(allTypesEntity.Char, allTypeFromDb.Char);
            Assert.Equal(allTypesEntity.CharNull, allTypeFromDb.CharNull);
            Assert.Equal(allTypesEntity.String, allTypeFromDb.String);
            Assert.Equal(allTypesEntity.StringNull, allTypeFromDb.StringNull);
            Assert.Equal(allTypesEntity.Enum, allTypeFromDb.Enum);
            Assert.Equal(allTypesEntity.EnumNull, allTypeFromDb.EnumNull);
            Assert.Equal(allTypesEntity.DateTime, allTypeFromDb.DateTime, TimeSpan.FromTicks(9));
            if (allTypesEntity.DateTimeNull == null)
            {
                Assert.Null(allTypeFromDb.DateTimeNull);
            }
            else
            {
                Assert.Equal(allTypesEntity.DateTimeNull.Value, allTypeFromDb.DateTimeNull.Value, TimeSpan.FromTicks(9));
            }
            Assert.True(allTypesEntity.TimeSpan - allTypeFromDb.TimeSpan < TimeSpan.FromSeconds(1) && allTypesEntity.TimeSpan - allTypeFromDb.TimeSpan > TimeSpan.FromSeconds(-1));
            Assert.True(allTypesEntity.TimeSpanNull == allTypeFromDb.TimeSpanNull
                        || (allTypesEntity.TimeSpanNull - allTypeFromDb.TimeSpanNull < TimeSpan.FromSeconds(1) && allTypesEntity.TimeSpanNull - allTypeFromDb.TimeSpanNull > TimeSpan.FromSeconds(-1)));
            Assert.Contains(allTypeFromDb.ByteArray, x => allTypesEntity.ByteArray.Contains(x));
            if (allTypesEntity.ByteArrayNull == null)
            {
                Assert.Null(allTypeFromDb.ByteArrayNull);
            }
            else
            {
                Assert.Contains(allTypeFromDb.ByteArrayNull, x => allTypesEntity.ByteArrayNull.Contains(x));
            }
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
        AllTypesEntity[] allTypesFromDb;

        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<AllTypesEntity>();

            await connection.BulkInsertParametersAsync(_allTypes);

            allTypesFromDb = (await connection.GetAllAsync<AllTypesEntity>()).ToArray();
        }

        foreach (AllTypesEntity allTypesEntity in _allTypes)
        {
            var allTypeFromDb = allTypesFromDb.First(x => x.Id == allTypesEntity.Id);

            Assert.NotNull(allTypeFromDb);

            Assert.Equal(allTypesEntity.Id, allTypeFromDb.Id);
            Assert.Equal(allTypesEntity.Ref, allTypeFromDb.Ref);
            Assert.Equal(allTypesEntity.Short, allTypeFromDb.Short);
            Assert.Equal(allTypesEntity.ShortNull, allTypeFromDb.ShortNull);
            Assert.Equal(allTypesEntity.UShort, allTypeFromDb.UShort);
            Assert.Equal(allTypesEntity.UShortNull, allTypeFromDb.UShortNull);
            Assert.Equal(allTypesEntity.Int, allTypeFromDb.Int);
            Assert.Equal(allTypesEntity.IntNull, allTypeFromDb.IntNull);
            Assert.Equal(allTypesEntity.UInt, allTypeFromDb.UInt);
            Assert.Equal(allTypesEntity.UIntNull, allTypeFromDb.UIntNull);
            Assert.Equal(allTypesEntity.Long, allTypeFromDb.Long);
            Assert.Equal(allTypesEntity.LongNull, allTypeFromDb.LongNull);
            Assert.Equal(allTypesEntity.ULong, allTypeFromDb.ULong);
            Assert.Equal(allTypesEntity.ULongNull, allTypeFromDb.ULongNull);
            Assert.Equal(allTypesEntity.Decimal, allTypeFromDb.Decimal);
            Assert.Equal(allTypesEntity.DecimalNull, allTypeFromDb.DecimalNull);
            Assert.True(Math.Abs(allTypesEntity.Float - allTypeFromDb.Float) < 0.000001);
            if (allTypesEntity.FloatNull == null)
            {
                Assert.Null(allTypeFromDb.FloatNull);
            }
            else
            {
                Assert.True(Math.Abs(allTypesEntity.FloatNull.Value - allTypeFromDb.FloatNull.Value) < 0.000001);
            }
            Assert.Equal(allTypesEntity.Double, allTypeFromDb.Double, 15);
            if (allTypesEntity.DoubleNull == null)
            {
                Assert.Null(allTypeFromDb.DoubleNull);
            }
            else
            {
                Assert.Equal(allTypesEntity.DoubleNull.Value, allTypeFromDb.DoubleNull.Value, 15);
            }
            Assert.Equal(allTypesEntity.Byte, allTypeFromDb.Byte);
            Assert.Equal(allTypesEntity.ByteNull, allTypeFromDb.ByteNull);
            Assert.Equal(allTypesEntity.SByte, allTypeFromDb.SByte);
            Assert.Equal(allTypesEntity.SByteNull, allTypeFromDb.SByteNull);
            Assert.Equal(allTypesEntity.Bool, allTypeFromDb.Bool);
            Assert.Equal(allTypesEntity.BoolNull, allTypeFromDb.BoolNull);
            Assert.Equal(allTypesEntity.Char, allTypeFromDb.Char);
            Assert.Equal(allTypesEntity.CharNull, allTypeFromDb.CharNull);
            Assert.Equal(allTypesEntity.String, allTypeFromDb.String);
            Assert.Equal(allTypesEntity.StringNull, allTypeFromDb.StringNull);
            Assert.Equal(allTypesEntity.Enum, allTypeFromDb.Enum);
            Assert.Equal(allTypesEntity.EnumNull, allTypeFromDb.EnumNull);
            Assert.Equal(allTypesEntity.DateTime, allTypeFromDb.DateTime, TimeSpan.FromSeconds(1));
            if (allTypesEntity.DateTimeNull == null)
            {
                Assert.Null(allTypeFromDb.DateTimeNull);
            }
            else
            {
                Assert.Equal(allTypesEntity.DateTimeNull.Value, allTypeFromDb.DateTimeNull.Value, TimeSpan.FromSeconds(1));
            }
            Assert.True(allTypesEntity.TimeSpan - allTypeFromDb.TimeSpan < TimeSpan.FromSeconds(1) && allTypesEntity.TimeSpan - allTypeFromDb.TimeSpan > TimeSpan.FromSeconds(-1));
            Assert.True(allTypesEntity.TimeSpanNull == allTypeFromDb.TimeSpanNull
                        || (allTypesEntity.TimeSpanNull - allTypeFromDb.TimeSpanNull < TimeSpan.FromSeconds(1) && allTypesEntity.TimeSpanNull - allTypeFromDb.TimeSpanNull > TimeSpan.FromSeconds(-1)));
            Assert.Contains(allTypeFromDb.ByteArray, x => allTypesEntity.ByteArray.Contains(x));
            if (allTypesEntity.ByteArrayNull == null)
            {
                Assert.Null(allTypeFromDb.ByteArrayNull);
            }
            else
            {
                Assert.Contains(allTypeFromDb.ByteArrayNull, x => allTypesEntity.ByteArrayNull.Contains(x));
            }
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

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\t")]
    [InlineData("\b")]
    [InlineData("\0")]
    [InlineData("\\")]
    [InlineData("'")]
    [InlineData("\"")]
    [InlineData("\\n")]
    [InlineData("Hello\nworld")]
    [InlineData("Hello\u001aworld")]
    [InlineData("Hello\0world")]
    [InlineData("\\¥Š₩∖﹨＼")]
    [InlineData("\"'`´ʹʺʻʼˈˊˋ˙̀́‘’‚′‵❛❜＇")]
    public async Task StringTest(string str)
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<StringValue>();

            StringValue[] stringValues = new[]
            {
                new StringValue
                {
                    Value = str
                }
            };

            await connection.BulkInsertAsync(stringValues);

            IEnumerable<StringValue> stringValuesFromDb = await connection.GetAllAsync<StringValue>();

            Assert.Single(stringValuesFromDb);

            Assert.Equal(stringValues.First().Value, stringValuesFromDb.First().Value);

            await connection.DeleteAllAsync<StringValue>();
        }
    }

    [Fact]
    public async Task PrimaryKeyErrorTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

            persons[0].Id = 1;
            persons[1].Id = 1;

            await Assert.ThrowsAnyAsync<Exception>(() => connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys));

            await connection.DeleteAllAsync<Person>();
        }
    }

    [Fact]
    public async Task PrimaryKeyIgnoreErrorsTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

            persons[0].Id = 1;
            persons[1].Id = 1;

            await connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys | ExecutionFlags.IgnoreErrors);

            IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

            Assert.Single(peopleFromDb);

            AssertPersonEqual(peopleFromDb.First(), persons[0]);

            await connection.DeleteAllAsync<Person>();
        }
    }

    [Fact]
    public async Task PrimaryKeyUpdateIfExistsTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

            persons[0].Id = 1;
            persons[1].Id = 1;

            await connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys | ExecutionFlags.UpdateIfExists);

            IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

            Assert.Single(peopleFromDb);

            AssertPersonEqual(peopleFromDb.First(), persons[1]);

            await connection.DeleteAllAsync<Person>();
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

    [Fact]
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

    [Fact]
    public async Task PrimaryKeyAndUniqueUpdateIfExistsIgnoreErrorsTest()
    {
        using (IDbConnection connection = GetOpenConnection())
        {
            await connection.DeleteAllAsync<Person>();

            Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(4).ToArray();

            persons[0].Id = 1;
            persons[0].FirstName = "Hello";
            persons[0].LastName = "world";

            persons[1].Id = 2;

            persons[2].Id = 2;
            persons[2].FirstName = "Hello";
            persons[2].LastName = "world";

            persons[3].Id = 2;

            await connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys | ExecutionFlags.UpdateIfExists | ExecutionFlags.IgnoreErrors);

            IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

            Assert.Equal(2, peopleFromDb.Count());

            AssertPersonEqual(peopleFromDb.First(), persons[0]);
            AssertPersonEqual(peopleFromDb.Skip(1).First(), persons[3]);

            await connection.DeleteAllAsync<Person>();
        }
    }

    private static void AssertPersonEqual(Person personFromDb, Person person)
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