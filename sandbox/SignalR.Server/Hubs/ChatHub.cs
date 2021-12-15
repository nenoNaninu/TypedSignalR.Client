using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalR.Shared;

namespace SignalR.Server.Hubs;

public class ChatHub : Hub<IClientContract>, IHubContract
{
    public async Task<Status> SendMessage(string user, string message)
    {
        var userDefine = new UserDefineClass() { Datetime = DateTime.Now, RandomId = Guid.NewGuid() };
        await Clients.All.ReceiveMessage(user, message, userDefine);
        return new Status() { StatusMessage = $"[Success] Call SendMessage : {userDefine.Datetime}, {userDefine.RandomId}" };
    }

    public async Task SomeHubMethod()
    {
        await Clients.Caller.SomeClientMethod();
    }
}
