namespace Dommel.Bulk.Extensions;

public static class PostgreSqlTextWriterExtensions
{
    private const string postgreQuote = "'";
    private const string postgreHexHeader = "0x";

    public static void WritePostgreSqlDateTime(this TextWriter textWriter, DateTime dateTime, bool quote)
    {
        int resultLength = quote ? 26 + (postgreQuote.Length * 2) : 26;
        Span<char> charArray = stackalloc char[resultLength];

        ReadOnlySpan<char> quotes = quote ? postgreQuote.AsSpan() : default;

        if (TextWriterExtensionsHelper.TryQuote(
                charArray,
                (DateTime source, Span<char> target, out int writtenInternal) => source.TryFormatPostgreSqlDate(target, out writtenInternal),
                dateTime,
                quotes,
                quotes,
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

        ReadOnlySpan<char> quotes = default;

        if (quote)
        {
            maxLength += (postgreQuote.Length * 2);
            quotes = postgreQuote.AsSpan();
        }

        Span<char> target = stackalloc char[maxLength];

        if (TextWriterExtensionsHelper.TryQuote(
                target,
                (string source, Span<char> span, out int i) => source.AsSpan().TryEscapePostgre(span, out i),
                value,
                quotes,
                quotes,
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
            textWriter.Write(postgreQuote);
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

    public static void WritePostgreSqlTimeSpan(this TextWriter textWriter, TimeSpan timeSpan, bool quote)
    {
        Span<char> charArray = stackalloc char[20];

        ReadOnlySpan<char> quotes = quote ? postgreQuote.AsSpan() : default;

        if(TextWriterExtensionsHelper.TryQuote(
               charArray,
               ((TimeSpan source, Span<char> target, out int charsWritten) => source.TryFormatPostgreSql(target, out charsWritten)),
               timeSpan,
               quotes,
               quotes,
               out int written))
        {
            textWriter.WriteSpan(charArray.Slice(0, written));
        }
        else
        {
            if (quote)
            {
                textWriter.Write(postgreQuote);
            }
            textWriter.Write((int) timeSpan.TotalHours);
            textWriter.Write(timeSpan.ToString("\\:mm\\:ss\\.ffffff"));if (quote)
            {
                textWriter.Write(postgreQuote);
            }
        }
    }

    public static void WritePostgreSqlHexString(this TextWriter textWriter, Span<byte> bytes, bool quote)
    {
        Span<char> target = stackalloc char[bytes.Length * 2 + postgreHexHeader.Length];

        if(TextWriterExtensionsHelper.TryQuoteSpan(target,
               (Span<byte> source, Span<char> span, out int written) => source.TryWriteHexString(span, out written),
               bytes,
               quote ? postgreHexHeader.AsSpan() : default,
               default,
               out int charsWritten))
        {
            textWriter.WriteSpan(target.Slice(0, charsWritten));
        }
        else
        {
            throw new FormatException($"Error by escape Postgre chars. Source length: {bytes.Length}. Target length: {target.Length}. Chars written: {charsWritten}");
        }
    }
}