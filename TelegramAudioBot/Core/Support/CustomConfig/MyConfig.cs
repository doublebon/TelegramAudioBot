using PySharpTelegram.Core.Config;

namespace TelegramAudioBot.Core.Support.CustomConfig;

public class MyConfig : BotConfig
{
    public string[] Administrators { get; set; }
}