using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// <see cref="ITypeMapper"/> implementation for <see cref="IFormattable"/>. Format value use specific format and culture info.
/// </summary>
public class FormatTypeMapper : TypeMapperBase<IFormattable>
{
    /// <param name="format">value type format</param>
    /// <param name="cultureInfo">format culture</param>
    /// <param name="quote">quote symbol</param>
    /// <param name="escape">escape special characters in result</param>
    public FormatTypeMapper(string? format, CultureInfo? cultureInfo = null, string? quote = null, bool escape = false)
        : base(GetExpression(format, cultureInfo, quote))
    {
    }

    private static Expression<Func<IFormattable, string>> GetExpression(string? format, CultureInfo? cultureInfo = null, string? quote = null, bool escape = false)
    {
        cultureInfo ??= CultureInfo.InvariantCulture;

        Func<IFormattable, string> formatFunc = string.IsNullOrEmpty(format)
            ? x => x is IConvertible convertible ? convertible.ToString(cultureInfo) : x.ToString()
            : x => x.ToString(format, cultureInfo);

        Func<IFormattable, string> escapeFunc = escape
            ? x => Escape(formatFunc(x))
            : formatFunc;

        if (string.IsNullOrEmpty(quote))
        {
            return x => escapeFunc(x);
        }
        else
        {
            string expressionFormat = $"{quote}{{0}}{quote}";

            return x => string.Format(expressionFormat, escapeFunc(x));
        }
    }

    private static string Escape(string value)
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