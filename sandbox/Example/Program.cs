using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using TypedSignalR.Client;

namespace Example
{
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

    [HubClientBase(typeof(IHubContract), typeof(IClientContract))]
    partial class Base
    {

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
}

