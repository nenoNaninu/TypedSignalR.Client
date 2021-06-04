using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace TypedSignalR.Client
{
    public class ReceiverTypeInfo : IEquatable<ReceiverTypeInfo>
    {
        public ITypeSymbol TypeSymbol { get; }
        public string InterfaceName { get; }
        public string InterfaceFullName { get; }
        public string CollisionFreeName { get; }
        public IReadOnlyList<MethodInfo> ClientMethods { get; }

        public ReceiverTypeInfo(ITypeSymbol typeSymbol, IReadOnlyList<MethodInfo> clientMethods)
        {
            TypeSymbol = typeSymbol;
            InterfaceName = typeSymbol.Name;
            InterfaceFullName = typeSymbol.ToDisplayString();
            CollisionFreeName = InterfaceFullName.Replace(".", null);
            ClientMethods = clientMethods;
        }

#pragma warning disable RS1024
        public override int GetHashCode() => TypeSymbol.GetHashCode();
#pragma warning restore RS1024 

        public bool Equals(ReceiverTypeInfo other)
        {
            return TypeSymbol.Equals(other.TypeSymbol, SymbolEqualityComparer.Default);
        }

        public override bool Equals(object other)
        {
            if (other is ReceiverTypeInfo invokerInfo)
            {
                return this.Equals(invokerInfo);
            }

            return false;
        }
    }
}