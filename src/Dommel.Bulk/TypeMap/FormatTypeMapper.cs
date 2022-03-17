using System.Globalization;
using System.Linq.Expressions;
using Dommel.Bulk.Extensions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// <see cref="ITypeMapper"/> implementation for <see cref="IFormattable"/>. Format value use specific format and culture info.
/// </summary>
public class FormatTypeMapper<T> : GenericTypeMapper<T>
    where T : IFormattable
{
    /// <param name="format">value type format</param>
    /// <param name="cultureInfo">format culture</param>
    /// <param name="quote">quote symbol</param>
    /// <param name="escape">escape special characters in result</param>
    public FormatTypeMapper(string? format, CultureInfo? cultureInfo = null, string? quote = null, bool escape = false)
        : base(GetExpression(format, cultureInfo, quote, escape))
    {
    }

    private static Expression<Func<T, string>> GetExpression(string? format, CultureInfo? cultureInfo = null, string? quote = null, bool escape = false)
    {
        cultureInfo ??= CultureInfo.InvariantCulture;

        Func<T, string> formatFunc = string.IsNullOrEmpty(format)
            ? x => x is IConvertible convertible ? convertible.ToString(cultureInfo) : x.ToString()
            : x => x.ToString(format, cultureInfo);

        Func<T, string> escapeFunc = escape
            ? x => formatFunc(x).Escape()
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
}