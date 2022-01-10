using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public sealed class HubProxyTypeMetadata : IEquatable<HubProxyTypeMetadata>
{
    public ITypeSymbol TypeSymbol { get; }
    public string InterfaceName { get; }
    public string InterfaceFullName { get; }
    public string CollisionFreeName { get; }
    public IReadOnlyList<MethodMetadata> Methods { get; }

    public HubProxyTypeMetadata(ITypeSymbol typeSymbol, IReadOnlyList<MethodMetadata> methods)
    {
        TypeSymbol = typeSymbol;
        InterfaceName = typeSymbol.Name;
        InterfaceFullName = typeSymbol.ToDisplayString();
        CollisionFreeName = InterfaceFullName.Replace(".", null);
        Methods = methods;
    }

#pragma warning disable RS1024
    public override int GetHashCode() => TypeSymbol.GetHashCode();
#pragma warning restore RS1024

    public bool Equals(HubProxyTypeMetadata other)
    {
        return TypeSymbol.Equals(other.TypeSymbol, SymbolEqualityComparer.Default);
    }

    public override bool Equals(object other)
    {
        if (other is HubProxyTypeMetadata invokerInfo)
        {
            return this.Equals(invokerInfo);
        }

        return false;
    }
}
