using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public class SpecialSymbols
{
    public readonly INamedTypeSymbol TaskSymbol;
    public readonly INamedTypeSymbol GenericTaskSymbol;
    public readonly INamedTypeSymbol CancellationTokenSymbol;
    public readonly INamedTypeSymbol AsyncEnumerableSymbol;
    public readonly INamedTypeSymbol ChannelReaderSymbol;
    public readonly ImmutableArray<INamedTypeSymbol> HubConnectionObserverSymbols;
    public readonly ImmutableArray<IMethodSymbol> CreateHubProxyMethodSymbols;
    public readonly ImmutableArray<IMethodSymbol> RegisterMethodSymbols;

    public SpecialSymbols(
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericTaskSymbol,
        INamedTypeSymbol cancellationTokenSymbol,
        INamedTypeSymbol asyncEnumerableSymbol,
        INamedTypeSymbol channelReaderSymbol,
        ImmutableArray<INamedTypeSymbol> hubConnectionObserverSymbols,
        ImmutableArray<IMethodSymbol> createHubProxyMethodSymbols,
        ImmutableArray<IMethodSymbol> registerMethodSymbols)
    {
        TaskSymbol = taskSymbol;
        GenericTaskSymbol = genericTaskSymbol;
        CancellationTokenSymbol = cancellationTokenSymbol;
        AsyncEnumerableSymbol = asyncEnumerableSymbol;
        ChannelReaderSymbol = channelReaderSymbol;
        HubConnectionObserverSymbols = hubConnectionObserverSymbols;
        CreateHubProxyMethodSymbols = createHubProxyMethodSymbols;
        RegisterMethodSymbols = registerMethodSymbols;
    }
}
