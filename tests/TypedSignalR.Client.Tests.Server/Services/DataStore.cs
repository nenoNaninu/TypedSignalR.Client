using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.Server.Services;

public interface IDataStore
{
    IUserData Get(string connectionId);
    void Remove(string connectionId);
}

/// <summary>
/// Singleton
/// </summary>
public class DataStore : IDataStore
{
    private readonly Dictionary<string, IUserData> _dictonary = new();

    public IUserData Get(string connectionId)
    {
        if (_dictonary.TryGetValue(connectionId, out var data))
        {
            return data;
        }
        else
        {
            var newData = new UserData();

            _dictonary.Add(connectionId, newData);

            return newData;
        }
    }

    public void Remove(string connectionId)
    {
        _dictonary.Remove(connectionId);
    }
}

public interface IUserData
{
    int Value { get; set; }
    List<UserDefinedType> Data { get; }
}

class UserData : IUserData
{
    public int Value { get; set; }
    public List<UserDefinedType> Data { get; set; } = new();
}
