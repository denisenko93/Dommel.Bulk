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
        using IDbConnection connection = GetOpenConnection();

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

                 CONSTRAINT name_unique unique (first_name, last_name)
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
                PRIMARY KEY(""id""));

            drop table if exists ""UserLog"";

            CREATE TABLE ""UserLog"" (
                Ref text not null primary key ,
                Increment int not null,
                Name text not null,
                TimeStamp timestamp not null
            );");

        connection.Dispose();
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
    public async Task UniqueUpdateIfExistsTest()
    {
        using IDbConnection connection = GetOpenConnection();

        await connection.DeleteAllAsync<Person>();

        Person[] persons = FakeGenerators.PersonFaker.GenerateForever().Take(2).ToArray();

        persons[0].FirstName = "Hello";
        persons[0].LastName = "world";

        persons[1].FirstName = "Hello";
        persons[1].LastName = "world";

        await connection.InsertAsync(persons[0]);

        await connection.BulkInsertAsync(persons.Skip(1), flags: ExecutionFlags.UpdateIfExists, constraintName: "name_unique");

        IEnumerable<Person> peopleFromDb = await connection.GetAllAsync<Person>();

        Assert.Single(peopleFromDb);

        AssertPersonEqual(peopleFromDb.First(), persons[1]);

        await connection.DeleteAllAsync<Person>();

        connection.Dispose();
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
        Assert.Equal(expected.Int, actual.Int);
        Assert.Equal(expected.IntNull, actual.IntNull);
        Assert.Equal(expected.Long, actual.Long);
        Assert.Equal(expected.LongNull, actual.LongNull);
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
    }
}