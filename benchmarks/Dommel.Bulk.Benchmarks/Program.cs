using BenchmarkDotNet.Running;
using Dommel.Bulk.Benchmarks;

BenchmarkRunner.Run<SqlBuilderBenchmarks>();
    //BenchmarkRunner.Run<MysqlBenchmarks>();