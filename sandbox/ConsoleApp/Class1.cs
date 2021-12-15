using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;


namespace ConsoleApp;

public class UserDefineClass2
{
    public Guid RandomId { get; set; }
    public DateTime Datetime { get; set; }
}

public class Status2
{
    public string StatusMessage { get; set; }
}

public interface IClientContract
{
    Task ReceiveMessage(string user, string message, UserDefineClass2 userDefine);
    Task SomeClientMethod();
}

public interface IHubContract
{
    Task<Status2> SendMessage(string user, string message);
    Task SomeHubMethod();
}

class Receiver2 : IClientContract
{
    public Task ReceiveMessage(string user, string message, UserDefineClass2 userDefine)
    {
        throw new NotImplementedException();
    }

    public Task SomeClientMethod()
    {
        throw new NotImplementedException();
    }
}
