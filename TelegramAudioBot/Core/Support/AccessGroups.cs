using PySharpTelegram.Core.Services.AccessGroups;
using Telegram.Bot.Types;
using TelegramAudioBot.Core.Support.CustomConfig;

namespace TelegramAudioBot.Core.Support;

public class AccessGroups : IAccessGroup
{
    private User[] _adminUsers;

    public AccessGroups()
    {
        _adminUsers = ConfigStorage.Config.Administrators
            .Select(username => new User{ Username = username })
            .ToArray();
    }
    
    public Task<User[]> GetGroupMembersAsync(params string[] accessGroupName)
    {
        return Task.FromResult(_adminUsers);
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