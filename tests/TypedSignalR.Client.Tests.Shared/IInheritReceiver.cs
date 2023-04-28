namespace TypedSignalR.Client.Tests.Shared;

public interface IReceiverBaseBase
{
    Task ReceiveMessage(string message, int value);
}

public interface IReceiverBase1 : IReceiverBaseBase
{
    Task ReceiveCustomMessage(UserDefinedType userDefined);
}
public interface IReceiverBase2 : IReceiverBaseBase
{
    Task Notify();
}

public interface IInheritReceiver : IReceiverBase1, IReceiverBase2
{
    Task ReceiveMessage2(string message, int value);
}

public interface IInheritReceiverTestHub
{
    Task Start();
}
