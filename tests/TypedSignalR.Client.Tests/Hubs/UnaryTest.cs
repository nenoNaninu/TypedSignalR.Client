using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class UnaryTest : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public UnaryTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(HttpTransportType.WebSockets, false)]
    [InlineData(HttpTransportType.WebSockets, true)]
    [InlineData(HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling, false)]
    [InlineData(HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling, true)]
    public async Task HubExceptionStackTrace(HttpTransportType httpTransportType, bool withResult)
    {
        await using var hubConnection = CreateHubConnection("/Hubs/UnaryHub", httpTransportType);
        var unaryHub = hubConnection.CreateHubProxy<IUnaryHub>(TestContext.Current.CancellationToken);
        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<HubException>(() => withResult
            ? unaryHub.ThrowHubExceptionWithResult()
            : unaryHub.ThrowHubException());

        _output.WriteLine(exception.ToString());

        var methodName = withResult ? nameof(IUnaryHub.ThrowHubExceptionWithResult) : nameof(IUnaryHub.ThrowHubException);
        Assert.Equal($"An unexpected error occurred invoking '{methodName}' on the server. HubException: Server-side HubException for stack trace inspection.", exception.Message);
        Assert.Null(exception.InnerException);
        Assert.NotNull(exception.StackTrace);
        Assert.Contains($"{unaryHub.GetType().Name}.{methodName}(", exception.StackTrace);
        Assert.DoesNotContain("TypedSignalR.Client.Tests.Server.Hubs.UnaryHub", exception.StackTrace);
    }

    [Theory]
    [InlineData(HttpTransportType.WebSockets)]
    [InlineData(HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling)]
    public async Task KeywordParameterNames(HttpTransportType httpTransportType)
    {
        await using var hubConnection = CreateHubConnection("/Hubs/UnaryHub", httpTransportType);
        var unaryHub = hubConnection.CreateHubProxy<IUnaryHub>(TestContext.Current.CancellationToken);
        await hubConnection.StartAsync(TestContext.Current.CancellationToken);

        var result = await unaryHub.EchoKeywords("reserved", "awaited", "contextual");

        Assert.Equal("reserved:awaited:contextual", result);
    }

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
