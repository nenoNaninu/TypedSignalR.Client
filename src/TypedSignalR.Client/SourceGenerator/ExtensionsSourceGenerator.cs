using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypedSignalR.Client.T4;

namespace TypedSignalR.Client.SourceGenerator
{
    [Generator]
    public class ExtensionsSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static ctx =>
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();

                ctx.AddSource("TypedSignalR.Client.Extensions.Generated.cs", new ExtensionsTemplate().TransformText());
            });

            var specialSymbols = context.CompilationProvider
                .Select(static (compilation, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return GetSpecialSymbols(compilation);
                });

            var methods = context.SyntaxProvider
                .CreateSyntaxProvider(Predicate, Transform)
                .Combine(specialSymbols)
                .Select(PostTransform)
                .Where(static x => x?.MethodSymbol is not null)
                .Collect();

            context.RegisterSourceOutput(methods.Combine(specialSymbols), GenerateSource);
        }

        private static bool Predicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                {
                    if (memberAccessExpressionSyntax.Name.Identifier.ValueText is "CreateHubProxy" or "Register")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static EssentialSymbols Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var invocationExpressionSyntax = context.Node as InvocationExpressionSyntax;
            var target = (invocationExpressionSyntax!.Expression as MemberAccessExpressionSyntax)!;

            var callerSymbol = context.SemanticModel.GetTypeInfo(target.Expression).Type;
            var extensionMethodSymbol = context.SemanticModel.GetSymbolInfo(target).Symbol as IMethodSymbol;

            return new EssentialSymbols(callerSymbol, extensionMethodSymbol, target.GetLocation());
        }

        private static MethodSymbolWithLocation? PostTransform((EssentialSymbols, SpecialSymbols) pair, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var callerSymbol = pair.Item1.CallerSymbol;
            var extensionMethodSymbol = pair.Item1.ExtensionMethodSymbol;
            var location = pair.Item1.Location;
            var specialSymbols = pair.Item2;

            if (callerSymbol is null)
            {
                return default;
            }

            if (extensionMethodSymbol is null)
            {
                return default;
            }

            if (!callerSymbol.Equals(specialSymbols.HubConnection, SymbolEqualityComparer.Default) ||
                !extensionMethodSymbol.ContainingNamespace.Equals(specialSymbols.TypedSignalRNamespace, SymbolEqualityComparer.Default))
            {
                return default;
            }

            return new MethodSymbolWithLocation(extensionMethodSymbol, location);
        }

        private static void GenerateSource(SourceProductionContext context, (ImmutableArray<MethodSymbolWithLocation?>, SpecialSymbols) data)
        {
            var methodSymbolWithLocations = data.Item1;
            var specialSymbols = data.Item2;

            var hubProxyMethodList = new List<MethodSymbolWithLocation>();
            var receiverMethodList = new List<MethodSymbolWithLocation>();

            foreach (var methodWithLocation in methodSymbolWithLocations)
            {
                var method = methodWithLocation!.MethodSymbol;

                if (method!.Name is "CreateHubProxy")
                {
                    hubProxyMethodList.Add(methodWithLocation);
                }
                else if (method.Name is "Register")
                {
                    receiverMethodList.Add(methodWithLocation);
                }
            }

            var hubProxyTypeList = ExtractFromCreateHubProxyMethods(context, hubProxyMethodList, specialSymbols);
            var receiverTypeList = ExtractFromRegisterMethods(context, receiverMethodList, specialSymbols);

            var template = new ExtensionsInternalTemplate()
            {
                HubProxyTypeList = hubProxyTypeList,
                ReceiverTypeList = receiverTypeList
            };

            var source = template.TransformText();

            Debug.WriteLine(source);

            context.AddSource("TypedSignalR.Client.Extensions.Internal.Generated.cs", source);
        }

        private static SpecialSymbols GetSpecialSymbols(Compilation compilation)
        {
            var hubConnectionSymbol = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Client.HubConnection");
            var taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var genericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var hubConnectionObserverSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.IHubConnectionObserver");
            var containingNamespace = compilation.GetTypeByMetadataName("TypedSignalR.Client.Extensions")?.ContainingNamespace;

            return new SpecialSymbols(hubConnectionSymbol!, taskSymbol!, genericTaskSymbol!, hubConnectionObserverSymbol!, containingNamespace!);
        }

        private static IReadOnlyList<HubProxyTypeInfo> ExtractFromCreateHubProxyMethods(
            SourceProductionContext context,
            IReadOnlyList<MethodSymbolWithLocation> createHubProxyMethods,
            SpecialSymbols specialSymbols)
        {
            var hubProxyTypeList = new List<HubProxyTypeInfo>(createHubProxyMethods.Count);

            foreach (var createHubProxyMethod in createHubProxyMethods)
            {
                var methodSymbol = createHubProxyMethod.MethodSymbol!;
                var location = createHubProxyMethod.Location;

                ITypeSymbol hubType = methodSymbol.TypeArguments[0];

                if (hubType.TypeKind != TypeKind.Interface)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.TypeArgumentRule,
                        location,
                        methodSymbol.OriginalDefinition.ToDisplayString(),
                        hubType.ToDisplayString()));

                    continue;
                }

                if (!hubProxyTypeList.Any(hubType))
                {
                    var (hubMethods, isValid) = AnalysisUtility.ExtractHubMethods(context, hubType, specialSymbols.Task, specialSymbols.GenericTask, location);

                    if (isValid)
                    {
                        var invoker = new HubProxyTypeInfo(hubType, hubMethods);
                        hubProxyTypeList.Add(invoker);
                    }
                }
            }

            return hubProxyTypeList;
        }

        private static IReadOnlyList<ReceiverTypeInfo> ExtractFromRegisterMethods(
            SourceProductionContext context,
            IReadOnlyList<MethodSymbolWithLocation> registerMethods,
            SpecialSymbols specialSymbols)
        {
            var receiverTypeList = new List<ReceiverTypeInfo>(registerMethods.Count);

            foreach (var registerMethod in registerMethods)
            {
                var methodSymbol = registerMethod.MethodSymbol!;
                var location = registerMethod.Location;

                ITypeSymbol receiverType = methodSymbol.TypeArguments[0];

                if (receiverType.TypeKind != TypeKind.Interface)
                {

                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorCollection.TypeArgumentRule,
                        location,
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
                    var (receiverMethods, isValid) = AnalysisUtility.ExtractClientMethods(context, receiverType, specialSymbols.Task, location);

                    if (isValid)
                    {
                        var receiverInfo = new ReceiverTypeInfo(receiverType, receiverMethods);
                        receiverTypeList.Add(receiverInfo);
                    }
                }
            }

            return receiverTypeList;
        }

        private class MethodSymbolWithLocation
        {
            public readonly IMethodSymbol? MethodSymbol;
            public readonly Location Location;

            public MethodSymbolWithLocation(IMethodSymbol? methodSymbol, Location location)
            {
                MethodSymbol = methodSymbol;
                Location = location;
            }
        }

        private struct EssentialSymbols
        {
            public readonly ITypeSymbol? CallerSymbol;
            public readonly IMethodSymbol? ExtensionMethodSymbol;
            public readonly Location Location;

            public EssentialSymbols(ITypeSymbol? callerSymbol, IMethodSymbol? extensionMethodSymbol, Location location)
            {
                CallerSymbol = callerSymbol;
                ExtensionMethodSymbol = extensionMethodSymbol;
                Location = location;
            }
        }

        private class SpecialSymbols
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
