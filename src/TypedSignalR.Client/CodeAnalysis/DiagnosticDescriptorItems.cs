using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public static class DiagnosticDescriptorItems
{
    public static readonly DiagnosticDescriptor TypeArgumentRule = new(
        id: "TSRC001",
        title: "The type argument must be an interface",
        messageFormat: "[The type argument of {0} must be an interface] {1} is not an interface",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The type argument must be an interface.");

    public static readonly DiagnosticDescriptor InterfaceDefineRule = new(
        id: "TSRC002",
        title: "Only define methods in the interface",
        messageFormat: "[Only define methods in the interface used for {0}] {1} is not a method",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Only define methods in the interface.");

    public static readonly DiagnosticDescriptor HubMethodReturnTypeRule = new(
        id: "TSRC003",
        title: "The return type of method in the interface must be Task or Task<T>",
        messageFormat: "[The return type of method in the interface used for hub proxy must be Task or Task<T>] The return type of {0} is not Task or Task<T>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of method in the interface used for hub proxy must be Task or Task<T>.");

    public static readonly DiagnosticDescriptor ReceiverMethodReturnTypeRule = new(
        id: "TSRC004",
        title: "The return type of method in the interface must be Task",
        messageFormat: "[The return type of method in the interface used for the receiver must be Task] The return type of {0} is not Task",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of method in the interface must be Task.");
}
