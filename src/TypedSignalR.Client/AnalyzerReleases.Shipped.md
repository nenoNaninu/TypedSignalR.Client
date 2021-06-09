; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 2.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
TypedSiRCA001 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.001: Type argument of the CreateHubProxy/CreateHubProxyWith/Register method must be an interface.
TypedSiRCA002 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.002: Only define methods in the interface used for HubProxy/Receiver.
TypedSiRCA003 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.003: The return type of the method in the interface used for HubProxy must be Task or Task<T>.
TypedSiRCA004 |  Usage   |  Error   | TypedSignalR.Client.Analyzer.004: The return type of the method in the interface used for Receiver must be Task.
