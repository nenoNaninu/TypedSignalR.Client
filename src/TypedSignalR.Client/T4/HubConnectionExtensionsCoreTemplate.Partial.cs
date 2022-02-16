using System.Collections.Generic;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.T4;

public partial class HubConnectionExtensionsCoreTemplate
{
    public IReadOnlyList<HubTypeMetadata>? HubTypes { get; set; }
    public IReadOnlyList<ReceiverTypeMetadata>? ReceiverTypes { get; set; }
}
