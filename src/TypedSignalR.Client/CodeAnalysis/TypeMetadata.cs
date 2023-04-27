using System.Collections.Generic;
using System.Linq;
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

        Methods = GetMethods(typeSymbol);

        InterfaceName = typeSymbol.Name;
        InterfaceFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        CollisionFreeName = InterfaceFullName.Replace('.', '_').Replace(':', '_');
    }

    private static IReadOnlyList<MethodMetadata> GetMethods(ITypeSymbol typeSymbol)
    {
        var allInterfaces = typeSymbol.AllInterfaces;

        var methods = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(static x => x.MethodKind is MethodKind.Ordinary)
            .Select(static x => new MethodMetadata(x));

        if (allInterfaces.IsEmpty)
        {
            return methods.ToArray();
        }

        var allMethods = allInterfaces
            .SelectMany(static x => x.GetMembers())
            .OfType<IMethodSymbol>()
            .Where(static x => x.MethodKind is MethodKind.Ordinary)
            .Select(static x => new MethodMetadata(x))
            .Concat(methods);

        return allMethods.ToArray();
    }
}
