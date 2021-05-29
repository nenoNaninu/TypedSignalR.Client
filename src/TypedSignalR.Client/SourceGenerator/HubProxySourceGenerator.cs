using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using TypedSignalR.Client.T4;
using System.Linq;
using System;

namespace TypedSignalR.Client
{
    [Generator]
    class HubProxySourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx => ctx.AddSource("TypedSignalR.Client.EssentialHubProxyComponent.cs", new EssentialHubProxyComponent().TransformText()));
            context.RegisterForSyntaxNotifications(() => new HubProxyMethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is HubProxyMethodSyntaxReceiver receiver)
            {
                try
                {
                    var (invokerList, receiverList) = ExtructInfo(context, receiver);

                    var template = new HubProxyTemplate()
                    {
                        InvokerList = invokerList,
                        ReceiverList = receiverList
                    };

                    var source = template.TransformText();

                    Debug.WriteLine(source);

                    context.AddSource("TypedSignalR.Client.HubProxy.Generated.cs", source);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private static (bool isVaild, INamedTypeSymbol? hubConnection, INamedTypeSymbol? task, INamedTypeSymbol? genericTask) GetImportantSymbols(GeneratorExecutionContext context, HubProxyMethodSyntaxReceiver receiver)
        {
            var firstSyntax = receiver.CreateHubProxyMethods.FirstOrDefault()
                ?? receiver.CreateHubProxyWithMethods.FirstOrDefault()
                ?? receiver.RegisterMethods.FirstOrDefault();

            if (firstSyntax is null)
            {
                return (false, null, null, null);
            }

            var semanticModel = context.Compilation.GetSemanticModel(firstSyntax.SyntaxTree);

            var hubConnectionSymbol = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Client.HubConnection");
            var taskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var genericTaskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            return (true, hubConnectionSymbol, taskSymbol, genericTaskSymbol);
        }

        private static (IReadOnlyList<InvokerInfo> invokerList, IReadOnlyList<ReceiverInfo> receiverList) ExtructInfo(GeneratorExecutionContext context, HubProxyMethodSyntaxReceiver receiver)
        {
            List<InvokerInfo> invokerList = new();
            List<ReceiverInfo> receiverList = new();

            var (isVaild, hubConnectionSymbol, taskSymbol, genericTaskSymbol) = GetImportantSymbols(context, receiver);

            if (!isVaild)
            {
                return (invokerList, receiverList);
            }

            foreach (var target in receiver.CreateHubProxyMethods)
            {

                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;

                if (hubConnectionSymbol!.Equals(callerSymbol, SymbolEqualityComparer.Default))
                {
                    var symbol = semanticModel.GetSymbolInfo(target).Symbol;

                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        ITypeSymbol hubType = methodSymbol.TypeArguments[0];

                        if (hubType.TypeKind != TypeKind.Interface)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptorCollection.TypeArgumentRule,
                                target.GetLocation(),
                                methodSymbol.OriginalDefinition.ToDisplayString(),
                                hubType.ToDisplayString()));

                            continue;
                        }

                        if (!invokerList.Any(hubType))
                        {
                            try
                            {
                                var hubMethods = AnalysisUtility.ExtractHubMethods(context, hubType, taskSymbol!, genericTaskSymbol!);

                                var invoker = new InvokerInfo(hubType, hubType.Name, hubType.ToDisplayString(), hubMethods);

                                invokerList.Add(invoker);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
                    }
                }
            }

            foreach (var target in receiver.CreateHubProxyWithMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;

                if (hubConnectionSymbol!.Equals(callerSymbol, SymbolEqualityComparer.Default))
                {
                    var symbol = semanticModel.GetSymbolInfo(target).Symbol;

                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        ITypeSymbol hubType = methodSymbol.TypeArguments[0];

                        if (hubType.TypeKind != TypeKind.Interface)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptorCollection.TypeArgumentRule,
                                target.GetLocation(),
                                methodSymbol.OriginalDefinition.ToDisplayString(),
                                hubType.ToDisplayString()));

                            continue;
                        }

                        if (!invokerList.Any(hubType))
                        {
                            try
                            {
                                var hubMethods = AnalysisUtility.ExtractHubMethods(context, hubType, taskSymbol!, genericTaskSymbol!);

                                var invoker = new InvokerInfo(hubType, hubType.Name, hubType.ToDisplayString(), hubMethods);

                                invokerList.Add(invoker);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }

                        ITypeSymbol reciverType = methodSymbol.TypeArguments[1];

                        if (reciverType.TypeKind != TypeKind.Interface)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptorCollection.TypeArgumentRule,
                                target.GetLocation(),
                                methodSymbol.OriginalDefinition.ToDisplayString(),
                                reciverType.ToDisplayString()));

                            continue;
                        }

                        if (!receiverList.Any(reciverType))
                        {
                            try
                            {
                                var reciverMethods = AnalysisUtility.ExtractClientMethods(context, reciverType, taskSymbol!);

                                var receiverInfo = new ReceiverInfo(reciverType, reciverType.Name, reciverType.ToDisplayString(), reciverMethods);

                                receiverList.Add(receiverInfo);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
                    }
                }
            }

            foreach (var target in receiver.RegisterMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;

                if (hubConnectionSymbol!.Equals(callerSymbol, SymbolEqualityComparer.Default))
                {
                    var symbol = semanticModel.GetSymbolInfo(target).Symbol;

                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        ITypeSymbol reciverType = methodSymbol.TypeArguments[0];

                        if (reciverType.TypeKind != TypeKind.Interface)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptorCollection.TypeArgumentRule,
                                target.GetLocation(),
                                methodSymbol.OriginalDefinition.ToDisplayString(),
                                reciverType.ToDisplayString()));

                            continue;
                        }

                        if (!receiverList.Any(reciverType))
                        {
                            try
                            {
                                var reciverMethods = AnalysisUtility.ExtractClientMethods(context, reciverType, taskSymbol!);

                                var receiverInfo = new ReceiverInfo(reciverType, reciverType.Name, reciverType.ToDisplayString(), reciverMethods);

                                receiverList.Add(receiverInfo);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
                    }
                }
            }

            return (invokerList, receiverList);
        }
    }
}
