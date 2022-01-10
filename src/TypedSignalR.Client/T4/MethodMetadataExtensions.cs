using System.Text;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.T4;

public static class MethodMetadataExtensions
{
    public static string GenerateArgParameterString(this MethodMetadata metadata)
    {
        if (metadata.Parameters.Count == 0)
        {
            return string.Empty;
        }

        if (metadata.Parameters.Count == 1)
        {
            return $"{metadata.Parameters[0].TypeName} {metadata.Parameters[0].Name}";
        }

        var sb = new StringBuilder();

        sb.Append(metadata.Parameters[0].TypeName);
        sb.Append(' ');
        sb.Append(metadata.Parameters[0].Name);

        for (int i = 1; i < metadata.Parameters.Count; i++)
        {
            sb.Append(',');
            sb.Append(metadata.Parameters[i].TypeName);
            sb.Append(' ');
            sb.Append(metadata.Parameters[i].Name);
        }

        return sb.ToString();
    }

    public static string GenerateArgNamesStringForInvokeCoreAsync(this MethodMetadata metadata)
    {
        if (metadata.Parameters.Count == 0)
        {
            return "System.Array.Empty<object>()";
        }

        var sb = new StringBuilder();

        sb.Append("new object[] {");
        sb.Append(metadata.Parameters[0].Name);

        for (int i = 1; i < metadata.Parameters.Count; i++)
        {
            sb.Append(',');
            sb.Append(metadata.Parameters[i].Name);
        }

        sb.Append("}");

        return sb.ToString();
    }

    public static string GenerateTypeArgsFromParameterTypesString(this MethodMetadata metadata)
    {
        if (metadata.Parameters.Count == 0)
        {
            return string.Empty;
        }

        if (metadata.Parameters.Count == 1)
        {
            return $"<{metadata.Parameters[0].TypeName}>";
        }

        var sb = new StringBuilder();

        sb.Append('<');
        sb.Append(metadata.Parameters[0].TypeName);

        for (int i = 1; i < metadata.Parameters.Count; i++)
        {
            sb.Append(',');
            sb.Append(metadata.Parameters[i].TypeName);
        }

        sb.Append('>');
        return sb.ToString();
    }

    public static string GenerateTypeArgsFromParameterTypesConcatenatedTaskString(this MethodMetadata metadata)
    {
        if (metadata.Parameters.Count == 0)
        {
            return "<System.Threading.Tasks.Task>";
        }

        if (metadata.Parameters.Count == 1)
        {
            return $"<{metadata.Parameters[0].TypeName},System.Threading.Tasks.Task>";
        }

        var sb = new StringBuilder();

        sb.Append('<');

        for (int i = 0; i < metadata.Parameters.Count; i++)
        {
            sb.Append(metadata.Parameters[i].TypeName);
            sb.Append(',');
        }

        sb.Append("System.Threading.Tasks.Task>");
        return sb.ToString();
    }

    public static string GenerateCastedArgsString(this MethodMetadata metadata, string argName)
    {
        if (metadata.Parameters.Count == 0)
        {
            return string.Empty;
        }

        if (metadata.Parameters.Count == 1)
        {
            return $"({metadata.Parameters[0].TypeName}){argName}[0]";
        }

        var sb = new StringBuilder();

        sb.Append('(');
        sb.Append(metadata.Parameters[0].TypeName);
        sb.Append(')');
        sb.Append(argName);
        sb.Append("[0]");

        for (int i = 1; i < metadata.Parameters.Count; i++)
        {
            sb.Append(',');
            sb.Append('(');
            sb.Append(metadata.Parameters[i].TypeName);
            sb.Append(')');
            sb.Append(argName);
            sb.Append('[');
            sb.Append(i.ToString());
            sb.Append(']');
        }

        return sb.ToString();
    }

    public static string GenerateParameterTypeArrayString(this MethodMetadata metadata)
    {
        if (metadata.Parameters.Count == 0)
        {
            return "System.Type.EmptyTypes";
        }

        if (metadata.Parameters.Count == 1)
        {
            return $"new[] {{typeof({metadata.Parameters[0].TypeName})}}";
        }

        var sb = new StringBuilder();

        sb.Append("new[] {");
        sb.Append($"typeof({metadata.Parameters[0].TypeName})");

        for (int i = 1; i < metadata.Parameters.Count; i++)
        {
            sb.Append(',');
            sb.Append($"typeof({metadata.Parameters[i].TypeName})");
        }

        sb.Append('}');
        return sb.ToString();
    }

    public static string GenerateGenericReturnTypeArgString(this MethodMetadata metadata)
    {
        return metadata.IsGenericReturnType ? $"<{metadata.GenericReturnTypeArg}>" : string.Empty;
    }
}
