using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PySharpTelegram.Core.Config;
using PySharpTelegram.Core.Extensions;
using PySharpTelegram.Core.Handlers;
using PySharpTelegram.Core.Services.Abstract;
using Telegram.Bot;
using TelegramAudioBot.Connector;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfig>(
            context.Configuration.GetSection("BotConfiguration"));
        
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetConfiguration<BotConfig>();
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
        services.AddSingleton<MessageAttributesHandler>();
        services.AddSingleton<InlineAttributesHandler>();
        services.AddSingleton<AbstractExternalConnector, ExternalConnector>(_ => new ExternalConnector("Chat"));
    })
    .Build();

await host.RunAsync();