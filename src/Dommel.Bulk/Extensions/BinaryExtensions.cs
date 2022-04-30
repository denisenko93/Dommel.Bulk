using System.Runtime.InteropServices;
using System.Text;

namespace Dommel.Bulk.Extensions;

internal static class BinaryExtensions
{
    private static readonly uint[] _Lookup32 = Enumerable.Range(0, 256).Select(i => {
        string s = i.ToString("x2");
        return ((uint)s[0]) + ((uint)s[1] << 16);
    }).ToArray();

    private static readonly unsafe uint* _lookup32UnsafeP = (uint*)GCHandle.Alloc(_Lookup32, GCHandleType.Pinned).AddrOfPinnedObject();

    public static unsafe bool TryWriteHexString(this Span<byte> source, Span<char> target, out int charsWritten)
    {
        if (target.Length < source.Length * 2)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = source.Length * 2;

        var lookupP = _lookup32UnsafeP;
        fixed (byte* bytesP = source)
        fixed (char* resultP = target) {
            uint* resultP2 = (uint*)resultP;
            for (int i = 0; i < source.Length; i++) {
                resultP2[i] = lookupP[bytesP[i]];
            }
        }

        return true;
    }

    public static unsafe void WriteHexString(Span<byte> bytes, Span<char> target)
    {
        var lookupP = _lookup32UnsafeP;
        fixed (byte* bytesP = bytes)
        fixed (char* targetP2 = target)
        {
            uint* targetUintP = (uint*) targetP2;
            for (int i = 0; i < bytes.Length; i++)
            {
                targetUintP[i] = lookupP[bytesP[i]];
            }
        }
    }

    public static unsafe void WriteHexStringReverse(Span<byte> bytes, Span<char> target)
    {
        var lookupP = _lookup32UnsafeP;
        fixed (byte* bytesP = bytes)
        fixed (char* targetP2 = target)
        {
            uint* targetUintP = (uint*) targetP2;
            for (int i = bytes.Length - 1; i >= 0; i--) {
                targetUintP[bytes.Length - 1 - i] = lookupP[bytesP[i]];
            }
        }
    }
}