using System.Collections.Concurrent;
using Telegram.Bot.Types.InlineQueryResults;

namespace TelegramAudioBot.Core.Storage;

public abstract class AbstractAudioStorage(string storeConnection)
{
    protected string StoreConnection { get; } = storeConnection;
    private readonly ConcurrentDictionary<int, List<StoredAudio>> _cachedVoices = new ();

    public abstract Task UpdateAudioCache();
    protected void CleanCache()
    {
        _cachedVoices.Clear();
    }

    protected void AddVoiceToCache(IEnumerable<StoredAudio> addAudios)
    {
        foreach (var (key, values) in SplitByPages(addAudios))
        {
            _cachedVoices.TryAdd(key, values);
        }
    }

    public abstract Task AddVoiceToStore(StoredAudio addAudios);
    
    public IEnumerable<InlineQueryResultCachedVoice> GetFilteredAudios(string matchText)
    {
        var isNumber = int.TryParse(matchText, out var number);
        IEnumerable<InlineQueryResultCachedVoice> audioPack;
        if (isNumber && _cachedVoices.ContainsKey(number))
        {
            audioPack = _cachedVoices.GetValueOrDefault(number, [])
                .Select((audio, i) => 
                    new InlineQueryResultCachedVoice(
                        id: Convert.ToString(i), 
                        title: audio.Title, 
                        fileId: audio.Id));
        }
        else
        {
            audioPack = _cachedVoices
                .SelectMany(audio => audio.Value)
                .Where(audio => audio.IsConsistent(matchText))
                .Select((audio, i) => 
                    new InlineQueryResultCachedVoice(
                        id: Convert.ToString(i), 
                        title: audio.Title, 
                        fileId: audio.Id));
        }
        
        return audioPack.Take(50);
    }
    

    public abstract Task ChangeStorage(string newStoreConnection);
    
    public abstract Task ClearStorage();

    public string GetStorageConnectorName()
    {
        return StoreConnection;
    }
    
    private static Dictionary<int, List<StoredAudio>> SplitByPages(IEnumerable<StoredAudio> storedAudios)
    {
        return storedAudios
            .Select((audio, index) => new { audio, index })
            .GroupBy(x => x.index / 50)
            .ToDictionary(g => g.Key + 1, g => g.Select(x => x.audio).ToList());
    }
}