using Dommel.Bulk.Extensions;

namespace Dommel.Bulk.TypeMap;

public class ByteArraySegmentTypeMapper : TypeMapperBase<ArraySegment<byte>>
{
    public ByteArraySegmentTypeMapper(string format)
        : base(x => string.Format(format, x.ToHexString()))
    {
    }
}