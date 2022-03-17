using Dommel.Bulk.Extensions;

namespace Dommel.Bulk.TypeMap;

public class ByteArraySegmentTypeMapper : GenericTypeMapper<ArraySegment<byte>>
{
    public ByteArraySegmentTypeMapper(string format)
        : base(x => string.Format(format, x.ToHexString()))
    {
    }
}