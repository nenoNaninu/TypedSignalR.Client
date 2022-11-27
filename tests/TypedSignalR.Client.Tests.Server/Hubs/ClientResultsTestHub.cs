using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Hubs;

public class ClientResultsTestHub : Hub<IClientResultsTestHubReceiver>, IClientResultsTestHub
{
    private readonly ILogger<ClientResultsTestHub> _logger;

    public ClientResultsTestHub(ILogger<ClientResultsTestHub> logger)
    {
        _logger = logger;
    }

    public async Task<bool> StartTest()
    {
        _logger.Log(LogLevel.Information, "StartTest");

        var guid = await this.Clients.Caller.GetGuidFromClient();

        _logger.Log(LogLevel.Information, "guid {guid}", guid);

        var ans = Guid.Parse("ba3088bb-e7ea-4924-b01b-695e879bb166");

        if (guid != ans)
        {
            return false;
        }

        _logger.Log(LogLevel.Information, "start GetPersonFromClient");

        var person = await this.Clients.Caller.GetPersonFromClient();

        _logger.Log(LogLevel.Information, "person: {person}", person);

        var ans2 = new Person(Guid.Parse("c2368532-2f13-4079-9631-a38a048d84e1"), "Nana Daiba", 7);

        if (person != ans2)
        {
            return false;
        }

        _logger.Log(LogLevel.Information, "start SumInClient");

        var sum = await this.Clients.Caller.SumInClient(7, 99);

        _logger.Log(LogLevel.Information, "sum: {sum}", sum);

        if (sum != 106)
        {
            return false;
        }

        return true;
    }
}
