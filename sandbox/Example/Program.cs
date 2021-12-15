using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Shared;
using TypedSignalR.Client;

namespace Example;

public class UserDefine
{
    public Guid RandomId { get; set; }
    public DateTime Datetime { get; set; }
}

// The return type of the client-side method must be Task. 
public interface IClientContract
{
    // Of course, user defined type is OK. 
    Task SomeClientMethod1(string user, string message, UserDefine userDefine);
    Task SomeClientMethod2();
}

// The return type of the method on the hub-side must be Task or Task <T>. 
public interface IHubContract
{
    Task<string> SomeHubMethod1(string user, string message);
    Task SomeHubMethod2();
    Task<string> SomeHubMethod3(string user, string message, CancellationToken cancellationToken);
}

class Receiver : IClientContract, IHubConnectionObserver
{
    public Task SomeClientMethod1(string user, string message, UserDefine userDefine)
    {
        throw new NotImplementedException();
    }

    public Task SomeClientMethod2()
    {
        throw new NotImplementedException();
    }

    public Task SomeClientMethod3()
    {
        throw new NotImplementedException();
    }

    public Task OnClosed(Exception e)
    {
        throw new NotImplementedException();
    }

    public Task OnReconnected(string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task OnReconnecting(Exception e)
    {
        throw new NotImplementedException();
    }
}

class Re2 : SignalR.Shared.IClientContract
{
    public Task ReceiveMessage(string user, string message, UserDefineClass userDefine)
    {
        throw new NotImplementedException();
    }

    public Task SomeClientMethod()
    {
        throw new NotImplementedException();
    }
}

interface IErrorProxy
{
    Task<string> Hoge();
    int Id { get; }
}

public static class Ex
{
    public static IDisposable Subscribe<T>(this HubConnection source, T o)
    {
        return null;
    }

}

class Program
{
    static void Main(string[] args)
    {
        var connection = new HubConnectionBuilder()
           .WithUrl("https://~~~")
           .Build();

        // var hub = connection.CreateHubProxy<IErrorProxy>();

        connection.Register<IClientContract>(new Receiver());
        connection.Register<SignalR.Shared.IClientContract>(new Re2());


        //hub.Hoge();
        //{
        //    var hub = connection.CreateHubProxy<IHubContract>();
        //    var subscription = connection.Register<IClientContract>(new Receiver());

        //    hub.SomeHubMethod1("user", "message");

        //    subscription.Dispose();
        //}

        //{
        //    var (hub, subscription) = connection.CreateHubProxyWith<IHubContract, IClientContract>(new Receiver());
        //}
    }
}
