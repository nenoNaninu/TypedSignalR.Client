using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalR.Shared;

namespace SignalR.Server.Hubs
{
    public class ChatHub : Hub<IClientContract>, IHubContract
    {
        public async Task<Status> SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message, new UserDefineClass(){ Now =  DateTime.Now, RandomId = Guid.NewGuid()});
            return new Status() { StatusMessage = $"[Success] Call SendMessageToServer : {DateTime.Now}" };
        }

        public async Task SomeHubMethod()
        {
            await Clients.Caller.SomeClientMethod();
        }
    }
}