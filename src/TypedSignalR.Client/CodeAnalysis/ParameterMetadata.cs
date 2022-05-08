using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public readonly struct ParameterMetadata
{
    public readonly string Name;
    public readonly string TypeName;

    public ParameterMetadata(IParameterSymbol parameterSymbol)
    {
        Name = parameterSymbol.Name;
        TypeName = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
