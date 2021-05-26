using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client
{
    public static class AnalysisUtility
    {
        public static IReadOnlyList<MethodInfo> ExtractHubMethods(ITypeSymbol hubTypeSymbol, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericsTaskSymbol)
        {
            var hubMethods = new List<MethodInfo>();
            foreach (ISymbol symbol in hubTypeSymbol.GetMembers())
            {
                if (symbol is IMethodSymbol methodSymbol)
                {
                    var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                    INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // Task or Task<T>

                    if (returnTypeSymbol is null)
                    {
                        throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task or Task<T>");
                    }

                    ValidateHubReturnType(returnTypeSymbol, methodSymbol, taskSymbol, genericsTaskSymbol);

                    ITypeSymbol? genericArg = returnTypeSymbol.IsGenericType ? returnTypeSymbol.TypeArguments[0] : null;

                    var methodInfo = new MethodInfo(
                        methodSymbol.Name,
                        methodSymbol.ReturnType.ToDisplayString(),
                        parameters,
                        returnTypeSymbol.IsGenericType,
                        genericArg?.ToDisplayString());

                    hubMethods.Add(methodInfo);
                }
                else
                {
                    throw new Exception($"Define only methods in the interface. {symbol.ToDisplayString()} is not method.");
                }
            }

            return hubMethods;
        }

        public static IReadOnlyList<MethodInfo> ExtractClientMethods(ITypeSymbol clientTypeSymbol, INamedTypeSymbol taskSymbol)
        {
            var clientMethods = new List<MethodInfo>();
            foreach (ISymbol symbol in clientTypeSymbol.GetMembers())
            {
                if (symbol is IMethodSymbol methodSymbol)
                {
                    INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // only Task. not Task<T>.

                    if (returnTypeSymbol is null)
                    {
                        throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task.");
                    }

                    ValidateClientReturnType(returnTypeSymbol, methodSymbol, taskSymbol);

                    var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                    var methodInfo = new MethodInfo(methodSymbol.Name, methodSymbol.ReturnType.ToDisplayString(), parameters, false, null);
                    clientMethods.Add(methodInfo);
                }
                else
                {
                    throw new Exception($"Define only methods in the interface. {symbol.ToDisplayString()} is not method.");
                }
            }

            return clientMethods;
        }

        private static void ValidateHubReturnType(INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericsTaskSymbol)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                if (returnTypeSymbol.IsUnboundGenericType || !returnTypeSymbol.OriginalDefinition.Equals(genericsTaskSymbol, SymbolEqualityComparer.Default))
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task or Task<T>.");
                }
            }
            else
            {
                if (!returnTypeSymbol.Equals(taskSymbol, SymbolEqualityComparer.Default))
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task or Task<T>.");
                }
            }
        }

        private static void ValidateClientReturnType(INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol, INamedTypeSymbol taskSymbol)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task.");
            }
            else
            {
                if (!returnTypeSymbol.Equals(taskSymbol, SymbolEqualityComparer.Default))
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task.");
                }
            }
        }

        public static bool Any(this List<InvokerInfo> source, ITypeSymbol typeSymbol)
        {
            foreach(var item in source)
            {
                if(item.TypeSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Any(this List<ReceiverInfo> source, ITypeSymbol typeSymbol)
        {
            foreach (var item in source)
            {
                if (item.TypeSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }

            return false;
        }
    }
}