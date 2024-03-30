using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.Templates;

public static class MethodMetadataExtensions
{
    public static string CreateParametersString(this MethodMetadata methodMetadata)
    {
        if (methodMetadata.Parameters.Count == 0)
        {
            return string.Empty;
        }

        if (methodMetadata.Parameters.Count == 1)
        {
            return $"{methodMetadata.Parameters[0].TypeName} {methodMetadata.Parameters[0].Name}";
        }

        var sb = new StringBuilder();

        sb.Append(methodMetadata.Parameters[0].TypeName);
        sb.Append(' ');
        sb.Append(methodMetadata.Parameters[0].Name);

        for (int i = 1; i < methodMetadata.Parameters.Count; i++)
        {
            sb.Append(", ");
            sb.Append(methodMetadata.Parameters[i].TypeName);
            sb.Append(' ');
            sb.Append(methodMetadata.Parameters[i].Name);
        }

        return sb.ToString();
    }

    public static string CreateArgumentsString(this MethodMetadata methodMetadata)
    {
        if (methodMetadata.Parameters.Count == 0)
        {
            return "global::System.Array.Empty<object>()";
        }

        var sb = new StringBuilder();

        sb.Append("new object?[] { ");
        sb.Append(methodMetadata.Parameters[0].Name);

        for (int i = 1; i < methodMetadata.Parameters.Count; i++)
        {
            sb.Append(", ");
            sb.Append(methodMetadata.Parameters[i].Name);
        }

        sb.Append(" }");

        return sb.ToString();
    }

    public static string CreateArgumentsStringExceptCancellationToken(this MethodMetadata methodMetadata, SpecialSymbols specialSymbols)
    {
        var parameters = methodMetadata.Parameters
            .Where(x => !SymbolEqualityComparer.Default.Equals(x.Type, specialSymbols.CancellationTokenSymbol))
            .ToArray();

        if (parameters.Length == 0)
        {
            return "global::System.Array.Empty<object>()";
        }

        var sb = new StringBuilder();

        sb.Append("new object?[] { ");
        sb.Append(parameters[0].Name);

        for (int i = 1; i < parameters.Length; i++)
        {
            sb.Append(", ");
            sb.Append(parameters[i].Name);
        }

        sb.Append(" }");

        return sb.ToString();
    }

    public static string CreateTypeArgumentsStringForHandlerConverter(this MethodMetadata methodMetadata)
    {
        if (methodMetadata.Parameters.Count == 0)
        {
            if (methodMetadata.IsGenericReturnType)
            {
                return $"<{methodMetadata.GenericReturnTypeArgument}>";
            }

            return string.Empty;
        }

        if (methodMetadata.Parameters.Count == 1)
        {
            if (methodMetadata.IsGenericReturnType)
            {
                return $"<{methodMetadata.Parameters[0].TypeName}, {methodMetadata.GenericReturnTypeArgument}>";
            }

            return $"<{methodMetadata.Parameters[0].TypeName}>";
        }

        var sb = new StringBuilder();

        sb.Append('<');
        sb.Append(methodMetadata.Parameters[0].TypeName);

        for (int i = 1; i < methodMetadata.Parameters.Count; i++)
        {
            sb.Append(", ");
            sb.Append(methodMetadata.Parameters[i].TypeName);
        }

        if (methodMetadata.IsGenericReturnType)
        {
            sb.Append(", ");
            sb.Append(methodMetadata.GenericReturnTypeArgument);
        }

        sb.Append('>');
        return sb.ToString();
    }

    public static string CreateParameterTypeArrayString(this MethodMetadata methodMetadata)
    {
        if (methodMetadata.Parameters.Count == 0)
        {
            return "global::System.Type.EmptyTypes";
        }

        if (methodMetadata.Parameters.Count == 1)
        {
            return $"new[] {{ typeof({methodMetadata.Parameters[0].TypeName}) }}";
        }

        var sb = new StringBuilder();

        sb.Append("new[] { ");
        sb.Append($"typeof({methodMetadata.Parameters[0].TypeName})");

        for (int i = 1; i < methodMetadata.Parameters.Count; i++)
        {
            sb.Append($", typeof({methodMetadata.Parameters[i].TypeName})");
        }

        sb.Append(" }");
        return sb.ToString();
    }

