namespace TypedSignalR.Client.Tests.Shared;

public class UserDefinedType
{
    public DateTime DateTime { get; set; }
    public Guid Guid { get; set; }
}

public interface IUnaryHub
{
    Task<string> Get();
    Task<int> Add(int x, int y);
    Task<string> Cat(string x, string y);
    Task<UserDefinedType> Echo(UserDefinedType instance);
}
