# Dommel.Bulk ⛓
High performance insert data into database

Dommel.Bulk provides a convenient API for bulk insert operations using extension methods on the `IDbConnection` interface. The SQL queries are generated based on your POCO entities. Dommel.Bulk translates you entities to SQL expressions. [Dapper](https://github.com/StackExchange/Dapper) is used for query execution and object mapping. [Dommel](https://github.com/henkmollema/Dommel) is used for table and column names conventions.

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

## Dommel.Bulk basic usage

### Bulk insert using type mappers
```cs
var products = await connection.BulkInsertAsync<Product>(products);
```
Generates simple SQL expression using database specific type mappers. Supports MySql and PostgreSql databases. Has high performance solutions inside.

### Bulk insert using SQL parameters
```cs
int insertedCount = await connection.BulkInsertParametersAsync<Product>(products);
```
Use SQL parameters for insert values. Support for all databases. Has middle performance.

## Dommel.Bulk extended usage

### Transaction
All extension methods supports database transaction usage. Example:
```csharp
using (IDbTransaction transaction = _connection.BeginTransaction())
{
    await _connection.BulkInsertAsync(data, transaction);
}
```
### ExecutionFlags
Dommel.Bulk supports flags that allow additional database functionality to control bulk insertion.

| Flag | Description                         |
|------|-------------------------------------|
| `None`   | Does not use extended functionality |
| `InsertDatabaseGeneratedKeys` | Insert auto increment keys into database |
| `UpdateIfExists` | Update rows in database when they exist. This flag is enabled if `propertiesToUpdate` exists |
| `IgnoreErrors` | Ignore duplicate keys or unique index errors |

Example:
```csharp
await _connection.BulkInsertAsync(data, flags: ExecutionFlags.IgnoreErrors);
```
### Properties to update
You can specify which properties to update if there are conflicts when inserting.
```csharp
await _connection.BulkInsertAsync(data, null, default, null, flags: ExecutionFlags.IgnoreErrors, nameof(Person.FirstName), nameof(Person.LastName));
```

## Type mappers
Mapping C# Types to Database Type Literals
Support CLR types: `bool`, `byte`, `char`, `double`, `float`, `int`, `long`, `sbyte`, `short`, `uint`, `ulong`, `ushort`, `decimal`, `DateTime`, `Guid`, `string`, `TimeSpan`, `byte[]`, `enum` and nullable types. For concrete type specification check sql provider.

## Async and non-async
All Dommel.Bulk methods have async and non-async variants, such as as `BulkInsert` & `BulkInsertAsync`, `BulkInsertParameters` & `BulkInsertParametersAsync`.

## Database adapters
Represents a concrete implementation of a database abstraction. Supports MySql and PostgreSql implementations.

## Extensibility
### Type mapper
Use the `AddTypeMapper()` method to register the custom type mapper.
```cs
DommelBulkMapper.AddTypeMapper(typeof(MySqlConnection), new GenericTypeMapper<JsonElement>((e, tw) => tw.Write(e.ToString())));
```
### Database adapter
Use the `AddTypeMapper()` method to register the custom type mapper.
```csharp
DommelBulkMapper.AddDatabaseAdapter(typeof(NpgsqlConnection), new NpgDatabaseAdapter());
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
|                             Method | Categories | DataSize |       Mean |      Error |     StdDev | Ratio |       Gen 0 |       Gen 1 |     Gen 2 | Allocated |
|----------------------------------- |------------|----------|-----------:|-----------:|-----------:|------:|------------:|------------:|----------:|----------:|
|           BulkInsertBenchmarkAsync |     simple | 10000    | 1,989.8 ms |   86.79 ms |   45.39 ms |  1.00 |  13000.0000 |   5000.0000 | 1000.0000 |    273 MB |
|                SqlBuilderBenchmark |     simple | 10000    |   140.7 ms |   22.24 ms |   13.23 ms |  0.07 |  13000.0000 |   5000.0000 | 1000.0000 |    143 MB |
|                                    |            |          |            |            |            |       |             |             |           |           |
| BulkInsertParametersBenchmarkAsync | parameters | 10000    | 3,029.4 ms |   71.10 ms |   42.31 ms |  1.00 |  43000.0000 |  12000.0000 | 2000.0000 |    368 MB |
|      SqlBuilderParametersBenchmark | parameters | 10000    |   516.9 ms |   16.59 ms |   10.98 ms |  0.17 |  19000.0000 |   7000.0000 | 1000.0000 |    156 MB |
|                                    |            |          |            |            |            |       |             |             |           |           |
|            InsertAllBenchmarkAsync |            | 10000    |   107.21 s |   29.986 s |   19.834 s |       |  38000.0000 |   3000.0000 |         - |    114 MB |
| InsertAllTransactionBenchmarkAsync |            | 10000    |    15.98 s |    1.354 s |    0.896 s |       |  37000.0000 |   1000.0000 |         - |    113 MB |

## Disclaimer
The material embodied in this software is provided to you "as-is" and without warranty of any kind, express, implied or otherwise, including without limitation, any warranty of fitness for a particular purpose. In no event shall the Centers for Disease Control and Prevention (CDC) or the United States (U.S.) government be liable to you or anyone else for any direct, special, incidental, indirect or consequential damages of any kind, or any damages whatsoever, including without limitation, loss of profit, loss of use, savings or revenue, or the claims of third parties, whether or not CDC or the U.S. government has been advised of the possibility of such loss, however caused and on any theory of liability, arising out of or in connection with the possession, use or performance of this software.
