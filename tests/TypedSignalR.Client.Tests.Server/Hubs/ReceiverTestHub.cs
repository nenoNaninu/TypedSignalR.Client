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

    private readonly string[] _guids = new[] {
        "b2f626e5-b4d4-4713-891d-f6cb107e502e",
        "22733524-2087-4701-a586-c6bf0ce36f74",
        "b89324bf-daf2-422a-85f2-6843b9c09b6a",
        "779769d1-0aee-4dba-82c7-9e1044836d75"
    };

    private readonly string[] _dateTimes = new[] {
        "2017-04-17",
        "2018-05-25",
        "2019-03-31",
        "2022-02-06",
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

        for (int i = 0; i < _guids.Length; i++)
        {
            await this.Clients.Caller.ReceiveCustomMessage(new UserDefinedType() { Guid = Guid.Parse(_guids[i]), DateTime = DateTime.Parse(_dateTimes[i]) });
        }
    }
}
