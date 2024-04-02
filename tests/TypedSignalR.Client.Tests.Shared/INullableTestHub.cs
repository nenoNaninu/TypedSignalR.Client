namespace TypedSignalR.Client.Tests.Shared;

public interface INullableTestHub
{
    Task<int> GetStruct(int message);
    Task<int?> GetNullableStruct(int? message);

    Task<string> GetReferenceType(string message);
    Task<string?> GetNullableReferenceType(string? message);
}


public interface INullableTestIReceiver
{
    Task<int> GetStruct(int message);
    Task<int?> GetNullableStruct(int? message);

    Task<string> GetReferenceType(string message);
    Task<string?> GetNullableReferenceType(string? message);
}
