using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public sealed class MethodMetadata
{
    public string MethodName { get; }

    public IReadOnlyList<MethodParameter> Parameters { get; }

    public string ReturnType { get; }
    public bool IsGenericReturnType { get; }
    public string? GenericReturnTypeArgument { get; }

    public MethodMetadata(IMethodSymbol methodSymbol)
    {
        INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol;

        if (returnTypeSymbol is null)
        {
            throw new ArgumentException("Unable to cast methodSymbol.ReturnType to INamedTypeSymbol");
        }

        MethodName = methodSymbol.Name;

        Parameters = methodSymbol.Parameters
            .Select(x => new MethodParameter(x))
            .ToArray();

        ReturnType = returnTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        IsGenericReturnType = returnTypeSymbol.IsGenericType;
        GenericReturnTypeArgument = returnTypeSymbol.IsGenericType ? returnTypeSymbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : null;
    }
}
