using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace TypedSignalR.Client.Tests;

// Lunch TypedSignalR.Client.Tests.Server.csproj before test!
public class DisposeTest : IAsyncLifetime, IReceiver
{
    private readonly HubConnection _connection;
    private readonly IReceiverTestHub _receiverTestHub;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ITestOutputHelper _output;

    private int _notifyCallCount;
    private readonly List<(string, int)> _receiveMessage = new();
    private readonly List<UserDefinedType> _userDefinedList = new();

    public DisposeTest(ITestOutputHelper output)
    {
        _output = output;

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5105/Hubs/ReceiverTestHub")
            .Build();

        _receiverTestHub = _connection.CreateHubProxy<IReceiverTestHub>(_cancellationTokenSource.Token);
        var subscription = _connection.Register<IReceiver>(this);

        subscription.Dispose();
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

    Task IReceiver.Nofity()
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
