namespace Dommel.Bulk.Extensions;

public static class SqLiteTextWriterExtensions
{
    private const string sqLiteQuote = "'";

    public static void WriteSqLiteDateTime(this TextWriter textWriter, DateTime dateTime, bool quote)
    {
        int resultLength = quote ? 26 + (sqLiteQuote.Length + sqLiteQuote.Length) : 26;
        Span<char> charArray = stackalloc char[resultLength];

        if (TextWriterExtensionsHelper.TryQuote(
                charArray,
                (DateTime source, Span<char> target, out int writtenInternal) => source.TryFormatSqLiteDate(target, out writtenInternal),
                dateTime,
                quote ? sqLiteQuote.AsSpan() : default,
                quote ? sqLiteQuote.AsSpan() : default,
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

    public static void WriteEscapeSqLite(this TextWriter textWriter, string value, bool quote)
    {
        int maxLength = value.Length * 2;

        ReadOnlySpan<char> quotes = default;

        if (quote)
        {
            maxLength += (sqLiteQuote.Length * 2);
            quotes = sqLiteQuote.AsSpan();
        }

        Span<char> target = stackalloc char[maxLength];

        if(TextWriterExtensionsHelper.TryQuote(
               target,
               (string source, Span<char> span, out int i) => source.AsSpan().TryEscapeSqLite(span, out i),
               value,
               quotes,
               quotes,
               out int written))
        {
            textWriter.WriteSpan(target.Slice(0, written));
        }
        else
        {
            throw new FormatException($"Error by escape SqLite chars. Source length: {value.Length}. Target length: {target.Length}. Chars written: {written}");
        }
    }

    public static void WriteEscapeSqLite(this TextWriter textWriter, char value, bool quote)
    {
        string? escaped = value.EscapeSqLite();

        if (quote)
        {
            textWriter.Write(sqLiteQuote);
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
            textWriter.Write(sqLiteQuote);
        }
    }
}