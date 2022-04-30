using System.Runtime.CompilerServices;

namespace Dommel.Bulk.Extensions;

internal static class NumberExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDigits(this uint value)
    {
        int digits = 1;
        if (value >= 100000)
        {
            value /= 100000;
            digits += 5;
        }

        if (value < 10)
        {
            // no-op
        }
        else if (value < 100)
        {
            digits++;
        }
        else if (value < 1000)
        {
            digits += 2;
        }
        else if (value < 10000)
        {
            digits += 3;
        }
        else
        {
            digits += 4;
        }

        return digits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDigits(this ulong value)
    {
        int digits = 1;
        uint part;
        if (value >= 10000000)
        {
            if (value >= 100000000000000)
            {
                part = (uint)(value / 100000000000000);
                digits += 14;
            }
            else
            {
                part = (uint)(value / 10000000);
                digits += 7;
            }
        }
        else
        {
            part = (uint)value;
        }

        if (part < 10)
        {
            // no-op
        }
        else if (part < 100)
        {
            digits++;
        }
        else if (part < 1000)
        {
            digits += 2;
        }
        else if (part < 10000)
        {
            digits += 3;
        }
        else if (part < 100000)
        {
            digits += 4;
        }
        else if (part < 1000000)
        {
            digits += 5;
        }
        else
        {
            digits += 6;
        }

        return digits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteDigits(this ulong source, Span<char> buffer)
    {
        ulong value = source;
        // We can mutate the 'value' parameter since it's a copy-by-value local.
        // It'll be used to represent the value left over after each division by 10.

        for (int i = buffer.Length - 1; i >= 1; i--)
        {
            ulong temp = '0' + value;
            value /= 10;
            buffer[i] = (char)(temp - (value * 10));
        }

        buffer[0] = (char)('0' + value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteTwoDecimalDigits(this int value, Span<char> destination, int offset)
    {
        int temp = '0' + value;
        value /= 10;
        destination[offset + 1] = (char)(temp - (value * 10));
        destination[offset] = (char)('0' + value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong Quotient, ulong Remainder) DivRem(ulong left, ulong right)
    {
        ulong quotient = left / right;
        return (quotient, left - (quotient * right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteFourDecimalDigits(this int value, Span<char> buffer, int startingIndex = 0)
    {
        int temp = '0' + value;
        value /= 10;
        buffer[startingIndex + 3] = (char)(temp - (value * 10));

        temp = '0' + value;
        value /= 10;
        buffer[startingIndex + 2] = (char)(temp - (value * 10));

        temp = '0' + value;
        value /= 10;
        buffer[startingIndex + 1] = (char)(temp - (value * 10));

        buffer[startingIndex] = (char)('0' + value);
    }
}