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

        public override Task OnClosed(Exception e)
        {
            Console.WriteLine($"[On Closed!]");
            return Task.CompletedTask;
        }

        public override Task OnReconnected(string connectionId)
        {
            Console.WriteLine($"[On Reconnected!]");
            return Task.CompletedTask;
        }

        public override Task OnReconnecting(Exception e)
        {
            Console.WriteLine($"[On Reconnecting!]");
            return Task.CompletedTask;
        }
    }

    class Receiver : IClientContract, IHubConnectionObserver
    {
        public Task ReceiveMessage(string user, string message, UserDefineClass userDefine)
        {
            Console.WriteLine($"{Environment.NewLine}[Call ReceiveMessage] user: {user}, message: {message}, userDefine.RandomId: {userDefine.RandomId}");

            return Task.CompletedTask;
        }

        public Task SomeClientMethod()
        {
            Console.WriteLine($"{Environment.NewLine}[Call SomeClientMethod]");

            return Task.CompletedTask;
        }

        public Task OnClosed(Exception e)
        {
            Console.WriteLine($"[On Closed!]");
            return Task.CompletedTask;
        }

        public Task OnReconnected(string connectionId)
        {
            Console.WriteLine($"[On Reconnected!]");
            return Task.CompletedTask;
        }

        public Task OnReconnecting(Exception e)
        {
            Console.WriteLine($"[On Reconnecting!]");
            return Task.CompletedTask;
        }
    }

    class Program
    {
        static async Task BaseAttribute(HubConnection connection)
        {
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
            await client.DisposeAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        static async Task Proxy(HubConnection connection)
        {
            var hub = connection.CreateHubProxy<IHubContract>();
            var subsc = connection.Register<IClientContract>(new Receiver());

            await connection.StartAsync();

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
                var status = await hub.SendMessage(user, message);

                Console.WriteLine($"[Return status] {status.StatusMessage}");

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            Console.WriteLine($"[Invoke SomeHubMethod]");
            await hub.SomeHubMethod();

            await connection.StopAsync();
            subsc.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }


        static async Task Main(string[] args)
        {
            var url = "https://localhost:5001/Realtime/ChatHub";

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            if (0 < args.Length && args[0] == "p")
            {
                Console.WriteLine("Proxy Mode");
                await Proxy(connection);
            }
            else
            {
                Console.WriteLine("Attribute Mode");
                await BaseAttribute(connection);
            }
        }
    }
}
