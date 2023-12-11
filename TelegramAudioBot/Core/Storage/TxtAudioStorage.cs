using System.Text;

namespace TelegramAudioBot.Core.Storage;

public class TxtAudioStorage : AbstractAudioStorage
{
    public TxtAudioStorage(string audioStoreFileName) : 
        base(audioStoreFileName)
    {
        File.AppendText(audioStoreFileName).Close();
    }

    public override Task<IEnumerator<StoredAudio>> GetAllAudiosFromStore()
    {
        throw new NotImplementedException();
    }

    public override async Task UpdateAudioCache()
    {
        CleanCache();
        var fileLines = await File.ReadAllLinesAsync(StoreConnection);
        var nonEmptyLines = fileLines.Where(x => !string.IsNullOrWhiteSpace(x)).Reverse();

        await Parallel.ForEachAsync(nonEmptyLines, (line, _) =>
        {
            var splitLines = line.Split(":");
            var audioInfo = new StoredAudio()
            {
                Id = splitLines[0],
                Title = splitLines[1],
                Keywords = splitLines[2].Split(',')
            };
            AddVoiceToCache(audioInfo);
            return ValueTask.CompletedTask;
        });
    }

    public override Task<IEnumerator<StoredAudio>> RemoveCachedVoices(IEnumerable<StoredAudio> removeAudios)
    {
        throw new NotImplementedException();
    }

    public override async Task AddVoiceToStore(StoredAudio addAudios)
    {
        if (!string.IsNullOrEmpty(addAudios.Title) && !string.IsNullOrEmpty(addAudios.Id))
        {
            var appending = $"{addAudios.Id}:{addAudios.Title}:{addAudios.KeyWordsAsString()}";
            await File.AppendAllTextAsync(path: StoreConnection, contents: appending, encoding: Encoding.UTF8);
        }
    }
}