using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedSignalR.Client.Tests.Shared;

public interface ISideEffectHub
{
    Task Init();
    Task Increment();
    Task<int> Result();

    Task Post(UserDefinedType instance);
    Task<UserDefinedType[]> Fetch();
}
