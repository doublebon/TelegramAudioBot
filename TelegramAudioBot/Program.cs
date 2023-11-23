using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PySharpTelegram.Core.Config;
using PySharpTelegram.Core.Extensions;
using PySharpTelegram.Core.Handlers;
using PySharpTelegram.Core.Services.Abstract;
using PySharpTelegram.Core.Services.AccessGroups;
using Telegram.Bot;
using TelegramAudioBot.Connector;
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
        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
        services.AddSingleton<MessageAttributesHandler>();
        services.AddSingleton<InlineAttributesHandler>();
        services.AddSingleton<IAccessGroup, AccessGroups>();
        services.AddSingleton<AbstractExternalConnector, ExternalConnector>(_ => new ExternalConnector("Chat"));
    })
    .Build();

ConfigStorage.Config = host.Services.GetConfiguration<MyConfig>();
StorageContainer.AudioStorage = new TxtAudioStorage("audioStore.txt");
await StorageContainer.AudioStorage.UpdateAudioCache();
await host.RunAsync();