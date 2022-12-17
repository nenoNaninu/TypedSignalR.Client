using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedSignalR.Client.Tests.Shared;

[Receiver]
public interface IAttributeReceiver
{
    Task ReceiveMessage(string message, int value);
    Task Notify();
    Task ReceiveCustomMessage(UserDefinedType userDefined);
}

[Hub]
public interface IAttributeReceiverTestHub
{
    Task Start();
}
