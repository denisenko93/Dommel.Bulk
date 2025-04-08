using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dommel.Bulk.Tests.Common;
using MySqlConnector;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public class MysqlBulkInsertTests : BulkInsertTestsBase<MySqlAllTypesEntity>
{
    public MysqlBulkInsertTests()
    {
        using IDbConnection connection = GetOpenConnection();

        connection.Execute($@"
            drop table if exists people;

            create table people(
                id int not null auto_increment,
                ref char(36) not null,
                first_name varchar(255) not null,
                last_name varchar(255) not null,
                gender int not null,
                age int not null,
                birth_day datetime not null,
                created_on datetime not null,
                full_name varchar(600) GENERATED ALWAYS AS (CONCAT(first_name, ' ', last_name)),

                primary key(id),

                unique(first_name, last_name)
            );

            create trigger people_before_insert
                before insert
                on people
                for each row
            BEGIN
                set NEW.created_on = NOW();
            end;

            drop table if exists `AllTypesEntities`;

            create table `AllTypesEntities`(
                `Id` char(36) not null,
                `Ref` char(36) null,

                `Short` SMALLINT not null,
                `ShortNull` SMALLINT null,
                `UShort` SMALLINT UNSIGNED not null,
                `UShortNull` SMALLINT UNSIGNED null,

                `Int` int not null,
                `IntNull` int null,
                `UInt` int UNSIGNED not null,
                `UIntNull` int UNSIGNED null,

                `Long` BIGINT not null,
                `LongNull` BIGINT null,
                `ULong` BIGINT UNSIGNED not null,
                `ULongNull` BIGINT UNSIGNED null,

                `Decimal` decimal(65,28) not null,
                `DecimalNull` decimal(65,28) null,

                `Float` float not null,
                `FloatNull` float null,

                `Double` double not null,
                `DoubleNull` double null,

                `Byte` TINYINT UNSIGNED not null,
                `ByteNull` TINYINT UNSIGNED null,
                `SByte` TINYINT not null,
                `SByteNull` TINYINT null,

                `Bool` tinyint(1) not null,
                `BoolNull` tinyint(1) null,

                `Char` char(1) not null,
                `CharNull` char(1) null,

                `String` varchar(255) null,
                `StringNull` text null,

                `Enum` int not null,
                `EnumNull` int null,

                `DateTime` datetime(6) not null,
                `DateTimeNull` datetime(6) null,

                `TimeSpan` TIME(6) not null,
                `TimeSpanNull` TIME(6) null,

                `ByteArray` VARBINARY(1003) not null,
                `ByteArrayNull` blob null,"
#if NET6_0_OR_GREATER
                +@"`DateOnly` datetime(6) not null,
                `DateOnlyNull` datetime(6) null,

                `TimeOnly` TIME(6) not null,
                `TimeOnlyNull` TIME(6) null,"
#endif

                +@"primary key(Id));

            drop table if exists `string_value`;

            CREATE TABLE string_value(
                `id` int AUTO_INCREMENT,
                `value` text not null,
                PRIMARY KEY(`id`));

drop table if exists UserLog;

CREATE TABLE UserLog (
    Ref varchar(100) not null primary key ,
    Increment int not null,
    Name text not null,
    TimeStamp datetime not null
);");
        connection.Dispose();
        SqlMapper.ResetTypeHandlers();
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
    [InlineData("Hello\0world")]
    [InlineData("Hello\u001aworld")]
    [InlineData("\\¥Š₩∖﹨＼")]
    [InlineData("\"'`´ʹʺʻʼˈˊˋ˙̀́‘’‚′‵❛❜＇")]
    public async Task StringTest(string str)
    {
        using IDbConnection connection = GetOpenConnection();

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

        connection.Dispose();
    }

    [Fact]
    public async Task InsertPrimaryKeyTest()
    {
        using IDbConnection connection = GetOpenConnection();

        await connection.DeleteAllAsync<Person>();

        Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

        await connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys);

        await connection.DeleteAllAsync<Person>();

        connection.Dispose();
    }

    [Fact]
    public async Task PrimaryKeyErrorTest()
    {
        using IDbConnection connection = GetOpenConnection();

        await connection.DeleteAllAsync<Person>();

        Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

        persons[0].Id = 1;
        persons[1].Id = 1;

        await Assert.ThrowsAnyAsync<Exception>(() => connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys));

        await connection.DeleteAllAsync<Person>();

        connection.Dispose();
    }

    [Fact]
    public async Task PrimaryKeyIgnoreErrorsTest()
    {
        using IDbConnection connection = GetOpenConnection();

        await connection.DeleteAllAsync<Person>();

        Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

        persons[0].Id = 1;
        persons[1].Id = 1;

        await connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys | ExecutionFlags.IgnoreErrors);

        IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

        Assert.Single(peopleFromDb);

        AssertPersonEqual(peopleFromDb.First(), persons[0]);

        await connection.DeleteAllAsync<Person>();

        connection.Dispose();
    }

    [Fact]
    public async Task PrimaryKeyUpdateIfExistsTest()
    {
        using IDbConnection connection = GetOpenConnection();

        await connection.DeleteAllAsync<Person>();

        Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

        persons[0].Id = 1;
        persons[1].Id = 1;

        await connection.BulkInsertAsync(persons, flags: ExecutionFlags.InsertDatabaseGeneratedKeys | ExecutionFlags.UpdateIfExists);

        IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

        Assert.Single(peopleFromDb);

        AssertPersonEqual(peopleFromDb.First(), persons[1]);

        await connection.DeleteAllAsync<Person>();

        connection.Dispose();
    }

    [Fact]
    public async Task PrimaryKeyAndUniqueUpdateIfExistsIgnoreErrorsTest()
    {
        using IDbConnection connection = GetOpenConnection();

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

        connection.Dispose();
    }


    [Fact]
    public async Task UniqueUpdateIfExistsTest()
    {
        using IDbConnection connection = GetOpenConnection();

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

        connection.Dispose();
    }

    protected override IDbConnection GetConnection()
    {
        return new MySqlConnection("Server=localhost;Database=test;Uid=root;Pwd=root;UseAffectedRows=false;");
    }

    protected override IReadOnlyCollection<MySqlAllTypesEntity> GetAllFakeTypes()
    {
        return Enumerable.Range(0, 100)
            .Select(_ => FakeGenerators.MySqlTypesFaker.Generate())
            .ToArray();
    }

    protected override void AssertAllTypesEqual(MySqlAllTypesEntity expected, MySqlAllTypesEntity actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Ref, actual.Ref);
        Assert.Equal(expected.Short, actual.Short);
        Assert.Equal(expected.ShortNull, actual.ShortNull);
        Assert.Equal(expected.UShort, actual.UShort);
        Assert.Equal(expected.UShortNull, actual.UShortNull);
        Assert.Equal(expected.Int, actual.Int);
        Assert.Equal(expected.IntNull, actual.IntNull);
        Assert.Equal(expected.UInt, actual.UInt);
        Assert.Equal(expected.UIntNull, actual.UIntNull);
        Assert.Equal(expected.Long, actual.Long);
        Assert.Equal(expected.LongNull, actual.LongNull);
        Assert.Equal(expected.ULong, actual.ULong);
        Assert.Equal(expected.ULongNull, actual.ULongNull);
        Assert.Equal(expected.Decimal, actual.Decimal);
        Assert.Equal(expected.DecimalNull, actual.DecimalNull);
        Assert.True(Math.Abs(expected.Float - actual.Float) < 0.000001);
        if (expected.FloatNull == null)
        {
            Assert.Null(actual.FloatNull);
        }
        else
        {
            Assert.True(Math.Abs(expected.FloatNull.Value - actual.FloatNull.Value) < 0.000001);
        }
        Assert.True(Math.Abs(expected.Double - actual.Double) < 0.000000000000001);
        if (expected.DoubleNull == null)
        {
            Assert.Null(actual.DoubleNull);
        }
        else
        {
            Assert.True(Math.Abs(expected.DoubleNull.Value - actual.DoubleNull.Value) < 0.000000000000001);
        }
        Assert.Equal(expected.Byte, actual.Byte);
        Assert.Equal(expected.ByteNull, actual.ByteNull);
        Assert.Equal(expected.Bool, actual.Bool);
        Assert.Equal(expected.BoolNull, actual.BoolNull);
        Assert.Equal(expected.Char, actual.Char);
        Assert.Equal(expected.CharNull, actual.CharNull);
        Assert.Equal(expected.String, actual.String);
        Assert.Equal(expected.StringNull, actual.StringNull);
        Assert.Equal(expected.Enum, actual.Enum);
        Assert.Equal(expected.EnumNull, actual.EnumNull);
        Assert.Equal(expected.DateTime, actual.DateTime, TimeSpan.FromSeconds(1));
        if (expected.DateTimeNull == null)
        {
            Assert.Null(actual.DateTimeNull);
        }
        else
        {
            Assert.Equal(expected.DateTimeNull.Value, actual.DateTimeNull.Value, TimeSpan.FromSeconds(1));
        }
        Assert.True(expected.TimeSpan - actual.TimeSpan < TimeSpan.FromSeconds(1) && expected.TimeSpan - actual.TimeSpan > TimeSpan.FromSeconds(-1));
        Assert.True(expected.TimeSpanNull == actual.TimeSpanNull
                    || (expected.TimeSpanNull - actual.TimeSpanNull < TimeSpan.FromSeconds(1) && expected.TimeSpanNull - actual.TimeSpanNull > TimeSpan.FromSeconds(-1)));
        Assert.Contains(actual.ByteArray, x => expected.ByteArray.Contains(x));
        if (expected.ByteArrayNull == null)
        {
            Assert.Null(actual.ByteArrayNull);
        }
        else
        {
            Assert.Contains(actual.ByteArrayNull, x => expected.ByteArrayNull.Contains(x));
        }
    }
}