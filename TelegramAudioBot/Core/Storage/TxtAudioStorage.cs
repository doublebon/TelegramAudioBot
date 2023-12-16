using System.Text;

namespace TelegramAudioBot.Core.Storage;

public class TxtAudioStorage : AbstractAudioStorage
{
    public TxtAudioStorage(string audioStoreFileName) : 
        base(audioStoreFileName)
    {
        File.AppendText(audioStoreFileName).Close();
    }
    
    public override async Task UpdateAudioCache()
    {
        CleanCache();
        var fileLines = await File.ReadAllLinesAsync(StoreConnection);
        var nonEmptyLines = fileLines.Where(x => !string.IsNullOrWhiteSpace(x)).Reverse();
        await Parallel.ForEachAsync(nonEmptyLines, (line, _) =>
        {
            if (StoredAudio.IsCorrectStoreString(line))
            {
                AddVoiceToCache(new StoredAudio(line));
            }
            
            return ValueTask.CompletedTask;
        });
    }

    public override async Task AddVoiceToStore(StoredAudio addAudios)
    {
        if (!string.IsNullOrEmpty(addAudios.Title) && !string.IsNullOrEmpty(addAudios.Id))
        {
            var appending = $"{addAudios.Id}:{addAudios.Title}:{addAudios.KeyWordsAsString()}";
            await File.AppendAllTextAsync(path: StoreConnection, contents: Environment.NewLine+appending, encoding: Encoding.UTF8);
        }
    }

    public override async void ChangeStorage(string newStoreConnection)
    {
        var storageConnectorName = GetStorageConnectorName();
        File.Delete(StoreConnection);
        File.Move(newStoreConnection, storageConnectorName);
        await UpdateAudioCache();
    }
}