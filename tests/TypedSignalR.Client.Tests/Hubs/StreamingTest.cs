using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests.Hubs;

public class StreamingTest : IntegrationTestBase, IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly IStreamingHub _streamingHub;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly Person[] _persons = new Person[]
    {
        new Person(Guid.Parse("c61bcc3f-f477-2206-3c1f-830b05a6ed0f"), "KAREN AIJO", 1),
        new Person(Guid.Parse("ef28aba1-1f85-daf2-2dac-a5cf138421cd"), "FUTABA ISURUGI", 2),
        new Person(Guid.Parse("478d3485-5914-1269-e4dc-dedcc749ebb6"), "CLAUDINU SAIJO", 11),
        new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15),
        new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "MAHIRU TSUYUZAKI", 17),
        new Person(Guid.Parse("ced51d7b-fe37-b619-4026-a39457c954b6"), "MAYA TENDO", 18),
        new Person(Guid.Parse("60287e1f-7b34-59f9-47d1-c29ae758872e"), "KAORUKO HANAYAGI", 22),
        new Person(Guid.Parse("5921c655-6012-48f1-50ef-1881277c22d4"), "JUNNA HOSIMI", 25),
        new Person(Guid.Parse("36d8986d-17de-2442-a33e-18ec942575f1"), "HIKARI KAGURA", 29),
    };

    public StreamingTest()
    {
        _connection = CreateHubConnection("/Hubs/StreamingHub", HttpTransportType.WebSockets);

        _streamingHub = _connection.CreateHubProxy<IStreamingHub>(_cancellationTokenSource.Token);
    }

    public async Task InitializeAsync()
    {
        await _connection.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.StopAsync();
    }

    [Fact]
    public async Task ZeroParameter()
    {
        var stream = _streamingHub.ZeroParameter();

        int idx = 0;

        await foreach (var it in stream)
        {
            Assert.Equal(_persons[idx], it);
            idx++;
        }
    }

    [Fact]
    public async Task CancellationTokenOnly()
    {
        var stream = _streamingHub.CancellationTokenOnly(_cancellationTokenSource.Token);

        int idx = 0;

        await foreach (var it in stream)
        {
            Assert.Equal(_persons[idx], it);
            idx++;
        }
    }

    [Fact]
    public async Task Counter()
    {
        var publisher = new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15);

        int value = 0;
        int step = 2;

        var stream = _streamingHub.Counter(publisher, value, step, 10);

        await foreach (var it in stream)
        {
            Assert.Equal(publisher, it.Publisher);
            Assert.Equal(value, it.Value);

            value += step;
        }

        Assert.Equal(20, value);
    }

    [Fact]
    public async Task CancelableCounter()
    {
        var publisher = new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15);

        int value = 0;
        int step = 2;

        var stream = _streamingHub.CancelableCounter(publisher, value, step, 10, _cancellationTokenSource.Token);

        await foreach (var it in stream)
        {
            Assert.Equal(publisher, it.Publisher);
            Assert.Equal(value, it.Value);

            value += step;
        }

        Assert.Equal(20, value);
    }

    [Fact]
    public async Task TaskCancelableCounter()
    {
        var publisher = new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15);

        int value = 0;
        int step = 2;

        var stream = await _streamingHub.TaskCancelableCounter(publisher, value, step, 10, _cancellationTokenSource.Token);

        await foreach (var it in stream)
        {
            Assert.Equal(publisher, it.Publisher);
            Assert.Equal(value, it.Value);

            value += step;
        }

        Assert.Equal(20, value);
    }

    [Fact]
    public async Task ZeroParameterChannel()
    {
        var stream = await _streamingHub.ZeroParameterChannel();

        int idx = 0;

        while (await stream.WaitToReadAsync(_cancellationTokenSource.Token))
        {
            while (stream.TryRead(out var it))
            {
                Assert.Equal(_persons[idx], it);
                idx++;
            }
        }
    }

    [Fact]
    public async Task CancellationTokenOnlyChannel()
    {
        var stream = await _streamingHub.CancellationTokenOnlyChannel(_cancellationTokenSource.Token);

        int idx = 0;

        while (await stream.WaitToReadAsync(_cancellationTokenSource.Token))
        {
            while (stream.TryRead(out var it))
            {
                Assert.Equal(_persons[idx], it);
                idx++;
            }
        }
    }

    [Fact]
    public async Task CounterChannel()
    {
        var publisher = new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15);

        int value = 0;
        int step = 2;

        var stream = await _streamingHub.CounterChannel(publisher, value, step, 10);

        while (await stream.WaitToReadAsync(_cancellationTokenSource.Token))
        {
            while (stream.TryRead(out var it))
            {
                Assert.Equal(publisher, it.Publisher);
                Assert.Equal(value, it.Value);

                value += step;
            }
        }

        Assert.Equal(20, value);
    }

    [Fact]
    public async Task CancelableCounterChannel()
    {
        var publisher = new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15);

        int value = 0;
        int step = 2;

        var stream = await _streamingHub.CancelableCounterChannel(publisher, value, step, 10, _cancellationTokenSource.Token);

        while (await stream.WaitToReadAsync(_cancellationTokenSource.Token))
        {
            while (stream.TryRead(out var it))
            {
                Assert.Equal(publisher, it.Publisher);
                Assert.Equal(value, it.Value);

                value += step;
            }
        }

        Assert.Equal(20, value);
    }

    // ALWAYS PASS
    // TODO: HOW TO TEST?
    [Fact]
    public async Task UploadStream()
    {
        var publisher = new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15);

        await _streamingHub.UploadStream(publisher, UploadStreamCore());

        Assert.True(true);
    }

    private async IAsyncEnumerable<Person> UploadStreamCore()
    {
        foreach (var person in _persons)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            yield return person;
        }
    }

    [Fact]
    public async Task UploadStreamAsChannel()
    {
        var publisher = new Person(Guid.Parse("8fd696c1-b102-7aa6-259b-4f8772457a7a"), "NANA DAIBA", 15);

        var channel = Channel.CreateUnbounded<Person>();

        _ = UploadStreamAsChannelCore(channel.Writer);

        await _streamingHub.UploadStreamAsChannel(publisher, channel.Reader);

        Assert.True(channel.Reader.Completion.IsCompleted);
    }

    private async Task UploadStreamAsChannelCore(ChannelWriter<Person> channelWriter)
    {
        foreach (var person in _persons)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            channelWriter.TryWrite(person);
        }

        channelWriter.Complete();
    }
}
