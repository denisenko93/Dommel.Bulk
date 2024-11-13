namespace Dommel.Bulk.Extensions;

public static class PostgreSqlTextWriterExtensions
{
    private const string postgreQuote = "'";
    private const string postgreStringStartQuote = "E'";

    public static void WritePostgreSqlDateTime(this TextWriter textWriter, DateTime dateTime, bool quote)
    {
        int resultLength = quote ? 26 + (postgreQuote.Length + postgreQuote.Length) : 26;
        Span<char> charArray = stackalloc char[resultLength];

        if (TextWriterExtensionsHelper.TryQuote(
                charArray,
                (DateTime source, Span<char> target, out int writtenInternal) => source.TryFormatPostgreSqlDate(target, out writtenInternal),
                dateTime,
                quote ? postgreQuote.AsSpan() : default,
                quote ? postgreQuote.AsSpan() : default,
                out int written))
        {
            textWriter.WriteSpan(charArray.Slice(0, written));
        }
        else
        {
            string format = quote
                ? "\\'yyyy-MM-dd hh:mm:ss.ffffff\\'"
                : "yyyy-MM-dd hh:mm:ss.ffffff";

            textWriter.Write(dateTime.ToString(format));
        }
    }

    public static void WriteEscapePostgreSql(this TextWriter textWriter, string value, bool quote)
    {
        int maxLength = value.Length * 2;

        ReadOnlySpan<char> startQuote = default;
        ReadOnlySpan<char> endQuote = default;

        if (quote)
        {
            startQuote = postgreStringStartQuote.AsSpan();
            endQuote = postgreQuote.AsSpan();
        }

        maxLength += (startQuote.Length + endQuote.Length);

        Span<char> target = maxLength > 1000
            ? new char[maxLength]
            : stackalloc char[maxLength];

        if (TextWriterExtensionsHelper.TryQuote(
                target,
                (string source, Span<char> span, out int i) => source.AsSpan().TryEscapePostgre(span, out i),
                value,
                startQuote,
                endQuote,
                out int written))
        {
            textWriter.WriteSpan(target.Slice(0, written));
        }
        else
        {
            throw new FormatException($"Error by escape Postgre chars. Source length: {value.Length}. Target length: {target.Length}. Chars written: {written}");
        }
    }

    public static void WriteEscapePostgreSql(this TextWriter textWriter, char value, bool quote)
    {
        string? escaped = value.EscapePostgreSql();

        if (quote)
        {
            textWriter.Write(postgreStringStartQuote);
        }

        if (escaped == null)
        {
            textWriter.Write(value);
        }
        else
        {
            textWriter.Write(escaped);
        }

        if (quote)
        {
            textWriter.Write(postgreQuote);
        }
    }
}