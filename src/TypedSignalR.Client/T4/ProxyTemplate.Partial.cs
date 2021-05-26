using System;
using System.Collections.Generic;
using System.Text;
using TypedSignalR.Client;

namespace TypedSignalR.Client.T4
{
    public partial class ProxyTemplate
    {
        public IReadOnlyList<InvokerInfo>? InvokerList { get; set; }
        public IReadOnlyList<ReceiverInfo>? ReceiverList { get; set; }
    }
}
