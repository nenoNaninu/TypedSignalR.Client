using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class DisposeTest : IntegrationTestBase, IAsyncLifetime, IReceiver
{
    private readonly HubConnection _connection;
    private readonly IReceiverTestHub _receiverTestHub;
    private readonly ITestOutputHelper _output;

    private int _notifyCallCount;
    private readonly List<(string, int)> _receiveMessage = new();
    private readonly List<UserDefinedType> _userDefinedList = new();

    public DisposeTest(ITestOutputHelper output)
    {
        _output = output;

        _connection = CreateHubConnection("/Hubs/ReceiverTestHub", HttpTransportType.WebSockets);

        _receiverTestHub = _connection.CreateHubProxy<IReceiverTestHub>(TestContext.Current.CancellationToken);
        var subscription = _connection.Register<IReceiver>(this);

        subscription.Dispose();
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
        await _receiverTestHub.Start();

        _output.WriteLine($"_notifyCallCount: {_notifyCallCount}");

        Assert.Equal(0, _notifyCallCount);
        Assert.Empty(_receiveMessage);
        Assert.Empty(_userDefinedList);
    }

    Task IReceiver.ReceiveMessage(string message, int value)
    {
        _receiveMessage.Add((message, value));

        return Task.CompletedTask;
    }

    Task IReceiver.Notify()
    {
        _notifyCallCount++;

        return Task.CompletedTask;
    }

    Task IReceiver.ReceiveCustomMessage(UserDefinedType userDefined)
    {
        _userDefinedList.Add(userDefined);

        return Task.CompletedTask;
    }
}
