#  TypedSignalR.Client

C# Source Generator to create strongly typed SignalR Client.

# Install
```
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package TypedSignalR.Client
```

# Introduction

The C # SignalR Client is untyped.
To call a Hub (server-side) function, you must specify the function defined in Hub as a string.

```cs
connection.InvokeAsync("HubMethod")
```

You also have to manually determine the return type.

```cs
var ret = await connection.InvokeAsync<SomeType>("HubMethod")
```

Registering a client function called by the server also requires a string, and the argument types must be set manually.

```cs
Connection.On<string, DateTime>("ClientMethodName", (str, dateTime) => {});
```

Therefore, if you change the code on the server-side, the modification on the client-side becomes very troublesome. The main cause is that it is not strongly typed.

This TypedSignalR.Client (Source Generator) aims to generate a strongly typed SignalR Client by sharing the server and client function definitions as an interface. 

# Quick Start
First, we define the interface of the client-side and hub-side(server).

```cs
public class UserDefineClass
{
    public Guid RandomId { get; set; }
    public DateTime Datetime { get; set; }
}

// The return type of the client-side method must be Task. 
public interface IClientContract
{
    // Of course, user defined type is OK. 
    Task SomeClientMethod1(string user, string message, UserDefineClass userDefine);
    Task SomeClientMethod2();
}

// The return type of the method on the hub-side must be Task or Task <T>. 
public interface IHubContract
{
    Task<string> SomeHubMethod1(string user, string message);
    Task SomeHubMethod2();
}
```

Next, define the partial class and annotate the HubClientBase Attribute. 
```cs
using TypedSignalR.Client;

[HubClientBase(typeof(IHubContract), typeof(IClientContract))]
partial class ClientBase
{
}
```

By annotating the HubClientBase Attribute, the following code will be generated (simplified here). 

```cs
public partial class ClientBase : IHubClient<IHubContract>, IClientContract, IAsyncDisposable
{
    private class HubInvoker : IHubContract
    {
        private readonly HubConnection _connection;

        public HubInvoker(HubConnection connection)
        {
            _connection = connection;
        }

        public Task<string> SomeHubMethod1(string user,string message)
        {
            return _connection.InvokeAsync<string>(nameof(SomeHubMethod1), user, message);
        }

        public Task SomeHubMethod2()
        {
            return _connection.InvokeAsync(nameof(SomeHubMethod2));
        }
    } // class HubInvoker

    public HubConnection Connection { get; }
    public IHubContract Hub { get; }
    protected List<IDisposable> disposableList = new();

    public ClientBase(HubConnection connection)
    {
        Connection = connection;
        Hub = new HubInvoker(connection);

        Connection.Closed += OnClosed;
        Connection.Reconnected += OnReconnected;
        Connection.Reconnecting += OnReconnecting;

        var d1 = Connection.On<string, string, UserDefineClass>(nameof(SomeClientMethod1), SomeClientMethod1);
        var d2 = Connection.On(nameof(SomeClientMethod2), SomeClientMethod2);

        disposableList.Add(d1);
        disposableList.Add(d2);
    }

    public virtual Task SomeClientMethod1(string user,string message, UserDefineClass userDefine)
        => Task.CompletedTask;

    public virtual Task SomeClientMethod2()
        => Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        Connection.Closed -= OnClosed;
        Connection.Reconnected -= OnReconnected;
        Connection.Reconnecting -= OnReconnecting;

        await Connection.DisposeAsync();

        foreach(var d in disposableList)
        {
            d.Dispose();
        }
    }

    public virtual Task OnClosed(Exception e) => Task.CompletedTask;
    public virtual Task OnReconnected(string connectionId) => Task.CompletedTask;
    public virtual Task OnReconnecting(Exception e) => Task.CompletedTask;
} // class ClientBase

```
Then, extend the class annotated with HubClientBase and implement the client-side function. 

```cs
class HubClient : ClientBase
{
    // HubConnection is required for the base class constructor. 
    public HubClient(HubConnection connection) : base(connection)
    {
    }

    // override and impl
    public override Task SomeClientMethod1(string user, string message, UserDefineClass userDefine)
    {
        Console.WriteLine("Call SomeClientMethod1!");
        return Task.CompletedTask;
    }

    public override Task SomeClientMethod2()
    {
        Console.WriteLine("Call SomeClientMethod1!");
        return Task.CompletedTask;
    }

    // SignalR event
    public override Task OnClosed(Exception e)
    {
        Console.WriteLine($"[On Closed!]");
        return Task.CompletedTask;
    }

    // SignalR event
    public override Task OnReconnected(string connectionId)
    {
        Console.WriteLine($"[On Reconnected!]");
        return Task.CompletedTask;
    }

    // SignalR event
    public override Task OnReconnecting(Exception e)
    {
        Console.WriteLine($"[On Reconnecting!]");
        return Task.CompletedTask;
    }
}
```
Let's use it!

```cs
HubConnection connection = ...;

var client = new HubClient(connection);

await client.Connection.StartAsync();

// Invoke hub methods
var response = await client.Hub.SomeHubMethod1("user", "message");
Console.WriteLine(response);

// client-side function is invoke from hub(server).

await client.Connection.StopAsync();
await client.DisposeAsync();
```

On the server side (ASP.NET Core), it can be strongly typed as follows:

```cs
using Microsoft.AspNetCore.SignalR;

public class SomeHub : Hub<IClientContract>, IHubContract
{
    public async Task<string> SomeHubMethod1(string user, string message)
    {
        await this.Clients.All.SomeClientMethod1(user, message, new UserDefineClass());
        return "OK!";
    }

    public async Task SomeHubMethod2()
    {
        await this.Clients.Caller.SomeClientMethod2();
    }
}
```

# Example
First, launch server. 

```
git clone https://github.com/nenoNaninu/TypedSignalR.Client.git
cd sandbox 
dotnet run --project SignalR.Server/SignalR.Server.csproj
```

Execute the console app in another shell. 

```
cd sandbox 
dotnet run --project SignalR.Client/SignalR.Client.csproj
```
