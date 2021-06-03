using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;


namespace ConsoleApp
{
    public class UserDefineClass2
    {
        public Guid RandomId { get; set; }
        public DateTime Datetime { get; set; }
    }

    public class Status2
    {
        public string StatusMessage { get; set; }
    }

    public interface IClientContract2
    {
        Task ReceiveMessage(string user, string message, UserDefineClass2 userDefine);
        Task SomeClientMethod();
    }

    public interface IHubContract2
    {
        Task<Status2> SendMessage(string user, string message);
        Task SomeHubMethod();
    }

    class Receiver2 : IClientContract2
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

    //class MyClass
    //{
    //    public MyClass(HubConnection connection)
    //    {
    //        var hub1 = connection.CreateHubProxy<IHubContract2>();
    //        var hub2 = connection.CreateHubProxy<IHubContract2>();
    //        var (hub3, subscription1) = connection.CreateHubProxyWith<IHubContract2, IClientContract2>(new Receiver2());

    //        IClientContract2 receiver = new Receiver2();
    //        var subscription2 = connection.Register<IClientContract2>(new Receiver2());
    //        var subscription3 = connection.Register(receiver);
    //    }
    //}
}