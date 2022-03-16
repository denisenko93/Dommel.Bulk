namespace Dommel.Bulk.TypeMap;

/// <summary>
/// <see cref="ITypeMapper"/> implementation for <see cref="bool"/>.
/// </summary>
public class BoolTypeMapper : TypeMapperBase<bool>
{
    public BoolTypeMapper()
        : base(x => x ? "1" : "0")
    {
    }
}