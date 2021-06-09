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

        public string ArgParameterToString()
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

        public string ArgNamesToStringForInvokeCoreAsync()
        {
            if (Args.Count == 0)
            {
                return "System.Array.Empty<object>()";
            }

            var sb = new StringBuilder();

            sb.Append("new object[]{");
            sb.Append(Args[0].argName);


            for (int i = 1; i < Args.Count; i++)
            {
                sb.Append(',');
                sb.Append(Args[i].argName);
            }

            sb.Append("}");

            return sb.ToString();
        }

        public string TypeArgsToString()
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

        public string ReturnGenericTypeArgToString()
        {
            return IsGenericTypeReturn ? $"<{ReturnTypeGenericArg}>" : string.Empty;
        }
    }
}