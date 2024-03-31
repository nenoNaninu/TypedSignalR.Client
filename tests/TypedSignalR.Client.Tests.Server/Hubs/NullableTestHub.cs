using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Hubs;

public class NullableTestHub : Hub, INullableTestHub
{
    public Task<int> GetStruct(int message)
    {
        return Task.FromResult(message + 7);
    }

    public Task<int?> GetNullableStruct(int? message)
    {
        if (message is null)
        {
            return Task.FromResult<int?>(null);
        }

        return Task.FromResult<int?>(message.Value + 99);
    }

    public Task<string> GetReferenceType(string message)
    {
        return Task.FromResult(message + "7");
    }

    public Task<string?> GetNullableReferenceType(string? message)
    {
        if (message is null)
        {
            return Task.FromResult<string?>(null);
        }

        return Task.FromResult<string?>(message + "99");
    }
}
