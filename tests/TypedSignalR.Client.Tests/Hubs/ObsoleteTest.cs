using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public interface IObsoleteHub
{
    [Obsolete]
    Task Old();

    [Obsolete("Use New instead.")]
    Task<string> OldWithMessage(string value);

    Task New();
}

public interface IObsoleteReceiver
{
    [Obsolete]
    Task Old();

    [Obsolete("Use New instead.")]
    Task OldWithMessage(string value);

    Task New();
}

// The test project treats CS0612 and CS0618 as errors, so this only compiles when the generated code suppresses them.
public class ObsoleteTest
{
    [Fact]
    public async Task GeneratedCodeCompilesForObsoleteMembers()
    {
        await using var hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/Hubs/ObsoleteHub")
            .Build();

        var hub = hubConnection.CreateHubProxy<IObsoleteHub>(TestContext.Current.CancellationToken);
        using var subscription = hubConnection.Register<IObsoleteReceiver>(new ObsoleteReceiver());

        Assert.NotNull(hub);
    }

    private sealed class ObsoleteReceiver : IObsoleteReceiver
    {
        public Task Old() => Task.CompletedTask;

        public Task OldWithMessage(string value) => Task.CompletedTask;

        public Task New() => Task.CompletedTask;
    }
}
