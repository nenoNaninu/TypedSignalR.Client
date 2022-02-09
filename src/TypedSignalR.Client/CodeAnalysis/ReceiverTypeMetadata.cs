using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public sealed class ReceiverTypeMetadata
{
    public ITypeSymbol TypeSymbol { get; }

    public IReadOnlyList<MethodMetadata> Methods { get; }

    public string InterfaceName { get; }
    public string InterfaceFullName { get; }
    public string CollisionFreeName { get; }

    public ReceiverTypeMetadata(ITypeSymbol typeSymbol, IReadOnlyList<MethodMetadata> methods)
    {
        TypeSymbol = typeSymbol;
        Methods = methods;

        InterfaceName = typeSymbol.Name;
        InterfaceFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        CollisionFreeName = InterfaceFullName.Replace(".", "_").Replace(":", "_");
    }
}
