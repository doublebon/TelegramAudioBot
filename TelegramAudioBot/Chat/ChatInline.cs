using PySharpTelegram.Core.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using TelegramAudioBot.Core.Storage;

namespace TelegramAudioBot.Chat;

public class ChatInline
{
    [InlineAttributes.Any]
    public static async Task ProcessInline(ITelegramBotClient bot, InlineQuery inline, User user, CancellationToken cancellationToken)
    {
        await bot.AnswerInlineQueryAsync(
            inline.Id,
            StorageContainer.AudioStorage.GetFilteredAudios(inline.Query),
            isPersonal: false,
            cacheTime: 0,
            cancellationToken: cancellationToken);
    }
}