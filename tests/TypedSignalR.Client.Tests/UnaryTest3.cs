using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests;

// Lunch TypedSignalR.Client.Tests.Server.csproj before test!
public class UnaryTest3
{
    [Fact]
    public async Task Add()
    {
        // case 1. holder's type is ConnectionHolder
        //var holder = new ConnectionHolder();

        //case 2. holder's type is IConnectionHolder
        var holder = ConnectionHolder.Create();

        // ---------

        // case 1. use var
        //var connection = holder.HubConnection;
        //var hubProxy = connection.CreateHubProxy<IUnaryHub3>();

        // case 2. use property directly
        var hubProxy = holder.HubConnection.CreateHubProxy<IUnaryHub3>();

        await holder.HubConnection.StartAsync();

        var x = Random.Shared.Next();
        var y = Random.Shared.Next();

        var added = await hubProxy.Add(x, y);

        Assert.Equal(added, x + y);

        await holder.HubConnection.StopAsync();
        await holder.HubConnection.DisposeAsync();
    }
}

interface IConnectionHolder
{
    HubConnection HubConnection { get; }
}

class ConnectionHolder : IConnectionHolder
{
    public HubConnection HubConnection { get; }

    public ConnectionHolder()
    {
        this.HubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5105/Hubs/UnaryHub3")
            .Build();
    }

    public static IConnectionHolder Create() => new ConnectionHolder();
}
