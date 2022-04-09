using System.Runtime.InteropServices;
using System.Text;

namespace Dommel.Bulk.Extensions;

internal static class StringBuilderExtensions
{
#if NETSTANDARD
    public static unsafe StringBuilder AppendJoin<T>(this StringBuilder sb, string? separator, IEnumerable<T> values)
    {
        separator ??= string.Empty;
        fixed (char* pSeparator = separator)
        {
            return AppendJoinCore(sb, pSeparator, separator.Length, values);
        }
    }

    private static unsafe StringBuilder AppendJoinCore<T>(StringBuilder sb, char* separator, int separatorLength, IEnumerable<T> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        using (IEnumerator<T> en = values.GetEnumerator())
        {
            if (!en.MoveNext())
            {
                return sb;
            }

            T value = en.Current;
            if (value != null)
            {
                sb.Append(value.ToString());
            }

            while (en.MoveNext())
            {
                sb.Append(separator, separatorLength);
                value = en.Current;
                if (value != null)
                {
                    sb.Append(value.ToString());
                }
            }
        }
        return sb;
    }

#endif
    public static StringBuilder AppendEscapeMysql(this StringBuilder stringBuilder, string value)
    {
        Span<char> target = stackalloc char[value.Length * 2];

        ReadOnlySpan<char> source = value.AsSpan();

        if (source.TryEscapeMysql(target, out int written))
        {
            return stringBuilder.AppendSpan(target.Slice(0, written));
        }

        throw new FormatException($"Error by escape Mysql chars. Source length: {value.Length}. Target length: {target.Length}. Chars written: {written}");
    }

    public static StringBuilder AppendEscapeMysql(this StringBuilder stringBuilder, char value)
    {
        string? escaped = value.EscapeMySql();

        return escaped == null ? stringBuilder.Append(value) : stringBuilder.Append(escaped);
    }

    public static unsafe StringBuilder AppendMysqlDateTime(this StringBuilder stringBuilder, DateTime dateTime)
    {
        const int resultLength = 26;
        Span<char> charArray = stackalloc char[resultLength];

        if (dateTime.TryFormatMysqlDate(charArray, out int written))
        {
            return stringBuilder.AppendSpan(charArray.Slice(0, written));
        }

        return stringBuilder.Append(dateTime.ToString("yyyy-MM-dd hh:mm:ss.ffffff"));
    }

    public static StringBuilder AppendGuid(this StringBuilder stringBuilder, Guid guid)
    {
        const int resultLength = 36;
        Span<char> charArray = stackalloc char[resultLength];

        if (guid.TryFormat(charArray, out int charsWritten))
        {
            return stringBuilder.AppendSpan(charArray.Slice(0, charsWritten));
        }

        return stringBuilder.Append(guid.ToString());
    }

    public static StringBuilder AppendSpan(this StringBuilder stringBuilder, Span<char> value)
    {
        if (value.Length > 0)
        {
            unsafe
            {
                fixed (char* valueChars = &MemoryMarshal.GetReference(value))
                {
                    stringBuilder.Append(valueChars, value.Length);
                }
            }
        }
        return stringBuilder;
    }

    public static StringBuilder AppendHexString(this StringBuilder stringBuilder, Span<byte> bytes)
    {
        Span<char> target = stackalloc char[bytes.Length * 2];

        if (bytes.TryWriteHexString(target, out int charsWritten))
        {
            return stringBuilder.AppendSpan(target.Slice(0, charsWritten));
        }

        throw new FormatException($"Error by escape Mysql chars. Source length: {bytes.Length}. Target length: {target.Length}. Chars written: {charsWritten}");
    }
}