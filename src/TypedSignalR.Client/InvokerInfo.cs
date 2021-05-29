using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace TypedSignalR.Client
{
    public class InvokerInfo : IEquatable<InvokerInfo>
    {
        public ITypeSymbol TypeSymbol { get; }
        public string InterfaceName { get; }
        public string InterfaceFullName { get; }
        public IReadOnlyList<MethodInfo> HubMethods { get; }

        public InvokerInfo(ITypeSymbol typeSymbol, string interfaceName, string interfaceFullName, IReadOnlyList<MethodInfo> hubMethods)
        {
            TypeSymbol = typeSymbol;
            InterfaceName = interfaceName;
            InterfaceFullName = interfaceFullName;
            HubMethods = hubMethods;
        }

#pragma warning disable RS1024 
        public override int GetHashCode() => TypeSymbol.GetHashCode();
#pragma warning restore RS1024

        public bool Equals(InvokerInfo other)
        {
            return TypeSymbol.Equals(other.TypeSymbol, SymbolEqualityComparer.Default);
        }

        public override bool Equals(object other)
        {
            if(other is InvokerInfo invokerInfo)
            {
                return this.Equals(invokerInfo);
            }

            return false;
        }
    }
}