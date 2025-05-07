using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class PostTest : IntegrationTestBase, IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly ISideEffectHub _sideEffectHub;

    public PostTest()
    {
        _connection = CreateHubConnection("/Hubs/SideEffectHub", HttpTransportType.WebSockets);

        _sideEffectHub = _connection.CreateHubProxy<ISideEffectHub>(TestContext.Current.CancellationToken);
    }

    public async ValueTask InitializeAsync()
    {
        await _connection.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoParameter()
    {
        await _sideEffectHub.Init(); // 0
        await _sideEffectHub.Increment(); // 1
        await _sideEffectHub.Increment(); // 2
        await _sideEffectHub.Increment(); // 3
        await _sideEffectHub.Increment(); // 4

        var result = await _sideEffectHub.Result();

        Assert.Equal(4, result);
    }

    [Fact]
    public async Task PostParameter()
    {
        List<UserDefinedType> list = new();

        for (int i = 0; i < 10; i++)
        {
            var instance = new UserDefinedType()
            {
                Guid = Guid.NewGuid(),
                DateTime = DateTime.Now,
            };
            list.Add(instance);
            await _sideEffectHub.Post(instance);
        }

        var data = await _sideEffectHub.Fetch();

        for (int i = 0; i < data.Length; i++)
        {
            Assert.Equal(data[i].DateTime, list[i].DateTime);
            Assert.Equal(data[i].Guid, list[i].Guid);
        }
    }
}
