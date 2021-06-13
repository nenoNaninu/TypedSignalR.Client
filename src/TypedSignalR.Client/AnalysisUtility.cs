using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace TypedSignalR.Client
{
    public static class AnalysisUtility
    {
        public static (IReadOnlyList<MethodInfo> Methods, bool IsValid) ExtractHubMethods(GeneratorExecutionContext context, ITypeSymbol hubTypeSymbol, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericsTaskSymbol, Location memberAccessLocation)
        {
            var hubMethods = new List<MethodInfo>();
            bool isValid = true;

            foreach (ISymbol symbol in hubTypeSymbol.GetMembers())
            {
                if (symbol is IMethodSymbol methodSymbol)
                {
                    if (methodSymbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
                    {
                        continue;
                    }

                    var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                    INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // Task or Task<T>

                    if (returnTypeSymbol is null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptorCollection.HubMethodReturnTypeRule,
                            memberAccessLocation,
                            methodSymbol.ToDisplayString()));

                        isValid = false;
                        continue;
                    }

                    if (!ValidateHubMethodReturnTypeRule(context, returnTypeSymbol, methodSymbol, taskSymbol, genericsTaskSymbol, memberAccessLocation))
                    {
                        isValid = false;
                        continue;
                    }

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
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.InterfaceDefineRule,
                        memberAccessLocation,
                        symbol.ToDisplayString()));
                    
                    isValid = false;
                    continue;
                }
            }

            return (hubMethods, isValid);
        }

        public static (IReadOnlyList<MethodInfo> Methods, bool IsValid) ExtractClientMethods(GeneratorExecutionContext context, ITypeSymbol clientTypeSymbol, INamedTypeSymbol taskSymbol, Location memberAccessLocation)
        {
            var clientMethods = new List<MethodInfo>();
            bool isValid = true;

            foreach (ISymbol symbol in clientTypeSymbol.GetMembers())
            {
                if (symbol is IMethodSymbol methodSymbol)
                {
                    if (methodSymbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
                    {
                        continue;
                    }

                    INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // only Task. not Task<T>.

                    if (returnTypeSymbol is null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptorCollection.ReceiverMethodReturnTypeRule,
                            memberAccessLocation,
                            methodSymbol.ToDisplayString()));

                        isValid = false;
                        continue;
                    }

                    if (!ValidateClientMethodReturnTypeRule(context, returnTypeSymbol, methodSymbol, taskSymbol, memberAccessLocation))
                    {
                        isValid = false;
                        continue;
                    }

                    var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                    var methodInfo = new MethodInfo(methodSymbol.Name, methodSymbol.ReturnType.ToDisplayString(), parameters, false, null);

                    clientMethods.Add(methodInfo);
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.InterfaceDefineRule,
                        memberAccessLocation,
                        symbol.ToDisplayString()));

                    isValid = false;
                    continue;
                }
            }

            return (clientMethods, isValid);
        }

        private static bool ValidateHubMethodReturnTypeRule(GeneratorExecutionContext context, INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericsTaskSymbol, Location memberAccessLocation)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                if (returnTypeSymbol.IsUnboundGenericType || !returnTypeSymbol.OriginalDefinition.Equals(genericsTaskSymbol, SymbolEqualityComparer.Default))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.HubMethodReturnTypeRule,
                        memberAccessLocation,
                        methodSymbol.ToDisplayString()));

                    return false;
                }
            }
            else
            {
                if (!returnTypeSymbol.Equals(taskSymbol, SymbolEqualityComparer.Default))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.HubMethodReturnTypeRule,
                        memberAccessLocation,
                        methodSymbol.ToDisplayString()));

                    return false;
                }
            }

            return true;
        }

        private static bool ValidateClientMethodReturnTypeRule(GeneratorExecutionContext context, INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol, INamedTypeSymbol taskSymbol, Location memberAccessLocation)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorCollection.ReceiverMethodReturnTypeRule,
                    memberAccessLocation,
                    methodSymbol.ToDisplayString()));

                return false;
            }
            else
            {
                if (!returnTypeSymbol.Equals(taskSymbol, SymbolEqualityComparer.Default))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.ReceiverMethodReturnTypeRule,
                        memberAccessLocation,
                        methodSymbol.ToDisplayString()));

                    return false;
                }
            }

            return true;
        }

        public static bool Any(this List<InvokerTypeInfo> source, ITypeSymbol typeSymbol)
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

        public static bool Any(this List<ReceiverTypeInfo> source, ITypeSymbol typeSymbol)
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