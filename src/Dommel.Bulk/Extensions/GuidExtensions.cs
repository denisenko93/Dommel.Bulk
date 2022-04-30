using System.Runtime.InteropServices;

namespace Dommel.Bulk.Extensions;

internal static class GuidExtensions
{
    public static bool TryFormat(this Guid guid, Span<char> destination, out int written)
    {
        if (destination.Length < 36)
        {
            written = 0;
            return false;
        }

        written = 36;

        Span<byte> bytes = stackalloc byte[16];

        MemoryMarshal.TryWrite(bytes, ref guid);

#if BIGENDIAN
        BinaryExtensions.WriteHexString(bytes.Slice(0, 4), destination);
        destination = destination.Slice(8);
        destination[0] = '-';
        destination = destination.Slice(1);
        BinaryExtensions.WriteHexString(bytes.Slice(4, 2), destination);
        destination = destination.Slice(4);
        destination[0] = '-';
        destination = destination.Slice(1);
        BinaryExtensions.WriteHexString(bytes.Slice(6, 2), destination);
#else
        BinaryExtensions.WriteHexStringReverse(bytes.Slice(0, 4), destination);

        destination = destination.Slice(8);
        destination[0] = '-';
        destination = destination.Slice(1);
        BinaryExtensions.WriteHexStringReverse(bytes.Slice(4, 2), destination);
        destination = destination.Slice(4);
        destination[0] = '-';
        destination = destination.Slice(1);
        BinaryExtensions.WriteHexStringReverse(bytes.Slice(6, 2), destination);
#endif
        destination = destination.Slice(4);
        destination[0] = '-';
        destination = destination.Slice(1);
        BinaryExtensions.WriteHexString(bytes.Slice(8, 2), destination);
        destination = destination.Slice(4);
        destination[0] = '-';
        destination = destination.Slice(1);
        BinaryExtensions.WriteHexString(bytes.Slice(10, 6), destination);

        return true;
    }
}