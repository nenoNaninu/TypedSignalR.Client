using System;
using System.Threading;
using TypedSignalR.Client;
using SignalR.Shared;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ConsoleApp
{
    class Receiver : SignalR.Shared.IClientContract
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

    public interface IErrorProxy3
    {
        Task<string> Hoge();
        int Id { get; } // forbidden property
    }

    public interface IEmptyProxy
    {
    }

    class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://~~~")
                .Build();

            //connection.CreateHubProxy<IErrorProxy>();
            //connection.CreateHubProxy<IErrorProxy2>();


            var id = connection.ConnectionId;
            {
                var hub1 = connection.CreateHubProxy<SignalR.Shared.IHubContract>();
                var hub2 = connection.CreateHubProxy<SignalR.Shared.IHubContract>();
                var (hub3, subscription1) = connection.CreateHubProxyWith<SignalR.Shared.IHubContract, SignalR.Shared.IClientContract>(new Receiver());

                var subscription2 = connection.Register<SignalR.Shared.IClientContract>(new Receiver());
                
                SignalR.Shared.IClientContract receiver = new Receiver();
                var subscription3 = connection.Register(receiver);

                hub1.SendMessage("a", "a");
            }

            {
                var hub1 = connection.CreateHubProxy<ConsoleApp.IHubContract>();
                var hub2 = connection.CreateHubProxy<ConsoleApp.IHubContract>();
                var (hub3, subscription1) = connection.CreateHubProxyWith<ConsoleApp.IHubContract, ConsoleApp.IClientContract>(new Receiver2());

                ConsoleApp.IClientContract receiver = new Receiver2();
                var subscription2 = connection.Register<ConsoleApp.IClientContract>(new Receiver2());
                var subscription3 = connection.Register(receiver);
            }
            {
                var empty = connection.CreateHubProxy<IEmptyProxy>();
            }

            {
                // error pattern!

                //var hub4 = connection.CreateHubProxy<Receiver>(); // error
                //var hub5 = connection.CreateHubProxy<IErrorProxy>(); // error
                //var hub6 = connection.CreateHubProxy<IErrorProxy2>(); // error
                //var hub7 = connection.CreateHubProxy<IErrorProxy3>(); // error


                //var subscription4 = connection.Register<IErrorReceiver>(new ErrorReceiver()); // error
                //var subscription5 = connection.Register(new Receiver()); // error. type argument must be interface
            }
        }
    }
}
