using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests;

// Lunch TypedSignalR.Client.Tests.Server.csproj before test!
public class InheritHubTest : IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly IInheritHub _inheritHub;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public InheritHubTest()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5105/Hubs/InheritTestHub")
            .Build();

        _inheritHub = _connection.CreateHubProxy<IInheritHub>(_cancellationTokenSource.Token);
    }

    public async Task InitializeAsync()
    {
        await _connection.StartAsync(_cancellationTokenSource.Token);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _connection.StopAsync(_cancellationTokenSource.Token);
        }
        finally
        {
            _cancellationTokenSource.Cancel();
        }
    }

    /// <summary>
    /// no parameter test
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Get()
    {
        var str = await _inheritHub.Get();
        Assert.Equal("TypedSignalR.Client", str);
    }

    [Fact]
    public async Task Add()
    {
        var x = Random.Shared.Next();
        var y = Random.Shared.Next();

        var added = await _inheritHub.Add(x, y);

        Assert.Equal(added, x + y);
    }

    [Fact]
    public async Task Cat()
    {
        var x = "revue";
        var y = "starlight";

        var cat = await _inheritHub.Cat(x, y);

        Assert.Equal(cat, x + y);
    }

    /// <summary>
    /// User defined type test
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Echo()
    {
        var instance = new UserDefinedType()
        {
            Guid = Guid.NewGuid(),
            DateTime = DateTime.Now,
        };

        var ret = await _inheritHub.Echo(instance);

        Assert.Equal(ret.DateTime, instance.DateTime);
        Assert.Equal(ret.Guid, instance.Guid);
    }
}
