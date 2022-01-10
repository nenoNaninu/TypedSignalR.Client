using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public static class MetadataListExtensions
{
    public static bool Any(this List<HubProxyTypeMetadata> source, ITypeSymbol typeSymbol)
    {
        foreach (var item in source)
        {
            if (item.TypeSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }

    public static bool Any(this List<ReceiverTypeMetadata> source, ITypeSymbol typeSymbol)
    {
        foreach (var item in source)
        {
            if (item.TypeSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }
}
