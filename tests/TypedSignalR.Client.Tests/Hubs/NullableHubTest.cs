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

public class NullableHubTest : IntegrationTestBase
{
    [Fact]
    public async Task GetStruct()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var x = Random.Shared.Next();

        var value = await hubProxy.GetStruct(x);

        Assert.Equal(x + 7, value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetNullableStruct1()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var x = Random.Shared.Next();

        var value = await hubProxy.GetNullableStruct(x);

        Assert.Equal(x + 99, value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetNullableStruct2()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var value = await hubProxy.GetNullableStruct(null);

        Assert.Null(value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetReferenceType()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var message = Guid.NewGuid().ToString();

        var value = await hubProxy.GetReferenceType(message);

        Assert.Equal(message + "7", value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetNullableReferenceType1()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var message = Guid.NewGuid().ToString();

        var value = await hubProxy.GetNullableReferenceType(message);

        Assert.Equal(message + "99", value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetNullableReferenceType2()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var value = await hubProxy.GetNullableReferenceType(null);

        Assert.Null(value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetNullableReferenceType3()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var value = await hubProxy.GetNullableReferenceType2(null, null);

        Assert.Null(value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetNullableReferenceType4()
    {
        var hubConnection = CreateHubConnection("/Hubs/NullableTestHub", HttpTransportType.WebSockets);

        var hubProxy = hubConnection.CreateHubProxy<INullableTestHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var message1 = Guid.NewGuid().ToString();
        var message2 = Guid.NewGuid().ToString();

        var value = await hubProxy.GetNullableReferenceType2(message1, message2);

        Assert.Equal(message1 + message2, value);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
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

        public Task<string?> GetNullableReferenceType2(string? message, int? value)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNullableReferenceType3(string? message, string message2)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNullableReferenceType4(string message, string? message2, int value)
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
