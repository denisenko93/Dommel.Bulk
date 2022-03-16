using System.Globalization;
using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// <see cref="ITypeMapper"/> implementation for value types.
/// </summary>
/*private class DefaultTypeMapper : ITypeMapper
{
    private static readonly Expression<Func<object, string>> _expression = x => x switch
    {
        null => Constants.NullStr,
        IConvertible convertible => convertible.ToString(CultureInfo.InvariantCulture),
        _ => x.ToString()
    };
    private static readonly Lazy<Func<TimeSpan, string>> _funcLazy = new Lazy<Func<TimeSpan, string>>(() => _expression.Compile());

    /// <inheritdoc/>
    public virtual string Map(object value)
    {
        return value switch
        {
            null => Constants.NullStr,
            IConvertible convertible => convertible.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}*/