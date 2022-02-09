using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client
{
    public static class DiagnosticDescriptorCollection
    {
        public static readonly DiagnosticDescriptor TypeArgumentRule = new(
            id: "TypedSiRCA001",
            title: "TypedSignalR.Client.Analyzer.001: Type argument of the CreateHubProxy/Register method must be an interface",
            messageFormat: "[Type argument of {0} method must be an interface] {1} is not interface",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Type argument of the CreateHubProxy/Register method must be an interface.");

        public static readonly DiagnosticDescriptor InterfaceDefineRule = new(
            id: "TypedSiRCA002",
            title: "TypedSignalR.Client.Analyzer.002: Only define methods in the interface used for HubProxy/Receiver",
            messageFormat: "[Only define methods in the interface used for HubProxy/Receiver] {0} is not method",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Only define methods in the interface used for HubProxy/Receiver.");

        public static readonly DiagnosticDescriptor HubMethodReturnTypeRule = new(
            id: "TypedSiRCA003",
            title: "TypedSignalR.Client.Analyzer.003: The return type of the method in the interface used for HubProxy must be Task or Task<T>",
            messageFormat: "[The return type of the method in the interface used for HubProxy must be Task or Task<T>] Return type of {0} is not Task or Task<T>",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The return type of the method in the interface used for HubProxy must be Task or Task<T>.");

        public static readonly DiagnosticDescriptor ReceiverMethodReturnTypeRule = new(
            id: "TypedSiRCA004",
            title: "TypedSignalR.Client.Analyzer.004: The return type of the method in the interface used for Receiver must be Task",
            messageFormat: "[The return type of the method in the interface used for Receiver must be Task or void] Return type of {0} is not Task",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The return type of the method in the interface used for Receiver must be Task.");
    }
}
