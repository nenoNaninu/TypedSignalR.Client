using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public static class TypeValidator
{
    public static bool ValidateHubTypeRule(
        SourceProductionContext context,
        ITypeSymbol hubTypeSymbol,
        SpecialSymbols specialSymbols,
        Location accessLocation)
    {
        if (hubTypeSymbol.TypeKind is not TypeKind.Interface)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.TypeArgumentRule,
                accessLocation,
                "CreateHubProxy",
                hubTypeSymbol.ToDisplayString()));

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

                if (!ValidateHubMethodReturnTypeRule(context, methodSymbol, returnTypeSymbol, specialSymbols, accessLocation))
                {
                    isValid = false;
                    continue;
                }

                if (!ValidateHubMethodCancellationTokenParameterRule(context, methodSymbol, returnTypeSymbol, specialSymbols, accessLocation))
                {
                    isValid = false;
                    continue;
                }

                if (!ValidateStreamingMethodRule(context, methodSymbol, returnTypeSymbol, specialSymbols, accessLocation))
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
        SpecialSymbols specialSymbols,
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

                // ReturnType
                //     Task : ordinary receiver method
                //     Task<T> : client results
                INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol;

                if (returnTypeSymbol is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
                        accessLocation,
                        methodSymbol.ToDisplayString()));

                    isValid = false;
                    continue;
                }

                if (!ValidateReceiverMethodReturnTypeRule(context, methodSymbol, returnTypeSymbol, specialSymbols, accessLocation))
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
        IMethodSymbol methodSymbol,
        INamedTypeSymbol returnTypeSymbol,
        SpecialSymbols specialSymbols,
        Location accessLocation)
    {
        if (returnTypeSymbol.IsGenericType)
        {
            if (returnTypeSymbol.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.HubMethodReturnTypeRule,
                    accessLocation,
                    methodSymbol.ToDisplayString()));

                return false;
            }

            // Task<T>
            if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
            {
                return true;
            }

            // IAsyncEnumerable<T>
            if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
            {
                return true;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.HubMethodReturnTypeRule,
                accessLocation,
                methodSymbol.ToDisplayString()));
        }
        else
        {
            // Task
            if (!SymbolEqualityComparer.Default.Equals(returnTypeSymbol, specialSymbols.TaskSymbol))
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
        IMethodSymbol methodSymbol,
        INamedTypeSymbol returnTypeSymbol,
        SpecialSymbols specialSymbols,
        Location accessLocation)
    {
        // Task
        if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol, specialSymbols.TaskSymbol))
        {
            return true;
        }

        // Task<T>
        if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            return true;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptorItems.ReceiverMethodReturnTypeRule,
            accessLocation,
            methodSymbol.ToDisplayString()));

        return false;
    }

    private static bool ValidateHubMethodCancellationTokenParameterRule(
        SourceProductionContext context,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol returnTypeSymbol,
        SpecialSymbols specialSymbols,
        Location accessLocation)
    {
        var parameters = methodSymbol.Parameters;

        if (parameters.Length == 0)
        {
            return true;
        }

        var count = parameters.Count(x => SymbolEqualityComparer.Default.Equals(x.Type, specialSymbols.CancellationTokenSymbol));

        if (count == 0)
        {
            return true;
        }

        if (count == 1)
        {
            // return type: Task<T>
            if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
            {
                var typeArgument = returnTypeSymbol.TypeArguments[0];

                // return type: Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
                if (SymbolEqualityComparer.Default.Equals(typeArgument.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                    || SymbolEqualityComparer.Default.Equals(typeArgument.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
                {
                    return true;
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                         DiagnosticDescriptorItems.HubMethodCancellationTokenParameterRule,
                         accessLocation,
                         methodSymbol.ToDisplayString()));

                    return false;
                }
            }
            // return type: IAsyncEnumerable<T>
            else if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
            {
                return true;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.HubMethodCancellationTokenParameterRule,
                accessLocation,
                methodSymbol.ToDisplayString()));

            return false;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptorItems.HubMethodMultipleCancellationTokenParameterRule,
            accessLocation,
            methodSymbol.ToDisplayString()));

        return false;
    }

    private static bool ValidateStreamingMethodRule(
        SourceProductionContext context,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol returnTypeSymbol,
        SpecialSymbols specialSymbols,
        Location accessLocation)
    {
        var isValid = true;

        // server streaming
        // return type: IAsyncEnumerable<T>, Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
        // parameters restriction: cannot use IAsyncEnumerable<T>, ChannelReader<T>

        // return type: Task<T>
        if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            var typeArgument = returnTypeSymbol.TypeArguments[0];

            // return type: Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
            if (SymbolEqualityComparer.Default.Equals(typeArgument.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(typeArgument.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
            {
                var contain = methodSymbol.Parameters.Any(x =>
                    SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                    || SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.ChannelReaderSymbol));

                if (contain)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorItems.ServerStreamingMethodParameterRule,
                        accessLocation,
                        methodSymbol.ToDisplayString()));

                    isValid = false;
                }
            }
        }
        // return type: IAsyncEnumerable<T>
        else if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
        {
            var contain = methodSymbol.Parameters.Any(x =>
                SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.ChannelReaderSymbol));

            if (contain)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.ServerStreamingMethodParameterRule,
                    accessLocation,
                    methodSymbol.ToDisplayString()));

                isValid = false;
            }
        }

        // client streaming
        // return type: Task
        // parameter: IAsyncEnumerable<T>, ChannelReader<T>
        // parameter restriction: cannot use CancellationToken --> ValidateHubMethodCancellationTokenParameterRule

        var isClientStreaming = methodSymbol.Parameters.Any(x =>
            SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
            || SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.ChannelReaderSymbol));

        if (isClientStreaming)
        {
            if (!SymbolEqualityComparer.Default.Equals(returnTypeSymbol, specialSymbols.TaskSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.ClientStreamingMethodReturnTypeRule,
                    accessLocation,
                    methodSymbol.ToDisplayString()));

                isValid = false;
            }
        }

        return isValid;
    }
}
