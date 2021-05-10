using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace TypedSignalR.Client
{
    public class SyntaxReceiver : ISyntaxReceiver
    {
        public List<(ClassDeclarationSyntax type, AttributeSyntax attr)> Targets { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                AttributeSyntax? attr = classDeclarationSyntax.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() is "HubClientBase" or "HubClientBaseAttribute");
                if (attr != null)
                {
                    Targets.Add((classDeclarationSyntax, attr));
                }
            }
        }
    }
}