    public static string CreateGenericReturnTypeArgumentString(this MethodMetadata methodMetadata)
    {
        return methodMetadata.IsGenericReturnType ? $"<{methodMetadata.GenericReturnTypeArgument}>" : string.Empty;
    }

    public static string CreateCancellationTokenString(this MethodMetadata methodMetadata, string cancellationTokenString, SpecialSymbols specialSymbols)
    {
        var parameter = methodMetadata.Parameters
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.Type, specialSymbols.CancellationTokenSymbol));

        if (parameter == default)
        {
            return cancellationTokenString;
        }

        return $"global::System.Threading.CancellationTokenSource.CreateLinkedTokenSource({cancellationTokenString}, {parameter.Name}).Token";
    }

    public static string CreateMethodString(this MethodMetadata methodMetadata, SpecialSymbols specialSymbols)
    {
        var type = methodMetadata.GetHubMethodType(specialSymbols);

        return type switch
        {
            HubMethodType.Unary => CreateUnaryMethodString(methodMetadata),
            HubMethodType.ServerStreamingAsAsyncEnumerable => CreateServerStreamingMethodAsAsyncEnumerableString(methodMetadata, specialSymbols),
            HubMethodType.ServerStreamingAsTaskAsyncEnumerable => CreateServerStreamingMethodAsTaskAsyncEnumerableString(methodMetadata, specialSymbols),
            HubMethodType.ServerStreamingAsChannel => CreateServerStreamingMethodAsChannelString(methodMetadata, specialSymbols),
            HubMethodType.ClientStreamingAsAsyncEnumerable => CreateClientStreamingMethodAsAsyncEnumerableString(methodMetadata),
            HubMethodType.ClientStreamingAsChannel => CreateClientStreamingMethodAsChannelString(methodMetadata),
            _ => string.Empty
        };
    }

    private static HubMethodType GetHubMethodType(this MethodMetadata methodMetadata, SpecialSymbols specialSymbols)
    {
        var returnTypeSymbol = methodMetadata.ReturnTypeSymbol as INamedTypeSymbol;

        if (returnTypeSymbol is null)
        {
            return HubMethodType.None;
        }

        // server streaming

        // Task<T>
        if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            var typeArgument = returnTypeSymbol.TypeArguments[0];

            // Task<IAsyncEnumerable<T>>
            if (SymbolEqualityComparer.Default.Equals(typeArgument.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
            {
                return HubMethodType.ServerStreamingAsTaskAsyncEnumerable;
            }

            // Task<ChannelReader<T>>
            if (SymbolEqualityComparer.Default.Equals(typeArgument.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
            {
                return HubMethodType.ServerStreamingAsChannel;
            }

            // Task<T>
            return HubMethodType.Unary;
        }

        // IAsyncEnumerable<T>
        if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
        {
            return HubMethodType.ServerStreamingAsAsyncEnumerable;
        }

        // client streaming

        // Task
        if (SymbolEqualityComparer.Default.Equals(returnTypeSymbol, specialSymbols.TaskSymbol))
        {
            if (methodMetadata.Parameters.Count == 1)
            {
                var parameter = methodMetadata.Parameters[0];

                if (SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
                {
                    return HubMethodType.ClientStreamingAsAsyncEnumerable;
                }

                if (SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
                {
                    return HubMethodType.ClientStreamingAsChannel;
                }
            }

            return HubMethodType.Unary;
        }

        return HubMethodType.None;
    }

    private static string CreateUnaryMethodString(MethodMetadata method)
    {
        return $$"""
            public {{method.ReturnType}} {{method.MethodName}}({{method.CreateParametersString()}})
            {
                return (System.Threading.Tasks.Task<SignalR.Shared.Status?>)global::Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions.InvokeCoreAsync{{method.CreateGenericReturnTypeArgumentString()}}(_connection, nameof({{method.MethodName}}), {{method.CreateArgumentsString()}}, _cancellationToken);
            }
""";
    }

    private static string CreateServerStreamingMethodAsAsyncEnumerableString(MethodMetadata method, SpecialSymbols specialSymbols)
    {
        return $$"""
            public {{method.ReturnType}} {{method.MethodName}}({{method.CreateParametersString()}})
            {
                return _connection.StreamAsyncCore{{method.CreateGenericReturnTypeArgumentString()}}(nameof({{method.MethodName}}), {{method.CreateArgumentsStringExceptCancellationToken(specialSymbols)}}, {{method.CreateCancellationTokenString("_cancellationToken", specialSymbols)}});
            }
""";
    }

    private static string CreateServerStreamingMethodAsTaskAsyncEnumerableString(MethodMetadata method, SpecialSymbols specialSymbols)
    {
        return $$"""
            public {{method.ReturnType}} {{method.MethodName}}({{method.CreateParametersString()}})
            {
                var ret = _connection.StreamAsyncCore{{method.CreateGenericReturnTypeArgumentStringForStreaming()}}(nameof({{method.MethodName}}), {{method.CreateArgumentsStringExceptCancellationToken(specialSymbols)}}, {{method.CreateCancellationTokenString("_cancellationToken", specialSymbols)}});
                return global::System.Threading.Tasks.Task.FromResult(ret);
            }
""";
    }

    private static string CreateServerStreamingMethodAsChannelString(MethodMetadata method, SpecialSymbols specialSymbols)
    {
        return $$"""
            public {{method.ReturnType}} {{method.MethodName}}({{method.CreateParametersString()}})
            {
                return global::Microsoft.AspNetCore.SignalR.Client.HubConnectionExtensions.StreamAsChannelCoreAsync{{method.CreateGenericReturnTypeArgumentStringForStreaming()}}(_connection, nameof({{method.MethodName}}), {{method.CreateArgumentsStringExceptCancellationToken(specialSymbols)}}, {{method.CreateCancellationTokenString("_cancellationToken", specialSymbols)}});
            }
""";
    }

    private static string CreateClientStreamingMethodAsAsyncEnumerableString(MethodMetadata method)
    {
        return $$"""
            public {{method.ReturnType}} {{method.MethodName}}({{method.CreateParametersString()}})
            {
                return _connection.SendCoreAsync(nameof({{method.MethodName}}), {{method.CreateArgumentsString()}}, _cancellationToken);
            }
""";
    }

    private static string CreateClientStreamingMethodAsChannelString(MethodMetadata method)
    {
        return $$"""

            public {{method.ReturnType}} {{method.MethodName}}({{method.CreateParametersString()}})
            {
                return _connection.SendCoreAsync(nameof({{method.MethodName}}), {{method.CreateArgumentsString()}}, _cancellationToken);
            }
""";
    }

    /// <summary>
    /// Task&lt;IAsyncEnumerable&lt;T&gt;&gt; --&gt; T
    /// </summary>
    private static string CreateGenericReturnTypeArgumentStringForStreaming(this MethodMetadata methodMetadata)
    {
        if (!methodMetadata.IsGenericReturnType)
        {
            return string.Empty;
        }

        var returnType = methodMetadata.MethodSymbol.ReturnType as INamedTypeSymbol;

        if (returnType?.TypeArguments[0] is not INamedTypeSymbol typeArgument)
        {
            return string.Empty;
        }

        if (!typeArgument.IsGenericType)
        {
            return string.Empty;
        }

        return $"<{typeArgument.TypeArguments[0]}>";
    }

    private enum HubMethodType
    {
        None,
        Unary,
        ServerStreamingAsAsyncEnumerable,
        ServerStreamingAsTaskAsyncEnumerable,
        ServerStreamingAsChannel,
        ClientStreamingAsAsyncEnumerable,
        ClientStreamingAsChannel,
    }
}

