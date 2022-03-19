using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// Generic implementation of <see cref="ITypeMapper"/>
/// Contains expression logic
/// </summary>
public class GenericTypeMapper<T> : ITypeMapper
{
    private readonly Expression<Func<T, string>> _expression;

    public GenericTypeMapper(Expression<Func<T, string>> expression)
    {
        _expression = expression;
    }

    /// <inheritdoc/>
    public LambdaExpression GetExpression() => _expression;
}