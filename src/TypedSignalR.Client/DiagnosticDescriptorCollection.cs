using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client
{
    public static class DiagnosticDescriptorCollection
    {
        public static readonly DiagnosticDescriptor TypeArgumentRule = new(
            id: "TypedSiRCA001",
            title: "TypedSignalR.Client.Analyzer.001: Type argument must be an interface",
            messageFormat: "[Type argument of {0} must be an interface] {1} is not interface",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Type argument must be an interface.");

        public static readonly DiagnosticDescriptor InterfaceDefineRule = new(
            id: "TypedSiRCA002",
            title: "TypedSignalR.Client.Analyzer.002: Only define methods in the interface used for HubProxy/Receiver/HubClientBase",
            messageFormat: "[Only define methods in the interface used for HubProxy/Receiver/HubClientBase] {0} is not method",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Only define methods in the interface used for HubProxy/Receiver/HubClientBase.");

        public static readonly DiagnosticDescriptor HubMethodReturnValueTypeRule = new(
            id: "TypedSiRAC003",
            title: "TypedSignalR.Client.Analyzer.003: The return type of the method in the interface used for Hub/HubProxy must be Task or Task<T>",
            messageFormat: "[The return type of the method in the interface used for Hub/HubProxy must be Task or Task<T>] Return type of {0} is not Task or Task<T>",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The return type of the method in the interface used for Hub/HubProxy must be Task or Task<T>.");

        public static readonly DiagnosticDescriptor ReceiverMethodReturnValueTypeRule = new(
            id: "TypedSiRCA004",
            title: "TypedSignalR.Client.Analyzer.004: The return type of the method in the interface used for Receiver/Client-side must be Task",
            messageFormat: "[The return type of the method in the interface used for Receiver/Client-side must be Task] Return type of {0} is not Task",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The return type of the method in the interface used for Receiver/Client-side must be Task.");

        public static readonly DiagnosticDescriptor AttributeArgumentRule = new(
            id: "TypedSiRCA005",
            title: "TypedSignalR.Client.Analyzer.005: Argument of HubClientBaseAttribute must be an interface",
            messageFormat: "[Argument of HubClientBaseAttribute must be typeof(interface)] {0} is not interface",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Argument of HubClientBaseAttribute must be typeof(interface).");
    }
}
