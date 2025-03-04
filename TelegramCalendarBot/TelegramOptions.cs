using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramCalendarBot
{
    public static class TelegramOptions
    {
        /// <summary>
        /// Creates an inline keyboard with one button per option, plus an optional "Cancel" button.
        /// The callback data is "CHOSEN;i" or "CANCEL;0" for the i-th option or a cancel click.
        /// </summary>
        public static InlineKeyboardMarkup CreateOptionsKeyboard(
            string[] options, 
            string cancelMsg)
        {
            var rows = new System.Collections.Generic.List<InlineKeyboardButton[]>();

            for (int i = 0; i < options.Length; i++)
            {
                // "CHOSEN;i"
                string callbackData = $"CHOSEN;{i}";
                rows.Add(new[] {
                    InlineKeyboardButton.WithCallbackData(options[i], callbackData)
                });
            }

            if (!string.IsNullOrEmpty(cancelMsg))
            {
                rows.Add(new[] {
                    InlineKeyboardButton.WithCallbackData(cancelMsg, "CANCEL;0")
                });
            }

            return new InlineKeyboardMarkup(rows);
        }

        /// <summary>
        /// Processes an option selection: returns (true, index) if "CHOSEN", or (false, 0) if canceled.
        /// </summary>
        public static async System.Threading.Tasks.Task<(bool, int)> 
            ProcessOptionSelection(
                ITelegramBotClient bot,
                CallbackQuery query,
                System.Threading.CancellationToken cancellationToken = default)
        {
            var data = query.Data; // e.g. "CHOSEN;2"
            var parts = data.Split(';');
            var action = parts[0];
            var index = int.Parse(parts[1]);

            if (action == "CHOSEN")
            {
                // remove the inline keyboard, but keep original text
                await bot.EditMessageReplyMarkupAsync(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    replyMarkup: null,
                    cancellationToken: cancellationToken
                );
                return (true, index);
            }
            else if (action == "CANCEL")
            {
                // also remove the inline keyboard
                await bot.EditMessageReplyMarkupAsync(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    replyMarkup: null,
                   cancellationToken: cancellationToken
                );
                return (false, 0);
            }
            else
            {
                // unknown action
                await bot.AnswerCallbackQueryAsync(query.Id, "Something went wrong!", cancellationToken: cancellationToken);
                return (false, 0);
            }
        }
    }
}
