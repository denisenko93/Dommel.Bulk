using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dommel.Bulk.Tests.Common;
using Npgsql;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public class PostgreSqlBulkInsertTests : BulkInsertTestsBase<PostgreSqlAllTypesEntity>
{
    public PostgreSqlBulkInsertTests()
    {
         using (IDbConnection connection = GetOpenConnection())
         {
             connection.Execute($@"
             drop table if exists ""people"";
             drop table if exists ""AllTypesEntities"";

             create table ""people""(
                 id int not null generated always as identity ,
                 ref uuid not null,
                 first_name varchar(255) not null,
                 last_name varchar(255) not null,
                 gender int not null,
                 age int not null,
                 birth_day timestamp  not null,
                 created_on timestamp  not null default current_timestamp,
                 full_name varchar(600) GENERATED ALWAYS AS (CASE WHEN first_name IS NULL THEN last_name
                                WHEN last_name  IS NULL THEN first_name
                                ELSE first_name || ' ' || last_name END) stored,
                 primary key(id),

                 unique (first_name, last_name),
                 unique (ref)
             );

             create table ""AllTypesEntities""(
                 ""Id"" uuid not null,
                 ""Ref"" uuid null,
                 ""Int"" int not null,
                 ""IntNull"" int null,
                 ""Long"" BIGINT not null,
                 ""LongNull"" BIGINT null,
                 ""Decimal"" decimal(65,28) not null,
                 ""DecimalNull"" decimal(65,28) null,
                 ""Float"" float not null,
                 ""FloatNull"" float null,
                 ""Double"" DOUBLE PRECISION not null,
                 ""DoubleNull"" DOUBLE PRECISION null,
                 ""Bool"" boolean not null,
                 ""BoolNull"" boolean null,
                 ""Char"" char(1) not null,
                 ""CharNull"" char(1) null,
                 ""String"" varchar(255) null,
                 ""StringNull"" text null,
                 ""Enum"" int not null,
                 ""EnumNull"" int null,
                 ""DateTime"" TIMESTAMP(6) not null,
                 ""DateTimeNull"" TIMESTAMP(6) null,
                 primary key(""Id""));

            drop table if exists ""string_value"";

            CREATE TABLE ""string_value""(
                ""id"" int generated always as identity,
                ""value"" text not null,
                PRIMARY KEY(""id""));");
         }
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\t")]
    [InlineData("\b")]
    [InlineData("\\")]
    [InlineData("'")]
    [InlineData("\"")]
    [InlineData("\\n")]
    [InlineData("Hello\nworld")]
    [InlineData("Hello\u001aworld")]
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

    [Fact(Skip = "future")]
    public async Task DoubleUniqueUpdateIfExistsIgnoreErrorsTest()
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

    protected override IDbConnection GetConnection()
    {
        return new NpgsqlConnection("User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=test;");
    }

    protected override IReadOnlyCollection<PostgreSqlAllTypesEntity> GetAllFakeTypes()
    {
        return Enumerable.Range(0, 100)
            .Select(_ => FakeGenerators.PostgreSqlTypesFaker.Generate())
            .ToArray();
    }

    protected override void AssertAllTypesEqual(PostgreSqlAllTypesEntity expected, PostgreSqlAllTypesEntity actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Ref, actual.Ref);
        // Assert.Equal(expected.Short, actual.Short);
        // Assert.Equal(expected.ShortNull, actual.ShortNull);
        // Assert.Equal(expected.UShort, actual.UShort);
        // Assert.Equal(expected.UShortNull, actual.UShortNull);
        Assert.Equal(expected.Int, actual.Int);
        Assert.Equal(expected.IntNull, actual.IntNull);
        // Assert.Equal(expected.UInt, actual.UInt);
        // Assert.Equal(expected.UIntNull, actual.UIntNull);
        Assert.Equal(expected.Long, actual.Long);
        Assert.Equal(expected.LongNull, actual.LongNull);
        // Assert.Equal(expected.ULong, actual.ULong);
        // Assert.Equal(expected.ULongNull, actual.ULongNull);
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
        // Assert.Equal(expected.Byte, actual.Byte);
        // Assert.Equal(expected.ByteNull, actual.ByteNull);
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
        // Assert.True(expected.TimeSpan - actual.TimeSpan < TimeSpan.FromSeconds(1) && expected.TimeSpan - actual.TimeSpan > TimeSpan.FromSeconds(-1));
        // Assert.True(expected.TimeSpanNull == actual.TimeSpanNull
        //             || (expected.TimeSpanNull - actual.TimeSpanNull < TimeSpan.FromSeconds(1) && expected.TimeSpanNull - actual.TimeSpanNull > TimeSpan.FromSeconds(-1)));
        // Assert.Contains(actual.ByteArray, x => expected.ByteArray.Contains(x));
        // if (expected.ByteArrayNull == null)
        // {
        //     Assert.Null(actual.ByteArrayNull);
        // }
        // else
        // {
        //     Assert.Contains(actual.ByteArrayNull, x => expected.ByteArrayNull.Contains(x));
        // }
    }
}