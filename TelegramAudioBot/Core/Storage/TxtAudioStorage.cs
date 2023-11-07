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
        CachedVoices.Clear();
        var fileLines = await File.ReadAllLinesAsync(StoreConnection);
        fileLines.Where(x => !string.IsNullOrWhiteSpace(x)).Reverse().ToList().ForEach(line =>
        {
            var splitLines = line.Split(":");
            var audioInfo = new StoredAudio()
            {
                Id = splitLines[1],
                Title = splitLines[0],
                Keywords = splitLines[2].Split(',')
            };
            CachedVoices.Add(audioInfo);
        });
    }

    public override Task<IEnumerator<StoredAudio>> RemoveCachedVoices(IEnumerable<StoredAudio> removeAudios)
    {
        throw new NotImplementedException();
    }
}