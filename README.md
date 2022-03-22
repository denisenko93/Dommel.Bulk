# Dommel.Bulk
High performance insert data into database

Dommel.Bulk provides a convenient API for bulk insert operations using extension methods on the `IDbConnection` interface. The SQL queries are generated based on your POCO entities. Dommel.Bulk translates you entities to SQL expressions. [Dapper](https://github.com/StackExchange/Dapper) is used for query execution and object mapping. [Dommel](https://github.com/henkmollema/Dommel) is used for table and column names conventions

## Installing Dommel.Bulk

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

## Performance
Performance comparison between bulk methods and `InsertAll` from [Dommel](https://github.com/henkmollema/Dommel) library
``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1586 (21H2)
Intel Core i5-7300HQ CPU 2.50GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT


```
|                             Method | Categories | DataSize |         Mean |        Error |       StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|----------------------------------- |----------- |--------- |-------------:|-------------:|-------------:|------:|--------:|-----------:|----------:|----------:|----------:|
|           BulkInsertBenchmarkAsync |     simple |    10000 |    353.41 ms |     7.023 ms |    17.228 ms |  1.00 |    0.00 |  2000.0000 | 1000.0000 |         - |     14 MB |
|                SqlBuilderBenchmark |     simple |    10000 |     18.75 ms |     0.370 ms |     0.380 ms |  0.05 |    0.00 |  2187.5000 |  750.0000 |  312.5000 |     11 MB |
|                                    |            |          |              |              |              |       |         |            |           |           |           |
| BulkInsertParametersBenchmarkAsync | parameters |    10000 |    546.74 ms |    10.874 ms |    26.877 ms |  1.00 |    0.00 |  6000.0000 | 3000.0000 | 1000.0000 |     48 MB |
|      SqlBuilderParametersBenchmark | parameters |    10000 |     65.50 ms |     1.301 ms |     3.605 ms |  0.12 |    0.01 |  3625.0000 | 1750.0000 |  875.0000 |     22 MB |~~~~
|                                    |            |          |              |              |              |       |         |            |           |           |           |
|            InsertAllBenchmarkAsync |            |    10000 | 57,054.74 ms | 2,369.037 ms | 6,157.442 ms |     ? |       ? | 14000.0000 | 1000.0000 |         - |     45 MB |

## Disclaimer
The material embodied in this software is provided to you "as-is" and without warranty of any kind, express, implied or otherwise, including without limitation, any warranty of fitness for a particular purpose. In no event shall the Centers for Disease Control and Prevention (CDC) or the United States (U.S.) government be liable to you or anyone else for any direct, special, incidental, indirect or consequential damages of any kind, or any damages whatsoever, including without limitation, loss of profit, loss of use, savings or revenue, or the claims of third parties, whether or not CDC or the U.S. government has been advised of the possibility of such loss, however caused and on any theory of liability, arising out of or in connection with the possession, use or performance of this software.
