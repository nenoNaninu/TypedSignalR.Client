using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypedSignalR.Client.SyntaxReceiver;
using TypedSignalR.Client.T4;

namespace TypedSignalR.Client.SourceGenerator
{
    [Generator]
    public class HubClientBaseSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx => ctx.AddSource("TypedSignalR.Client.HubClientBaseAttribute.Generated.cs", new HubClientBaseAttributeTemplate().TransformText()));
            context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is AttributeSyntaxReceiver receiver)
            {
                try
                {
                    ExecuteCore(context, receiver);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private static void ExecuteCore(in GeneratorExecutionContext context, AttributeSyntaxReceiver receiver)
        {
            var (taskSymbol, genericTaskSymbol) = GetSpecialSymbols(context);

            var targetClassWithAttributeList = new List<(ClassDeclarationSyntax, AttributeProperty)>();

            foreach (var (targetType, attributeSyntax) in receiver.Targets)
            {

                if (!targetType.Modifiers.Any("partial"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.AttributeAnnotationTargetTypeRule,
                        targetType.GetLocation(),
                        targetType.Identifier));

                    continue;
                }

                var semanticModel = context.Compilation.GetSemanticModel(targetType.SyntaxTree);

                var hubClientBaseAttributeSymbol = context.Compilation.GetTypeByMetadataName("TypedSignalR.Client.HubClientBaseAttribute");
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
                var (success, hintName, source) = GenerateSource(context, targetType, attributeProperty, taskSymbol!, genericTaskSymbol!);
                
                if (success)
                {
                    context.AddSource(hintName, source);
                    
                    Debug.WriteLine(source);
                }
            }
        }

        private static (INamedTypeSymbol? task, INamedTypeSymbol? genericTask) GetSpecialSymbols(in GeneratorExecutionContext context)
        {
            var taskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var genericTaskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            return (taskSymbol, genericTaskSymbol);
        }

        private static (bool isValid, string hintName, string source) GenerateSource(in GeneratorExecutionContext context, ClassDeclarationSyntax targetType, AttributeProperty attributeProperty, INamedTypeSymbol taskTypeSymbol, INamedTypeSymbol genericTaskTypeSymbol)
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

                return (true, $"{template.NameSpace}.{template.TargetTypeName}.ClientBase.Generated.cs", text);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
                return (false, string.Empty, string.Empty);
            }
        }

        private static AttributeProperty ExtractAttributeProperty(in GeneratorExecutionContext context, ClassDeclarationSyntax targetType, AttributeSyntax attributeSyntax)
        {
            bool isValid = true;
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

                isValid = false;
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

                isValid = false;
            }

            if (isValid)
            {
                return new AttributeProperty(hubTypeSymbol!, clientTypeSymbol!);
            }
            else
            {
                throw new Exception($"Set the HubClientBaseAttribute argument correctly. Hub: {attributeSyntax.ArgumentList!.Arguments[0]}, Client: {attributeSyntax.ArgumentList!.Arguments[1]}");
            }
        }

        private readonly struct AttributeProperty
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
}
