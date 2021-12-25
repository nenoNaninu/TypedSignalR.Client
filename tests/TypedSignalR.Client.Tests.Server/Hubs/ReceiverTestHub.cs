using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Hubs;

public class ReceiverTestHub : Hub<IReceiver>, IReceiverTestHub
{
    private readonly string[] _message = new[] {
        "b1f7cd73-13b8-49bd-9557-ffb38859d18b",
        "3f5c3585-d01b-4f8f-8139-62a1241850e2",
        "92021a22-5823-4501-8cbd-c20d4ca6e54c",
        "5b134f73-2dc1-4271-8316-1a4250f42241",
        "e73acd30-e034-4569-8f30-88ac34b99052",
        "0d7531b5-0a36-4fe7-bbe5-8fee38c38c07",
        "32915627-3df6-41dc-8d30-7c655c2f7e61",
        "c875a6f9-9ddb-440b-a7e4-6e893f59ab9e",
    };

    private readonly ILogger<ReceiverTestHub> _logger;

    public ReceiverTestHub(ILogger<ReceiverTestHub> logger)
    {
        _logger = logger;
    }

    public async Task Start()
    {
        _logger.Log(LogLevel.Information, "ReceiverTestHub.Start");

        for (int i = 0; i < 17; i++)
        {
            await this.Clients.Caller.Nofity();
        }

        for (int i = 0; i < _message.Length; i++)
        {
            await this.Clients.Caller.ReceiveMessage(_message[i], i);
        }
    }
}
