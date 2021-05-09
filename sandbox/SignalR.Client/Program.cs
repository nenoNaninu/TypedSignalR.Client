using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Shared;

namespace SignalR.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var url = "https://localhost:5001/Realtime/ChatHub";

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();
            await connection.StartAsync();
            // await connection.InvokeAsync("SendMessageToServer", "user_name~~~", "testtesttest");
            var status = await connection.InvokeAsync<Status>("SendMessageToServer", "user_name~~~", "testtesttest");
            Console.WriteLine($"{status.StatusMessage}");
        }
    }
}
