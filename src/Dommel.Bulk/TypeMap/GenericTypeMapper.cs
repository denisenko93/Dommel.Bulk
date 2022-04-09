using System.Linq.Expressions;
using System.Text;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// Generic implementation of <see cref="ITypeMapper"/>
/// Contains expression logic
/// </summary>
public class GenericTypeMapper<T> : ITypeMapper
{
    private readonly Expression<Func<T, StringBuilder, StringBuilder>> _expression;

    public GenericTypeMapper(Expression<Func<T, StringBuilder, StringBuilder>> expression)
    {
        _expression = expression;
    }

    /// <inheritdoc/>
    public LambdaExpression GetExpression() => _expression;
}