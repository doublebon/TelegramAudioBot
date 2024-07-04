using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PySharpTelegram.Core.Services.AccessGroups;
using Telegram.Bot.Types;
using TelegramAudioBot.Core.Support.CustomConfig;

namespace TelegramAudioBot.Core.Support;

public class AccessGroups(IOptions<MyConfig> configuration) : IChatAccessGroup
{
    private User[] AdminUsers => configuration.Value.Administrators
        .Select(username => new User{ Username = username })
        .ToArray();
    
    public Task<User[]> GetGroupMembersAsync(params string[] accessGroupName)
    {
        return Task.FromResult(AdminUsers);
    }

    public Task<bool> AddMembersToGroupAsync<T>(string accessGroupName, params User[] members)
    {
        return Task.FromResult(false);
    }

    public Task<bool> RemoveMemberFromGroupAsync<T>(string accessGroupName, params User[] members)
    {
        return Task.FromResult(false);
    }
}