using PySharpTelegram.Core.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace TelegramAudioBot.Chat;

public class ChatInline
{
    [InlineAttributes.Any]
    public static async Task ProcessInline(ITelegramBotClient bot, InlineQuery inline, User user, CancellationToken cancellationToken)
    {
        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "1",
                title: "First",
                inputMessageContent: new InputTextMessageContent("Hello")),
            new InlineQueryResultArticle(
                id: "2",
                title: "Second",
                inputMessageContent: new InputTextMessageContent("Hi")),
        };
        
        await bot.AnswerInlineQueryAsync(
            inline.Id,
            results: results,
            isPersonal: false,
            cacheTime: 0, 
            cancellationToken: cancellationToken);
    }
}