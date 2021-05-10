using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Shared;
using TypedSignalR.Client;

namespace SignalR.Client
{
    [HubClientBase(typeof(IHubContract), typeof(IClientContract))]
    partial class ClientBase
    {
    }

    class HubClient : ClientBase
    {
        public HubClient(HubConnection connection) : base(connection)
        {
        }

        public override Task ReceiveMessage(string user, string message, UserDefineClass userDefine)
        {
            Console.WriteLine($"{Environment.NewLine}[Call ReceiveMessage] user: {user}, message: {message}, userDefine.RandomId: {userDefine.RandomId}");

            return Task.CompletedTask;
        }

        public override Task SomeClientMethod()
        {
            Console.WriteLine($"{Environment.NewLine}[Call SomeClientMethod]");

            return Task.CompletedTask;
        }

        public override void SomeVoidMethod()
        {
            Console.WriteLine($"{Environment.NewLine}[Call SomeVoidMethod]");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var url = "https://localhost:5001/Realtime/ChatHub";

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            var client = new HubClient(connection);

            await client.Connection.StartAsync();

            while (true)
            {
                Console.Write("UserName: ");
                var user = Console.ReadLine();

                Console.Write("Message: ");
                var message = Console.ReadLine();

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(message))
                {
                    break;
                }

                Console.WriteLine($"[Invoke SendMessage]");
                var status = await client.Hub.SendMessage(user, message);

                Console.WriteLine($"[Return status] {status.StatusMessage}");

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            Console.WriteLine($"[Invoke SomeHubMethod]");
            await client.Hub.SomeHubMethod();

            await client.Connection.StopAsync();
            await client.Connection.DisposeAsync();
        }
    }
}
