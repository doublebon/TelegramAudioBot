using System.Diagnostics;

namespace TelegramAudioBot.Core.Support;

public class OpusConverter
{
    public static async Task<string> ConvertAudioToOpusAsync(string inputFilePath)
    {
        var convertedFileName = $"{inputFilePath}.ogg";
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-loglevel panic -i {inputFilePath} -vn -c:a libopus -b:a 128k {convertedFileName}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
            
        using (var process = Process.Start(startInfo))
        {
            await process!.WaitForExitAsync();
            var result = process.ExitCode;
            if (result != 0)
            {
                return string.Empty;
            }
        }
        
        var isFileCreated = await WaitTillFileWasCreatedAsync(convertedFileName, TimeSpan.FromSeconds(30));
        return isFileCreated? convertedFileName : string.Empty;
    }
    
    private static async Task<bool> WaitTillFileWasCreatedAsync(string filePath, TimeSpan timeout)
    {
        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    return true;
                }

                await Task.Delay(100, cts.Token);
            }

            return false;
        }
    }
    
    // public static void MoveAndDeleteFile(File file)
    // {
    //     string musicFolderPath = Path.Combine(file.DirectoryName!, "music");
    //
    //     // Create the music folder if it doesn't exist
    //     if (!Directory.Exists(musicFolderPath))
    //     {
    //         Directory.CreateDirectory(musicFolderPath);
    //     }
    //
    //     // Move the file to the music folder
    //     string destinationFilePath = Path.Combine(musicFolderPath, file.Name);
    //     file.MoveTo(destinationFilePath);
    //
    //     // Delete the file
    //     file.Delete();
    // }
}