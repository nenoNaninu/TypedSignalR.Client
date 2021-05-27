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

            var subscription2 =  connection.Register<IClientContract>(new Receiver());
        }
    }
}
