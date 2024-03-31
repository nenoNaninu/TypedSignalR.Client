using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

internal static class SymbolDisplayFormatRule
{
    public static SymbolDisplayFormat Default { get; } = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}
