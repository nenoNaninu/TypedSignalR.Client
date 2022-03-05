using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public sealed class TypeMetadata : ITypeSymbolHolder
{
    public ITypeSymbol TypeSymbol { get; }

    public IReadOnlyList<MethodMetadata> Methods { get; }

    public string InterfaceName { get; }
    public string InterfaceFullName { get; }
    public string CollisionFreeName { get; }

    public TypeMetadata(ITypeSymbol typeSymbol)
    {
        TypeSymbol = typeSymbol;

        Methods = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(static x => x.MethodKind is MethodKind.Ordinary)
            .Select(static x => new MethodMetadata(x))
            .ToArray();

        InterfaceName = typeSymbol.Name;
        InterfaceFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        CollisionFreeName = InterfaceFullName.Replace('.', '_').Replace(':', '_');
    }
}
