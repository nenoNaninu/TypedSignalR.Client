using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypedSignalR.Client.SyntaxReceiver
{
    public class ExtensionMethodSyntaxReceiver : ISyntaxReceiver
    {
        public IReadOnlyList<MemberAccessExpressionSyntax> CreateHubProxyMethods => _createHubProxyMethods;
        public IReadOnlyList<MemberAccessExpressionSyntax> CreateHubProxyWithMethods => _createHubProxyWithMethods;
        public IReadOnlyList<MemberAccessExpressionSyntax> RegisterMethods => _registerMethods;

        private readonly List<MemberAccessExpressionSyntax> _createHubProxyMethods = new();
        private readonly List<MemberAccessExpressionSyntax> _createHubProxyWithMethods = new();
        private readonly List<MemberAccessExpressionSyntax> _registerMethods = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                {
                    var list = memberAccessExpressionSyntax.Name.Identifier.ValueText switch
                    {
                        "CreateHubProxy" => _createHubProxyMethods,
                        "CreateHubProxyWith" => _createHubProxyWithMethods,
                        "Register" => _registerMethods,
                        _ => null
                    };

                    list?.Add(memberAccessExpressionSyntax);
                }
            }
        }
    }
}
