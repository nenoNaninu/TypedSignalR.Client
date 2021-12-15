using System;
using System.Threading.Tasks;

namespace SignalR.Shared
{
    public class UserDefineClass
    {
        public Guid RandomId { get; set; }
        public DateTime Datetime { get; set; }
    }

    public class Status
    {
        public string StatusMessage { get; set; }
    }

    public interface IClientContract
    {
        Task ReceiveMessage(string user, string message, UserDefineClass userDefine);
        Task SomeClientMethod();
    }

    public interface IHubContract
    {
        Task<Status> SendMessage(string user, string message);
        Task SomeHubMethod();
    }

    public interface IErrorReceiver
    {
        Task<string> Hoge(); // must Task. not Task<T>
    }

    public class ErrorReceiver : IErrorReceiver
    {
        public Task<string> Hoge()
        {
            throw new NotImplementedException();
        }
    }


}
