namespace Dommel.Bulk.Extensions;

internal static class TimeSpanExtensions
{
    public static bool TryFormatMysql(this TimeSpan timeSpan, Span<char> target, out int written)
    {
        int requiredOutputLength = 15; // start with "hh:mm:ss.ffffff" and adjust as necessary

        uint fraction;
        ulong totalSecondsRemaining;

        // Turn this into a non-negative TimeSpan if possible.
        long ticks = timeSpan.Ticks;
        if (ticks < 0)
        {
            requiredOutputLength++; // requiredOutputLength + 1 for the leading '-' sign
            ticks = -ticks;
        }

        if (ticks < 0)
        {
            // We computed these ahead of time; they're straight from the decimal representation of Int64.MinValue.
            fraction = 4775808;
            totalSecondsRemaining = 922337203685;
        }
        else
        {
            ulong fraction64;
            (totalSecondsRemaining, fraction64) = NumberExtensions.DivRem((ulong) ticks, TimeSpan.TicksPerSecond);
            fraction = (uint) fraction64;
        }

        if (target.Length < requiredOutputLength)
        {
            written = 0;
            return false;
        }

        ulong totalMinutesRemaining = 0, seconds = 0;
        if (totalSecondsRemaining > 0)
        {
            // Only compute minutes if the TimeSpan has an absolute value of >= 1 minute.
            (totalMinutesRemaining, seconds) = NumberExtensions.DivRem(totalSecondsRemaining, 60 /* seconds per minute */);
        }

        ulong totalHoursRemaining = 0;
        ulong minutes = 0;
        if (totalMinutesRemaining > 0)
        {
            // Only compute hours if the TimeSpan has an absolute value of >= 1 hour.
            (totalHoursRemaining, minutes) = NumberExtensions.DivRem(totalMinutesRemaining, 60 /* minutes per hour */);
        }

        int hourDigits = Math.Max(totalHoursRemaining.CountDigits(), 2);

        int idx = 0;
        if (timeSpan.Ticks < 0)
        {
            target[idx++] = '-';
        }

        // Write day and separator, if necessary
        totalHoursRemaining.WriteDigits(target.Slice(idx, hourDigits));
        idx += hourDigits;
        target[idx++] = ':';
        ((int)minutes).WriteTwoDecimalDigits(target, idx);
        idx += 2;
        target[idx++] = ':';
        ((int)seconds).WriteTwoDecimalDigits(target, idx);
        idx += 2;

        if (fraction != 0)
        {
            target[idx++] = '.';

            ulong fractionToWrite = fraction / 10;

            fractionToWrite.WriteDigits(target.Slice(idx,6));
            idx += 6;
        }

        written = idx;
        return true;
    }

    public static bool TryFormatPostgreSql(this TimeSpan timeSpan, Span<char> target, out int written)
    {
        int requiredOutputLength = 15; // start with "hh:mm:ss.ffffff" and adjust as necessary

        uint fraction;
        ulong totalSecondsRemaining;

        // Turn this into a non-negative TimeSpan if possible.
        long ticks = timeSpan.Ticks;
        if (ticks < 0)
        {
            requiredOutputLength++; // requiredOutputLength + 1 for the leading '-' sign
            ticks = -ticks;
        }

        if (ticks < 0)
        {
            // We computed these ahead of time; they're straight from the decimal representation of Int64.MinValue.
            fraction = 4775808;
            totalSecondsRemaining = 922337203685;
        }
        else
        {
            ulong fraction64;
            (totalSecondsRemaining, fraction64) = NumberExtensions.DivRem((ulong) ticks, TimeSpan.TicksPerSecond);
            fraction = (uint) fraction64;
        }

        if (target.Length < requiredOutputLength)
        {
            written = 0;
            return false;
        }

        ulong totalMinutesRemaining = 0, seconds = 0;
        if (totalSecondsRemaining > 0)
        {
            // Only compute minutes if the TimeSpan has an absolute value of >= 1 minute.
            (totalMinutesRemaining, seconds) = NumberExtensions.DivRem(totalSecondsRemaining, 60 /* seconds per minute */);
        }

        ulong totalHoursRemaining = 0;
        ulong minutes = 0;
        if (totalMinutesRemaining > 0)
        {
            // Only compute hours if the TimeSpan has an absolute value of >= 1 hour.
            (totalHoursRemaining, minutes) = NumberExtensions.DivRem(totalMinutesRemaining, 60 /* minutes per hour */);
        }

        int hourDigits = Math.Max(totalHoursRemaining.CountDigits(), 2);

        int idx = 0;
        if (timeSpan.Ticks < 0)
        {
            target[idx++] = '-';
        }

        // Write day and separator, if necessary
        totalHoursRemaining.WriteDigits(target.Slice(idx, hourDigits));
        idx += hourDigits;
        target[idx++] = ':';
        ((int)minutes).WriteTwoDecimalDigits(target, idx);
        idx += 2;
        target[idx++] = ':';
        ((int)seconds).WriteTwoDecimalDigits(target, idx);
        idx += 2;

        if (fraction != 0)
        {
            target[idx++] = '.';

            ulong fractionToWrite = fraction / 10;

            fractionToWrite.WriteDigits(target.Slice(idx,6));
            idx += 6;
        }

        written = idx;
        return true;
    }
}