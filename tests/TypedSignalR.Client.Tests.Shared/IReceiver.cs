using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedSignalR.Client.Tests.Shared;

public interface IReceiver
{
    Task ReceiveMessage(string message, int value);
    Task Notify();
    Task ReceiveCustomMessage(UserDefinedType userDefined);
}


public interface IReceiverWithCancellationToken
{
    Task ReceiveMessage(string message, int value, CancellationToken cancellationToken);
    Task Notify(CancellationToken cancellationToken);
    Task ReceiveCustomMessage(UserDefinedType userDefined, CancellationToken cancellationToken);
}

public interface IReceiverTestHub
{
    Task Start();
}
