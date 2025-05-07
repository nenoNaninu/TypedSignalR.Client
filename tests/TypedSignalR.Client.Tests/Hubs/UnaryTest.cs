using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class UnaryTest : IntegrationTestBase
{
    /// <summary>
    /// no parameter test
    /// </summary>
    /// <returns></returns>
    [Theory]
    [InlineData(HttpTransportType.WebSockets)]
    [InlineData(HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling)]
    public async Task Get(HttpTransportType httpTransportType)
    {
        var hubConnection = CreateHubConnection("/Hubs/UnaryHub", httpTransportType);

        var unaryHub = hubConnection.CreateHubProxy<IUnaryHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var str = await unaryHub.Get();
        Assert.Equal("TypedSignalR.Client", str);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(HttpTransportType.WebSockets)]
    [InlineData(HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling)]
    public async Task Add(HttpTransportType httpTransportType)
    {
        var hubConnection = CreateHubConnection("/Hubs/UnaryHub", httpTransportType);

        var unaryHub = hubConnection.CreateHubProxy<IUnaryHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var x = Random.Shared.Next();
        var y = Random.Shared.Next();

        var added = await unaryHub.Add(x, y);

        Assert.Equal(added, x + y);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(HttpTransportType.WebSockets)]
    [InlineData(HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling)]
    public async Task Cat(HttpTransportType httpTransportType)
    {
        var hubConnection = CreateHubConnection("/Hubs/UnaryHub", httpTransportType);

        var unaryHub = hubConnection.CreateHubProxy<IUnaryHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var x = "revue";
        var y = "starlight";

        var cat = await unaryHub.Cat(x, y);

        Assert.Equal(cat, x + y);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// User defined type test
    /// </summary>
    /// <returns></returns>
    [Theory]
    [InlineData(HttpTransportType.WebSockets)]
    [InlineData(HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling)]
    public async Task Echo(HttpTransportType httpTransportType)
    {
        var hubConnection = CreateHubConnection("/Hubs/UnaryHub", httpTransportType);

        var unaryHub = hubConnection.CreateHubProxy<IUnaryHub>(TestContext.Current.CancellationToken);

        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var instance = new UserDefinedType()
        {
            Guid = Guid.NewGuid(),
            DateTime = DateTime.Now,
        };

        var ret = await unaryHub.Echo(instance);

        Assert.Equal(ret.DateTime, instance.DateTime);
        Assert.Equal(ret.Guid, instance.Guid);

        await hubConnection.StopAsync(TestContext.Current.CancellationToken);
    }
}
