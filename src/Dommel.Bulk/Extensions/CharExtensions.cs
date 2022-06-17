namespace Dommel.Bulk.Extensions;

internal static class CharExtensions
{
    private const string ZString = "\\Z";
    private const string ZeroString = "\\0";
    private const string QuoteString = "\\'";
    private const string DoubleQuoteString = "\\\"";
    private const string BString = "\\b";
    private const string NString = "\\n";
    private const string RString = "\\r";
    private const string TString = "\\t";
    private const string FString = "\\f";
    private const string SlashString = "\\\\";

    public static string? EscapeMySql(this char c)
    {
        return c switch
        {
            (char) 26 => ZString,
            '\0' => ZeroString,
            '\'' => QuoteString,
            '"' => DoubleQuoteString,
            '\b' => BString,
            '\n' => NString,
            '\r' => RString,
            '\t' => TString,
            '\\' => SlashString,
            _ => null
        };
    }

    public static string? EscapePostgreSql(this char c)
    {
        return c switch
        {
            '\f' => FString,
            '\0' => ZeroString,
            '\'' => QuoteString,
            '\b' => BString,
            '\n' => NString,
            '\r' => RString,
            '\t' => TString,
            '\\' => SlashString,
            _ => null
        };
    }

    public static string? EscapeSqLite(this char c)
    {
        return c switch
        {
            (char) 26 => ZString,
            '\f' => FString,
            '\0' => ZeroString,
            '\'' => "\'\'",
            _ => null
        };
    }
}