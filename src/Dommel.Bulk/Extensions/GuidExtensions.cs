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

        unsafe
        {
            fixed (char* destinationReference = &MemoryMarshal.GetReference(destination))
            {
                char* p = destinationReference;
#if BIGENDIAN
                BinaryExtensions.WriteHexString(bytes.Slice(0, 4), p);
                p += 8;
                *p++ = '-';
                BinaryExtensions.WriteHexString(bytes.Slice(4, 2), p);
                p += 4;
                *p++ = '-';
                BinaryExtensions.WriteHexString(bytes.Slice(6, 2), p);
#else
                BinaryExtensions.WriteHexStringReverse(bytes.Slice(0, 4), p);
                p += 8;
                *p++ = '-';
                BinaryExtensions.WriteHexStringReverse(bytes.Slice(4, 2), p);
                p += 4;
                *p++ = '-';
                BinaryExtensions.WriteHexStringReverse(bytes.Slice(6, 2), p);
#endif
                p += 4;
                *p++ = '-';
                BinaryExtensions.WriteHexString(bytes.Slice(8, 2), p);
                p += 4;
                *p++ = '-';
                BinaryExtensions.WriteHexString(bytes.Slice(10, 6), p);
            }
        }

        return true;
    }
}