namespace Dommel.Bulk.Extensions;

public static class TextWriterExtensionsHelper
{
    public static bool TryQuote<T>(Span<char> span, TryQuoteFunc<T> writeFunc, T source, ReadOnlySpan<char> startQuote, ReadOnlySpan<char> endQuote, out int written)
    {
        if (startQuote.Length > 0)
        {
            if (span.Length < startQuote.Length)
            {
                written = 0;
                return false;
            }

            startQuote.CopyTo(span);
            span = span.Slice(startQuote.Length);
        }

        if (!writeFunc(source, span, out int writtenInternal))
        {
            written = 0;
            return false;
        }

        if (endQuote.Length > 0)
        {
            if (span.Length < writtenInternal + endQuote.Length)
            {
                written = 0;
                return false;
            }

            endQuote.CopyTo(span.Slice(writtenInternal));
        }

        written = writtenInternal + startQuote.Length + endQuote.Length;
        return true;
    }

    public static bool TryQuoteSpan<T>(Span<char> span, TryQuoteSpanFunc<T> writeFunc, Span<T> source, ReadOnlySpan<char> startQuote, ReadOnlySpan<char> endQuote, out int written)
    {
        if (startQuote.Length > 0)
        {
            if (span.Length < startQuote.Length)
            {
                written = 0;
                return false;
            }

            startQuote.CopyTo(span);
            span = span.Slice(startQuote.Length);
        }

        if (!writeFunc(source, span, out int writtenInternal))
        {
            written = 0;
            return false;
        }

        if (endQuote.Length > 0)
        {
            if (span.Length < writtenInternal + endQuote.Length)
            {
                written = 0;
                return false;
            }

            endQuote.CopyTo(span.Slice(writtenInternal));
        }

        written = writtenInternal + startQuote.Length + endQuote.Length;
        return true;
    }

    public delegate bool TryQuoteSpanFunc<T>(Span<T> source, Span<char> target, out int written);

    public delegate bool TryQuoteFunc<in T>(T source, Span<char> target, out int written);
}