using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TypedSignalR.Client.T4;
using System.Linq;

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
            var (isVaild, taskSymbol, genericTaskSymbol) = GetImportantSymbols(context, receiver);

            if (!isVaild)
            {
                return;
            }

            var targetClassWithAttributeList = new List<(ClassDeclarationSyntax, AttributeProperty)>();

            foreach (var (targetType, attributeSyntax) in receiver.Targets)
            {
                var semanticModel = context.Compilation.GetSemanticModel(targetType.SyntaxTree);

                var hubClientBaseAttributeSymbol = semanticModel.Compilation.GetTypeByMetadataName("TypedSignalR.Client.HubClientBaseAttribute");
                var attributeSymbol = semanticModel.GetTypeInfo(attributeSyntax).ConvertedType;

                if (hubClientBaseAttributeSymbol!.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                {
                    try
                    {
                        var attributeProperty = ExtractAttributeProperty(context, targetType, attributeSyntax);
                        targetClassWithAttributeList.Add((targetType, attributeProperty));
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }

            foreach (var (targetType, attributeProperty) in targetClassWithAttributeList)
            {
                var (isValid, hintName, source) = GenerateSource(context, targetType, attributeProperty, taskSymbol!, genericTaskSymbol!);

                if (isValid)
                {
                    context.AddSource(hintName, source);
                    
                    Debug.WriteLine(source);
                }
            }
        }

        private static (bool isVaild, INamedTypeSymbol? task, INamedTypeSymbol? genericTask) GetImportantSymbols(GeneratorExecutionContext context, AttributeSyntaxReceiver receiver)
        {
            var (firstSyntax, _) = receiver.Targets.FirstOrDefault();

            if (firstSyntax is null)
            {
                return (false, null, null);
            }

            var semanticModel = context.Compilation.GetSemanticModel(firstSyntax.SyntaxTree);

            var taskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var genericTaskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            return (true, taskSymbol, genericTaskSymbol);
        }

        private static (bool isValid, string hintName, string source) GenerateSource(GeneratorExecutionContext context, ClassDeclarationSyntax targetType, AttributeProperty attributeProperty, INamedTypeSymbol taskTypeSymbol, INamedTypeSymbol genericTaskTypeSymbol)
        {
            INamedTypeSymbol? typeSymbol = context.Compilation.GetSemanticModel(targetType.SyntaxTree).GetDeclaredSymbol(targetType);

            if (typeSymbol is null)
            {
                return (false, string.Empty, string.Empty);
            }

            try
            {
                var hubMethods = AnalysisUtility.ExtractHubMethods(context, attributeProperty.HubTypeSymbol, taskTypeSymbol!, genericTaskTypeSymbol!);
                var clientMethods = AnalysisUtility.ExtractClientMethods(context, attributeProperty.ClientTypeSymbol, taskTypeSymbol!);

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
            catch(Exception e)
            {
                Debug.WriteLine(e);
                return (false, string.Empty, string.Empty);
            }
        }

        private static AttributeProperty ExtractAttributeProperty(GeneratorExecutionContext context, ClassDeclarationSyntax targetType, AttributeSyntax attributeSyntax)
        {
            bool isVaild = true;
            var semanticModel = context.Compilation.GetSemanticModel(targetType.SyntaxTree);

            var hubArg = attributeSyntax.ArgumentList!.Arguments[0];

            ITypeSymbol? hubTypeSymbol = hubArg.Expression is TypeOfExpressionSyntax typeOfExpressionSyntaxHub
                ? semanticModel.GetSymbolInfo(typeOfExpressionSyntaxHub.Type).Symbol as ITypeSymbol : null;

            if (hubTypeSymbol?.TypeKind != TypeKind.Interface)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorCollection.AttributeArgumentRule,
                    attributeSyntax.GetLocation(),
                    hubTypeSymbol?.ToDisplayString()));

                isVaild = false;
            }

            var clientArg = attributeSyntax.ArgumentList!.Arguments[1];

            ITypeSymbol? clientTypeSymbol = clientArg.Expression is TypeOfExpressionSyntax typeOfExpressionSyntaxClient
                ? semanticModel.GetSymbolInfo(typeOfExpressionSyntaxClient.Type).Symbol as ITypeSymbol : null;

            if (clientTypeSymbol?.TypeKind != TypeKind.Interface)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorCollection.AttributeArgumentRule,
                    attributeSyntax.GetLocation(),
                    clientTypeSymbol?.ToDisplayString()));

                isVaild = false;
            }

            if (isVaild)
            {
                return new AttributeProperty(hubTypeSymbol!, clientTypeSymbol!);
            }
            else
            {
                throw new Exception($"Set the HubClientBaseAttribute argument correctly. Hub: {attributeSyntax.ArgumentList!.Arguments[0]}, Client: {attributeSyntax.ArgumentList!.Arguments[1]}");
            }
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
