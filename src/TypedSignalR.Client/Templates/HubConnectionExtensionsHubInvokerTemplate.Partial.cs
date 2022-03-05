using System;
using System.Collections.Generic;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.Templates;

public partial class HubConnectionExtensionsHubInvokerTemplate
{
    public IReadOnlyList<TypeMetadata> HubTypes { get; set; } = Array.Empty<TypeMetadata>();
}
