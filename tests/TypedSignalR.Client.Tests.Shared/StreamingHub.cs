using System.Threading.Channels;

namespace TypedSignalR.Client.Tests.Shared;

public interface IStreamingHub
{
    // Server-to-Client streaming
    IAsyncEnumerable<Person> ZeroParameter();
    IAsyncEnumerable<Person> CancellationTokenOnly(CancellationToken cancellationToken);
    IAsyncEnumerable<Message> Counter(Person publisher, int init, int step, int count);
    IAsyncEnumerable<Message> CancelableCounter(Person publisher, int init, int step, int count, CancellationToken cancellationToken);
    Task<IAsyncEnumerable<Message>> TaskCancelableCounter(Person publisher, int init, int step, int count, CancellationToken cancellationToken);

    // Server-to-Client streaming
    Task<ChannelReader<Person>> ZeroParameterChannel();
    Task<ChannelReader<Person>> CancellationTokenOnlyChannel(CancellationToken cancellationToken);
    Task<ChannelReader<Message>> CounterChannel(Person publisher, int init, int step, int count);
    Task<ChannelReader<Message>> CancelableCounterChannel(Person publisher, int init, int step, int count, CancellationToken cancellationToken);
    Task<ChannelReader<Message>> TaskCancelableCounterChannel(Person publisher, int init, int step, int count, CancellationToken cancellationToken);

    // Client-to-Server streaming
    // TODO: HOW TO TEST?
    Task UploadStream(Person publisher, IAsyncEnumerable<Person> stream);
    Task UploadStreamAsChannel(Person publisher, ChannelReader<Person> stream);
}

public record Person(Guid Id, string Name, int Number);

public record Message(Person Publisher, int Value);
