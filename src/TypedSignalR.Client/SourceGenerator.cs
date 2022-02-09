using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypedSignalR.Client.CodeAnalysis;
using TypedSignalR.Client.T4;

namespace TypedSignalR.Client;

[Generator]
public sealed class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();

            ctx.AddSource("TypedSignalR.Client.Components.Generated.cs", new ComponentsTemplate().TransformText());
            ctx.AddSource("TypedSignalR.Client.HubConnectionExtensions.Generated.cs", new HubConnectionExtensionsTemplate().TransformText());
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
            .Where(static x => x is not null)
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
            return null;
        }

        if (extensionMethodSymbol is null)
        {
            return null;
        }

        var sourceMethod = extensionMethodSymbol.ReducedFrom;

        if (sourceMethod is null)
        {
            return null;
        }

        if (SymbolEqualityComparer.Default.Equals(sourceMethod, specialSymbols.CreateHubProxySymbol))
        {
            return new MethodSymbolWithLocation(extensionMethodSymbol, location, ExtensionMethod.CreateHubProxy);
        }

        if (SymbolEqualityComparer.Default.Equals(sourceMethod, specialSymbols.RegisterSymbol))
        {
            return new MethodSymbolWithLocation(extensionMethodSymbol, location, ExtensionMethod.Register);
        }

        return null;
    }

    private static void GenerateSource(SourceProductionContext context, (ImmutableArray<MethodSymbolWithLocation?>, SpecialSymbols) data)
    {
        var methodSymbolWithLocations = data.Item1;
        var specialSymbols = data.Item2;

        var hubProxyMethodList = new List<MethodSymbolWithLocation>();
        var receiverMethodList = new List<MethodSymbolWithLocation>();

        foreach (var methodWithLocation in methodSymbolWithLocations)
        {
            if (methodWithLocation is null)
            {
                continue;
            }

            if (methodWithLocation.ExtensionMethod is ExtensionMethod.CreateHubProxy)
            {
                hubProxyMethodList.Add(methodWithLocation);
            }
            else if (methodWithLocation.ExtensionMethod is ExtensionMethod.Register)
            {
                receiverMethodList.Add(methodWithLocation);
            }
        }

        var hubProxyTypeList = ExtractFromCreateHubProxyMethods(context, hubProxyMethodList, specialSymbols);
        var receiverTypeList = ExtractFromRegisterMethods(context, receiverMethodList, specialSymbols);

        var template = new HubConnectionExtensionsCoreTemplate()
        {
            HubProxyTypeList = hubProxyTypeList,
            ReceiverTypeList = receiverTypeList
        };

        var source = template.TransformText();

        Debug.WriteLine(source);

        context.AddSource("TypedSignalR.Client.HubConnectionExtensions.Core.Generated.cs", source);
    }

    private static SpecialSymbols GetSpecialSymbols(Compilation compilation)
    {
        var taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var genericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var hubConnectionObserverSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.IHubConnectionObserver");
        var membersSymbols = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubConnectionExtensions")!.GetMembers()!;

        IMethodSymbol? createHubProxySymbol = null;
        IMethodSymbol? registerSymbol = null;

        foreach (var symbol in membersSymbols)
        {
            if (symbol.Name is "CreateHubProxy")
            {
                if (symbol is IMethodSymbol method)
                {
                    createHubProxySymbol = method;

                    if (registerSymbol is not null)
                    {
                        break;
                    }
                }
            }
            else if (symbol.Name is "Register")
            {
                if (symbol is IMethodSymbol method)
                {
                    registerSymbol = method;

                    if (createHubProxySymbol is not null)
                    {
                        break;
                    }
                }
            }
        }

        return new SpecialSymbols(taskSymbol!, genericTaskSymbol!, hubConnectionObserverSymbol!, createHubProxySymbol!, registerSymbol!);
    }

    private static IReadOnlyList<HubProxyTypeMetadata> ExtractFromCreateHubProxyMethods(
        SourceProductionContext context,
        IReadOnlyList<MethodSymbolWithLocation> createHubProxyMethods,
        SpecialSymbols specialSymbols)
    {
        var hubProxyTypeList = new List<HubProxyTypeMetadata>(createHubProxyMethods.Count);

        foreach (var createHubProxyMethod in createHubProxyMethods)
        {
            var methodSymbol = createHubProxyMethod.MethodSymbol;
            var location = createHubProxyMethod.Location;

            ITypeSymbol hubType = methodSymbol.TypeArguments[0];

            if (hubType.TypeKind != TypeKind.Interface)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.TypeArgumentRule,
                    location,
                    "CreateHubProxy",
                    hubType.ToDisplayString())); ;

                continue;
            }

            if (!hubProxyTypeList.Any(hubType))
            {
                var (hubMethods, isValid) = MetadataUtilities.ExtractHubMethods(context, hubType, specialSymbols.Task, specialSymbols.GenericTask, location);

                if (isValid)
                {
                    var invoker = new HubProxyTypeMetadata(hubType, hubMethods);
                    hubProxyTypeList.Add(invoker);
                }
            }
        }

        return hubProxyTypeList;
    }

    private static IReadOnlyList<ReceiverTypeMetadata> ExtractFromRegisterMethods(
        SourceProductionContext context,
        IReadOnlyList<MethodSymbolWithLocation> registerMethods,
        SpecialSymbols specialSymbols)
    {
        var receiverTypeList = new List<ReceiverTypeMetadata>(registerMethods.Count);

        foreach (var registerMethod in registerMethods)
        {
            var methodSymbol = registerMethod.MethodSymbol;
            var location = registerMethod.Location;

            ITypeSymbol receiverType = methodSymbol.TypeArguments[0];

            if (receiverType.TypeKind != TypeKind.Interface)
            {

                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptorItems.TypeArgumentRule,
                    location,
                    "Register",
                    receiverType.ToDisplayString()));

                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(receiverType, specialSymbols.HubConnectionObserver))
            {
                continue;
            }

            if (!receiverTypeList.Any(receiverType))
            {
                var (receiverMethods, isValid) = MetadataUtilities.ExtractReceiverMethods(context, receiverType, specialSymbols.Task, location);

                if (isValid)
                {
                    var receiverInfo = new ReceiverTypeMetadata(receiverType, receiverMethods);
                    receiverTypeList.Add(receiverInfo);
                }
            }
        }

        return receiverTypeList;
    }

    private enum ExtensionMethod
    {
        None,
        CreateHubProxy,
        Register
    }

    private class MethodSymbolWithLocation
    {
        public readonly IMethodSymbol MethodSymbol;
        public readonly Location Location;
        public readonly ExtensionMethod ExtensionMethod;

        public MethodSymbolWithLocation(IMethodSymbol methodSymbol, Location location, ExtensionMethod extensionMethod)
        {
            MethodSymbol = methodSymbol;
            Location = location;
            ExtensionMethod = extensionMethod;
        }
    }

    private readonly struct EssentialSymbols
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
        public readonly INamedTypeSymbol Task;
        public readonly INamedTypeSymbol GenericTask;
        public readonly INamedTypeSymbol HubConnectionObserver;
        public readonly IMethodSymbol CreateHubProxySymbol;
        public readonly IMethodSymbol RegisterSymbol;

        public SpecialSymbols(
            INamedTypeSymbol task,
            INamedTypeSymbol genericTask,
            INamedTypeSymbol hubConnectionObserver,
            IMethodSymbol createHubProxySymbol,
            IMethodSymbol registerSymbol
           )
        {
            Task = task;
            GenericTask = genericTask;
            HubConnectionObserver = hubConnectionObserver;
            CreateHubProxySymbol = createHubProxySymbol;
            RegisterSymbol = registerSymbol;
        }
    }
}
