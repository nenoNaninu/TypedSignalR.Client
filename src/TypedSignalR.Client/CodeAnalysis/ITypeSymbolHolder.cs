using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public interface ITypeSymbolHolder
{
    ITypeSymbol TypeSymbol { get; }
}
