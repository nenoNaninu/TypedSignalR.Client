namespace TypedSignalR.Client.Tests.Shared;

public interface INullableTestHub
{
    Task<int> GetStruct(int message);
    Task<int?> GetNullableStruct(int? message);

    Task<string> GetReferenceType(string message);
    Task<string?> GetNullableReferenceType(string? message);
    Task<string?> GetNullableReferenceType2(string? message1, string? message2);
}


public interface INullableTestIReceiver
{
    Task<int> GetStruct(int message);
    Task<int?> GetNullableStruct(int? message);

    Task<string> GetReferenceType(string message);
    Task<string?> GetNullableReferenceType(string? message);
    Task<string?> GetNullableReferenceType2(string? message, int? value);
    Task<string?> GetNullableReferenceType3(string? message, string message2);
    Task<string?> GetNullableReferenceType4(string message, string? message2, int value);
}
