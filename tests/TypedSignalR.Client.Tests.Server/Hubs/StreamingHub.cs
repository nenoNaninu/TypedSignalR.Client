using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Hubs;

public sealed class StreamingHub : Hub, IStreamingHub
{
    private readonly ILogger<StreamingHub> _logger;

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

    public StreamingHub(ILogger<StreamingHub> logger)
    {
        _logger = logger;
    }

    // Server-to-Client streaming
    public async IAsyncEnumerable<Person> ZeroParameter()
    {
        foreach (var person in _persons)
        {
            this.Context.ConnectionAborted.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromMilliseconds(20));
            yield return person;
        }
    }

    public async IAsyncEnumerable<Person> CancellationTokenOnly([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var person in _persons)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);
            yield return person;
        }
    }

    public async IAsyncEnumerable<Message> Counter(Person publisher, int init, int step, int count)
    {
        int value = init;

        for (int i = 0; i < count; i++)
        {
            this.Context.ConnectionAborted.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromMilliseconds(20));
            yield return new Message(publisher, value);
            value += step;
        }
    }

    public async IAsyncEnumerable<Message> CancelableCounter(Person publisher, int init, int step, int count, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int value = init;

        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);
            yield return new Message(publisher, value);
            value += step;
        }
    }

    public Task<IAsyncEnumerable<Message>> TaskCancelableCounter(Person publisher, int init, int step, int count, CancellationToken cancellationToken)
    {
        return Task.FromResult(CancelableCounter(publisher, init, step, count, cancellationToken));
    }

    // Server-to-Client streaming
    public Task<ChannelReader<Person>> ZeroParameterChannel()
    {
        var channel = Channel.CreateUnbounded<Person>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = true });

        _ = WritePersonToChannelAsync(channel.Writer, 1, this.Context.ConnectionAborted);

        return Task.FromResult(channel.Reader);
    }

    public Task<ChannelReader<Person>> CancellationTokenOnlyChannel(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<Person>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = true });

        _ = WritePersonToChannelAsync(channel.Writer, 1, cancellationToken);

        return Task.FromResult(channel.Reader);
    }

    public Task<ChannelReader<Message>> CounterChannel(Person publisher, int init, int step, int count)
    {
        var channel = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = true });

        _ = WriteMessageToChannelAsync(channel.Writer, publisher, init, step, count, this.Context.ConnectionAborted);

        return Task.FromResult(channel.Reader);
    }

    public Task<ChannelReader<Message>> CancelableCounterChannel(Person publisher, int init, int step, int count, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = true });

        _ = WriteMessageToChannelAsync(channel.Writer, publisher, init, step, count, this.Context.ConnectionAborted);

        return Task.FromResult(channel.Reader);
    }

    public Task<ChannelReader<Message>> TaskCancelableCounterChannel(Person publisher, int init, int step, int count, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = true });

        _ = WriteMessageToChannelAsync(channel.Writer, publisher, init, step, count, cancellationToken);

        return Task.FromResult(channel.Reader);
    }

    // Client-to-Server streaming
    // TODO: HOW TO TEST?
    public async Task UploadStream(Person publisher, IAsyncEnumerable<Person> stream)
    {
        try
        {
            _logger.Log(LogLevel.Information, "UploadStream: publisher {publisher}", publisher);

            await foreach (var it in stream)
            {
                _logger.Log(LogLevel.Information, "UploadStream: it {it}", it);
            }
        }
        catch (Exception exception)
        {
            _logger.Log(LogLevel.Information, "UploadStream: Exception {exception}", exception);
        }
    }

    public async Task UploadStreamAsChannel(Person publisher, ChannelReader<Person> stream)
    {
        try
        {
            _logger.Log(LogLevel.Information, "UploadStreamAsChannel: publisher {publisher}", publisher);

            while (await stream.WaitToReadAsync())
            {
                while (stream.TryRead(out var it))
                {
                    _logger.Log(LogLevel.Information, "UploadStreamAsChannel: it {it}", it);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.Log(LogLevel.Information, "UploadStreamAsChannel: Exception {exception}", exception);
        }
    }

    private async Task WritePersonToChannelAsync(
        ChannelWriter<Person> writer,
        int delay,
        CancellationToken cancellationToken)
    {
        Exception? localException = null;
        try
        {
            foreach (var it in _persons)
            {
                await writer.WriteAsync(it, cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            localException = ex;
        }
        finally
        {
            writer.Complete(localException);
        }
    }

    private static async Task WriteMessageToChannelAsync(
        ChannelWriter<Message> writer,
        Person publisher,
        int init,
        int step,
        int count,
        CancellationToken cancellationToken)
    {
        Exception? localException = null;

        try
        {
            int value = init;

            for (int i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);
                await writer.WriteAsync(new Message(publisher, value), cancellationToken);

                value += step;
            }
        }
        catch (Exception ex)
        {
            localException = ex;
        }
        finally
        {
            writer.Complete(localException);
        }
    }
}
