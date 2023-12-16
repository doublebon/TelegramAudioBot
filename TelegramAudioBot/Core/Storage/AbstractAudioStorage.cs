using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PySharpTelegram.Core.Handlers;
using Telegram.Bot.Types.InlineQueryResults;

namespace TelegramAudioBot.Core.Storage;

public abstract class AbstractAudioStorage
{
    protected string StoreConnection { get; }
    private readonly ConcurrentDictionary<string, StoredAudio> _cachedVoices = new ();
    
    protected AbstractAudioStorage(string storeConnection)
    {
        StoreConnection = storeConnection;
    }
    
    public abstract Task<IEnumerator<StoredAudio>> GetAllAudiosFromStore();
    public abstract Task UpdateAudioCache();
    public abstract Task<IEnumerator<StoredAudio>> RemoveCachedVoices(IEnumerable<StoredAudio> removeAudios);

    protected void CleanCache()
    {
        _cachedVoices.Clear();
    }

    protected void AddVoiceToCache(StoredAudio addAudios)
    {
        _cachedVoices.TryAdd(addAudios.Id, addAudios);
    }

    public abstract Task AddVoiceToStore(StoredAudio addAudios);
    
    public IEnumerable<InlineQueryResultCachedVoice> GetFilteredAudios(string matchText)
    {
        return _cachedVoices
            .Where(audio => audio.Value.IsConsistent(matchText))
            .Select((audio, i) => 
                new InlineQueryResultCachedVoice(
                    id: Convert.ToString(i), 
                    title: audio.Value.Title, 
                    fileId: audio.Value.Id)
            );
    }
}