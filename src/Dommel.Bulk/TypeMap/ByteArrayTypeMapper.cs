using Dommel.Bulk.Extensions;

namespace Dommel.Bulk.TypeMap;

/// <summary>
/// <see cref="ITypeMapper"/> implementation for byte array type.
/// </summary>
public class ByteArrayTypeMapper : TypeMapperBase<byte[]>
{
    public ByteArrayTypeMapper(string format)
        : base(x => string.Format(format, x.ToHexString()))
    {
    }
}