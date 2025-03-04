using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramCalendarBot;

public static class TelegramCalendar
{
    /// <summary>
    ///     Helper to package callback data for each button.
    ///     Python: create_callback_data("DAY", year, month, day)
    /// </summary>
    private static string CreateCallbackData(
        string action, int year, int month, int day)
    {
        // Example: "CALENDAR;DAY;2025;03;15"
        return $"{Messages.CALENDAR_CALLBACK};{action};{year};{month};{day}";
    }

    /// <summary>
    ///     Creates an inline keyboard containing a month view of the specified year/month.
    ///     If year/month are null, uses the current year/month.
    /// </summary>
    public static InlineKeyboardMarkup CreateCalendar(int? year = null, int? month = null)
    {
        var now = DateTime.Now;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        // "Ignore" callback data that does nothing.
        string dataIgnore = CreateCallbackData(CallbackActions.Ignore, y, m, 0);

        // We’ll build up rows of InlineKeyboardButton[] for each line in the keyboard.
        var keyboardRows = new List<InlineKeyboardButton[]>();

        DateTime currentDateTime = new DateTime(y, m, 1);

        // ────────── First row: Year label ──────────
        // e.g. "2025"

        var prevYear = currentDateTime.AddYears(-1);
        var nextYear = currentDateTime.AddYears(1);
        var prevYearDouble = currentDateTime.AddYears(-3);
        var nextYearDouble = currentDateTime.AddYears(3);
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData($"<<",  CreateCallbackData(CallbackActions.PreviousYearDouble, prevYearDouble.Year, m, 1)),
            InlineKeyboardButton.WithCallbackData($"<",  CreateCallbackData(CallbackActions.PreviousYear, prevYear.Year, m, 1)),
            InlineKeyboardButton.WithCallbackData($"{y}", CreateCallbackData(CallbackActions.SelectYear, currentDateTime.Year, m, 1)),
            InlineKeyboardButton.WithCallbackData($">", CreateCallbackData(CallbackActions.NextYear, nextYear.Year, m, 1)),
            InlineKeyboardButton.WithCallbackData($">>", CreateCallbackData(CallbackActions.NextYearDouble, nextYearDouble.Year, m, 1))
        ]);

        // ────────── Second row: Month label ──────────
        // e.g. "2025"

        var prevMonth = currentDateTime.AddMonths(-1);
        var nextMonth = currentDateTime.AddMonths(1);
        var prevMonthDouble = currentDateTime.AddMonths(-3);
        var nextMonthDouble = currentDateTime.AddMonths(3);
        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData($"<<",  CreateCallbackData(CallbackActions.PreviousMonthDouble, y, prevMonthDouble.Month, 1)),
            InlineKeyboardButton.WithCallbackData($"<",  CreateCallbackData(CallbackActions.PreviousMonth, y, prevMonth.Month, 1)),
            InlineKeyboardButton.WithCallbackData($"{m} {monthName}", CreateCallbackData(CallbackActions.SelectMonth, currentDateTime.Year, m, 1)),
            InlineKeyboardButton.WithCallbackData($">",  CreateCallbackData(CallbackActions.NextMonth, y, nextMonth.Month, 1)),
            InlineKeyboardButton.WithCallbackData($">>",  CreateCallbackData(CallbackActions.NextMonthDouble, y, nextMonthDouble.Month, 1))
        ]);


        // ────────── Third row: Day-of-week headers ──────────
        // "Mo Tu We Th Fr Sa Su"
        string[] dayShortNames = { "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su" };
        keyboardRows.Add(
            dayShortNames
                .Select(dayName => InlineKeyboardButton.WithCallbackData(dayName, dataIgnore))
                .ToArray()
        );

        // ────────── Calendar rows ──────────
        // We’ll use the System.Globalization.Calendar to get each week row
        var daysInMonth = DateTime.DaysInMonth(y, m);
        // 1-based day indexing from 1..daysInMonth

        // We’ll figure out the day-of-week of the 1st
        var firstOfMonth = new DateTime(y, m, 1);
        // In .NET, Sunday=0, Monday=1, etc.  We’ll align so that Monday is first column
        // to match your Python code
        int firstDayOfWeek = ((int)firstOfMonth.DayOfWeek + 6) % 7;
        // this will shift Sunday -> 6, Monday -> 0, etc.

        // We'll build rows of 7 days
        // Track the date number we’re about to place
        int currentDay = 1 - firstDayOfWeek; // might start negative if the first day is mid-week

        while (currentDay <= daysInMonth)
        {
            var rowButtons = new InlineKeyboardButton[7];
            for (int col = 0; col < 7; col++)
            {
                if (currentDay < 1 || currentDay > daysInMonth)
                    // empty cell
                    rowButtons[col] = InlineKeyboardButton.WithCallbackData(" ", dataIgnore);
                else
                    rowButtons[col] = InlineKeyboardButton.WithCallbackData(
                        currentDay.ToString(),
                        CreateCallbackData(CallbackActions.Day, y, m, currentDay));
                currentDay++;
            }

            keyboardRows.Add(rowButtons);
        }

        return new InlineKeyboardMarkup(keyboardRows);
    }

    /// <summary>
    ///     Creates an inline keyboard containing years.
    /// </summary>
    public static InlineKeyboardMarkup CreateCalendarYearSelection(int year, int month, string action)
    {

        // We’ll build up rows of InlineKeyboardButton[] for each line in the keyboard.
        var keyboardRows = new List<InlineKeyboardButton[]>();
        DateTime currentDateTime = new(year, month, 1);
        int maxYearsToShow = 28;

        // If the action is to select a year, we want to center the selected year in the middle of the keyboard
        if (action == CallbackActions.SelectYear)
        {
            int maxYearsToShowCenter = maxYearsToShow / 2;
            currentDateTime = new(currentDateTime.AddYears(-maxYearsToShowCenter).Year, month, 1);
        }

        int yearCounter = 1;
        for (int i = 0; i < 7; i++)
        {
            InlineKeyboardButton[] years = new InlineKeyboardButton[4];
            for (int j = 0; j < 4; j++)
            {
                int calculatedYear = currentDateTime.AddYears(yearCounter++).Year;
                string calculatedYearText = calculatedYear.ToString();
                if (calculatedYear == year)
                {
                    calculatedYearText = $"{calculatedYearText} ✔";
                }

                years[j] = InlineKeyboardButton.WithCallbackData(calculatedYearText,
                    CreateCallbackData(CallbackActions.Year, calculatedYear, month, 1));
            }
            keyboardRows.Add(years);

        }


        DateTime prevYearsStart = new(currentDateTime.AddYears(-maxYearsToShow).Year, month, 1);
        DateTime nextYearsStart = new(currentDateTime.AddYears(maxYearsToShow).Year, month, 1);
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData($"⏪ Previous Years",  CreateCallbackData(CallbackActions.PreviousYears, prevYearsStart.Year, month, 1)),
            InlineKeyboardButton.WithCallbackData($"Next Years ⏩", CreateCallbackData(CallbackActions.NextYears, nextYearsStart.Year, month, 1))
        ]);

        return new InlineKeyboardMarkup(keyboardRows);
    }


    /// <summary>
    ///     Creates an inline keyboard containing months.
    /// </summary>
    public static InlineKeyboardMarkup CreateCalendarMonthSelection(int year, int month)
    {

        // We’ll build up rows of InlineKeyboardButton[] for each line in the keyboard.
        var keyboardRows = new List<InlineKeyboardButton[]>();

        int monthCounter = 0;
        for (int i = 0; i < 4; i++)
        {
            InlineKeyboardButton[] years = new InlineKeyboardButton[3];
            for (int j = 0; j < 3; j++)
            {
                monthCounter++;
                string monthName = $"{monthCounter} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthCounter)}";
                if (monthCounter == month)
                {
                    monthName = $"{monthName} ✔";
                }

                years[j] = InlineKeyboardButton.WithCallbackData(monthName,
                    CreateCallbackData(CallbackActions.Month, year, monthCounter, 1));
            }
            keyboardRows.Add(years);

        }

        return new InlineKeyboardMarkup(keyboardRows);
    }

    /// <summary>
    ///     Processes a calendar callback selection. If a date is chosen, returns (true, chosenDate).
    ///     Otherwise, returns (false, null).
    ///     You would call this in your callback handler after confirming the callback starts with CALENDAR;...
    /// </summary>
    public static async Task<(bool, DateTime?)>
        ProcessCalendarSelection(
            ITelegramBotClient bot,
            CallbackQuery query,
            CancellationToken cancellationToken = default)
    {
        // parse the callback data: e.g. "CALENDAR;DAY;2025;03;15"
        var parts = Utils.SeparateCallbackData(query.Data);
        // parts[0] = "CALENDAR"
        // parts[1] = action => "DAY", "IGNORE", "PREV-MONTH", etc.
        // parts[2] = year
        // parts[3] = month
        // parts[4] = day

        var action = parts[1];
        var year = int.Parse(parts[2]);
        var month = int.Parse(parts[3]);
        var day = int.Parse(parts[4]);

        switch (action)
        {
            case CallbackActions.Ignore:
                // Do nothing, just answer to remove the "loading" state
                await bot.AnswerCallbackQueryAsync(query.Id, cancellationToken: cancellationToken);
                return (false, null);

            case CallbackActions.Day:
                // The user picked a day => remove the inline keyboard, return that date
                await bot.EditMessageReplyMarkupAsync(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    cancellationToken: cancellationToken
                );
                return (true, new DateTime(year, month, day));

            case CallbackActions.Year:
            case CallbackActions.Month:
            case CallbackActions.NextYear:
            case CallbackActions.PreviousYear:
            case CallbackActions.NextYearDouble:
            case CallbackActions.PreviousYearDouble:
            case CallbackActions.NextMonth:
            case CallbackActions.PreviousMonth:
            case CallbackActions.NextMonthDouble:
            case CallbackActions.PreviousMonthDouble:
                await bot.EditMessageReplyMarkupAsync(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    CreateCalendar(year, month),
                    cancellationToken: cancellationToken
                );
                return (false, null);


            case CallbackActions.SelectYear:
            case CallbackActions.PreviousYears:
            case CallbackActions.NextYears:
                // Select years keyboard
                await bot.EditMessageReplyMarkupAsync(
             query.Message.Chat.Id,
             query.Message.MessageId,
             CreateCalendarYearSelection(year, month, action),
             cancellationToken: cancellationToken
                 );
                return (false, null);

            case CallbackActions.SelectMonth:
                // Select months keyboard
                await bot.EditMessageReplyMarkupAsync(
             query.Message.Chat.Id,
             query.Message.MessageId,
             CreateCalendarMonthSelection(year, month),
             cancellationToken: cancellationToken
                 );
                return (false, null);


            default:
                // unknown
                await bot.AnswerCallbackQueryAsync(query.Id, "Something went wrong!",
                    cancellationToken: cancellationToken);
                return (false, null);
        }
    }
}