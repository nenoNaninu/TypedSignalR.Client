using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class ClientResultsTest : IntegrationTestBase, IAsyncLifetime, IClientResultsTestHubReceiver
{
    private readonly HubConnection _connection;
    private readonly IClientResultsTestHub _hubProxy;

    private readonly ITestOutputHelper _output;

    public ClientResultsTest(ITestOutputHelper output)
    {
        _output = output;

        _connection = CreateHubConnection("/Hubs/ClientResultsTestHub", HttpTransportType.WebSockets);

        _hubProxy = _connection.CreateHubProxy<IClientResultsTestHub>(TestContext.Current.CancellationToken);
        _connection.Register<IClientResultsTestHubReceiver>(this);
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
    public async Task TestReceiver()
    {
        var result = await _hubProxy.StartTest();

        Assert.True(result);
    }

    public Task<Guid> GetGuidFromClient()
    {
        return Task.FromResult(Guid.Parse("ba3088bb-e7ea-4924-b01b-695e879bb166"));
    }

    public Task<Person> GetPersonFromClient()
    {
        return Task.FromResult(new Person(Guid.Parse("c2368532-2f13-4079-9631-a38a048d84e1"), "Nana Daiba", 7));
    }

    public Task<int> SumInClient(int left, int right)
    {
        return Task.FromResult(left + right);
    }
}
