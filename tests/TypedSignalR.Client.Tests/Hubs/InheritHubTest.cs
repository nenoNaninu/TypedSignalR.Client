using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class InheritHubTest : IntegrationTestBase, IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly IInheritHub _inheritHub;

    public InheritHubTest()
    {
        _connection = CreateHubConnection("/Hubs/InheritTestHub", HttpTransportType.WebSockets);

        _inheritHub = _connection.CreateHubProxy<IInheritHub>(TestContext.Current.CancellationToken);
    }

    public async ValueTask InitializeAsync()
    {
        await _connection.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.StopAsync(TestContext.Current.CancellationToken);
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
