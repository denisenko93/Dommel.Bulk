using BenchmarkDotNet.Running;
using Dommel.Bulk.Benchmarks;

//BenchmarkRunner.Run<FormatterBenchamarks>();
//BenchmarkRunner.Run<SqlBuilderBenchmarks>();

SqlBuilderBenchmarks benchmarks = new SqlBuilderBenchmarks();
benchmarks.DataSize = 10000;
benchmarks.Setup();
Console.WriteLine("Setaped");
Console.ReadKey();
benchmarks.SqlBuilderBenchmark();
Console.WriteLine("Done");
Console.ReadKey();