using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypedSignalR.Client
{
    public class AttributeSyntaxReceiver : ISyntaxReceiver
    {
        public IReadOnlyList<(ClassDeclarationSyntax type, AttributeSyntax attr)> Targets => _targets;

        private readonly List<(ClassDeclarationSyntax type, AttributeSyntax attr)> _targets  = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                AttributeSyntax? attr = classDeclarationSyntax.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() is "HubClientBase" or "HubClientBaseAttribute");

                if (attr != null)
                {
                    _targets.Add((classDeclarationSyntax, attr));
                }
            }
        }
    }
}