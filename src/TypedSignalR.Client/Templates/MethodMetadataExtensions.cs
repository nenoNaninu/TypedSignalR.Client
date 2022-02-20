using System.Text;
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

    public static string CreateArgumentsStringForInvokeCoreAsync(this MethodMetadata methodMetadata)
    {
        if (methodMetadata.Parameters.Count == 0)
        {
            return "global::System.Array.Empty<object>()";
        }

        var sb = new StringBuilder();

        sb.Append("new object[] { ");
        sb.Append(methodMetadata.Parameters[0].Name);

        for (int i = 1; i < methodMetadata.Parameters.Count; i++)
        {
            sb.Append(", ");
            sb.Append(methodMetadata.Parameters[i].Name);
        }

        sb.Append(" }");

        return sb.ToString();
    }

    public static string CreateTypeArgumentsStringFromParameterTypes(this MethodMetadata methodMetadata)
    {
        if (methodMetadata.Parameters.Count == 0)
        {
            return string.Empty;
        }

        if (methodMetadata.Parameters.Count == 1)
        {
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
}
