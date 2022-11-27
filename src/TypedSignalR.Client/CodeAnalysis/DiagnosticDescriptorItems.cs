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
        messageFormat: "{0} is not an interface. The type argument must be an interface.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The type argument must be an interface.");

    public static readonly DiagnosticDescriptor InterfaceDefineRule = new(
        id: "TSRC002",
        title: "Only method definitions are allowed in the interface",
        messageFormat: "{0} is not a method. Define only methods in the interface.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Only method definitions are allowed in the interface.");

    public static readonly DiagnosticDescriptor HubMethodReturnTypeRule = new(
        id: "TSRC003",
        title: "The return type of methods in the interface must be Task or Task<T> or IAsyncEnumerable<T> or Task<IAsyncEnumerable<T> or Task<ChannelReader<T>>",
        messageFormat: "The return type of {0} is not suitable. Instead, use Task or Task<T> or IAsyncEnumerable<T> or Task<IAsyncEnumerable<T> or Task<ChannelReader<T>>.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of methods in the interface used for hub proxy must be Task or Task<T>.");

    public static readonly DiagnosticDescriptor ReceiverMethodReturnTypeRule = new(
        id: "TSRC004",
        title: "The return type of methods in the interface must be Task or Task<T>",
        messageFormat: "The return type of {0} is not suitable. Instead, use Task or Task<T>.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of methods in the interface must be Task or Task<T>.");

    public static readonly DiagnosticDescriptor HubMethodCancellationTokenParameterRule = new(
        id: "TSRC005",
        title: "CancellationToken can be used as a parameter only in the server-to-client streaming method",
        messageFormat: "CancellationToken cannot be used as a parameter in the {0}.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "CancellationToken can be used as a parameter only in the server-to-client streaming method.");

    public static readonly DiagnosticDescriptor HubMethodMultipleCancellationTokenParameterRule = new(
        id: "TSRC006",
        title: "Using multiple CancellationToken in method parameters is prohibited",
        messageFormat: "Multiple CancellationToken cannot be used as a parameter in the {0}.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using multiple CancellationToken in method parameters is prohibited.");

    public static readonly DiagnosticDescriptor ServerStreamingMethodParameterRule = new(
        id: "TSRC007",
        title: "Using IAsyncEnumerable<T> or ChannelReader<T> as a parameter in a server-to-client streaming method is prohibited",
        messageFormat: "Do not use IAsyncEnumerable<T> or ChannelReader<T> as a parameter because the {0} was analyzed as a server-to-client streaming method.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using IAsyncEnumerable<T> or ChannelReader<T> as a parameter in a server-to-client streaming method is prohibited.");

    public static readonly DiagnosticDescriptor ClientStreamingMethodReturnTypeRule = new(
        id: "TSRC008",
        title: "The return type of client-to-server streaming method must be Task",
        messageFormat: "The return type of a client-to-server streaming method must be Task. The return type of {0} is not Task.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of client-to-server streaming method must be Task.");
}
