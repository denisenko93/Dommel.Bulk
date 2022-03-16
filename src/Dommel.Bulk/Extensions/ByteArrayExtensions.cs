using System.Runtime.InteropServices;
using System.Text;

namespace Dommel.Bulk.Extensions;

internal static class ByteArrayExtensions
{
    private const string hexAlphabet = "0123456789ABCDEF";

    private static readonly uint[] _Lookup32 = Enumerable.Range(0, 256).Select(i => {
        string s = i.ToString("X2");
        return ((uint)s[0]) + ((uint)s[1] << 16);
    }).ToArray();

    private static readonly unsafe uint* _lookup32UnsafeP = (uint*)GCHandle.Alloc(_Lookup32, GCHandleType.Pinned).AddrOfPinnedObject();

    public static unsafe string ToHexString(this byte[] bytes) {
        var lookupP = _lookup32UnsafeP;
        var result = new string((char)0, bytes.Length * 2);
        fixed (byte* bytesP = bytes)
        fixed (char* resultP = result) {
            uint* resultP2 = (uint*)resultP;
            for (int i = 0; i < bytes.Length; i++) {
                resultP2[i] = lookupP[bytesP[i]];
            }
        }

        return result;
    }

    public static unsafe string ToHexString(this Span<byte> bytes) {
        var lookupP = _lookup32UnsafeP;
        var result = new string((char)0, bytes.Length * 2);
        fixed (byte* bytesP = bytes)
        fixed (char* resultP = result) {
            uint* resultP2 = (uint*)resultP;
            for (int i = 0; i < bytes.Length; i++) {
                resultP2[i] = lookupP[bytesP[i]];
            }
        }

        return result;
    }

    public static unsafe string ToHexString(this ReadOnlySpan<byte> bytes) {
        var lookupP = _lookup32UnsafeP;
        var result = new string((char)0, bytes.Length * 2);
        fixed (byte* bytesP = bytes)
        fixed (char* resultP = result) {
            uint* resultP2 = (uint*)resultP;
            for (int i = 0; i < bytes.Length; i++) {
                resultP2[i] = lookupP[bytesP[i]];
            }
        }

        return result;
    }

    public static string ToHexString(this IReadOnlyCollection<byte> bytes) {
        var result = new char[bytes.Count * 2];
        int i = 0;
        foreach (byte b in bytes)
        {
            var val = _Lookup32[b];
            result[2*i] = (char)val;
            result[2*i + 1] = (char) (val >> 16);
            i++;
        }
        return new string(result);
    }

    public static string ToHexString(this IEnumerable<byte> bytes) {
        StringBuilder result = new StringBuilder();
        foreach (byte b in bytes) {
            result.Append(hexAlphabet[(int)(b >> 4)]);
            result.Append(hexAlphabet[(int)(b & 0xF)]);
        }
        return result.ToString();
    }
}