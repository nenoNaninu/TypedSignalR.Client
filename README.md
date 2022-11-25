# TypedSignalR.Client

[![build-and-test](https://github.com/nenoNaninu/TypedSignalR.Client/actions/workflows/build-and-test.yaml/badge.svg)](https://github.com/nenoNaninu/TypedSignalR.Client/actions/workflows/build-and-test.yaml)

C# [Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to create strongly typed SignalR clients.

## Table of Contents
- [Install](#install)
- [Why TypedSignalR.Client?](#why-typedsignalrclient)
- [API](#api)
- [Usage](#usage)
  - [Client](#client)
    - [Cancellation](#cancellation)
  - [Server](#server)
- [Recommendation](#recommendation)
  - [Sharing a Project](#sharing-a-project)
  - [Client Code Format](#client-code-format)
- [Streaming Support](#streaming-support)
- [Client Results Support](#client-results-support)
- [Compile-Time Error Support](#compile-time-error-support)
- [Generated Source Code](#generated-source-code)
- [Related Work](#related-work)

## Install
NuGet: [TypedSignalR.Client](https://www.nuget.org/packages/TypedSignalR.Client/)

```
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package TypedSignalR.Client
```

## Why TypedSignalR.Client?
The ASP.NET Core SignalR C# client is not strongly typed.
To call a Hub (server-side) method, we must specify the method defined in Hub using a string.
We also have to determine the return type manually.
Moreover, registering client methods called from a server also requires specifying the method name as a string, and we must set parameter types manually.

```cs
// ASP.NET Core SignalR Client

// Specify a hub method to invoke using string.
await connection.InvokeAsync("HubMethod1");

// Manually determine a return type.
// Parameters are cast to object type.
var guid = await connection.InvokeAsync<Guid>("HubMethod2", "message", 99);

// Registering a client method requires a string, and parameter types must be set manually.
var subscription = connection.On<string, DateTime>("ClientMethod", (message, dateTime) => {});
```

These are very painful and cause bugs easily.
Moreover, if we change the code on the server-side, the modification on the client-side becomes very troublesome. 
The leading cause of the problems is that they are not strongly typed.

TypedSignalR.Client aims to generate strongly typed SignalR clients using interfaces in which the server and client methods are defined. 
Defining interfaces is helpful not only for the client-side but also for the server-side.
See [Usage](#usage) section for details.

```cs
// TypedSignalR.Client

// First, create a hub proxy.
IHub hubProxy = connection.CreateHubProxy<IHub>();

// Invoke a hub method through hub proxy.
// We no longer need to specify the method using a string.
await hubProxy.HubMethod1();

// Both parameters and return types are strongly typed.
var guid = await hubProxy.HubMethod2("message", 99);

// Client method registration is also strongly typed, so it's safe and easy.
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
    // implementation
}

```

## API
This Source Generator provides two extension methods and one interface. 

```cs
static class HubConnectionExtensions
{
    THub CreateHubProxy<THub>(this HubConnection connection, CancellationToken cancellationToken = default){...}
    IDisposable Register<TReceiver>(this HubConnection connection, TReceiver receiver){...}
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

## Usage
For example, we have the following interface defined.

```cs
public class UserDefinedType
{
    public Guid Id { get; set; }
    public DateTime Datetime { get; set; }
}

// The return type of methods on the client-side must be Task. 
public interface IClientContract
{
    // Of course, user defined type is OK. 
    Task ClientMethod1(string user, string message, UserDefinedType userDefine);
    Task ClientMethod2();
}

// The return type of methods on the hub-side must be Task or Task<T>. 
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

### Client
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

#### Cancellation
In ASP.NET Core SignalR, `CancellationToken` is passed for each invoke.

On the other hand, in TypedSignalR.Client, `CancellationToken` is passed only once when creating a hub proxy.
The passed `CancelationToken` will be used for each invoke internally.

```cs
var cts = new CancellationTokenSource();

// The following two are equivalent.

// 1: ASP.NET Core SignalR Client
var ret =  await connection.InvokeAsync<string>("HubMethod1", "user", "message", cts.Token);
await connection.InvokeAsync("HubMethod2", cts.Token);

// 2: TypedSignalR.Client
var hubProxy = connection.CreateHubProxy<IHubContract>(cts.Token);
var ret = await hubProxy.HubMethod1("user", "message");
await hubProxy.HubMethod2();
```

### Server
Using the interface definitions, we can write as follows on the server-side (ASP.NET Core). 
TypedSignalR.Client is not necessary.

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

## Recommendation
### Sharing a Project
I recommend that these interfaces be shared between the client-side and server-side project, for example, by project references.

```
server.csproj --> shared.csproj <-- client.csproj
```

### Client Code Format
It is easier to handle if we write client code in the following format.

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

## Streaming Support

SignalR supports both [server-to-client streaming and client-to-server streaming](https://docs.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-6.0).

TypedSignalR.Client supports both server-to-client streaming and client-to-server streaming.
If you use `IAsyncEnumerable<T>`, `Task<IAsyncEnumerable<T>`, or `Task<ChannelReader<T>>` for the method return type, it is analyzed as server-to-client streaming.
And if `IAsyncEnumerable<T>` or `ChannelReader<T>` is used in the method parameter, it is analyzed as client-to-server streaming.

When using server-to-client streaming, a single `CancellationToken` can be used as a method parameter (Note: `CancellationToken` cannot be used as a parameter except for server-to-client streaming).

## Client Results Support

.NET 7 and later, [client results](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-7.0#client-results) can be used.

TypedSignalR.Client supports client results.
If you use `Task<T>` for the method return type in the receiver interface, you can use client results.

## Compile-Time Error Support
This library has some restrictions, including those that come from server-side implementations.

- Type argument of the `CreateHubProxy/Register` method must be an interface.
- Only method definitions are allowed in the interface used for `CreateHubProxy/Register`.
  - It is forbidden to define properties and events.
- The return type of the method in the interface used for `CreateHubProxy` must be `Task` or `Task<T>`.
- The return type of the method in the interface used for `Register` must be `Task`.

It is complicated for humans to comply with these restrictions properly.
So, this library looks for parts that do not follow the restriction and report detailed errors at compile time. 
Therefore, no runtime error occurs. 

![compile-time-error](https://user-images.githubusercontent.com/27144255/155505022-0a13bf1b-643c-472c-882e-8508e52c2b63.png)

## Generated Source Code
TypedSignalR.Client checks the type argument of a methods `CreateHubProxy` and `Register` and generates source code.
Generated source code can be seen in Visual Studio. 

![generated-code-visible-from-solution-explorer](https://user-images.githubusercontent.com/27144255/154827948-dca0b9b1-0a1b-4833-8b32-3d5ceaa41414.png)

## Related Work
- [nenoNaninu/TypedSignalR.Client.TypeScript](https://github.com/nenoNaninu/TypedSignalR.Client.TypeScript)
  - TypeScript source generator to provide strongly typed SignalR clients by analyzing C# type definitions.
