using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests;

// Lunch TypedSignalR.Client.Tests.Server.csproj before test!
public class UnaryTest2
{
    [Fact]
    public async Task Add()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5105/Hubs/UnaryHub2")
            .Build();

        var hubProxy = connection.CreateHubProxy<IUnaryHub2>();

        await connection.StartAsync();

        var x = Random.Shared.Next();
        var y = Random.Shared.Next();

        var added = await hubProxy.Add(x, y);

        Assert.Equal(added, x + y);

        await connection.StopAsync();
        await connection.DisposeAsync();
    }
}
