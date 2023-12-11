namespace TelegramAudioBot.Core.Storage;

public class StoredAudio
{
    public string Id { init; get; }
    public string Title {init; get; }
    public string[] Keywords { init; get; } = { "" };
    private string KeywordsAsString => string.Join(",", Keywords);
    
    public bool IsConsistent(string matchPattern)
    {
        return Title.Contains(matchPattern, StringComparison.OrdinalIgnoreCase) |
               Keywords.Contains(matchPattern);
    }

    public string KeyWordsAsString()
    {
        return KeywordsAsString;
    }
    
    
}