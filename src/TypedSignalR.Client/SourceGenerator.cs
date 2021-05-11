using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TypedSignalR.Client
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx => ctx.AddSource("TypedSignalR.Client.EssentialComponent.cs", new EssentialComponent().TransformText()));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = context.SyntaxReceiver as SyntaxReceiver;

            if (receiver is null)
            {
                return;
            }

            var targetClassWithAttributeList = new List<(ClassDeclarationSyntax, AttributeProperty)>();

            foreach (var (targetType, attributeSyntax) in receiver.Targets)
            {
                var attributeProperty = ExtractAttributeProperty(context, targetType, attributeSyntax);
                targetClassWithAttributeList.Add((targetType, attributeProperty));
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

            var hubMethods = ExtractHubMethods(attributeProperty);
            var clientMethods = ExtractClientMethods(attributeProperty);

            var template = new CodeTemplate()
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

            throw new Exception("Set the HubClientBaseAttribute argument correctly.");
        }

        private static IReadOnlyList<MethodInfo> ExtractHubMethods(AttributeProperty attributeProperty)
        {
            var hubMethods = new List<MethodInfo>();
            foreach (ISymbol symbol in attributeProperty.HubTypeSymbol.GetMembers())
            {
                if (symbol is IMethodSymbol methodSymbol)
                {
                    var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                    INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // Task or Task<T>

                    if (returnTypeSymbol is null)
                    {
                        throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task or Task<T>");
                    }

                    ValidateHubReturnType(returnTypeSymbol, methodSymbol);

                    ITypeSymbol? genericArg = returnTypeSymbol.IsGenericType ? returnTypeSymbol.TypeArguments[0] : null;

                    var methodInfo = new MethodInfo(
                        methodSymbol.Name,
                        methodSymbol.ReturnType.ToDisplayString(),
                        parameters,
                        returnTypeSymbol.IsGenericType,
                        genericArg?.ToDisplayString());

                    hubMethods.Add(methodInfo);
                }
                else
                {
                    throw new Exception($"Define only methods in the interface. {symbol.ToDisplayString()} is not method.");
                }
            }

            return hubMethods;
        }

        private static IReadOnlyList<MethodInfo> ExtractClientMethods(AttributeProperty attributeProperty)
        {
            var clientMethods = new List<MethodInfo>();
            foreach (ISymbol symbol in attributeProperty.ClientTypeSymbol.GetMembers())
            {
                if (symbol is IMethodSymbol methodSymbol)
                {
                    INamedTypeSymbol? returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // only Task. not Task<T>.

                    if (returnTypeSymbol is null)
                    {
                        throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task.");
                    }

                    ValidateClientReturnType(returnTypeSymbol, methodSymbol);

                    var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                    var methodInfo = new MethodInfo(methodSymbol.Name, methodSymbol.ReturnType.ToDisplayString(), parameters, false, null);
                    clientMethods.Add(methodInfo);
                }
                else
                {
                    throw new Exception($"Define only methods in the interface. {symbol.ToDisplayString()} is not method.");
                }
            }

            return clientMethods;
        }

        private static void ValidateHubReturnType(INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                if (returnTypeSymbol.IsUnboundGenericType || returnTypeSymbol.ConstructUnboundGenericType().ToDisplayString() is not "System.Threading.Tasks.Task<>")
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task or Task<T>.");
                }
            }
            else
            {
                if (returnTypeSymbol.ToDisplayString() is not "System.Threading.Tasks.Task")
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task or Task<T>.");
                }
            }
        }

        private static void ValidateClientReturnType(INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task.");
            }
            else
            {
                if (returnTypeSymbol.ToDisplayString() is not "System.Threading.Tasks.Task")
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task.");
                }
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
