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
Connection.On<string, DateTime>("ClientMethod", (str, dateTime) => {});
```

Therefore, if you change the code on the server-side, the modification on the client-side becomes very troublesome. The main cause is that it is not strongly typed.

This TypedSignalR.Client (Source Generator) aims to generate a strongly typed SignalR Client by sharing the server and client function definitions as an interface. 

# API
There are two types of APIs.
1. A pattern to generate a `HubProxy` from an interface and register a `Receiver` (callback).
2. A pattern that can be written in a similar way to inheriting Hub <T> on the server-side. 

## Pattern of HubProxy and Receiver
Three extension methods for HubConnection are defined as API. 
```cs
THub CreateHubProxy<THub>(this HubConnection source);

IDisposable Register<TReceiver>(this HubConnection source, TReceiver receiver);

(THub HubProxy, IDisposable Subscription) CreateHubProxyWith<THub, TReceiver>(this HubConnection source, TReceiver receiver);
```

## Pattern similar to Hub\<T\> on the server-side
Only one Attribute is provided.
```
public class HubClientBaseAttribute : Attribute
{
    public HubClientBaseAttribute(Type hub, Type client){ }
}
```

# Usage
Suppose you have the following interface defined:
```cs
public class UserDefine
{
    public Guid RandomId { get; set; }
    public DateTime Datetime { get; set; }
}

// The return type of the client-side method must be Task. 
public interface IClientContract
{
    // Of course, user defined type is OK. 
    Task SomeClientMethod1(string user, string message, UserDefine userDefine);
    Task SomeClientMethod2();
}

// The return type of the method on the hub-side must be Task or Task <T>. 
public interface IHubContract
{
    Task<string> SomeHubMethod1(string user, string message);
    Task SomeHubMethod2();
}

class Receiver : IClientContract
{
    // impl
}
```
I recommend that these interfaces be shared between the client and server sides, for example, by project references.

```
server.csproj => shared.csproj <= client.csproj
```

By the way, using these definitions, you can write as follows on the server side (ASP.NET Core). 
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


## Pattern of HubProxy and Receiver
It's very easy to use. 
```cs
HubConnection connection = ...;

var hub = connection.CreateHubProxy<IHubContract>();
var subscription = connection.Register<IClientContract>(new Receiver());
// or
var (hub, subscription) = connection.CreateHubProxyWith<IHubContract, IClientContract>(new Receiver());

// Invoke hub methods
hub.SomeHubMethod1("user", "message");

// Unregister the receiver
subscription.Dispose();
```

## Pattern similar to Hub\<T\> on the server-side
Define a base class that annotates HubClientBaseAttribute. 
Then just define a class that inherits from that base class. 

The HubClientBaseAttribute constructor takes the type of the interface. 

```cs
// The base class must be a partial class. 
[HubClientBase(typeof(IHubContract), typeof(IClientContract))]
partial class ClientBase
{
}

// inherit base class
// If you type "ctrl + ." or "override", visual studio (or rider) will generate the function for you. 
class HubClient : ClientBase
{
    // HubConnection is required for the base class constructor. 
    public HubClient(HubConnection connection) : base(connection)
    {
    }

    // override and impl!
    // These methods are automatically registered for connection. 
    public override Task SomeClientMethod1(string user, string message, UserDefineClass userDefine)
    {
        Console.WriteLine("Call SomeClientMethod1!");
        return Task.CompletedTask;
    }

    // override and impl!
    public override Task SomeClientMethod2()
    {
        Console.WriteLine("Call SomeClientMethod1!");
        return Task.CompletedTask;
    }

    // SignalR events can also be implemented without callbacks. 
    // OnClosed, OnReconnected, OnReconnecting are supported.
    public override Task OnClosed(Exception e)
    {
        Console.WriteLine($"[On Closed!]");
        return Task.CompletedTask;
    }
}
```

Usage is simple. 

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

# Compile-time error support
This source generator has some restrictions, including those that come from the server side. 

- Type argument of `CreateHubProxy/CreateHubProxyWith/Register` must be an interface.
- Only define methods in the interface used for `HubProxy/Receiver/HubClientBase`. 
  - Properties should not be defined. 
- The return type of the method in the interface used for `Hub/HubProxy` must be `Task` or `Task<T>`.
- The return type of the method in the interface used for `Receiver/Client-side` must be `Task`.
- Argument of `HubClientBaseAttribute` must be interface type.

It is very difficult for humans to properly comply with these restrictions. Therefore, it is designed so that the compiler (Roslyn) looks for the part where the constraint is not observed at compile time and reports a detailed error. Therefore, no run-time error occurs. 

![compile-time-error](img/compile-time-error.png)

# What kind of source code will be generated?
By annotating the `[HubClientBase(typeof(IHubContract), typeof(IClientContract))]` Attribute, the following code will be generated (simplified here). 

```cs
partial abstract class ClientBase : IHubClient<IHubContract>, IClientContract, IAsyncDisposable
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

    public abstract Task SomeClientMethod1(string user,string message, UserDefineClass userDefine);

    public abstract Task SomeClientMethod2();

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

# Demo
First, launch server.
Then access it from your browser and open the console(F12). 

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
