using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Shared;
using TypedSignalR.Client;

namespace SignalR.Client;

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

class Client : IClientContract, IHubConnectionObserver, IDisposable
{
    private readonly IHubContract _hub;
    private readonly IDisposable _subscription;

    public Client(HubConnection connection)
    {
        _hub = connection.CreateHubProxy<IHubContract>();
        _subscription = connection.Register<IClientContract>(this);
    }

    Task IClientContract.ReceiveMessage(string user, string message, UserDefineClass userDefine)
    {
        Console.WriteLine($"{Environment.NewLine}[Call ReceiveMessage] user: {user}, message: {message}, userDefine.RandomId: {userDefine.RandomId}");
        return Task.CompletedTask;
    }

    Task IClientContract.SomeClientMethod()
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

    public Task<Status> SendMessage(string user, string message)
    {
        return _hub.SendMessage(user, message);
    }

    public Task SomeHubMethod()
    {
        return _hub.SomeHubMethod();
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}

class Program
{
    static async Task Sample1(HubConnection connection)
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

    static async Task Sample2(HubConnection connection)
    {
        var client = new Client(connection);

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
            var status = await client.SendMessage(user, message);

            Console.WriteLine($"[Return status] {status.StatusMessage}");

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        Console.WriteLine($"[Invoke SomeHubMethod]");
        await client.SomeHubMethod();

        await connection.StopAsync();
        client.Dispose();

        await Task.Delay(TimeSpan.FromSeconds(1));
    }


    static async Task Main(string[] args)
    {
        var url = "https://localhost:5001/Realtime/ChatHub";

        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();


        //await Sample1(connection);
        await Sample2(connection);
    }
}
