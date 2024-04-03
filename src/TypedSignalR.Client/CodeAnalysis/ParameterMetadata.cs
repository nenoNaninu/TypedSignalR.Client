using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TypedSignalR.Client.CodeAnalysis;

public readonly record struct ParameterMetadata
{
    public readonly IParameterSymbol ParameterSymbol;
    public readonly string Name;
    public readonly string TypeName;

    public ITypeSymbol Type => ParameterSymbol.Type;
    public string FullyQualifiedTypeName => ParameterSymbol.Type.ToDisplayString(SymbolDisplayFormatRule.FullyQualifiedFormat);

    public ParameterMetadata(IParameterSymbol parameterSymbol)
    {
        ParameterSymbol = parameterSymbol;

        Name = SyntaxFacts.IsReservedKeyword(SyntaxFacts.GetKeywordKind(parameterSymbol.Name))
            ? $"@{parameterSymbol.Name}"
            : parameterSymbol.Name;

        TypeName = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormatRule.FullyQualifiedNullableReferenceTypeFormat);
    }
}
