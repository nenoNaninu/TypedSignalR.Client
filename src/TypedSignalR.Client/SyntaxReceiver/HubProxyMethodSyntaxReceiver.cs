using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TypedSignalR.Client
{
    class HubProxyMethodSyntaxReceiver : ISyntaxReceiver
    {
        public List<MemberAccessExpressionSyntax> CreateHubProxyMethods { get; } = new();
        public List<MemberAccessExpressionSyntax> CreateHubProxyWithMethods { get; } = new();
        public List<MemberAccessExpressionSyntax> RegisterMethods { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                {
                    if (memberAccessExpressionSyntax.Name.Identifier.ValueText == "CreateHubProxy")
                    {
                        CreateHubProxyMethods.Add(memberAccessExpressionSyntax);
                        Debug.WriteLine(memberAccessExpressionSyntax.ToFullString());
                    }

                    if (memberAccessExpressionSyntax.Name.Identifier.ValueText == "CreateHubProxyWith")
                    {
                        CreateHubProxyWithMethods.Add(memberAccessExpressionSyntax);
                        Debug.WriteLine(memberAccessExpressionSyntax.ToFullString());
                    }

                    if (memberAccessExpressionSyntax.Name.Identifier.ValueText == "Register")
                    {
                        RegisterMethods.Add(memberAccessExpressionSyntax);
                        Debug.WriteLine(memberAccessExpressionSyntax.ToFullString());
                    }
                }
            }
        }
    }
}
