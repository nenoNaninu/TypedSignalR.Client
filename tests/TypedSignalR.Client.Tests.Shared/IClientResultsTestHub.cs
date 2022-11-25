namespace TypedSignalR.Client.Tests.Shared;

public interface IClientResultsTestHub
{
    Task<bool> StartTest();
}

public interface IClientResultsTestHubReceiver
{
    Task<Guid> GetGuidFromClient(); // struct
    Task<Person> GetPersonFromClient(); // user defined type
    Task<int> SumInClient(int left, int right); // calc
}
