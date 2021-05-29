; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.2

### New Rules

Rule ID       | Category | Severity | Notes
--------------|----------|----------|--------------------
TypedSiRCA001 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.001: Type argument must be an interface.
TypedSiRCA002 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.002: Only define methods in the interface used for HubProxy/Receiver/HubClientBase.
TypedSiRCA003 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.003: The return type of the method in the interface used for Hub/HubProxy must be Task or Task<T>.
TypedSiRCA004 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.004: The return type of the method in the interface used for Receiver/Client-side must be Task.
TypedSiRCA005 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.005: Argument of HubClientBaseAttribute must be an interface.