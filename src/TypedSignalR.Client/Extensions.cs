using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client
{
    public static class Extensions
    {
        public static bool Any(this List<HubProxyTypeInfo> source, ITypeSymbol typeSymbol)
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

        public static bool Any(this List<ReceiverTypeInfo> source, ITypeSymbol typeSymbol)
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
}
