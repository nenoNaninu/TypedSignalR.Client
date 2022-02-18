using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public static class MetadataUtilities
{
    public static (IReadOnlyList<MethodMetadata> Methods, bool IsValid) ExtractHubMethods(
        SourceProductionContext context,
        ITypeSymbol hubTypeSymbol,
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericsTaskSymbol,
        Location memberAccessLocation)
    {
        var hubMethods = new List<MethodMetadata>();
        bool isValid = true;

        foreach (ISymbol symbol in hubTypeSymbol.GetMembers())
        {
            if (symbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
                {
                    continue;
                }

                INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // Task or Task<T>

                if (returnTypeSymbol is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorItems.HubMethodReturnTypeRule,
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

                var methodMetadata = new MethodMetadata(methodSymbol);

                hubMethods.Add(methodMetadata);
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.InterfaceDefineRule,
                    memberAccessLocation,
                    "hub proxy",
                    symbol.ToDisplayString()));

                isValid = false;
                continue;
            }
        }

        return (hubMethods, isValid);
    }

    public static (IReadOnlyList<MethodMetadata> Methods, bool IsValid) ExtractReceiverMethods(
        SourceProductionContext context,
        ITypeSymbol clientTypeSymbol,
        INamedTypeSymbol taskSymbol,
        Location memberAccessLocation)
    {
        var clientMethods = new List<MethodMetadata>();
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
                        DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
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

                var methodMetadata = new MethodMetadata(methodSymbol);

                clientMethods.Add(methodMetadata);
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.InterfaceDefineRule,
                    memberAccessLocation,
                    "receiver",
                    symbol.ToDisplayString()));

                isValid = false;
                continue;
            }
        }

        return (clientMethods, isValid);
    }

    private static bool ValidateHubMethodReturnTypeRule(
        SourceProductionContext context,
        INamedTypeSymbol returnTypeSymbol,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericsTaskSymbol,
        Location memberAccessLocation)
    {
        if (returnTypeSymbol.IsGenericType)
        {
            if (returnTypeSymbol.IsUnboundGenericType || !SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, genericsTaskSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.HubMethodReturnTypeRule,
                    memberAccessLocation,
                    methodSymbol.ToDisplayString()));

                return false;
            }
        }
        else
        {
            if (!SymbolEqualityComparer.Default.Equals(returnTypeSymbol, taskSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.HubMethodReturnTypeRule,
                    memberAccessLocation,
                    methodSymbol.ToDisplayString()));

                return false;
            }
        }

        return true;
    }

    private static bool ValidateClientMethodReturnTypeRule(
        SourceProductionContext context,
        INamedTypeSymbol returnTypeSymbol,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol taskSymbol,
        Location memberAccessLocation)
    {
        if (returnTypeSymbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
                memberAccessLocation,
                methodSymbol.ToDisplayString()));

            return false;
        }
        else
        {
            if (!SymbolEqualityComparer.Default.Equals(returnTypeSymbol, taskSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
                    memberAccessLocation,
                    methodSymbol.ToDisplayString()));

                return false;
            }
        }

        return true;
    }
}
