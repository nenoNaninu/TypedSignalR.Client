using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypedSignalR.Client.SyntaxReceiver;
using TypedSignalR.Client.T4;

namespace TypedSignalR.Client.SourceGenerator
{
    [Generator]
    public class ExtensionsSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx => ctx.AddSource("TypedSignalR.Client.Extensions.Generated.cs", new ExtensionsTemplate().TransformText()));
            context.RegisterForSyntaxNotifications(() => new ExtensionMethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is ExtensionMethodSyntaxReceiver receiver)
            {
                try
                {
                    var (hubProxyTypeList, receiverTypeList) = ExtractTargetTypes(context, receiver);

                    var template = new ExtensionsInternalTemplate()
                    {
                        HubProxyTypeList = hubProxyTypeList,
                        ReceiverTypeList = receiverTypeList
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

        private static (IReadOnlyList<HubProxyTypeInfo> HubProxyTypeList, IReadOnlyList<ReceiverTypeInfo> ReceiverTypeList) ExtractTargetTypes(GeneratorExecutionContext context, ExtensionMethodSyntaxReceiver receiver)
        {
            var hubProxyTypeList = new List<HubProxyTypeInfo>();
            var receiverTypeList = new List<ReceiverTypeInfo>();

            var specialSymbols = GetSpecialSymbols(context);

            ExtractFromCreateHubProxyMethods(context, receiver.CreateHubProxyMethods, specialSymbols, hubProxyTypeList);
            ExtractFromCreateHubProxyWithMethods(context, receiver.CreateHubProxyWithMethods, specialSymbols, hubProxyTypeList, receiverTypeList);
            ExtractFromRegisterMethods(context, receiver.RegisterMethods, specialSymbols, receiverTypeList);

            return (hubProxyTypeList, receiverTypeList);
        }

        private static void ExtractFromCreateHubProxyMethods(
            GeneratorExecutionContext context,
            IReadOnlyList<MemberAccessExpressionSyntax> createHubProxyMethods,
            in SpecialSymbols specialSymbols,
            List<HubProxyTypeInfo> hubProxyTypeList)
        {
            foreach (var target in createHubProxyMethods)
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

                    if (!hubProxyTypeList.Any(hubType))
                    {
                        var (hubMethods, isValid) = AnalysisUtility.ExtractHubMethods(context, hubType, specialSymbols.Task, specialSymbols.GenericTask, target.GetLocation());

                        if (isValid)
                        {
                            var invoker = new HubProxyTypeInfo(hubType, hubMethods);
                            hubProxyTypeList.Add(invoker);
                        }
                    }
                }
            }
        }

        private static void ExtractFromCreateHubProxyWithMethods(
            GeneratorExecutionContext context,
            IReadOnlyList<MemberAccessExpressionSyntax> createHubProxyWithMethods,
            in SpecialSymbols specialSymbols,
            List<HubProxyTypeInfo> hubProxyTypeList,
            List<ReceiverTypeInfo> receiverTypeList)
        {
            foreach (var target in createHubProxyWithMethods)
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

                    if (!hubProxyTypeList.Any(hubType))
                    {
                        var (hubMethods, isValid) = AnalysisUtility.ExtractHubMethods(context, hubType, specialSymbols.Task, specialSymbols.GenericTask, target.GetLocation());

                        if (isValid)
                        {
                            var invoker = new HubProxyTypeInfo(hubType, hubMethods);
                            hubProxyTypeList.Add(invoker);
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

                    if (!receiverTypeList.Any(receiverType))
                    {
                        var (receiverMethods, isValid) = AnalysisUtility.ExtractClientMethods(context, receiverType, specialSymbols.Task, target.GetLocation());

                        if (isValid)
                        {
                            var receiverInfo = new ReceiverTypeInfo(receiverType, receiverMethods);
                            receiverTypeList.Add(receiverInfo);
                        }
                    }
                }
            }
        }

        private static void ExtractFromRegisterMethods(
            GeneratorExecutionContext context,
            IReadOnlyList<MemberAccessExpressionSyntax> registerMethods,
            in SpecialSymbols specialSymbols,
            List<ReceiverTypeInfo> receiverTypeList)
        {
            foreach (var target in registerMethods)
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

                    if (!receiverTypeList.Any(receiverType))
                    {
                        var (receiverMethods, isValid) = AnalysisUtility.ExtractClientMethods(context, receiverType, specialSymbols.Task, target.GetLocation());

                        if (isValid)
                        {
                            var receiverInfo = new ReceiverTypeInfo(receiverType, receiverMethods);
                            receiverTypeList.Add(receiverInfo);
                        }
                    }
                }
            }
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
