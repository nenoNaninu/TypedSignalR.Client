; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 3.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
TSRC001 |  Usage   |  Error   | The type argument must be an interface.
TSRC002 |  Usage   |  Error   | Only define methods in the interface.
TSRC003 |  Usage   |  Error   | The return type of methods in the interface must be Task or Task<T>.
TSRC004 |  Usage   |  Error   | The return type of methods in the interface must be Task.
