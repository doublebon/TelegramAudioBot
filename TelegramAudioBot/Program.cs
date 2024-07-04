using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PySharpTelegram.Core.Extensions;
using PySharpTelegram.Core.Services.AccessGroups;
using Telegram.Bot;
using TelegramAudioBot.Chat;
using TelegramAudioBot.Core.Storage;
using TelegramAudioBot.Core.Support;
using TelegramAudioBot.Core.Support.CustomConfig;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<MyConfig>( context.Configuration.GetSection("BotConfiguration") );
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetConfiguration<MyConfig>();
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
        services.ConfigureDefaultPySharpServices();
        services.AddSingletonPySharpChatClasses([typeof(ChatInline), typeof(ChatMessage)]);
        services.AddSingleton<IChatAccessGroup, AccessGroups>();
    })
    .Build();

StorageContainer.AudioStorage = new TxtAudioStorage("audioStore.txt");
await StorageContainer.AudioStorage.UpdateAudioCache();
await host.RunAsync();