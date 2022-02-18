using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public readonly struct MethodParameter
{
    public readonly string Name;
    public readonly string TypeName;

    public MethodParameter(IParameterSymbol parameterSymbol)
    {
        Name = parameterSymbol.Name;
        TypeName = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
