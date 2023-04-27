using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace TypedSignalR.Client.Tests;

// Lunch TypedSignalR.Client.Tests.Server.csproj before test!
public class InheritReceiverTest : IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly IInheritReceiverTestHub _receiverTestHub;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ITestOutputHelper _output;
    private readonly Receiver _receiver = new Receiver();


    public InheritReceiverTest(ITestOutputHelper output)
    {
        _output = output;

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5105/Hubs/InheritReceiverTestHub")
            .Build();

        _receiverTestHub = _connection.CreateHubProxy<IInheritReceiverTestHub>(_cancellationTokenSource.Token);
        _connection.Register<IInheritReceiver>(_receiver);
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

    private readonly string[] _answerMessages = new[] {
        "b1f7cd73-13b8-49bd-9557-ffb38859d18b",
        "3f5c3585-d01b-4f8f-8139-62a1241850e2",
        "92021a22-5823-4501-8cbd-c20d4ca6e54c",
        "5b134f73-2dc1-4271-8316-1a4250f42241",
        "e73acd30-e034-4569-8f30-88ac34b99052",
        "0d7531b5-0a36-4fe7-bbe5-8fee38c38c07",
        "32915627-3df6-41dc-8d30-7c655c2f7e61",
        "c875a6f9-9ddb-440b-a7e4-6e893f59ab9e",
    };

    private readonly string[] _guids = new[] {
        "b2f626e5-b4d4-4713-891d-f6cb107e502e",
        "22733524-2087-4701-a586-c6bf0ce36f74",
        "b89324bf-daf2-422a-85f2-6843b9c09b6a",
        "779769d1-0aee-4dba-82c7-9e1044836d75"
    };

    private readonly string[] _dateTimes = new[] {
        "2017-04-17",
        "2018-05-25",
        "2019-03-31",
        "2022-02-06",
    };

    [Fact]
    public async Task TestReceiver()
    {
        await _receiverTestHub.Start();

        _output.WriteLine($"_notifyCallCount: {_receiver.NotifyCallCount}");

        Assert.Equal(17, _receiver.NotifyCallCount);

        for (int i = 0; i < _receiver.ReceiveMessages.Count; i++)
        {
            _output.WriteLine($"_receiveMessage[{i}].Item1: {_receiver.ReceiveMessages[i].Item1}");
            _output.WriteLine($"_receiveMessage[{i}].Item2: {_receiver.ReceiveMessages[i].Item2}");

            Assert.Equal(_receiver.ReceiveMessages[i].Item1, _answerMessages[i]);
            Assert.Equal(_receiver.ReceiveMessages[i].Item2, i);
        }

        for (int i = 0; i < _receiver.UserDefinedList.Count; i++)
        {
            _output.WriteLine($"_userDefinedList[{i}].Guid: {_receiver.UserDefinedList[i].Guid}");
            _output.WriteLine($"_userDefinedList[{i}].DateTime: {_receiver.UserDefinedList[i].DateTime}");

            var guid = Guid.Parse(_guids[i]);
            var dateTime = DateTime.Parse(_dateTimes[i]);

            Assert.Equal(_receiver.UserDefinedList[i].Guid, guid);
            Assert.Equal(_receiver.UserDefinedList[i].DateTime, dateTime);
        }
    }
}

class Receiver : IInheritReceiver
{
    public int NotifyCallCount { get; private set; }
    public readonly List<(string, int)> ReceiveMessages = new();
    public readonly List<UserDefinedType> UserDefinedList = new();

    public Task ReceiveMessage(string message, int value)
    {
        ReceiveMessages.Add((message, value));

        return Task.CompletedTask;
    }

    public Task Notify()
    {
        NotifyCallCount++;

        return Task.CompletedTask;
    }

    public Task ReceiveCustomMessage(UserDefinedType userDefined)
    {
        UserDefinedList.Add(userDefined);

        return Task.CompletedTask;
    }

    public Task ReceiveMessage2(string message, int value)
    {
        return Task.CompletedTask;
    }
}
