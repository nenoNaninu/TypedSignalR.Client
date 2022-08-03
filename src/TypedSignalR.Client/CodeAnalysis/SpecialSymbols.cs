using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public class SpecialSymbols
{
    public readonly INamedTypeSymbol TaskSymbol;
    public readonly INamedTypeSymbol GenericTaskSymbol;
    public readonly INamedTypeSymbol CancellationTokenSymbol;
    public readonly INamedTypeSymbol AsyncEnumerableSymbol;
    public readonly INamedTypeSymbol ChannelReaderSymbol;
    public readonly INamedTypeSymbol HubConnectionObserverSymbol;
    public readonly IMethodSymbol CreateHubProxyMethodSymbol;
    public readonly IMethodSymbol RegisterMethodSymbol;

    public SpecialSymbols(
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericTaskSymbol,
        INamedTypeSymbol cancellationTokenSymbol,
        INamedTypeSymbol asyncEnumerableSymbol,
        INamedTypeSymbol channelReaderSymbol,
        INamedTypeSymbol hubConnectionObserverSymbol,
        IMethodSymbol createHubProxyMethodSymbol,
        IMethodSymbol registerMethodSymbol)
    {
        TaskSymbol = taskSymbol;
        GenericTaskSymbol = genericTaskSymbol;
        CancellationTokenSymbol = cancellationTokenSymbol;
        AsyncEnumerableSymbol = asyncEnumerableSymbol;
        ChannelReaderSymbol = channelReaderSymbol;
        HubConnectionObserverSymbol = hubConnectionObserverSymbol;
        CreateHubProxyMethodSymbol = createHubProxyMethodSymbol;
        RegisterMethodSymbol = registerMethodSymbol;
    }
}
