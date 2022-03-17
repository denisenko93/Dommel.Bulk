using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Bogus;
using Bogus.DataSets;
using Xunit;

namespace Dommel.Bulk.Tests;

public class SqlBuilderTests
{
    private readonly ISqlBuilder _sqlBuilder = new MySqlSqlBuilder();
    private readonly IReadOnlyCollection<Person> _data;

    public SqlBuilderTests()
    {
        var personGenerator = new Faker<Person>()
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.Age, f => f.Person.Random.Number(100))
            .RuleFor(x => x.Gender, f => f.Person.Gender)
            .RuleFor(x => x.BirthDay, f => f.Person.DateOfBirth);

        _data = Enumerable.Range(0, 100).Select(x => personGenerator.Generate()).ToArray();
    }

    [Fact]
    public void ExpressionSqlBuilderTest()
    {
        DommelBulkMapper.BuildInsertQueryExpression(_sqlBuilder, _data);
    }
}

[Table("people")]
public class Person
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

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