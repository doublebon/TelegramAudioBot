using System.Collections;
using System.Text.RegularExpressions;
using PySharpTelegram.Core.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAudioBot.Core.Storage;
using TelegramAudioBot.Core.Support;

namespace TelegramAudioBot.Chat;

public class ChatMessage
{
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.ByType(MessageType.Audio, MessageType.Document, MessageType.Video, MessageType.Voice)]
    public static async Task ProcessAudio(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        var infoMsg = await bot.SendTextMessageAsync(message.Chat, "Got file, start upload...", cancellationToken: cancellationToken);
        var fileId = message.Audio?.FileId ?? message.Document?.FileId ?? message.Video?.FileId;
        if (fileId == null)
        {
            await bot.SendTextMessageAsync(message.Chat, "Can't convert this file. Apply only audio or video format", cancellationToken: cancellationToken);
            return;
        }
        
        var file = await bot.GetFileAsync(fileId, cancellationToken);
        var filesDir = Directory.CreateDirectory($"music/{file.FileId}");
        var fileFullPath = $"{filesDir}/{Path.GetFileName(file.FilePath)}";
        try
        {
            await using (var createStream = new FileStream(fileFullPath, FileMode.Create))
            {
                await bot.DownloadFileAsync(file.FilePath!, createStream, cancellationToken);
            }
            
            infoMsg = await bot.EditMessageTextAsync(message.Chat, infoMsg.MessageId, "File uploaded. Start convert to voice...", cancellationToken: cancellationToken);
            var convertedFile = await OpusConverter.ConvertAudioToOpusAsync(fileFullPath);
            //await bot.SendChatActionAsync(message.Chat, ChatAction.UploadVoice, cancellationToken: cancellationToken);
            if (convertedFile == "")
            {
                await bot.SendTextMessageAsync(message.Chat, "Can't convert this file. Apply only audio or video format", cancellationToken: cancellationToken);
                return;
            }
                
            await using (var openStream = new FileStream(convertedFile, FileMode.Open))
            {
                await bot.EditMessageTextAsync(message.Chat, infoMsg.MessageId, "Voice file | FileId: ", cancellationToken: cancellationToken);
                var uploadedVoice = await bot.SendVoiceAsync(message.Chat, new InputFileStream(openStream), cancellationToken: cancellationToken);
                await bot.SendTextMessageAsync(message.Chat, uploadedVoice.Voice?.FileId ?? "", cancellationToken: cancellationToken);
            }
        } finally
        {
                filesDir.Delete(true);
        }
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.ByCommand("/addvoice")]
    public static async Task ProcessAddVoice(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(
            message.Chat,
            "Add new voice record in format fileId:title:keywords",
            replyMarkup: new ForceReplyMarkup { Selective = false },
            cancellationToken: cancellationToken);
    }
    
    [MessageFilter.ByReplyOnTextEquals("Add new voice record in format fileId:title:keywords")]
    public static async Task ProcessReplyOnAddVoice(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        var textLines = message.Text?.Split("\n");
        if (textLines is { Length: > 0 })
        {
            var wasAdded = new List<StoredAudio>();
            foreach (var line in textLines)
            {
                if (!StoredAudio.IsCorrectStoreString(line))
                {
                    continue;
                }
                var storedAudio = new StoredAudio(line);
                await StorageContainer.AudioStorage.AddVoiceToStore(storedAudio);
                wasAdded.Add(storedAudio);
            }

            await StorageContainer.AudioStorage.UpdateAudioCache();
            await bot.SendTextMessageAsync(message.Chat, $"New voices was added: {string.Join(" ", wasAdded.Select(s => s.Title).ToList())}", cancellationToken: cancellationToken);
        }
        else
        {
            await bot.SendTextMessageAsync(message.Chat, $"Wrong text line. Can't convert. Exp format: 'fileId:title:keywords'", cancellationToken: cancellationToken);
        }
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.ByCommand("/update")]
    public static async Task ProcessUpdateStorage(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await StorageContainer.AudioStorage.UpdateAudioCache();
        
        await bot.SendTextMessageAsync(
            message.Chat,
            "Storage was updated",
            cancellationToken: cancellationToken);
    }
    
    [MessageFilter.Any]
    public static async Task ProcessTextAny(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(
            chatId: message.Chat,
            text: $"Got any filter. {user.FirstName}",
            cancellationToken: cancellationToken
        );
    }
}