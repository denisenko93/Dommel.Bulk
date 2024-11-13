﻿namespace Dommel.Bulk.Extensions;

internal static class MySqlTextWriterExtensions
{
    private const string mysqlQuote = "'";
    private const string mysqlHexHeader = "0x";

    public static void WriteEscapeMysql(this TextWriter textWriter, string value, bool quote)
    {
        int maxLength = value.Length * 2;

        ReadOnlySpan<char> quotes = default;

        if (quote)
        {
            maxLength += (mysqlQuote.Length * 2);
            quotes = mysqlQuote.AsSpan();
        }

        Span<char> target = maxLength > 1000
            ? new char[maxLength]
            : stackalloc char[maxLength];

        if(TextWriterExtensionsHelper.TryQuote(
            target,
            (string source, Span<char> span, out int i) => source.AsSpan().TryEscapeMysql(span, out i),
            value,
            quotes,
            quotes,
            out int written))
        {
            textWriter.WriteSpan(target.Slice(0, written));
        }
        else
        {
            throw new FormatException($"Error by escape Mysql chars. Source length: {value.Length}. Target length: {target.Length}. Chars written: {written}");
        }
    }

    public static void WriteEscapeMysql(this TextWriter textWriter, char value, bool quote)
    {
        string? escaped = value.EscapeMySql();

        if (quote)
        {
            textWriter.Write(mysqlQuote);
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
            textWriter.Write(mysqlQuote);
        }
    }

    public static void WriteMysqlDateTime(this TextWriter textWriter, DateTime dateTime, bool quote)
    {
        int resultLength = quote ? 26 + (mysqlQuote.Length * 2) : 26;
        Span<char> charArray = stackalloc char[resultLength];

        ReadOnlySpan<char> quotes = quote ? mysqlQuote.AsSpan() : default;

        if(TextWriterExtensionsHelper.TryQuote(
               charArray,
               (DateTime source, Span<char> target, out int writtenInternal) => source.TryFormatMysqlDate(target, out writtenInternal),
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

    public static void WriteGuid(this TextWriter textWriter, Guid guid, bool quote)
    {
        int resultLength = quote ? 40 + (mysqlQuote.Length * 2) : 36;
        Span<char> charArray = stackalloc char[resultLength];

        ReadOnlySpan<char> quotes = quote ? mysqlQuote.AsSpan() : default;

        if(TextWriterExtensionsHelper.TryQuote(
               charArray,
               ((Guid source, Span<char> target, out int charsWritten) => source.TryFormat(target, out charsWritten)),
               guid,
               quotes,
               quotes,
               out int written))
        {
            textWriter.WriteSpan(charArray.Slice(0, written));
        }
        else
        {
            textWriter.Write(guid.ToString());
        }
    }

    public static void WriteSpan(this TextWriter textWriter, Span<char> value)
    {
#if NETCOREAPP2_1_OR_GREATER
        textWriter.Write(value);
#else
        textWriter.Write(value.ToArray());
#endif
    }

    public static void WriteMySqlHexString(this TextWriter textWriter, Span<byte> bytes, bool quote)
    {
        int maxLength = bytes.Length * 2 + mysqlHexHeader.Length;

        Span<char> target = maxLength > 1000 ? new char[maxLength] : stackalloc char[maxLength];

        if(TextWriterExtensionsHelper.TryQuoteSpan(target,
               (Span<byte> source, Span<char> span, out int written) => source.TryWriteHexString(span, out written),
               bytes,
               quote ? mysqlHexHeader.AsSpan() : default,
               default,
               out int charsWritten))
        {
            textWriter.WriteSpan(target.Slice(0, charsWritten));
        }
        else
        {
            throw new FormatException($"Error by escape Mysql chars. Source length: {bytes.Length}. Target length: {target.Length}. Chars written: {charsWritten}");
        }
    }

    public static void WriteMysqlTimeSpan(this TextWriter textWriter, TimeSpan timeSpan, bool quote)
    {
        Span<char> charArray = stackalloc char[20];

        ReadOnlySpan<char> quotes = quote ? mysqlQuote.AsSpan() : default;

        if(TextWriterExtensionsHelper.TryQuote(
               charArray,
               ((TimeSpan source, Span<char> target, out int charsWritten) => source.TryFormatMysql(target, out charsWritten)),
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
                textWriter.Write(mysqlQuote);
            }
            textWriter.Write((int) timeSpan.TotalHours);
            textWriter.Write(timeSpan.ToString("\\:mm\\:ss\\.ffffff"));if (quote)
            {
                textWriter.Write(mysqlQuote);
            }
        }
    }

    #if NET6_0_OR_GREATER
    public static void WriteMysqlDateOnly(this TextWriter textWriter, DateOnly dateOnly, bool quote)
    {
        Span<char> charArray = stackalloc char[20];

        ReadOnlySpan<char> quotes = quote ? mysqlQuote.AsSpan() : default;

        if(TextWriterExtensionsHelper.TryQuote(
               charArray,
               ((DateOnly source, Span<char> target, out int charsWritten) => source.TryFormat(target, out charsWritten)),
               dateOnly,
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
                textWriter.Write(mysqlQuote);
            }
            textWriter.Write(dateOnly.ToString("yyyy-MM-dd"));
            if (quote)
            {
                textWriter.Write(mysqlQuote);
            }
        }
    }

    public static void WriteMysqlTimeOnly(this TextWriter textWriter, TimeOnly timeOnly, bool quote)
    {
        Span<char> charArray = stackalloc char[20];

        ReadOnlySpan<char> quotes = quote ? mysqlQuote.AsSpan() : default;

        if(TextWriterExtensionsHelper.TryQuote(
               charArray,
               ((TimeOnly source, Span<char> target, out int charsWritten) => source.TryFormat(target, out charsWritten)),
               timeOnly,
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
                textWriter.Write(mysqlQuote);
            }
            textWriter.Write(timeOnly.ToString("HH:mm:ss.ffffff"));
            if (quote)
            {
                textWriter.Write(mysqlQuote);
            }
        }
    }
#endif
}