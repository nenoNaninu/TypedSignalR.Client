using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.InMemoryServer.Hubs;

public class UnaryTest : IntegrationTestBase, IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly IUnaryHub _unaryHub;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public UnaryTest()
    {
        // Arrange
        var client = CreateClient();
        var handler = CreateHandler();

        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(client.BaseAddress!, "/Hubs/UnaryHub"), options =>
            {
                options.Transports = HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => handler;
            })
            .WithAutomaticReconnect()
            .Build();

        _unaryHub = _connection.CreateHubProxy<IUnaryHub>(_cancellationTokenSource.Token);
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
        var str = await _unaryHub.Get();
        Assert.Equal("TypedSignalR.Client", str);
    }

    [Fact]
    public async Task Add()
    {
        var x = Random.Shared.Next();
        var y = Random.Shared.Next();

        var added = await _unaryHub.Add(x, y);

        Assert.Equal(added, x + y);
    }

    [Fact]
    public async Task Cat()
    {
        var x = "revue";
        var y = "starlight";

        var cat = await _unaryHub.Cat(x, y);

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

        var ret = await _unaryHub.Echo(instance);

        Assert.Equal(ret.DateTime, instance.DateTime);
        Assert.Equal(ret.Guid, instance.Guid);
    }
}
