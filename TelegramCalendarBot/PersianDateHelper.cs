using System.Globalization;

namespace TelegramCalendarBot;

public static class PersianDateHelper
{
    private static readonly PersianCalendar _persianCalendar = new();

    private static readonly Dictionary<DayOfWeek, string> _persianDayNames = new()
    {
        { DayOfWeek.Saturday, "شنبه" },
        { DayOfWeek.Sunday, "یکشنبه" },
        { DayOfWeek.Monday, "دوشنبه" },
        { DayOfWeek.Tuesday, "سه‌شنبه" },
        { DayOfWeek.Wednesday, "چهارشنبه" },
        { DayOfWeek.Thursday, "پنجشنبه" },
        { DayOfWeek.Friday, "جمعه" }
    };

    public static readonly Dictionary<string, int> PersianDayOffsets = new()
    {
        ["شنبه"] = 0, // Typically we consider Saturday as column 0
        ["یکشنبه"] = 1,
        ["دوشنبه"] = 2,
        ["سه‌شنبه"] = 3,
        ["چهارشنبه"] = 4,
        ["پنجشنبه"] = 5,
        ["جمعه"] = 6
    };

    public static IEnumerable<string> GetPersianDayNames()
    {
        return _persianDayNames.Select(x => x.Value);
    }

    /// <summary>
    ///     Returns the Persian name of the day of the week for the given Persian date.
    /// </summary>
    /// <param name="persianYear">e.g. 1402</param>
    /// <param name="persianMonth">1..12</param>
    /// <param name="persianDay">1..31</param>
    /// <returns>E.g. "شنبه", "یکشنبه", "دوشنبه", etc.</returns>
    public static string GetPersianDayOfWeek(int persianYear, int persianMonth, int persianDay)
    {
        // Convert Solar Hijri date to DateTime
        var dateTime = _persianCalendar.ToDateTime(persianYear, persianMonth, persianDay, 0, 0, 0, 0);

        // Map .NET DayOfWeek to the corresponding Persian day name
        return _persianDayNames[dateTime.DayOfWeek];
    }

    /// <summary>
    ///     Returns the number of days in the specified Persian (Solar Hijri) year-month.
    /// </summary>
    /// <param name="persianYear">The year in Solar Hijri (e.g. 1402).</param>
    /// <param name="persianMonth">A month from 1 to 12.</param>
    /// <returns>The total days in that Persian month.</returns>
    public static int GetDaysInMonth(int persianYear, int persianMonth)
    {
        return _persianCalendar.GetDaysInMonth(persianYear, persianMonth);
    }

    /// <summary>
    ///     Gets the Persian weekday name of the first day of a given Solar Hijri (Persian) month.
    /// </summary>
    /// <param name="persianYear">e.g. 1402</param>
    /// <param name="persianMonth">1..12</param>
    /// <returns>e.g. "شنبه", "یکشنبه", "دوشنبه", etc.</returns>
    public static string GetFirstDayOfMonthPersianWeekday(int persianYear, int persianMonth)
    {
        // Create a DateTime for the 1st day of the specified Persian month
        var dateTime = _persianCalendar.ToDateTime(persianYear, persianMonth, 1, 0, 0, 0, 0);

        // Convert standard DayOfWeek enum to a Persian weekday name
        return _persianDayNames[dateTime.DayOfWeek];
    }


    public static string GetPersianMonthName(int monthNumber)
    {
        // Iranian (Solar) Persian months
        string[] persianMonths =
        {
            "فروردین",
            "اردیبهشت",
            "خرداد",
            "تیر",
            "مرداد",
            "شهریور",
            "مهر",
            "آبان",
            "آذر",
            "دی",
            "بهمن",
            "اسفند"
        };

        return persianMonths[monthNumber - 1];
    }
}