using BenchmarkDotNet.Attributes;
using Bogus;
using Dommel.Bulk.Extensions;

namespace Dommel.Bulk.Benchmarks;

[MemoryDiagnoser]
public class FormatterBenchamarks
{
    private Guid _guid;
    private DateTime _dateTime;
    private string _str;

    [GlobalSetup]
    public void Setup()
    {
        _guid = Guid.NewGuid();
        _dateTime = DateTime.Now;
        _str = new Faker().Random.String2(200, "abcdefghijklmnopqrstuvwxyz\r\n\t\b\0'\"\\" + (char) 26);
    }

    [Benchmark]
    public void StandardGuidFormatter()
    {
        Span<char> result = stackalloc char[36];

        _guid.TryFormat(result, out int charsWritten);
    }

    [Benchmark]
    public void SpanGuidFormatter()
    {
        Span<char> result = stackalloc char[36];

        GuidExtensions.TryFormat(_guid, result, out int written);
    }

    [Benchmark]
    public void StandardDateTimeFormatter()
    {
        Span<char> result = stackalloc char[26];

        _dateTime.TryFormat(result, out int written, "yyyy-MM-dd hh:mm:ss.ffffff");
    }

    [Benchmark]
    public void SpanDateTimeFormatter()
    {
        Span<char> result = stackalloc char[26];

        DateTimeExtensions.TryFormatMysqlDate(_dateTime, result, out int written);
    }

    [Benchmark]
    public void MysqlEscape()
    {
        Span<char> target = stackalloc char[_str.Length * 2];

        if (!_str.AsSpan().TryEscapeMysql(target, out int written) || written < _str.Length)
        {
            throw new Exception();
        }
    }
}