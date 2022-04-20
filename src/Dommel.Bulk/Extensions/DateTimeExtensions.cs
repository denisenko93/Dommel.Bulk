using System.Runtime.CompilerServices;

namespace Dommel.Bulk.Extensions;

internal static class DateTimeExtensions
{
    // Number of 100ns ticks per time unit
    private const long TicksPerMillisecond = 10000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;
    private const long TicksPerMinute = TicksPerSecond * 60;
    private const long TicksPerHour = TicksPerMinute * 60;
    private const long TicksPerDay = TicksPerHour * 24;

    // Number of days in a non-leap year
    private const int DaysPerYear = 365;
    // Number of days in 4 years
    private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
    // Number of days in 100 years
    private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
    // Number of days in 400 years
    private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

    private static readonly uint[] s_daysToMonth365 = {
        0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
    private static readonly uint[] s_daysToMonth366 = {
        0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

    public static void GetDate(this DateTime dateTime, out int year, out int month, out int day)
    {
        // n = number of days since 1/1/0001
        uint n = (uint)(dateTime.Ticks / TicksPerDay);
        // y400 = number of whole 400-year periods since 1/1/0001
        uint y400 = n / DaysPer400Years;
        // n = day number within 400-year period
        n -= y400 * DaysPer400Years;
        // y100 = number of whole 100-year periods within 400-year period
        uint y100 = n / DaysPer100Years;
        // Last 100-year period has an extra day, so decrement result if 4
        if (y100 == 4) y100 = 3;
        // n = day number within 100-year period
        n -= y100 * DaysPer100Years;
        // y4 = number of whole 4-year periods within 100-year period
        uint y4 = n / DaysPer4Years;
        // n = day number within 4-year period
        n -= y4 * DaysPer4Years;
        // y1 = number of whole years within 4-year period
        uint y1 = n / DaysPerYear;
        // Last year has an extra day, so decrement result if 4
        if (y1 == 4) y1 = 3;
        // compute year
        year = (int)(y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1);
        // n = day number within year
        n -= y1 * DaysPerYear;
        // dayOfYear = n + 1;
        // Leap year calculation looks different from IsLeapYear since y1, y4,
        // and y100 are relative to year 1, not year 0
        uint[] days = y1 == 3 && (y4 != 24 || y100 == 3) ? s_daysToMonth366 : s_daysToMonth365;
        // All months have less than 32 days, so n >> 5 is a good conservative
        // estimate for the month
        uint m = (n >> 5) + 1;
        // m = 1-based month number
        while (n >= days[m]) m++;
        // compute month and day
        month = (int)m;
        day = (int)(n - days[m - 1] + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetTimePrecise(this DateTime date, out int hour, out int minute, out int second, out int tick)
    {
        long ticks = date.Ticks;
        long seconds = ticks / TicksPerSecond;
        tick = (int)(ticks - (seconds * TicksPerSecond));
        long minutes = seconds / 60;
        second = (int)(seconds - (minutes * 60));
        long hours = minutes / 60;
        minute = (int)(minutes - (hours * 60));
        hour = (int)((uint)hours % 24);
    }

    public static bool TryFormatMysqlDate(this DateTime dateTime, Span<char> destination, out int written)
    {
        const int MinimumCharsNeeded = 26;

        if (destination.Length < MinimumCharsNeeded)
        {
            written = 0;
            return false;
        }

        written = MinimumCharsNeeded;

        // Hoist most of the bounds checks on destination.
        { _ = destination[MinimumCharsNeeded - 1]; }

        dateTime.GetDate(out int year, out int month, out int day);
        dateTime.GetTimePrecise(out int hour, out int minute, out int second, out int tick);

        WriteFourDecimalDigits(year, destination);
        destination[4] = '-';
        month.WriteTwoDecimalDigits(destination, 5);
        destination[7] = '-';
        day.WriteTwoDecimalDigits(destination, 8);
        destination[10] = ' ';
        hour.WriteTwoDecimalDigits(destination, 11);
        destination[13] = ':';
        minute.WriteTwoDecimalDigits(destination, 14);
        destination[16] = ':';
        second.WriteTwoDecimalDigits(destination, 17);
        destination[19] = '.';
        ((ulong)(tick/10)).WriteDigits(destination.Slice(20, 6));

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteFourDecimalDigits(int value, Span<char> buffer, int startingIndex = 0)
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