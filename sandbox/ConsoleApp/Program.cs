using System;
using TypedSignalR.Client;
using SignalR.Shared;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ConsoleApp
{
    [HubClientBase(typeof(IHubContract), typeof(IClientContract))]
    partial class ClientBase
    {
    }

    //public static class Ex
    //{
    //    public static T Build<T>(this IHubConnectionBuilder source, Func<HubConnection,T> factory)
    //    {
    //        HubConnection connection = source.Build();

    //        return factory.Invoke(connection);
    //    }
    //}

    class Clinet : ClientBase
    {

        public Clinet(HubConnection connection, string arg) : base(connection)
        {
        }

        public override Task ReceiveMessage(string user, string message, UserDefineClass userDefine)
        {
            throw new NotImplementedException();
        }

        public override Task SomeClientMethod()
        {
            throw new NotImplementedException();
        }
    }

    class Receiver : IClientContract
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

    interface IErrorProxy2
    {
        Task<string> Hoge();
        int Id();
    }

    interface IErrorReceiver
    {
        Task<string> Hoge();
    }

    class ErrorReceiver : IErrorReceiver
    {
        public Task<string> Hoge()
        {
            throw new NotImplementedException();
        }
    }

    [HubClientBase(typeof(IErrorProxy), typeof(IClientContract))] // error
    partial class ClientErrorBase
    {
    }

    [HubClientBase(typeof(IErrorProxy), typeof(Clinet))] // error
    partial class ClientErrorBase1
    {
    }

    class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://~~~")
                .Build();
            
            var id = connection.ConnectionId;

            var hub1 = connection.CreateHubProxy<IHubContract>();
            var hub2 = connection.CreateHubProxy<IHubContract>();
            var (hub3, subscription1) = connection.CreateHubProxyWith<IHubContract, IClientContract>(new Receiver());

            IClientContract receiver = new Receiver();
            var subscription2 = connection.Register<IClientContract>(new Receiver());
            var subscription3 = connection.Register(receiver);

            var hub5 = connection.CreateHubProxy<IErrorProxy>(); // error
            var hub4 = connection.CreateHubProxy<Receiver>(); // error
            var hub6 = connection.CreateHubProxy<IErrorProxy2>(); // error

            var subscription4 = connection.Register<IErrorReceiver>(new ErrorReceiver()); // error
            var subscription5 = connection.Register(new ErrorReceiver()); // error

        }
    }
}
