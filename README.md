# Dommel.Bulk
High performance insert data into database

Dommel.Bulk provides a convenient API for bulk insert operations using extension methods on the `IDbConnection` interface. The SQL queries are generated based on your POCO entities. Dommel.Bulk translates you entities to SQL expressions. [Dapper](https://github.com/StackExchange/Dapper) is used for query execution and object mapping. [Dommel](https://github.com/henkmollema/Dommel) is used for table and column names conventions

## Installing Dommel

Dommel.Bulk is available on [NuGet](https://www.nuget.org/packages/Dommel.Bulk).

### Install using the .NET CLI:
```
dotnet add package Dommel.Bulk
```

### Install using the NuGet Package Manager:
```
Install-Package Dommel.Bulk
```

## Using Dommel.Bulk

### Bulk insert using type mappers
```cs
var products = await connection.BulkInsertAsync<Product>(products);
```
Generate simple SQL expression using database specific type mappers. Supports MySql database. Has high performance.

### Bulk insert using SQL parameters
```cs
int insertedCount = await connection.BulkInsertParametersAsync<Product>(products);
```
Use SQL parameters for insert values. Support for all databases. Has middle performance.

## Type mappers

Support CLR types: `bool`, `byte`, `char`, `double`, `float`, `int`, `long`, `sbyte`, `short`, `uint`, `ulong`, `ushort`, `decimal`, `DateTime`, `DateTimeOffset`, `Guid`, `string`, `TimeSpan`, `DateOnly`, `TimeOnly`, `ArraySegment<byte>`, `byte[]`, enum types and nullable types

## Async and non-async
All Dommel.Bulk methods have async and non-async variants, such as as `BulkInsert` & `BulkInsertAsync`, `BulkInsertParameters` & `BulkInsertParametersAsync`.

## Extensibility
#### `ITypeMapper`
Implement this interface if you want to customize the mapping types to SQL query

```cs
public class JsonTypeMapper : ITypeMapper
{
    public LambdaExpression GetExpression()
    {
        // Map JSON type to SQL string.
        return (Expression<Func<JObject, string>>)x => $"'{x.ToString().Escape()}'";
    }
}
```

Use the `AddTypeMapper()` method to register the custom implementation:
```cs
AddTypeMapper(typeof(JObject), new JsonTypeMapper());
```
or
```cs
AddTypeMapper(typeof(JObject), new GenericTypeMapper<JObject>(x => $"'{x.ToString().Escape()}'"));
```