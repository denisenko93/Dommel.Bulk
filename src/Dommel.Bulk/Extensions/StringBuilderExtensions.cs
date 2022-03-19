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
}