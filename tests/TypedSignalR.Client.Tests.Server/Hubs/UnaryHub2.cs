using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Hubs;

public class UnaryHub2 : Hub, IUnaryHub2
{
    private readonly ILogger<UnaryHub2> _logger;

    public UnaryHub2(ILogger<UnaryHub2> logger)
    {
        _logger = logger;
    }

    public Task<int> Add(int x, int y)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Add");

        return Task.FromResult(x + y);
    }
}
