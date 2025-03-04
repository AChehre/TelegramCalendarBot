namespace TelegramCalendarBot
{
    public static class Utils
    {
        /// <summary>
        /// Splits the callback data (e.g. "CALENDAR;DAY;2025;03;15") into parts.
        /// </summary>
        public static string[] SeparateCallbackData(string data)
        {
            return data.Split(';');
        }

        /// <summary>
        /// Replaces Persian date strings with half-spaces, etc.
        /// Analogous to reformat_persian_date in Python.
        /// </summary>
        public static string ReformatPersianDate(string date)
        {
            return date
                .Replace("یکشنبه", "یک‌شنبه")
                .Replace("سه شنبه", "سه‌شنبه")
                .Replace("پنجشنبه", "پنج‌شنبه");
        }
    }
}
