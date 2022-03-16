namespace Dommel.Bulk.TypeMap;

/// <summary>
/// <see cref="ITypeMapper"/> implementation for null.
/// </summary>
public class NullTypeMapper : TypeMapperBase<object>
{
    public NullTypeMapper()
        : base(x => Constants.NullStr)
    {
    }
}