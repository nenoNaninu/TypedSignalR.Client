using System.Collections.Generic;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.T4
{
    public partial class HubConnectionExtensionsCoreTemplate
    {
        public IReadOnlyList<HubProxyTypeMetadata>? HubProxyTypeList { get; set; }
        public IReadOnlyList<ReceiverTypeMetadata>? ReceiverTypeList { get; set; }
    }
}
