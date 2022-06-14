using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dommel.Bulk.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Dommel.Bulk.IntegrationTests;

public class SqLiteBulkInsertTests : BulkInsertTestsBase<SqLiteAllTypesEntity>
{
    public SqLiteBulkInsertTests()
    {
        using IDbConnection connection = GetOpenConnection();

        connection.Execute($@"
             drop table if exists people;
drop table if exists AllTypesEntities;

create table people(
                       id integer not null primary key AUTOINCREMENT,
                       ref text not null,
                       first_name text not null,
                       last_name text not null,
                       gender int not null,
                       age int not null,
                       birth_day text not null,
                       created_on text not null default current_timestamp,
                       full_name text GENERATED ALWAYS AS (CASE WHEN first_name IS NULL THEN last_name
                                                                        WHEN last_name  IS NULL THEN first_name
                                                                        ELSE first_name || ' ' || last_name END) stored,
                       CONSTRAINT name_unique unique (first_name, last_name)
);

create table AllTypesEntities(
                                 Id text not null,
                                 Ref text null,
                                 Int int not null,
                                 IntNull int null,
                                 Long int not null,
                                 LongNull int null,
                                 Decimal real not null,
                                 DecimalNull real null,
                                 Float real not null,
                                 FloatNull real null,
                                 Double real not null,
                                 DoubleNull real null,
                                 Bool int not null,
                                 BoolNull int null,
                                 Char text not null,
                                 CharNull text null,
                                 String text null,
                                 StringNull text null,
                                 Enum int not null,
                                 EnumNull int null,
                                 DateTime text not null,
                                 DateTimeNull text null,
                                 primary key(Id));

drop table if exists string_value;

CREATE TABLE string_value(
                             id int,
                             value text not null,
                             PRIMARY KEY(id));");
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
    }

    protected override IDbConnection GetConnection()
    {
        return new SqliteConnection(@"Data Source=C:\Users\User1\DataGripProjects\default\identifier.sqlite");
    }

    protected override IReadOnlyCollection<SqLiteAllTypesEntity> GetAllFakeTypes()
    {
        return Enumerable.Range(0, 100)
            .Select(_ => FakeGenerators.SqLiteTypesFaker.Generate())
            .ToArray();
    }

    protected override void AssertAllTypesEqual(SqLiteAllTypesEntity expected, SqLiteAllTypesEntity actual)
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