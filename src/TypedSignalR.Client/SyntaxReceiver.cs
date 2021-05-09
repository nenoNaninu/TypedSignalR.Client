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
                AttributeSyntax attr = classDeclarationSyntax.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() is "HubClientBase" or "HubClientBaseAttribute");
                if (attr != null)
                {
                    Targets.Add((classDeclarationSyntax, attr));
                }
            }
        }
    }

    //public class SyntaxContextReceiver : ISyntaxContextReceiver
    //{
    //    public List<(ClassDeclarationSyntax typeSyntax, ITypeSymbol typeSymbol, AttributeData attributeData)> Targets { get; } = new();

    //    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    //    {
    //        if (context.Node is ClassDeclarationSyntax classDeclarationSyntax &&
    //            classDeclarationSyntax.AttributeLists.Count > 0)
    //        {
    //            var typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as ITypeSymbol;

    //            if (typeSymbol == null)
    //            {
    //                return;
    //            }

    //            AttributeData attributeData = typeSymbol
    //                .GetAttributes()
    //                .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == "TypedSignalR.Client.HubClientBaseAttribute");

    //            if (attributeData == null)
    //            {
    //                return;
    //            }

    //            Targets.Add((classDeclarationSyntax, typeSymbol, attributeData));

    //            var e = attributeData.

    //            //var tv = attributeData.ApplicationSyntaxReference.SyntaxTree.GetRoot().DescendantNodes().OfType<TypeOfExpressionSyntax>();


    //            //attributeData.ConstructorArguments[0].Type

    //            SyntaxNode attributeSyntaxNode = attributeData.ApplicationSyntaxReference?.GetSyntax();
    //            //SyntaxNode attributeSyntaxNode = attributeData.ApplicationSyntaxReference?.GetSyntax();

    //            IMethodSymbol m = default;

    //            //m.Parameters[0].Type.ToDisplayString()

    //            if (attributeSyntaxNode == null)
    //            {
    //                return;
    //            }

    //            SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntaxNode);

    //            //context.SemanticModel.GetDeclaredSymbol()

    //            ISymbol attribSymbol = context.SemanticModel.GetDeclaredSymbol(attributeSyntaxNode);
    //            //context.SemanticModel.get

    //            string name = attributeData.NamedArguments[0].Key;
    //            //TypedConstant type = attributeData.NamedArguments[0].Value;
    //            //type.Type

    //            //var typedConstants = attributeData.ConstructorArguments;
    //            //TypedConstantKind kind = typedConstants[0].Kind;
    //            //SymbolKind
    //        }
    //    }
    //}
}