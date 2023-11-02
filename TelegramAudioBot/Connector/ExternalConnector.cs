using PySharpTelegram.Core.Services.Abstract;

namespace TelegramAudioBot.Connector;

public class ExternalConnector : AbstractExternalConnector
{
    public ExternalConnector(string namespaceFromRoot) : base(typeof(ExternalConnector), namespaceFromRoot){}
}