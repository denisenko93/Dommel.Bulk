namespace Dommel.Bulk.Extensions;

internal static class Extensions
{
    public static IEnumerable<T> Yield<T>(this T obj)
    {
        yield return obj;
    }
}