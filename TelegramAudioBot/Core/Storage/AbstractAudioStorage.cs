using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PySharpTelegram.Core.Handlers;
using Telegram.Bot.Types.InlineQueryResults;

namespace TelegramAudioBot.Core.Storage;

public abstract class AbstractAudioStorage
{
    protected string StoreConnection { get; }
    protected ConcurrentBag<StoredAudio> CachedVoices = new(); 

    protected AbstractAudioStorage(string storeConnection)
    {
        StoreConnection = storeConnection;
    }
    
    public abstract Task<IEnumerator<StoredAudio>> GetAllAudiosFromStore();
    public abstract Task UpdateAudioCache();
    public abstract Task<IEnumerator<StoredAudio>> RemoveCachedVoices(IEnumerable<StoredAudio> removeAudios);
    
    public InlineQueryResultCachedVoice[] GetFilteredAudios(string matchText)
    {
        return CachedVoices
            .Where(audio => audio.IsConsistent(matchText))
            .Select((audio, i) => new InlineQueryResultCachedVoice(id: Convert.ToString(i), title: audio.Title, fileId: audio.Id))
            .ToArray();
    }
}