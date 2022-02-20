using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public static class MetadataUtilities
{
    public static (IReadOnlyList<MethodMetadata> Methods, bool IsValid) ExtractHubMethods(
        SourceProductionContext context,
        ITypeSymbol hubTypeSymbol,
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericTaskSymbol,
        Location memberAccessLocation)
    {
        var hubMethods = new List<MethodMetadata>();
        bool isValid = true;

        foreach (ISymbol memberSymbol in hubTypeSymbol.GetMembers())
        {
            if (memberSymbol is IMethodSymbol methodSymbol)
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

                if (!ValidateHubMethodReturnTypeRule(context, returnTypeSymbol, methodSymbol, taskSymbol, genericTaskSymbol, memberAccessLocation))
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
                    memberSymbol.ToDisplayString()));

                isValid = false;
                continue;
            }
        }

        return (hubMethods, isValid);
    }

    public static (IReadOnlyList<MethodMetadata> Methods, bool IsValid) ExtractReceiverMethods(
        SourceProductionContext context,
        ITypeSymbol receiverTypeSymbol,
        INamedTypeSymbol taskSymbol,
        Location memberAccessLocation)
    {
        var receiverMethods = new List<MethodMetadata>();
        bool isValid = true;

        foreach (ISymbol memberSymbol in receiverTypeSymbol.GetMembers())
        {
            if (memberSymbol is IMethodSymbol methodSymbol)
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

                if (!ValidateReceiverMethodReturnTypeRule(context, returnTypeSymbol, methodSymbol, taskSymbol, memberAccessLocation))
                {
                    isValid = false;
                    continue;
                }

                var methodMetadata = new MethodMetadata(methodSymbol);

                receiverMethods.Add(methodMetadata);
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.InterfaceDefineRule,
                    memberAccessLocation,
                    "receiver",
                    memberSymbol.ToDisplayString()));

                isValid = false;
                continue;
            }
        }

        return (receiverMethods, isValid);
    }

    private static bool ValidateHubMethodReturnTypeRule(
        SourceProductionContext context,
        INamedTypeSymbol returnTypeSymbol,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericTaskSymbol,
        Location memberAccessLocation)
    {
        if (returnTypeSymbol.IsGenericType)
        {
            if (returnTypeSymbol.IsUnboundGenericType || !SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, genericTaskSymbol))
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

    private static bool ValidateReceiverMethodReturnTypeRule(
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
