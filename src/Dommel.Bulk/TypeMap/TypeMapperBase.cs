using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// Base type implement <see cref="ITypeMapper"/>
/// Contains expression logic
/// </summary>
public abstract class TypeMapperBase<T> : ITypeMapper
{
    private readonly Expression<Func<object, string>> _expression;
    private readonly Lazy<Func<object, string>> _funcLazy;

    protected TypeMapperBase(Expression<Func<T, string>> expression)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(object), "value");

        _expression = (Expression<Func<object, string>>)Expression.Lambda(
            Expression.Condition(
                Expression.Equal(parameter, Expression.Constant(null)),
                Expression.Invoke(expression, parameter),
                Expression.Constant(Constants.NullStr)),
            parameter);

        _funcLazy = new Lazy<Func<object, string>>(() => _expression.Compile());
    }
    /// <inheritdoc/>
    public string Map(object value)
    {
        return _funcLazy.Value.Invoke(value);
    }

    /// <inheritdoc/>
    public Expression GetExpression() => _expression;
}