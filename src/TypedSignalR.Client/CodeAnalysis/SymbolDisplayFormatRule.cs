using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

internal static class SymbolDisplayFormatRule
{
    public static SymbolDisplayFormat FullyQualifiedFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat;

    public static SymbolDisplayFormat FullyQualifiedNullableFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}
