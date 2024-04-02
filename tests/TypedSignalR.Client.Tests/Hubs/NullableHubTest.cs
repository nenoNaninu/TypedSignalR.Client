using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class NullableHubTest : IntegrationTestBase, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task GetStruct()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(_cancellationTokenSource.Token);

        await hubConnection.StartAsync(_cancellationTokenSource.Token);

        var x = Random.Shared.Next();

        var value = await hubProxy.GetStruct(x);

        Assert.Equal(x + 7, value);

        await hubConnection.StopAsync(_cancellationTokenSource.Token);
    }

    [Fact]
    public async Task GetNullableStruct1()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(_cancellationTokenSource.Token);

        await hubConnection.StartAsync(_cancellationTokenSource.Token);

        var x = Random.Shared.Next();

        var value = await hubProxy.GetNullableStruct(x);

        Assert.Equal(x + 99, value);

        await hubConnection.StopAsync(_cancellationTokenSource.Token);
    }

    [Fact]
    public async Task GetNullableStruct2()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(_cancellationTokenSource.Token);

        await hubConnection.StartAsync(_cancellationTokenSource.Token);

        var value = await hubProxy.GetNullableStruct(null);

        Assert.Null(value);

        await hubConnection.StopAsync(_cancellationTokenSource.Token);
    }

    [Fact]
    public async Task GetReferenceType()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(_cancellationTokenSource.Token);

        await hubConnection.StartAsync(_cancellationTokenSource.Token);

        var message = Guid.NewGuid().ToString();

        var value = await hubProxy.GetReferenceType(message);

        Assert.Equal(message + "7", value);

        await hubConnection.StopAsync(_cancellationTokenSource.Token);
    }

    [Fact]
    public async Task GetNullableReferenceType1()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(_cancellationTokenSource.Token);

        await hubConnection.StartAsync(_cancellationTokenSource.Token);

        var message = Guid.NewGuid().ToString();

        var value = await hubProxy.GetNullableReferenceType(message);

        Assert.Equal(message + "99", value);

        await hubConnection.StopAsync(_cancellationTokenSource.Token);
    }

    [Fact]
    public async Task GetNullableReferenceType2()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(_cancellationTokenSource.Token);

        await hubConnection.StartAsync(_cancellationTokenSource.Token);

        var value = await hubProxy.GetNullableReferenceType(null);

        Assert.Null(value);

        await hubConnection.StopAsync(_cancellationTokenSource.Token);
    }

    private void CompileTest()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);
        hubConnection.Register<INullableTestIReceiver>(new NullableTestIReceiver());
    }

    private class NullableTestIReceiver : INullableTestIReceiver
    {
        public Task<string?> GetNullableReferenceType(string? message)
        {
            throw new NotImplementedException();
        }

        public Task<int?> GetNullableStruct(int? message)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetReferenceType(string message)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetStruct(int message)
        {
            throw new NotImplementedException();
        }
    }
}
