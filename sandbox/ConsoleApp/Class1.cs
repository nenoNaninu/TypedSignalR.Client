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
    public string? StatusMessage { get; set; }
}

public interface IClientContract
{
    Task ReceiveMessage(string user, string message, UserDefineClass2 userDefine);
    Task SomeClientMethod();
    Task ReceiveMessage2(string user, string message, UserDefineClass2 userDefine, int a0, int a1, int a2, int a3, int a4, int a5, int a6);
}

public interface IClientContract2
{
    void ReceiveMessage(string user, string message, UserDefineClass2 userDefine);
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

    public Task ReceiveMessage2(string user, string message, UserDefineClass2 userDefine, int a0, int a1, int a2, int a3, int a4, int a5, int a6)
    {
        throw new NotImplementedException();
    }

    public Task SomeClientMethod()
    {
        throw new NotImplementedException();
    }
}

class Receiver3 : IClientContract2
{
    public void ReceiveMessage(string user, string message, UserDefineClass2 userDefine)
    {
        throw new NotImplementedException();
    }
}
