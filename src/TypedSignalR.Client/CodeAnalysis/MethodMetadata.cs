using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public sealed class MethodMetadata
{
    public string MethodName { get; }

    public IReadOnlyList<ParameterMetadata> Parameters { get; }

    public string ReturnType { get; }
    public bool IsGenericReturnType { get; }
    public string? GenericReturnTypeArgument { get; }

    public MethodMetadata(IMethodSymbol methodSymbol)
    {
        MethodName = methodSymbol.Name;

        Parameters = methodSymbol.Parameters
            .Select(x => new ParameterMetadata(x))
            .ToArray();

        ReturnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol;

        if (returnTypeSymbol is not null)
        {
            IsGenericReturnType = returnTypeSymbol.IsGenericType;

            if (returnTypeSymbol.IsGenericType)
            {
                GenericReturnTypeArgument = returnTypeSymbol.TypeArguments.Length == 1
                    ? returnTypeSymbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    : string.Join(", ", returnTypeSymbol.TypeArguments.Select(x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            }
        }
    }
}
