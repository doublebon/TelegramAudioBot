namespace TelegramAudioBot.Core.Storage;

public class StoredAudio
{
    public string Id { get; }
    public string Title { get; }
    public string[] Keywords { get; }
    private string KeywordsAsString => string.Join(",", Keywords);
    
    public StoredAudio(string storeString)
    {
        var splitString = storeString.Split(":");
        
        Id = splitString[0];
        Title = splitString[1];
        Keywords = splitString.Length > 2 ? splitString[2].Split(',') : new []{""};
    }
    
    public bool IsConsistent(string matchPattern)
    {
        return Title.StartsWith(matchPattern, StringComparison.OrdinalIgnoreCase) |
               Keywords.Contains(matchPattern);
    }
    
    public static bool IsCorrectStoreString(string storeString)
    {
        return storeString.Length > 2 && storeString.Count(c => c == ':') is 1 or 2;
    }

    public string KeyWordsAsString()
    {
        return KeywordsAsString;
    }
    
    
}