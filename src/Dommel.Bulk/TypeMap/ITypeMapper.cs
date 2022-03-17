using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// Defines methods for resolving database values from entity value.
/// Custom implementations can be registered with <see cref="DommelBulkMapper.AddTypeMapper(Type,ITypeMapper)"/>.
/// </summary>
public interface ITypeMapper
{
    /// <summary>
    /// Resolves string value for the specified value.
    /// </summary>
    /// <param name="value">The value to resolve string for.</param>
    /// <returns>A string interpretation of <paramref name="value"/>.</returns>
    string Map(object value);

    /// <summary>
    /// return Expression interpret map function for type.
    /// </summary>
    /// <returns>Expression interpret map function.</returns>
    LambdaExpression GetExpression();
}