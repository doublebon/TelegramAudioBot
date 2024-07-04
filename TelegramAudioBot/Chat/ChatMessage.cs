using PySharpTelegram.Core.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAudioBot.Core.Constants;
using TelegramAudioBot.Core.Storage;
using TelegramAudioBot.Core.Support;
using File = System.IO.File;

namespace TelegramAudioBot.Chat;

public class ChatMessage
{
    [Restrictions.AccessGroups("*")]
    [MessageFilter.Text(Constants.Command.HELP)]
    public static async Task ProcessHelp(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(
            message.Chat,
            string.Join('\n', Constants.Command.GetAllCommands()),
            replyMarkup: new ForceReplyMarkup { Selective = false },
            cancellationToken: cancellationToken);
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.Text(Constants.Command.ADD_VOICE)]
    public static async Task ProcessAddVoice(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(
            message.Chat,
            Constants.Text.ADD_NEW_VOICE,
            replyMarkup: new ForceReplyMarkup { Selective = false },
            cancellationToken: cancellationToken);
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.ReplyOnText(Constants.Text.ADD_NEW_VOICE)]
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
    [MessageFilter.Text(Constants.Command.UPDATE)]
    public static async Task ProcessUpdateStorage(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await StorageContainer.AudioStorage.UpdateAudioCache();
        
        await bot.SendTextMessageAsync(
            message.Chat,
            "Storage was updated",
            cancellationToken: cancellationToken);
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.Text(Constants.Command.CHANGE)]
    public static async Task ProcessChangeStorage(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(
            message.Chat,
            Constants.Text.SEND_NEW_FILE,
            replyMarkup: new ForceReplyMarkup { Selective = false },
            cancellationToken: cancellationToken);
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.ReplyOnText(Constants.Text.SEND_NEW_FILE)]
    public static async Task ProcessReplyOnChangeStorage(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        if (message.Type is not MessageType.Document)
        {
            await bot.SendTextMessageAsync(
                message.Chat,
                "Wrong storage file type. Expect type: Document",
                cancellationToken: cancellationToken);
            return;
        }
        
        var file = await bot.GetFileAsync(message.Document!.FileId, cancellationToken);
        await using (var createStream = new FileStream(Path.GetFileName(file.FilePath!), FileMode.Create))
        {
            await bot.DownloadFileAsync(file.FilePath!, createStream, cancellationToken);
        }
        
        await StorageContainer.AudioStorage.ChangeStorage(Path.GetFileName(file.FilePath!));
        await bot.SendTextMessageAsync(
            message.Chat,
            "Storage was changed!",
            cancellationToken: cancellationToken);
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.Text(Constants.Command.GET_STORAGE)]
    public static async Task ProcessGetStorage(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await using var stream = File.Open(StorageContainer.AudioStorage.GetStorageConnectorName(), FileMode.Open);
        var iof = new InputFileStream(stream, StorageContainer.AudioStorage.GetStorageConnectorName());
        await bot.SendDocumentAsync(message.Chat.Id, iof, cancellationToken: cancellationToken);
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.Text(Constants.Command.CLEAN)]
    public static async Task ProcessCleanStorage(ITelegramBotClient bot, Message message, User user, CancellationToken cancellationToken)
    {
        await StorageContainer.AudioStorage.ClearStorage();
        await bot.SendTextMessageAsync(message.Chat, "Storage was cleared!", cancellationToken: cancellationToken);
    }
    
    [Restrictions.AccessGroups("*")]
    [MessageFilter.ContentType(MessageType.Audio, MessageType.Document, MessageType.Video, MessageType.Voice)]
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