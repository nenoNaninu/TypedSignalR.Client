namespace TypedSignalR.Client.CodeAnalysis;

public readonly struct MethodParameter
{
    public readonly string Name;
    public readonly string TypeName;

    public MethodParameter(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }
}
