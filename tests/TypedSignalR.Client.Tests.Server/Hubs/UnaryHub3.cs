using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Hubs;

public class UnaryHub3 : Hub, IUnaryHub3
{
    private readonly ILogger<UnaryHub3> _logger;

    public UnaryHub3(ILogger<UnaryHub3> logger)
    {
        _logger = logger;
    }

    public Task<int> Add(int x, int y)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Add");

        return Task.FromResult(x + y);
    }
}
