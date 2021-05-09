using System;
using TypedSignalR.Client;
using SignalR.Shared;

namespace ConsoleApp
{
    [HubClientBase(typeof(IHubContract), typeof(IClientContract))]
    partial class ClientBase
    {
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
