using System.Linq.Expressions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// Defines methods for resolving database values from entity value.
/// Custom implementations can be registered with <see cref="DommelBulkMapper.AddTypeMapper(Type,ITypeMapper)"/>.
/// </summary>
internal interface ITypeMapper
{
    /// <summary>
    /// return Expression interpret map function for type.
    /// </summary>
    /// <returns>Expression interpret map function.</returns>
    LambdaExpression GetExpression();
}