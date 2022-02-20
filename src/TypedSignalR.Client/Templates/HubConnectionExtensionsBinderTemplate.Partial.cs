using System;
using System.Collections.Generic;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.Templates;

public partial class HubConnectionExtensionsBinderTemplate
{
    public IReadOnlyList<ReceiverTypeMetadata> ReceiverTypes { get; set; } = Array.Empty<ReceiverTypeMetadata>();
}
