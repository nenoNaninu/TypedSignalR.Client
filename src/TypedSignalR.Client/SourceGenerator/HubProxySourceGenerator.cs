using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using TypedSignalR.Client.T4;

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
                var (invokerList, receiverList) = ExtructInfo(context, receiver);

                var template = new HubProxyTemplate()
                {
                    InvokerList = invokerList,
                    ReceiverList = receiverList
                };

                var sourceCode = template.TransformText();
//#if DEBUG
//                Debug.WriteLine(sourceCode);
//#endif
                context.AddSource("TypedSignalR.Client.HubProxy.Generated.cs", sourceCode);
            }
        }

        private static (IReadOnlyList<InvokerInfo> invokerList, IReadOnlyList<ReceiverInfo> receiverList) ExtructInfo(GeneratorExecutionContext context, HubProxyMethodSyntaxReceiver receiver)
        {
            List<InvokerInfo> invokerList = new();
            List<ReceiverInfo> receiverList = new();

            foreach (var target in receiver.CreateHubProxyMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var hubConnectionSymbol = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Client.HubConnection");
                var taskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var genericTaskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;

                if (hubConnectionSymbol!.Equals(callerSymbol, SymbolEqualityComparer.Default))
                {
                    var symbol = semanticModel.GetSymbolInfo(target).Symbol;

                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        ITypeSymbol hubType = methodSymbol.TypeArguments[0];

                        if (!invokerList.Any(hubType))
                        {
                            var hubMethods = AnalysisUtility.ExtractHubMethods(hubType, taskSymbol!, genericTaskSymbol!);

                            var invoker = new InvokerInfo(hubType, hubType.Name, hubType.ToDisplayString(), hubMethods);

                            invokerList.Add(invoker);
                        }
                    }
                }
            }

            foreach (var target in receiver.CreateHubProxyWithMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var hubConnectionSymbol = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Client.HubConnection");
                var taskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var genericTaskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;

                if (hubConnectionSymbol!.Equals(callerSymbol, SymbolEqualityComparer.Default))
                {
                    var symbol = semanticModel.GetSymbolInfo(target).Symbol;

                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        ITypeSymbol hubType = methodSymbol.TypeArguments[0];

                        if (!invokerList.Any(hubType))
                        {
                            var hubMethods = AnalysisUtility.ExtractHubMethods(hubType, taskSymbol!, genericTaskSymbol!);

                            var invoker = new InvokerInfo(hubType, hubType.Name, hubType.ToDisplayString(), hubMethods);

                            invokerList.Add(invoker);
                        }

                        ITypeSymbol reciverType = methodSymbol.TypeArguments[1];

                        if (!receiverList.Any(reciverType))
                        {
                            var reciverMethods = AnalysisUtility.ExtractClientMethods(reciverType, taskSymbol!);

                            var receiverInfo = new ReceiverInfo(reciverType, reciverType.Name, reciverType.ToDisplayString(), reciverMethods);

                            receiverList.Add(receiverInfo);
                        }
                    }
                }
            }

            foreach (var target in receiver.RegisterMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var hubConnectionSymbol = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Client.HubConnection");
                var taskSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;

                if (hubConnectionSymbol!.Equals(callerSymbol, SymbolEqualityComparer.Default))
                {
                    var symbol = semanticModel.GetSymbolInfo(target).Symbol;

                    if (symbol is IMethodSymbol methodSymbol)
                    {
                        ITypeSymbol reciverType = methodSymbol.TypeArguments[0];

                        if (!receiverList.Any(reciverType))
                        {
                            var reciverMethods = AnalysisUtility.ExtractClientMethods(reciverType, taskSymbol!);

                            var receiverInfo = new ReceiverInfo(reciverType, reciverType.Name, reciverType.ToDisplayString(), reciverMethods);

                            receiverList.Add(receiverInfo);
                        }
                    }
                }
            }

            return (invokerList, receiverList);
        }
    }
}
