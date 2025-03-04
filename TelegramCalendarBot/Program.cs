using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramCalendarBot;

public class Program
{
    public static Task Main()
    {
        // 1) Provide your bot token
        string token = "--------------- your token -------------";
        var bot = new TelegramBotClient(token);

        // 2) Start receiving updates
        using var cancellationTokenSource = new CancellationTokenSource();
        bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            cancellationToken: cancellationTokenSource.Token
        );

        Console.WriteLine("Bot started. Press any key to exit.");
        Console.ReadKey();
        cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private static async Task HandleUpdateAsync(
        ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            // User sent a text message
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            // If user typed /calendar
            if (text == "/calendar")
            {
                // Send a prompt
                await bot.SendTextMessageAsync(
                    chatId,
                    Messages.CalendarPrompt,
                    //replyMarkup: TelegramCalendar.CreateCalendar(),
                    replyMarkup: TelegramCalendar.CreateCalendar(),
                    cancellationToken: cancellationToken
                );
            }
            // If user typed /jcalendar
            else if (text == "/jcalendar")
            {
                await bot.SendTextMessageAsync(
                    chatId,
                    Messages.JCalendarPrompt,
                    replyMarkup: TelegramJCalendar.CreateCalendar(),
                    cancellationToken: cancellationToken
                );
            }
            // If user typed /options
            else if (text == "/options")
            {
                string[] opts = { "Option A", "Option B", "Option C" };
                await bot.SendTextMessageAsync(
                    chatId,
                    "Please choose an option:",
                    replyMarkup: TelegramOptions.CreateOptionsKeyboard(opts, "Cancel"),
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                // For anything else, respond with a start message or help
                await bot.SendTextMessageAsync(
                    chatId,
                    string.Format(Messages.StartMessage, update.Message.From.FirstName),
                    cancellationToken: cancellationToken
                );
            }
        }
        else if (update.CallbackQuery != null)
        {
            // A button was pressed on an inline keyboard
            var query = update.CallbackQuery;
            if (query.Data.StartsWith(Messages.CALENDAR_CALLBACK))
            {
                var (isDay, datePicked) =
                    await TelegramCalendar.ProcessCalendarSelection(bot, query, cancellationToken);
                if (isDay && datePicked.HasValue)
                    // They picked a date
                    await bot.SendTextMessageAsync(
                        query.Message.Chat.Id,
                        string.Format(Messages.CalendarResponse, datePicked.Value.ToShortDateString()),
                        cancellationToken: cancellationToken
                    );
            }
            else if (query.Data.StartsWith(Messages.JCALENDAR_CALLBACK))
            {
                var (isDay, dateStr) =
                    await TelegramJCalendar.ProcessCalendarSelection(bot, query, cancellationToken);
                if (isDay && !string.IsNullOrEmpty(dateStr))
                    await bot.SendTextMessageAsync(
                        query.Message.Chat.Id,
                        string.Format(Messages.JCalendarResponse, dateStr),
                        cancellationToken: cancellationToken
                    );
            }
            else if (query.Data.StartsWith("CHOSEN;") || query.Data.StartsWith("CANCEL;"))
            {
                var (chosen, index) = await TelegramOptions.ProcessOptionSelection(bot, query, cancellationToken);
                if (chosen)
                    await bot.SendTextMessageAsync(
                        query.Message.Chat.Id,
                        $"You chose option index = {index}",
                        cancellationToken: cancellationToken
                    );
                else
                    await bot.SendTextMessageAsync(
                        query.Message.Chat.Id,
                        "You canceled.",
                        cancellationToken: cancellationToken
                    );
            }
        }
    }

    private static Task HandleErrorAsync(
        ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception}");
        return Task.CompletedTask;
    }
}