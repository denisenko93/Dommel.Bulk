using System.Collections.Generic;
using System.Linq;
using Dommel.Bulk.Tests.Common;
using Xunit;

namespace Dommel.Bulk.Tests;

public class SqlBuilderTests
{
    private readonly ISqlBuilder _sqlBuilder = new MySqlSqlBuilder();
    private readonly IReadOnlyCollection<Person> _people;
    private readonly IReadOnlyCollection<AllTypesEntity> _allTypes;

    public SqlBuilderTests()
    {
        _people = Enumerable.Range(0, 100).Select(x => FakeGenerators.PersonFaker.Generate()).ToArray();
        _allTypes = Enumerable.Range(0, 100).Select(x => FakeGenerators.AllTypesFaker.Generate()).ToArray();
    }

    [Fact]
    public void ExpressionSqlBuilderPersonTest()
    {
        DommelBulkMapper.BuildInsertQueryExpression(_sqlBuilder, _people);
    }

    [Fact]
    public void ReflectionSqlBuilderPersonTest()
    {
        DommelBulkMapper.BuildInsertQueryReflection(_sqlBuilder, _people);
    }

    [Fact]
    public void ExpressionSqlBuilderAllTypesTest()
    {
        var sql =DommelBulkMapper.BuildInsertQueryExpression(_sqlBuilder, _allTypes);
    }

    [Fact]
    public void ExpressionStringBuilderSqlBuilderAllTypesTest()
    {
        var sql =DommelBulkMapper.BuildInsertQueryExpressionStringBuilder(_sqlBuilder, _allTypes);
    }

    [Fact]
    public void ReflectionSqlBuilderAllTypesTest()
    {
        var sql =DommelBulkMapper.BuildInsertQueryReflection(_sqlBuilder, _allTypes);
    }
}