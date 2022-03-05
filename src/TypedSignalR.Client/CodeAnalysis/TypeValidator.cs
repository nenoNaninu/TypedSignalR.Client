using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public static class TypeValidator
{
    public static bool ValidateHubTypeRule(
        SourceProductionContext context,
        ITypeSymbol hubTypeSymbol,
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericTaskSymbol,
        Location accessLocation)
    {
        if (hubTypeSymbol.TypeKind is not TypeKind.Interface)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.TypeArgumentRule,
                accessLocation,
                "CreateHubProxy",
                hubTypeSymbol.ToDisplayString())); ;

            return false;
        }

        bool isValid = true;

        foreach (ISymbol memberSymbol in hubTypeSymbol.GetMembers())
        {
            if (memberSymbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
                {
                    isValid = false;
                    continue;
                }

                if (methodSymbol.MethodKind is MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise)
                {
                    isValid = false;
                    continue;
                }

                if (methodSymbol.MethodKind is not MethodKind.Ordinary)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorItems.InterfaceDefineRule,
                        accessLocation,
                        "hub proxy",
                        memberSymbol.ToDisplayString()));

                    isValid = false;
                    continue;
                }

                INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // Task or Task<T>

                if (returnTypeSymbol is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorItems.HubMethodReturnTypeRule,
                        accessLocation,
                        methodSymbol.ToDisplayString()));

                    isValid = false;
                    continue;
                }

                if (!ValidateHubMethodReturnTypeRule(context, returnTypeSymbol, methodSymbol, taskSymbol, genericTaskSymbol, accessLocation))
                {
                    isValid = false;
                    continue;
                }
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.InterfaceDefineRule,
                    accessLocation,
                    "hub proxy",
                    memberSymbol.ToDisplayString()));

                isValid = false;
                continue;
            }
        }

        return isValid;
    }

    public static bool ValidateReceiverTypeRule(
        SourceProductionContext context,
        ITypeSymbol receiverTypeSymbol,
        INamedTypeSymbol taskSymbol,
        Location accessLocation)
    {
        if (receiverTypeSymbol.TypeKind is not TypeKind.Interface)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.TypeArgumentRule,
                accessLocation,
                "Register",
                receiverTypeSymbol.ToDisplayString()));

            return false;
        }

        bool isValid = true;

        foreach (ISymbol memberSymbol in receiverTypeSymbol.GetMembers())
        {
            if (memberSymbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
                {
                    isValid = false;
                    continue;
                }

                if (methodSymbol.MethodKind is MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise)
                {
                    isValid = false;
                    continue;
                }

                if (methodSymbol.MethodKind is not MethodKind.Ordinary)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorItems.InterfaceDefineRule,
                        accessLocation,
                        "receiver",
                        memberSymbol.ToDisplayString()));

                    isValid = false;
                    continue;
                }

                INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // only Task. not Task<T>.

                if (returnTypeSymbol is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
                        accessLocation,
                        methodSymbol.ToDisplayString()));

                    isValid = false;
                    continue;
                }

                if (!ValidateReceiverMethodReturnTypeRule(context, returnTypeSymbol, methodSymbol, taskSymbol, accessLocation))
                {
                    isValid = false;
                    continue;
                }
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.InterfaceDefineRule,
                    accessLocation,
                    "receiver",
                    memberSymbol.ToDisplayString()));

                isValid = false;
                continue;
            }
        }

        return isValid;
    }

    private static bool ValidateHubMethodReturnTypeRule(
        SourceProductionContext context,
        INamedTypeSymbol returnTypeSymbol,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol taskSymbol,
        INamedTypeSymbol genericTaskSymbol,
        Location accessLocation)
    {
        if (returnTypeSymbol.IsGenericType)
        {
            if (returnTypeSymbol.IsUnboundGenericType || !SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, genericTaskSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.HubMethodReturnTypeRule,
                    accessLocation,
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
                    accessLocation,
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
        Location accessLocation)
    {
        if (returnTypeSymbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
                accessLocation,
                methodSymbol.ToDisplayString()));

            return false;
        }
        else
        {
            if (!SymbolEqualityComparer.Default.Equals(returnTypeSymbol, taskSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
                    accessLocation,
                    methodSymbol.ToDisplayString()));

                return false;
            }
        }

        return true;
    }
}
