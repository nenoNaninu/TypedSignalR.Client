using System.Collections.Generic;

namespace TypedSignalR.Client.T4
{
    public partial class HubProxyTemplate
    {
        public IReadOnlyList<InvokerInfo>? InvokerList { get; set; }
        public IReadOnlyList<ReceiverInfo>? ReceiverList { get; set; }
    }
}