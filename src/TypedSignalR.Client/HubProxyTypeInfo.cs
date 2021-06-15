using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace TypedSignalR.Client
{
    public class HubProxyTypeInfo : IEquatable<HubProxyTypeInfo>
    {
        public ITypeSymbol TypeSymbol { get; }
        public string InterfaceName { get; }
        public string InterfaceFullName { get; }
        public string CollisionFreeName { get; }
        public IReadOnlyList<MethodInfo> Methods { get; }

        public HubProxyTypeInfo(ITypeSymbol typeSymbol, IReadOnlyList<MethodInfo> methods)
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

        public bool Equals(HubProxyTypeInfo other)
        {
            return TypeSymbol.Equals(other.TypeSymbol, SymbolEqualityComparer.Default);
        }

        public override bool Equals(object other)
        {
            if (other is HubProxyTypeInfo invokerInfo)
            {
                return this.Equals(invokerInfo);
            }

            return false;
        }
    }
}