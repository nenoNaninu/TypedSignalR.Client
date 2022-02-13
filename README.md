# TypedSignalR.Client

[![build-and-test](https://github.com/nenoNaninu/TypedSignalR.Client/actions/workflows/build-and-test.yaml/badge.svg)](https://github.com/nenoNaninu/TypedSignalR.Client/actions/workflows/build-and-test.yaml)

C# [Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to create strongly typed SignalR Client.

## Table of Contents
- [Install](#install)
- [Why TypedSignalR.Client?](#why-typedsignalrclient)
- [API](#api)
- [Usage](#usage)
  - [Client](#client)
    - [Cancellation](#cancellation)
  - [Server](#server)
- [Recommendation](#recommendation)
  - [Sharing a project](#sharing-a-project)
  - [Client code format](#client-code-format)
- [Compile-time error support](#compile-time-error-support)
- [Generated source code](#generated-source-code)

# Install
NuGet: [TypedSignalR.Client](https://www.nuget.org/packages/TypedSignalR.Client/)

```
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package TypedSignalR.Client
```

# Why TypedSignalR.Client?
The C# SignalR Client is untyped.
To call a Hub (server-side) function, you must specify the function defined in Hub using a string.
We also have to determine the return type manually.
Moreover, registering a client function called from the server also requires a string, and we must set the parameter types manually.

```cs
// pure SignalR Client

// Specify the hub method to invoke using string.
await connection.InvokeAsync("HubMethod1");

// Manually determine the return type.
// The parameter is cast to object type.
var guid = await connection.InvokeAsync<Guid>("HubMethod2", "message", 99);

// Registering a client function requires a string, 
// and the parameter types must be set manually.
var subscription = connection.On<string, DateTime>("ClientMethod", (message, dateTime) => {});
```

Therefore, if you change the code on the server-side, the modification on the client-side becomes very troublesome. 
The leading cause is that it is not strongly typed.

TypedSignalR.Client aims to generate a strongly typed SignalR Client by sharing interfaces in which the server and client functions are defined. 
Defining interfaces are helpful not only for the client-side but also for the server-side.
See [Usage](#usage) section for details.

```cs
// TypedSignalR.Client

// First, create a hub proxy.
IHub hubProxy = connection.CreateHubProxy<IHub>();

// Invoke a hub method through hub proxy.
// You no longer need to specify the function using a string.
await hubProxy.HubMethod1();

// Both parameters and return types are strongly typed.
var guid = await hubProxy.HubMethod2("message", 99);

// The client's function registration is also strongly typed, so it's safe and easy.
var subscription = connection.Register<IReceiver>(new Receiver());

// Defining interfaces are useful not only for the client-side but also for the server-side.
// See Usage in this README.md for details.
interface IHub
{
    Task HubMethod1();
    Task<Guid> HubMethod2(string message, int value);
}

interface IReceiver
{
    Task ClientMethod(string message, DateTime dateTime);
}

class Receiver : IReceiver
{
    ...
}

```

# API
This Source Generator provides two extension methods and one interface. 

```cs
static class HubConnectionExtensions
{
    THub CreateHubProxy<THub>(this HubConnection source, CancellationToken cancellationToken = default){...}
    IDisposable Register<TReceiver>(this HubConnection source, TReceiver receiver){...}
}

// An interface for observing SignalR events.
interface IHubConnectionObserver
{
    Task OnClosed(Exception? exception);
    Task OnReconnected(string? connectionId);
    Task OnReconnecting(Exception? exception);
}
```

Use it as follows. 

```cs
HubConnection connection = ...;

IHub hub = connection.CreateHubProxy<IHub>();
IDisposable subscription = connection.Register<IReceiver>(new Receiver());
```

# Usage
For example, you have the following interface defined.

```cs
public class UserDefinedType
{
    public Guid Id { get; set; }
    public DateTime Datetime { get; set; }
}

// The return type of the client-side method must be Task. 
public interface IClientContract
{
    // Of course, user defined type is OK. 
    Task ClientMethod1(string user, string message, UserDefinedType userDefine);
    Task ClientMethod2();
}

// The return type of the method on the hub-side must be Task or Task <T>. 
public interface IHubContract
{
    Task<string> HubMethod1(string user, string message);
    Task HubMethod2();
}

class Receiver1 : IClientContract
{
    // implementation
}

class Receiver2 : IClientContract, IHubConnectionObserver
{
    // implementation
}
```

## Client
It's very easy to use. 

```cs

HubConnection connection = ...;

var hub = connection.CreateHubProxy<IHubContract>();
var subscription1 = connection.Register<IClientContract>(new Receiver1());

// When an instance of a class that implements IHubConnectionObserver is registered (Receiver2 in this case), 
// the method defined in IHubConnectionObserver is automatically registered regardless of the type argument. 
var subscription2 = connection.Register<IClientContract>(new Receiver2());

// Invoke hub methods
hub.HubMethod1("user", "message");

// Unregister the receiver
subscription.Dispose();
```

### Cancellation
In pure SignalR, `CancellationToken` is passed for each invoke.

On the other hand, in TypedSignalR.Client, `CancellationToken` is passed only once when creating hub proxy.
The passed `CancelationToken` will be used for each invoke internally.

```cs
var cts = new CancellationTokenSource();

// The following two are equivalent.

// 1: Pure SignalR
var ret =  await connection.InvokeAsync<string>("HubMethod1", "user", "message", cts.Token);
await connection.InvokeAsync("HubMethod2", cts.Token);

// 2: TypedSignalR.Client
var hubProxy = connection.CreateHubProxy<IHubContract>(cts.Token);
var ret = await hubProxy.HubMethod1("user", "message");
await hubProxy.HubMethod2();
```

## Server
Using the interface definitions, you can write as follows on the server-side (ASP.NET Core). 
TypedSignalR.Client is not nessesary.

```cs
using Microsoft.AspNetCore.SignalR;

public class SomeHub : Hub<IClientContract>, IHubContract
{
    public async Task<string> HubMethod1(string user, string message)
    {
        var instance = new UserDefinedType()
        {
            Id = Guid.NewGuid(),
            DateTime = DateTime.Now,
        };

        // broadcast
        await this.Clients.All.ClientMethod1(user, message, instance);
        return "OK!";
    }

    public async Task HubMethod2()
    {
        await this.Clients.Caller.ClientMethod2();
    }
}
```

# Recommendation
## Sharing a project
I recommend that these interfaces be shared between the client-side and server-side project, for example, by project references.

```
server.csproj => shared.csproj <= client.csproj
```

## Client code format
It is easier to handle if you write the client code in the following format.

```cs
class Client : IReceiver, IHubConnectionObserver, IDisposable
{
    private readonly IHub _hubProxy;
    private readonly IDisposable _subscription;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Client(HubConnection connection)
    {
        _hubProxy = connection.CreateHubProxy<IHub>(_cancellationTokenSource.Token);
        _subscription = connection.Register<IReceiver>(this);
    }

    // implementation
}
```

# Compile-time error support
This library has some restrictions, including those that come from server-side implementations.

- Type argument of the `CreateHubProxy/Register` method must be an interface.
- Only methods must be defined in the interface used for `CreateHubProxy/Register`.
  - It is forbidden to define properties.
- The return type of the method in the interface used for `CreateHubProxy` must be `Task` or `Task<T>`.
- The return type of the method in the interface used for `Register` must be `Task`.

It is complicated for humans to comply with these restrictions properly.
So, this library looks for parts that do not follow the restriction and report detailed errors at compile-time. 
Therefore, no run-time error occurs. 

![compile-time-error](https://user-images.githubusercontent.com/27144255/153356331-3b4d9af6-b289-457c-8f45-2a4fcb8bb049.png)

# Generated source code
TypedSignalR.Client checks the type argument of a methods `CreateHubProxy` and `Register` and generates source code.
Generated source code can be seen in Visual Studio. 

![generated-source-code-in-dependencies](https://user-images.githubusercontent.com/27144255/153228062-08d56132-01d0-4015-9c2d-bb9cd34ea76f.png)
