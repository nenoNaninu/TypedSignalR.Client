using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedSignalR.Client
{
    public partial class CodeTemplate
    {
        public string? NameSpace { get; set; }
        public string? TargetTypeName { get; set; }

        public string? ClientInterfaceName { get; set; }
        public string? HubInterfaceName { get; set; }

        public IReadOnlyList<MethodInfo>? ClientMethods { get; set; }
        public IReadOnlyList<MethodInfo>? HubMethods { get; set; }
    }
}
