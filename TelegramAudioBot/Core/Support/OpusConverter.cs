using NAudio.Wave;
using Concentus.Oggfile;

namespace TelegramAudioBot.Core.Support;

public class OpusConverter
{
    public static async Task ConvertToOpusAsync(string inputFilePath, string outputFilePath)
    {
        await Task.Run(() => ConvertToOpus(inputFilePath, outputFilePath));
    }

    private static void ConvertToOpus(string inputFilePath, string outputFilePath)
    {
        using (var reader = new AudioFileReader(inputFilePath))
        {
            var resampler = new MediaFoundationResampler(reader, new WaveFormat(48000, 1));
            var opusWriter = new OpusOggWriteStream(new Concentus.Structs.OpusEncoder(48000, 1, Concentus.Enums.OpusApplication.OPUS_APPLICATION_AUDIO), new FileStream(outputFilePath, FileMode.Create));
            byte[] buffer = new byte[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels * sizeof(float)];
            int bytesRead;
            while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
            {
                float[] floatBuffer = new float[bytesRead / sizeof(float)];
                Buffer.BlockCopy(buffer, 0, floatBuffer, 0, bytesRead);
                opusWriter.WriteSamples(floatBuffer, 0, bytesRead / sizeof(float));
            }
            opusWriter.Finish();
        }
    }
}