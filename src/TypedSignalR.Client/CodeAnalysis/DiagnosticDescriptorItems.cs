using Microsoft.CodeAnalysis;

namespace TypedSignalR.Client.CodeAnalysis;

public static class DiagnosticDescriptorItems
{
    public static readonly DiagnosticDescriptor UnexpectedException = new(
        id: "TSRC000",
        title: "Unexpected exception",
        messageFormat: "[Unexpected exception] {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Unexpected exception.");

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
        title: "Only method definitions are allowed in the interface",
        messageFormat: "[Only method definitions are allowed in the interface used for {0}] {1} is not a method",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Only method definitions are allowed in the interface.");

    public static readonly DiagnosticDescriptor HubMethodReturnTypeRule = new(
        id: "TSRC003",
        title: "The return type of methods in the interface must be Task or Task<T>",
        messageFormat: "[The return type of methods in the interface used for hub proxy must be Task or Task<T>] The return type of {0} is not Task or Task<T>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of methods in the interface used for hub proxy must be Task or Task<T>.");

    public static readonly DiagnosticDescriptor ReceiverMethodReturnTypeRule = new(
        id: "TSRC004",
        title: "The return type of methods in the interface must be Task",
        messageFormat: "[The return type of methods in the interface used for the receiver must be Task] The return type of {0} is not Task",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of methods in the interface must be Task.");

    public static readonly DiagnosticDescriptor HubMethodCancellationTokenParameterRule = new(
        id: "TSRC005",
        title: "CancellationToken can be used as a parameter only in the server-to-client streaming method",
        messageFormat: "[CancellationToken can be used as a parameter only in the server-to-client streaming method] CancellationToken is used in the {0} parameters",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using multiple CancellationToken in method parameters is prohibited.");

    public static readonly DiagnosticDescriptor HubMethodMultipleCancellationTokenParameterRule = new(
        id: "TSRC006",
        title: "Using multiple CancellationToken in method parameters is prohibited",
        messageFormat: "[Using multiple CancellationToken in method parameters is prohibited] Multiple CancellationToken are used in the {0} parameters",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using multiple CancellationToken in method parameters is prohibited.");

    public static readonly DiagnosticDescriptor ServerStreamingMethodParameterRule = new(
        id: "TSRC007",
        title: "Using IAsyncEnumerable<T> or ChannelReader<T> as a parameter in a server-to-client streaming method is prohibited",
        messageFormat: "[Using IAsyncEnumerable<T> or ChannelReader<T> as a parameter in a server-to-client streaming method is prohibited] {0} use IAsyncEnumerable<T> or ChannelReader<T> as a parameter",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Must be following the server streaming method rules.");

    public static readonly DiagnosticDescriptor ClientStreamingMethodReturnTypeRule = new(
        id: "TSRC008",
        title: "The return type of client-to-server streaming method must be Task",
        messageFormat: "[The return type of client-to-server streaming method must be Task] The return type of {0} is not Task",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of client-to-server streaming method must be Task.");
}
