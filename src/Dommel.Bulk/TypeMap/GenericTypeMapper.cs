using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// Generic implementation of <see cref="ITypeMapper"/>
/// Contains expression logic
/// </summary>
public class GenericTypeMapper<T> : ITypeMapper
{
    private readonly Expression<Func<T, string>> _expression;
    private readonly Lazy<Func<T, string>> _funcLazy;

    protected GenericTypeMapper(Expression<Func<T, string>> expression)
    {
        _expression = expression;

        _funcLazy = new Lazy<Func<T, string>>(() => _expression.Compile());
    }
    /// <inheritdoc/>
    public string Map(object value)
    {
        return _funcLazy.Value.Invoke((T)value);
    }

    /// <inheritdoc/>
    public Expression GetExpression() => _expression;
}