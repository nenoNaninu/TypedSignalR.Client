using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TypedSignalR.Client
{
    public static class AnalysisUtility
    {
        public static IReadOnlyList<MethodInfo> ExtractHubMethods(GeneratorExecutionContext context, ITypeSymbol hubTypeSymbol, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericsTaskSymbol)
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
                        var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptorCollection.HubMethodReturnValueTypeRule,
                            location,
                            methodSymbol.ToDisplayString()));

                        throw new Exception($"Return type of {methodSymbol.ToDisplayString()} must be Task or Task<T>");
                    }

                    ValidateHubReturnType(context, returnTypeSymbol, methodSymbol, taskSymbol, genericsTaskSymbol);

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
                    var location = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.InterfaceDefineRule,
                        location,
                        symbol.ToDisplayString()));

                    throw new Exception($"Only define methods in the interface. {symbol.ToDisplayString()} is not method.");
                }
            }

            return hubMethods;
        }

        public static IReadOnlyList<MethodInfo> ExtractClientMethods(GeneratorExecutionContext context, ITypeSymbol clientTypeSymbol, INamedTypeSymbol taskSymbol)
        {
            var clientMethods = new List<MethodInfo>();
            foreach (ISymbol symbol in clientTypeSymbol.GetMembers())
            {
                if (symbol is IMethodSymbol methodSymbol)
                {
                    INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // only Task. not Task<T>.

                    if (returnTypeSymbol is null)
                    {
                        var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptorCollection.ReceiverMethodReturnValueTypeRule,
                            location,
                            methodSymbol.ToDisplayString()));

                        throw new Exception($"Return value type of {methodSymbol.ToDisplayString()} must be Task.");
                    }

                    ValidateClientReturnType(context, returnTypeSymbol, methodSymbol, taskSymbol);

                    var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                    var methodInfo = new MethodInfo(methodSymbol.Name, methodSymbol.ReturnType.ToDisplayString(), parameters, false, null);

                    clientMethods.Add(methodInfo);
                }
                else
                {
                    var location = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.InterfaceDefineRule,
                        location,
                        symbol.ToDisplayString()));

                    throw new Exception($"Only define methods in the interface. {symbol.ToDisplayString()} is not method.");
                }
            }

            return clientMethods;
        }

        private static void ValidateHubReturnType(GeneratorExecutionContext context, INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericsTaskSymbol)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                if (returnTypeSymbol.IsUnboundGenericType || !returnTypeSymbol.OriginalDefinition.Equals(genericsTaskSymbol, SymbolEqualityComparer.Default))
                {
                    var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.HubMethodReturnValueTypeRule,
                        location,
                        methodSymbol.ToDisplayString()));

                    throw new Exception($"Return value type of {methodSymbol.ToDisplayString()} must be Task or Task<T>.");
                }
            }
            else
            {
                if (!returnTypeSymbol.Equals(taskSymbol, SymbolEqualityComparer.Default))
                {
                    var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.HubMethodReturnValueTypeRule,
                        location,
                        methodSymbol.ToDisplayString()));

                    throw new Exception($"Return value type of {methodSymbol.ToDisplayString()} must be Task or Task<T>.");
                }
            }
        }

        private static void ValidateClientReturnType(GeneratorExecutionContext context, INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol, INamedTypeSymbol taskSymbol)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorCollection.ReceiverMethodReturnValueTypeRule,
                    location,
                    methodSymbol.ToDisplayString()));

                throw new Exception($"Return value type of {methodSymbol.ToDisplayString()} must be Task.");
            }
            else
            {
                if (!returnTypeSymbol.Equals(taskSymbol, SymbolEqualityComparer.Default))
                {

                    var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.ReceiverMethodReturnValueTypeRule,
                        location,
                        methodSymbol.ToDisplayString()));

                    throw new Exception($"Return value type of {methodSymbol.ToDisplayString()} must be Task.");
                }
            }
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