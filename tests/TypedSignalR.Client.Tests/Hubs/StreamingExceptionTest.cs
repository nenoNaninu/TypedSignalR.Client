using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class StreamingExceptionTest : IntegrationTestBase
{
    [Fact]
    public async Task ChannelAcquisitionFailureIncludesProxyMethod()
    {
        await using var connection = CreateHubConnection("/Hubs/StreamingHub", HttpTransportType.WebSockets);
        var hub = connection.CreateHubProxy<IStreamingHub>(TestContext.Current.CancellationToken);

        // Leave the connection stopped so the asynchronous operation fails.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => hub.ZeroParameterChannel());

        Assert.NotNull(exception.StackTrace);
        Assert.Contains($"{hub.GetType().Name}.{nameof(IStreamingHub.ZeroParameterChannel)}(", exception.StackTrace);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClientStreamingSendFailureIncludesProxyMethod(bool asChannel)
    {
        await using var connection = CreateHubConnection("/Hubs/StreamingHub", HttpTransportType.WebSockets);
        var hub = connection.CreateHubProxy<IStreamingHub>(TestContext.Current.CancellationToken);
        var channel = Channel.CreateUnbounded<Person>();
        channel.Writer.Complete();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => asChannel
            ? hub.UploadStreamAsChannelWithoutPublisher(channel.Reader)
            : hub.UploadStreamWithoutPublisher(channel.Reader.ReadAllAsync(TestContext.Current.CancellationToken)));

        var methodName = asChannel ? nameof(IStreamingHub.UploadStreamAsChannelWithoutPublisher) : nameof(IStreamingHub.UploadStreamWithoutPublisher);
        Assert.NotNull(exception.StackTrace);
        Assert.Contains($"{hub.GetType().Name}.{methodName}(", exception.StackTrace);
    }
}
