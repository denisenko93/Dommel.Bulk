using System.Text;

namespace Dommel.Bulk.Extensions;

internal static class StringExtensions
{
    public static string Escape(this string value)
    {
        StringBuilder sb = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            switch (c)
            {
                case (char)26:
                    sb.Append("\\Z");
                    break;
                case '\0':
                    sb.Append("\\0");
                    break;
                case '\'':
                    sb.Append("\\'");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}