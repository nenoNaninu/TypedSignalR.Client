using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace TypedSignalR.Client
{
    public class ReceiverInfo : IEquatable<ReceiverInfo>
    {
        public ITypeSymbol TypeSymbol { get; }
        public string InterfaceName { get; }
        public string InterfaceFullName { get; }
        public IReadOnlyList<MethodInfo> ClientMethods { get; }

        public ReceiverInfo(ITypeSymbol typeSymbol, string interfaceName, string interfaceFullName, IReadOnlyList<MethodInfo> clientMethods)
        {
            TypeSymbol = typeSymbol;
            InterfaceName = interfaceName;
            InterfaceFullName = interfaceFullName;
            ClientMethods = clientMethods;
        }

        public override int GetHashCode() => TypeSymbol.GetHashCode();

        public bool Equals(ReceiverInfo other)
        {
            return TypeSymbol.Equals(other.TypeSymbol, SymbolEqualityComparer.Default);
        }

        public override bool Equals(object other)
        {
            if (other is ReceiverInfo invokerInfo)
            {
                return this.Equals(invokerInfo);
            }

            return false;
        }
    }
}