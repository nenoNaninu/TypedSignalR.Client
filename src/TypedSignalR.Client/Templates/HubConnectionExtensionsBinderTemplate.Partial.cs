using System;
using System.Collections.Generic;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.Templates;

public partial class HubConnectionExtensionsBinderTemplate
{
    public IReadOnlyList<TypeMetadata> ReceiverTypes { get; set; } = Array.Empty<TypeMetadata>();
}
