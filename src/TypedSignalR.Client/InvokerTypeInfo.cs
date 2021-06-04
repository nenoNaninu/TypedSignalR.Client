using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace TypedSignalR.Client
{
    public class InvokerTypeInfo : IEquatable<InvokerTypeInfo>
    {
        public ITypeSymbol TypeSymbol { get; }
        public string InterfaceName { get; }
        public string InterfaceFullName { get; }
        public string CollisionFreeName { get; }
        public IReadOnlyList<MethodInfo> HubMethods { get; }

        public InvokerTypeInfo(ITypeSymbol typeSymbol, IReadOnlyList<MethodInfo> hubMethods)
        {
            TypeSymbol = typeSymbol;
            InterfaceName = typeSymbol.Name;
            InterfaceFullName = typeSymbol.ToDisplayString();
            CollisionFreeName = InterfaceFullName.Replace(".", null);
            HubMethods = hubMethods;
        }

#pragma warning disable RS1024 
        public override int GetHashCode() => TypeSymbol.GetHashCode();
#pragma warning restore RS1024

        public bool Equals(InvokerTypeInfo other)
        {
            return TypeSymbol.Equals(other.TypeSymbol, SymbolEqualityComparer.Default);
        }

        public override bool Equals(object other)
        {
            if (other is InvokerTypeInfo invokerInfo)
            {
                return this.Equals(invokerInfo);
            }

            return false;
        }
    }
}