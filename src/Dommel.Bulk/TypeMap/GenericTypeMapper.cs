using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// Generic implementation of <see cref="ITypeMapper"/>
/// Contains expression logic
/// </summary>
public class GenericTypeMapper<T> : ITypeMapper
{
    private readonly Expression<Action<T, TextWriter>> _expression;

    public GenericTypeMapper(Expression<Action<T, TextWriter>> expression)
    {
        _expression = expression;
    }

    /// <inheritdoc/>
    public LambdaExpression GetExpression() => _expression;
}