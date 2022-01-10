using System.Collections.Generic;

namespace TypedSignalR.Client.CodeAnalysis;

public sealed class MethodMetadata
{
    public string MethodName { get; }

    public IReadOnlyList<MethodParameter> Parameters { get; }

    public string ReturnType { get; }
    public bool IsGenericReturnType { get; }
    public string? GenericReturnTypeArg { get; }

    /// <param name="isGenericReturnType">if return type is Task<T>, must be true. if return type is Task or void, must be false. </param>
    /// <param name="genericReturnTypeArg">e.g. if return type is Task<Datetime>, you must be input System.Datetime. if not generics, you must be input null. </param>
    public MethodMetadata(string methodName, IReadOnlyList<MethodParameter> parameters, string returnType, bool isGenericReturnType, string? genericReturnTypeArg)
    {
        MethodName = methodName;
        Parameters = parameters;
        ReturnType = returnType;
        IsGenericReturnType = isGenericReturnType;
        GenericReturnTypeArg = genericReturnTypeArg;
    }
}
