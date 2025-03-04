using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramCalendarBot;

public static class TelegramJCalendar
{
    // Modify to use your actual Jalali date library
    private static string CreateCallbackData(string action, int year, int month, int day)
    {
        // e.g. "JCALENDAR;DAY;1401;05;13"
        return $"{Messages.JCALENDAR_CALLBACK};{action};{year};{month};{day}";
    }


    /// <summary>
    ///     Creates a Jalali inline calendar.
    ///     if not provided. Here you will do something similar, e.g. get today's Jalali date.
    /// </summary>
    public static InlineKeyboardMarkup CreateCalendar(int? year = null, int? month = null)
    {
        // You’ll need to convert from Gregorian to Jalali (Persian) here:
        (int y, int m, int d) = (year ?? 1402, month ?? 12, 1);

        var keyboardRows = new List<InlineKeyboardButton[]>();

        // ────────── First row: Year label ──────────
        // e.g. "1403"

        var prevYear = y - 1;
        var nextYear = y + 1;
        var prevYearDouble = y - 3;
        var nextYearDouble = y + 3;
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData("<<", CreateCallbackData(CallbackActions.PreviousYearDouble, prevYearDouble, m, 1)),
            InlineKeyboardButton.WithCallbackData("<", CreateCallbackData(CallbackActions.PreviousYear, prevYear, m, 1)),
            InlineKeyboardButton.WithCallbackData($"{y}", CreateCallbackData(CallbackActions.SelectYear, y, m, 1)),
            InlineKeyboardButton.WithCallbackData(">", CreateCallbackData(CallbackActions.NextYear, nextYear, m, 1)),
            InlineKeyboardButton.WithCallbackData(">>", CreateCallbackData(CallbackActions.NextYearDouble, nextYearDouble, m, 1))
        ]);

        // ────────── Second row: Month label ──────────
        // e.g. "مرداد"

        var prevMonth = m - 1 <= 0 ? 1 : m - 1;
        var nextMonth = m + 1 >= 13 ? 12 : m + 1;
        var prevMonthDouble = m - 3;
        var nextMonthDouble = m + 3;
        var monthName = PersianDateHelper.GetPersianMonthName(m);
        keyboardRows.Add([
            InlineKeyboardButton.WithCallbackData("<<", CreateCallbackData(CallbackActions.PreviousMonthDouble, y, prevMonthDouble, 1)),
            InlineKeyboardButton.WithCallbackData("<", CreateCallbackData(CallbackActions.PreviousMonth, y, prevMonth, 1)),
            InlineKeyboardButton.WithCallbackData($"{m} {monthName}", CreateCallbackData(CallbackActions.SelectMonth, y, m, 1)),
            InlineKeyboardButton.WithCallbackData(">", CreateCallbackData(CallbackActions.NextMonth, y, nextMonth, 1)),
            InlineKeyboardButton.WithCallbackData(">>", CreateCallbackData(CallbackActions.NextMonthDouble, y, nextMonthDouble, 1))
        ]);


        string dataIgnore = CreateCallbackData(CallbackActions.Ignore, y, m, 0);

        // Third row: day-of-week headers in Persian
        IEnumerable<string> daysOfWeek = PersianDateHelper.GetPersianDayNames();
        keyboardRows.Add(
            daysOfWeek.Select(d => InlineKeyboardButton.WithCallbackData(d, dataIgnore)).ToArray()
        );

        int daysInMonth = PersianDateHelper.GetDaysInMonth(y, m);

        // Find which weekday is the 1st of y/m, map to an offset.
        //    For example, if it's "دوشنبه", offset might be 2.
        string firstDayName = PersianDateHelper.GetFirstDayOfMonthPersianWeekday(y, m);
        int offset = PersianDateHelper.PersianDayOffsets[firstDayName]; // e.g. 2 if it's Monday ("دوشنبه")

        int currentDay = 1;

        //  Build rows until we’ve placed all days.
        while (currentDay <= daysInMonth)
        {
            // Each row has 7 columns: [Sat, Sun, Mon, Tue, Wed, Thu, Fri]
            var rowButtons = new InlineKeyboardButton[7];

            // For each column in the row...
            for (int col = 0; col < 7; col++)
                // If we're on the very first row, skip columns until offset.
                // Place blanks for columns < offset.
                if ((col < offset && currentDay == 1) || currentDay > daysInMonth)
                {
                    rowButtons[col] = InlineKeyboardButton.WithCallbackData(" ", dataIgnore);
                }
                else
                {
                    rowButtons[col] = InlineKeyboardButton.WithCallbackData(
                        currentDay.ToString(),
                        CreateCallbackData(CallbackActions.Day, y, m, currentDay)
                    );
                    currentDay++;
                }

            // After the first row, offset should be zeroed out
            offset = 0;

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
        int startYear = year;
        int maxYearsToShow = 28;

        // If the action is to select a year, we want to center the selected year in the middle of the keyboard
        if (action == CallbackActions.SelectYear)
        {
            int maxYearsToShowCenter = maxYearsToShow / 2;
            startYear = startYear - maxYearsToShowCenter;
        }

        int yearCounter = 1;
        for (int i = 0; i < 7; i++)
        {
            InlineKeyboardButton[] years = new InlineKeyboardButton[4];
            for (int j = 0; j < 4; j++)
            {
                int calculatedYear = startYear + yearCounter++;
                string calculatedYearText = calculatedYear.ToString();
                if (calculatedYear == year) calculatedYearText = $"{calculatedYearText} ✔";

                years[j] = InlineKeyboardButton.WithCallbackData(calculatedYearText,
                    CreateCallbackData(CallbackActions.Year, calculatedYear, month, 1));
            }

            keyboardRows.Add(years);
        }


        int prevYearsStart = startYear - maxYearsToShow;
        int nextYearsStart = startYear + maxYearsToShow;

        InlineKeyboardButton[] buttons = new InlineKeyboardButton[2];

        string dataIgnore = CreateCallbackData(CallbackActions.Ignore, year, month, 0);

        if (prevYearsStart <= 1)
            buttons[0] = InlineKeyboardButton.WithCallbackData("\u23ea Previous Years", dataIgnore);
        else
            buttons[0] = InlineKeyboardButton.WithCallbackData("\u23ea Previous Years",
                CreateCallbackData(CallbackActions.PreviousYears, prevYearsStart, month, 1));

        if (prevYearsStart >= 9378)
            buttons[1] = InlineKeyboardButton.WithCallbackData("Next Years \u23e9", dataIgnore);
        else
            buttons[1] = InlineKeyboardButton.WithCallbackData("Next Years \u23e9",
                CreateCallbackData(CallbackActions.NextYears, nextYearsStart, month, 1));

        keyboardRows.Add(buttons);

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
            for (int j = 2; j >= 0; j--)
            {
                monthCounter++;
                string monthName =
                    $"{monthCounter} {PersianDateHelper.GetPersianMonthName(monthCounter)}";
                if (monthCounter == month) monthName = $"{monthName} ✔";

                years[j] = InlineKeyboardButton.WithCallbackData(monthName,
                    CreateCallbackData(CallbackActions.Month, year, monthCounter, 1));
            }

            keyboardRows.Add(years);
        }

        return new InlineKeyboardMarkup(keyboardRows);
    }


    /// <summary>
    ///     Processes user selection from the Jalali calendar inline keyboard.
    /// </summary>
    public static async Task<(bool, string)>
        ProcessCalendarSelection(
            ITelegramBotClient bot,
            CallbackQuery query,
            CancellationToken cancellationToken = default)
    {
        var parts = Utils.SeparateCallbackData(query.Data);
        var action = parts[1];
        var y = int.Parse(parts[2]);
        var m = int.Parse(parts[3]);
        var d = int.Parse(parts[4]);

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
                //        // Here you'd typically convert that Jalali y/m/d back to a string with Persian day names,
                //        // e.g., "شنبه ۱۳ مرداد". For demonstration:
                string dateString = $"{y} {PersianDateHelper.GetPersianMonthName(m)} {d}";
                return (true, dateString);

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
                    CreateCalendar(y, m),
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
                    CreateCalendarYearSelection(y, m, action),
                    cancellationToken: cancellationToken
                );
                return (false, null);

            case CallbackActions.SelectMonth:
                // Select months keyboard
                await bot.EditMessageReplyMarkupAsync(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    CreateCalendarMonthSelection(y, m),
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