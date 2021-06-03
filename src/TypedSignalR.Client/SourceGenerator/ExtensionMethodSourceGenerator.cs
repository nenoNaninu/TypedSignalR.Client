﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using TypedSignalR.Client.SyntaxReceiver;
using TypedSignalR.Client.T4;

namespace TypedSignalR.Client.SourceGenerator
{
    [Generator]
    class ExtensionMethodSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx => ctx.AddSource("TypedSignalR.Client.Extensions.Generated.cs", new ExtensionMethodTemplate().TransformText()));
            context.RegisterForSyntaxNotifications(() => new ExtensionMethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is ExtensionMethodSyntaxReceiver receiver)
            {
                try
                {
                    var (invokerList, receiverList) = ExtractTargetTypes(context, receiver);

                    var template = new ExtensionMethodInternalTemplate()
                    {
                        InvokerList = invokerList,
                        ReceiverList = receiverList
                    };

                    var source = template.TransformText();

                    Debug.WriteLine(source);

                    context.AddSource("TypedSignalR.Client.Extensions.Internal.Generated.cs", source);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private static SpecialSymbols GetSpecialSymbols(GeneratorExecutionContext context)
        {
            var hubConnectionSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Client.HubConnection");
            var taskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var genericTaskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var hubConnectionObserverSymbol = context.Compilation.GetTypeByMetadataName("TypedSignalR.Client.IHubConnectionObserver");
            var containingNamespace = context.Compilation.GetTypeByMetadataName("TypedSignalR.Client.Extensions")?.ContainingNamespace;
            
            return new SpecialSymbols(hubConnectionSymbol!, taskSymbol!, genericTaskSymbol!, hubConnectionObserverSymbol!, containingNamespace!);
        }

        private static (IReadOnlyList<InvokerTypeInfo> invokerList, IReadOnlyList<ReceiverTypeInfo> receiverList) ExtractTargetTypes(GeneratorExecutionContext context, ExtensionMethodSyntaxReceiver receiver)
        {
            List<InvokerTypeInfo> invokerList = new();
            List<ReceiverTypeInfo> receiverList = new();

            var specialSymbols = GetSpecialSymbols(context);

            foreach (var target in receiver.CreateHubProxyMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);
                
                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;
                var createHubProxySymbol = semanticModel.GetSymbolInfo(target).Symbol;

                if (callerSymbol is null)
                {
                    continue;
                }

                if (createHubProxySymbol is null)
                {
                    continue;
                }

                if (!callerSymbol.Equals(specialSymbols.HubConnection, SymbolEqualityComparer.Default) ||
                    !createHubProxySymbol.ContainingNamespace.Equals(specialSymbols.TypedSignalRNamespace, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                if (createHubProxySymbol is IMethodSymbol methodSymbol)
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
                            var hubMethods = AnalysisUtility.ExtractHubMethods(context, hubType, specialSymbols.Task, specialSymbols.GenericTask);

                            var invoker = new InvokerTypeInfo(hubType, hubType.Name, hubType.ToDisplayString(), hubMethods);

                            invokerList.Add(invoker);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }
            }

            foreach (var target in receiver.CreateHubProxyWithMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;
                var createHubProxyWithSymbol = semanticModel.GetSymbolInfo(target).Symbol;

                if (callerSymbol is null)
                {
                    continue;
                }

                if (createHubProxyWithSymbol is null)
                {
                    continue;
                }

                if (!callerSymbol.Equals(specialSymbols.HubConnection, SymbolEqualityComparer.Default) ||
                    !createHubProxyWithSymbol.ContainingNamespace.Equals(specialSymbols.TypedSignalRNamespace, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                if (createHubProxyWithSymbol is IMethodSymbol methodSymbol)
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
                            var hubMethods = AnalysisUtility.ExtractHubMethods(context, hubType, specialSymbols.Task, specialSymbols.GenericTask);

                            var invoker = new InvokerTypeInfo(hubType, hubType.Name, hubType.ToDisplayString(), hubMethods);

                            invokerList.Add(invoker);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }

                    ITypeSymbol receiverType = methodSymbol.TypeArguments[1];

                    if (receiverType.TypeKind != TypeKind.Interface)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptorCollection.TypeArgumentRule,
                            target.GetLocation(),
                            methodSymbol.OriginalDefinition.ToDisplayString(),
                            receiverType.ToDisplayString()));

                        continue;
                    }

                    if (!receiverList.Any(receiverType))
                    {
                        try
                        {
                            var receiverMethods = AnalysisUtility.ExtractClientMethods(context, receiverType, specialSymbols.Task);

                            var receiverInfo = new ReceiverTypeInfo(receiverType, receiverType.Name, receiverType.ToDisplayString(), receiverMethods);

                            receiverList.Add(receiverInfo);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }
            }

            foreach (var target in receiver.RegisterMethods)
            {
                var semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);

                var callerSymbol = semanticModel.GetTypeInfo(target.Expression).Type;
                var registerSymbol = semanticModel.GetSymbolInfo(target).Symbol;

                if (callerSymbol is null)
                {
                    continue;
                }

                if (registerSymbol is null)
                {
                    continue;
                }

                if (!callerSymbol.Equals(specialSymbols.HubConnection, SymbolEqualityComparer.Default) || 
                    !registerSymbol.ContainingNamespace.Equals(specialSymbols.TypedSignalRNamespace, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                if (registerSymbol is IMethodSymbol methodSymbol)
                {
                    ITypeSymbol receiverType = methodSymbol.TypeArguments[0];

                    if (receiverType.TypeKind != TypeKind.Interface)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptorCollection.TypeArgumentRule,
                            target.GetLocation(),
                            methodSymbol.OriginalDefinition.ToDisplayString(),
                            receiverType.ToDisplayString()));

                        continue;
                    }

                    if (receiverType.Equals(specialSymbols.HubConnectionObserver, SymbolEqualityComparer.Default))
                    {
                        continue;
                    }

                    if (!receiverList.Any(receiverType))
                    {
                        try
                        {
                            var receiverMethods = AnalysisUtility.ExtractClientMethods(context, receiverType, specialSymbols.Task);

                            var receiverInfo = new ReceiverTypeInfo(receiverType, receiverType.Name, receiverType.ToDisplayString(), receiverMethods);

                            receiverList.Add(receiverInfo);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }
            }

            return (invokerList, receiverList);
        }

        private readonly struct SpecialSymbols
        {
            public readonly INamedTypeSymbol HubConnection;
            public readonly INamedTypeSymbol Task;
            public readonly INamedTypeSymbol GenericTask;
            public readonly INamedTypeSymbol HubConnectionObserver;
            public readonly INamespaceSymbol TypedSignalRNamespace;

            public SpecialSymbols(
                INamedTypeSymbol hubConnection, 
                INamedTypeSymbol task, 
                INamedTypeSymbol genericTask, 
                INamedTypeSymbol hubConnectionObserver,
                INamespaceSymbol typedSignalRNamespace
               )
            {
                HubConnection = hubConnection;
                Task = task;
                GenericTask = genericTask;
                HubConnectionObserver = hubConnectionObserver;
                TypedSignalRNamespace = typedSignalRNamespace;
            }
        }
    }
}
