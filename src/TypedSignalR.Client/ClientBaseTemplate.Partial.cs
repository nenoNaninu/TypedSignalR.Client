using System.Collections.Generic;

namespace TypedSignalR.Client
{
    public partial class ClientBaseTemplate
    {
        public string? NameSpace { get; set; }
        public string? TargetTypeName { get; set; }

        public string? ClientInterfaceName { get; set; }
        public string? HubInterfaceName { get; set; }

        public IReadOnlyList<MethodInfo>? ClientMethods { get; set; }
        public IReadOnlyList<MethodInfo>? HubMethods { get; set; }
    }
}
