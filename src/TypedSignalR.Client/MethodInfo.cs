using System.Collections.Generic;
using System.Text;

namespace TypedSignalR.Client
{
    public class MethodInfo
    {
        public string MethodName { get; }
        public string ReturnValueType { get; }
        public IReadOnlyList<(string typeName, string argName)> Args { get; }

        public bool IsGenericTypeReturn { get; }
        public string? ReturnTypeGenericArg { get; }

        /// <param name="isGenericTypeReturn">if return type is Task<T>, must be true. if return type is Task, must be false. </param>
        /// <param name="returnTypeGenericArg">e.g. if return type is Task<Datetime>, you must be input System.Datetime. if not generics, you must be input null. </param>
        public MethodInfo(string methodName, string returnValueType, IReadOnlyList<(string typeName, string argName)> args, bool isGenericTypeReturn, string? returnTypeGenericArg)
        {
            MethodName = methodName;
            ReturnValueType = returnValueType;
            Args = args;
            IsGenericTypeReturn = isGenericTypeReturn;
            ReturnTypeGenericArg = returnTypeGenericArg;
        }

        public string GenerateArgParameterString()
        {
            if (Args.Count == 0)
            {
                return string.Empty;
            }

            if (Args.Count == 1)
            {
                return $"{Args[0].typeName} {Args[0].argName}";
            }

            var sb = new StringBuilder();

            sb.Append(Args[0].typeName);
            sb.Append(' ');
            sb.Append(Args[0].argName);

            for (int i = 1; i < Args.Count; i++)
            {
                sb.Append(',');
                sb.Append(Args[i].typeName);
                sb.Append(' ');
                sb.Append(Args[i].argName);
            }

            return sb.ToString();
        }

        public string GenerateArgNamesStringForInvokeCoreAsync()
        {
            if (Args.Count == 0)
            {
                return "System.Array.Empty<object>()";
            }

            var sb = new StringBuilder();

            sb.Append("new object[] {");
            sb.Append(Args[0].argName);

            for (int i = 1; i < Args.Count; i++)
            {
                sb.Append(',');
                sb.Append(Args[i].argName);
            }

            sb.Append("}");

            return sb.ToString();
        }

        public string GenerateTypeArgsFromArgTypesString()
        {
            if (Args.Count == 0)
            {
                return string.Empty;
            }

            if (Args.Count == 1)
            {
                return $"<{Args[0].typeName}>";
            }

            var sb = new StringBuilder();

            sb.Append('<');
            sb.Append(Args[0].typeName);

            for (int i = 1; i < Args.Count; i++)
            {
                sb.Append(',');
                sb.Append(Args[i].typeName);
            }

            sb.Append('>');
            return sb.ToString();
        }

        public string GenerateTypeArgsFromArgTypesConcatenatedTaskString()
        {
            if (Args.Count == 0)
            {
                return "<System.Threading.Tasks.Task>";
            }

            if (Args.Count == 1)
            {
                return $"<{Args[0].typeName},System.Threading.Tasks.Task>";
            }

            var sb = new StringBuilder();

            sb.Append('<');

            for (int i = 0; i < Args.Count; i++)
            {
                sb.Append(Args[i].typeName);
                sb.Append(',');
            }

            sb.Append("System.Threading.Tasks.Task>");
            return sb.ToString();
        }

        public string GenerateCastedArgsString(string argName)
        {
            if (Args.Count == 0)
            {
                return string.Empty;
            }

            if (Args.Count == 1)
            {
                return $"({Args[0].typeName}){argName}[0]";
            }

            var sb = new StringBuilder();

            sb.Append('(');
            sb.Append(Args[0].typeName);
            sb.Append(')');
            sb.Append(argName);
            sb.Append("[0]");

            for (int i = 1; i < Args.Count; i++)
            {
                sb.Append(',');
                sb.Append('(');
                sb.Append(Args[i].typeName);
                sb.Append(')');
                sb.Append(argName);
                sb.Append('[');
                sb.Append(i.ToString());
                sb.Append(']');
            }

            return sb.ToString();
        }

        public string GenerateArgTypeArrayString()
        {
            if (Args.Count == 0)
            {
                return "System.Type.EmptyTypes";
            }

            if (Args.Count == 1)
            {
                return $"new[] {{typeof({Args[0].typeName})}}";
            }

            var sb = new StringBuilder();

            sb.Append("new[] {");
            sb.Append($"typeof({Args[0].typeName})");

            for (int i = 1; i < Args.Count; i++)
            {
                sb.Append(',');
                sb.Append($"typeof({Args[i].typeName})");
            }

            sb.Append('}');
            return sb.ToString();
        }

        public string GenerateReturnGenericTypeArgString()
        {
            return IsGenericTypeReturn ? $"<{ReturnTypeGenericArg}>" : string.Empty;
        }
    }
}
