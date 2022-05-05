namespace Dommel.Bulk.Extensions;

internal static class StringExtensions
{

    public static bool TryEscapeMysql(this ReadOnlySpan<char> source, Span<char> target, out int written)
    {
        written = 0;

        foreach (char c in source)
        {
            string? targetChars = c.EscapeMySql();

            if (written + (targetChars?.Length ?? 1) > target.Length)
            {
                return false;
            }

            if (targetChars == null)
            {
                target[written++] = c;
            }
            else
            {
                foreach (char targetChar in targetChars)
                {
                    target[written++] = targetChar;
                }
            }
        }

        return true;
    }

    public static bool TryEscapePostgre(this ReadOnlySpan<char> source, Span<char> target, out int written)
    {
        written = 0;

        foreach (char c in source)
        {
            string? targetChars = c.EscapePostgreSql();

            if (written + (targetChars?.Length ?? 1) > target.Length)
            {
                return false;
            }

            if (targetChars == null)
            {
                target[written++] = c;
            }
            else
            {
                foreach (char targetChar in targetChars)
                {
                    target[written++] = targetChar;
                }
            }
        }

        return true;
    }
}