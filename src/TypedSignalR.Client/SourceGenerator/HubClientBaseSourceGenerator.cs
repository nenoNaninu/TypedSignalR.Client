using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TypedSignalR.Client.T4;

namespace TypedSignalR.Client
{
    [Generator]
    public class HubClientBaseSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx => ctx.AddSource("TypedSignalR.Client.EssentialBaseComponent.cs", new EssentialBaseComponent().TransformText()));
            context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is AttributeSyntaxReceiver receiver)
            {
                ExecuteCore(context, receiver);
            }
        }

        private static void ExecuteCore(GeneratorExecutionContext context, AttributeSyntaxReceiver receiver)
        {
            var targetClassWithAttributeList = new List<(ClassDeclarationSyntax, AttributeProperty)>();

            foreach (var (targetType, attributeSyntax) in receiver.Targets)
            {
                var semanticModel = context.Compilation.GetSemanticModel(targetType.SyntaxTree);

                var hubClientBaseAttributeSymbol = semanticModel.Compilation.GetTypeByMetadataName("TypedSignalR.Client.HubClientBaseAttribute");
                var attributeSymbol = semanticModel.GetTypeInfo(attributeSyntax).ConvertedType;

                if (hubClientBaseAttributeSymbol!.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                {
                    var attributeProperty = ExtractAttributeProperty(context, targetType, attributeSyntax);
                    targetClassWithAttributeList.Add((targetType, attributeProperty));
                }
            }

            foreach (var (targetType, attributeProperty) in targetClassWithAttributeList)
            {
                var (isValid, hintName, source) = GenerateSource(context, targetType, attributeProperty);

                if (isValid)
                {
                    context.AddSource(hintName, source);
//#if DEBUG
//                    Debug.WriteLine(source);
//#endif       
                }
            }
        }

        private static (bool isValid, string hintName, string source) GenerateSource(GeneratorExecutionContext context, ClassDeclarationSyntax targetType, AttributeProperty attributeProperty)
        {
            INamedTypeSymbol? typeSymbol = context.Compilation.GetSemanticModel(targetType.SyntaxTree).GetDeclaredSymbol(targetType);

            if (typeSymbol is null)
            {
                return (false, string.Empty, string.Empty);
            }

            var semanticModel = context.Compilation.GetSemanticModel(targetType.SyntaxTree);

            var taskTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var genericTaskTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            var hubMethods = AnalysisUtility.ExtractHubMethods(attributeProperty.HubTypeSymbol, taskTypeSymbol!, genericTaskTypeSymbol!);
            var clientMethods = AnalysisUtility.ExtractClientMethods(attributeProperty.ClientTypeSymbol, taskTypeSymbol!);

            var template = new ClientBaseTemplate()
            {
                NameSpace = typeSymbol.ContainingNamespace.ToDisplayString(),
                TargetTypeName = typeSymbol.Name,
                HubInterfaceName = attributeProperty.HubTypeSymbol.ToDisplayString(),
                ClientInterfaceName = attributeProperty.ClientTypeSymbol.ToDisplayString(),
                HubMethods = hubMethods,
                ClientMethods = clientMethods
            };

            string text = template.TransformText();

            return (true, $"{template.NameSpace}.{template.TargetTypeName}.Generated.cs", text);
        }

        private static AttributeProperty ExtractAttributeProperty(GeneratorExecutionContext context, ClassDeclarationSyntax targetType, AttributeSyntax attributeSyntax)
        {
            var semanticModel = context.Compilation.GetSemanticModel(targetType.SyntaxTree);

            var hubArg = attributeSyntax.ArgumentList!.Arguments[0];

            ITypeSymbol? hubTypeSymbol = hubArg.Expression is TypeOfExpressionSyntax typeOfExpressionSyntaxHub
                ? semanticModel.GetSymbolInfo(typeOfExpressionSyntaxHub.Type).Symbol as ITypeSymbol : null;

            var clientArg = attributeSyntax.ArgumentList!.Arguments[1];

            ITypeSymbol? clientTypeSymbol = clientArg.Expression is TypeOfExpressionSyntax typeOfExpressionSyntaxClient
                ? semanticModel.GetSymbolInfo(typeOfExpressionSyntaxClient.Type).Symbol as ITypeSymbol : null;

            if (hubTypeSymbol is not null && clientTypeSymbol is not null)
            {
                return new AttributeProperty(hubTypeSymbol, clientTypeSymbol);
            }

            throw new Exception($"Set the HubClientBaseAttribute argument correctly. Hub: {attributeSyntax.ArgumentList!.Arguments[0]}, Client: {attributeSyntax.ArgumentList!.Arguments[1]}");
        }
    }

    internal readonly struct AttributeProperty
    {
        public readonly ITypeSymbol HubTypeSymbol;
        public readonly ITypeSymbol ClientTypeSymbol;

        public AttributeProperty(ITypeSymbol hubTypeSymbol, ITypeSymbol clientTypeSymbol)
        {
            HubTypeSymbol = hubTypeSymbol;
            ClientTypeSymbol = clientTypeSymbol;
        }
    }
}
