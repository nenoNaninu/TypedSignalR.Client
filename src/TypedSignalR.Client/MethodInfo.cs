using System.Collections.Generic;
using System.Text;

namespace TypedSignalR.Client
{
    public class MethodInfo
    {
        public string MethodName { get; }

        public IReadOnlyList<MethodParameter> Parameters { get; }

        public string ReturnType { get; }
        public bool IsGenericReturnType { get; }
        public string? GenericReturnTypeArg { get; }

        /// <param name="isGenericReturnType">if return type is Task<T>, must be true. if return type is Task or void, must be false. </param>
        /// <param name="genericReturnTypeArg">e.g. if return type is Task<Datetime>, you must be input System.Datetime. if not generics, you must be input null. </param>
        public MethodInfo(string methodName, IReadOnlyList<MethodParameter> parameters, string returnType, bool isGenericReturnType, string? genericReturnTypeArg)
        {
            MethodName = methodName;
            Parameters = parameters;
            ReturnType = returnType;
            IsGenericReturnType = isGenericReturnType;
            GenericReturnTypeArg = genericReturnTypeArg;
        }

        public string GenerateArgParameterString()
        {
            if (Parameters.Count == 0)
            {
                return string.Empty;
            }

            if (Parameters.Count == 1)
            {
                return $"{Parameters[0].TypeName} {Parameters[0].Name}";
            }

            var sb = new StringBuilder();

            sb.Append(Parameters[0].TypeName);
            sb.Append(' ');
            sb.Append(Parameters[0].Name);

            for (int i = 1; i < Parameters.Count; i++)
            {
                sb.Append(',');
                sb.Append(Parameters[i].TypeName);
                sb.Append(' ');
                sb.Append(Parameters[i].Name);
            }

            return sb.ToString();
        }

        public string GenerateArgNamesStringForInvokeCoreAsync()
        {
            if (Parameters.Count == 0)
            {
                return "System.Array.Empty<object>()";
            }

            var sb = new StringBuilder();

            sb.Append("new object[] {");
            sb.Append(Parameters[0].Name);

            for (int i = 1; i < Parameters.Count; i++)
            {
                sb.Append(',');
                sb.Append(Parameters[i].Name);
            }

            sb.Append("}");

            return sb.ToString();
        }

        public string GenerateTypeArgsFromParameterTypesString()
        {
            if (Parameters.Count == 0)
            {
                return string.Empty;
            }

            if (Parameters.Count == 1)
            {
                return $"<{Parameters[0].TypeName}>";
            }

            var sb = new StringBuilder();

            sb.Append('<');
            sb.Append(Parameters[0].TypeName);

            for (int i = 1; i < Parameters.Count; i++)
            {
                sb.Append(',');
                sb.Append(Parameters[i].TypeName);
            }

            sb.Append('>');
            return sb.ToString();
        }

        public string GenerateTypeArgsFromParameterTypesConcatenatedTaskString()
        {
            if (Parameters.Count == 0)
            {
                return "<System.Threading.Tasks.Task>";
            }

            if (Parameters.Count == 1)
            {
                return $"<{Parameters[0].TypeName},System.Threading.Tasks.Task>";
            }

            var sb = new StringBuilder();

            sb.Append('<');

            for (int i = 0; i < Parameters.Count; i++)
            {
                sb.Append(Parameters[i].TypeName);
                sb.Append(',');
            }

            sb.Append("System.Threading.Tasks.Task>");
            return sb.ToString();
        }

        public string GenerateCastedArgsString(string argName)
        {
            if (Parameters.Count == 0)
            {
                return string.Empty;
            }

            if (Parameters.Count == 1)
            {
                return $"({Parameters[0].TypeName}){argName}[0]";
            }

            var sb = new StringBuilder();

            sb.Append('(');
            sb.Append(Parameters[0].TypeName);
            sb.Append(')');
            sb.Append(argName);
            sb.Append("[0]");

            for (int i = 1; i < Parameters.Count; i++)
            {
                sb.Append(',');
                sb.Append('(');
                sb.Append(Parameters[i].TypeName);
                sb.Append(')');
                sb.Append(argName);
                sb.Append('[');
                sb.Append(i.ToString());
                sb.Append(']');
            }

            return sb.ToString();
        }

        public string GenerateParameterTypeArrayString()
        {
            if (Parameters.Count == 0)
            {
                return "System.Type.EmptyTypes";
            }

            if (Parameters.Count == 1)
            {
                return $"new[] {{typeof({Parameters[0].TypeName})}}";
            }

            var sb = new StringBuilder();

            sb.Append("new[] {");
            sb.Append($"typeof({Parameters[0].TypeName})");

            for (int i = 1; i < Parameters.Count; i++)
            {
                sb.Append(',');
                sb.Append($"typeof({Parameters[i].TypeName})");
            }

            sb.Append('}');
            return sb.ToString();
        }

        public string GenerateGenericReturnTypeArgString()
        {
            return IsGenericReturnType ? $"<{GenericReturnTypeArg}>" : string.Empty;
        }
    }
}
