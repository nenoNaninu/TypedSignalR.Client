using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Hubs;

public class InheritTestHub : Hub, IInheritHub
{
    private readonly ILogger<UnaryHub> _logger;

    public InheritTestHub(ILogger<UnaryHub> logger)
    {
        _logger = logger;
    }

    public Task<int> Add(int x, int y)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Add");

        return Task.FromResult(x + y);
    }

    public Task<string> Cat(string x, string y)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Cat");

        return Task.FromResult(x + y);
    }

    public Task<UserDefinedType> Echo(UserDefinedType instance)
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Echo");

        return Task.FromResult(instance);
    }

    public Task<string> Get()
    {
        _logger.Log(LogLevel.Information, "UnaryHub.Get");

        return Task.FromResult("TypedSignalR.Client");
    }
}
