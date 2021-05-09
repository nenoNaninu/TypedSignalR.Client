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
            context.RegisterForPostInitialization(ctx => ctx.AddSource("TypedSignalR.Client.Component.cs", new RequiredComponent().TransformText()));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = context.SyntaxReceiver as SyntaxReceiver;
            if (receiver == null) return;

            var targetClassWithAttributeList = new List<(ClassDeclarationSyntax, AttributeProperty)>();

            foreach (var (targetType, attributeSyntax) in receiver.Targets)
            {
                var semanticModel = context.Compilation.GetSemanticModel(targetType.SyntaxTree);

                var attributeProperty = new AttributeProperty();

                var hubArg = attributeSyntax.ArgumentList.Arguments[0];
                if (hubArg.Expression is TypeOfExpressionSyntax typeOfExpressionSyntaxHub)
                {
                    attributeProperty.HubTypeSymbol = semanticModel.GetSymbolInfo(typeOfExpressionSyntaxHub.Type).Symbol as ITypeSymbol;
                }
                else
                {
                    throw new Exception();
                }

                var clientArg = attributeSyntax.ArgumentList.Arguments[1];
                if (clientArg.Expression is TypeOfExpressionSyntax typeOfExpressionSyntaxClient)
                {
                    attributeProperty.ClientTypeSymbol = semanticModel.GetSymbolInfo(typeOfExpressionSyntaxClient.Type).Symbol as ITypeSymbol;
                }
                else
                {
                    throw new Exception();
                }

                targetClassWithAttributeList.Add((targetType, attributeProperty));
            }

            foreach (var (targetType, attributeProperty) in targetClassWithAttributeList)
            {
                INamedTypeSymbol typeSymbol = context.Compilation.GetSemanticModel(targetType.SyntaxTree).GetDeclaredSymbol(targetType);

                if (typeSymbol == null)
                {
                    continue;
                }

                var clientMethods = new List<MethodInfo>();
                foreach (ISymbol symbol in attributeProperty.ClientTypeSymbol.GetMembers())
                {
                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        INamedTypeSymbol returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // Task or Task<T>

                        ValidateReturnType(returnTypeSymbol, methodSymbol);

                        var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                        var methodInfo = new MethodInfo(methodSymbol.Name, methodSymbol.ReturnType.ToDisplayString(), parameters, false, null);
                        clientMethods.Add(methodInfo);
                    }
                    else
                    {
                        throw new Exception($"Define only methods in the interface. {symbol.ToDisplayString()} is not method.");
                    }
                }

                var hubMethods = new List<MethodInfo>();
                foreach (ISymbol symbol in attributeProperty.HubTypeSymbol.GetMembers())
                {
                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        var parameters = methodSymbol.Parameters.Select(x => (x.Type.ToDisplayString(), x.Name)).ToArray();
                        //var typeArg = methodSymbol.TypeArguments;
                        INamedTypeSymbol returnTypeSymbol = methodSymbol.ReturnType as INamedTypeSymbol; // Task or Task<T>

                        ValidateReturnType(returnTypeSymbol, methodSymbol);

                        ITypeSymbol genericArg = returnTypeSymbol.IsGenericType ? returnTypeSymbol.TypeArguments[0] : null;

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

                var template = new CodeTemplate()
                {
                    NameSpace = typeSymbol.ContainingNamespace.ToDisplayString(),
                    TypeName = typeSymbol.Name,
                    HubInterfaceName = attributeProperty.HubTypeSymbol.ToDisplayString(),
                    ClientInterfaceName = attributeProperty.ClientTypeSymbol.ToDisplayString(),
                    HubMethods = hubMethods,
                    ClientMethods = clientMethods
                };

                var text = template.TransformText();
                Debug.WriteLine(text);
                context.AddSource($"{template.NameSpace}.{template.TypeName}.Generated.cs", text);
            }
        }

        private void ValidateReturnType(INamedTypeSymbol returnTypeSymbol, IMethodSymbol methodSymbol)
        {
            if (returnTypeSymbol.IsGenericType)
            {
                if (returnTypeSymbol.BaseType.ToDisplayString() is not "System.Threading.Tasks.Task")
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task of Task<T>");
                }
            }
            else
            {
                if (returnTypeSymbol.ToDisplayString() is not "System.Threading.Tasks.Task")
                {
                    throw new Exception($"return type of {methodSymbol.ToDisplayString()} must be Task of Task<T>");
                }
            }
        }
    }

    struct AttributeProperty
    {
        public ITypeSymbol ClientTypeSymbol { get; set; }
        public ITypeSymbol HubTypeSymbol { get; set; }
    }
}
