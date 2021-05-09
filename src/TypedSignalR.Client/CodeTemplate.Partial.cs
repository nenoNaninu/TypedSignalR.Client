using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedSignalR.Client
{
    public partial class CodeTemplate
    {
        public string NameSpace { get; set; }
        public string TypeName { get; set; }

        public string ClientInterfaceName;
        public string HubInterfaceName;

        public IEnumerable<MethodInfo> ClientMethods { get; set; }
        public IEnumerable<MethodInfo> HubMethods { get; set; }
    }
}
