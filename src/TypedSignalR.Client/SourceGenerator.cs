using System;
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

        var createHubProxyMethodSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(WhereCreateHubProxyMethod, TransformToSourceSymbol)
            .Combine(specialSymbols)
            .Select(ValidateCreateHubProxyMethod)
            .Where(static x => x.IsValid())
            .Collect();

        var registerMethodSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(WhereRegisterMethod, TransformToSourceSymbol)
            .Combine(specialSymbols)
            .Select(ValidateRegisterMethod)
            .Where(static x => x.IsValid())
            .Collect();

        context.RegisterSourceOutput(createHubProxyMethodSymbols.Combine(specialSymbols), GenerateHubInvokerSource);
        context.RegisterSourceOutput(registerMethodSymbols.Combine(specialSymbols), GenerateBinderSource);
    }

    private static bool WhereCreateHubProxyMethod(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
        {
            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                if (memberAccessExpressionSyntax.Name.Identifier.ValueText is "CreateHubProxy")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool WhereRegisterMethod(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
        {
            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                if (memberAccessExpressionSyntax.Name.Identifier.ValueText is "Register")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static SourceSymbol TransformToSourceSymbol(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var invocationExpressionSyntax = context.Node as InvocationExpressionSyntax;
        var target = invocationExpressionSyntax?.Expression as MemberAccessExpressionSyntax;

        if (target is null)
        {
            return default;
        }

        var methodSymbol = context.SemanticModel.GetSymbolInfo(target).Symbol as IMethodSymbol;

        if (methodSymbol is null)
        {
            return default;
        }

        return new SourceSymbol(methodSymbol, target.GetLocation());
    }

    private static ValidatedSourceSymbol ValidateCreateHubProxyMethod((SourceSymbol, SpecialSymbols) pair, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourceSymbol = pair.Item1;

        if (sourceSymbol.IsNone())
        {
            return default;
        }

        var methodSymbol = sourceSymbol.MethodSymbol;
        var location = sourceSymbol.Location;
        var specialSymbols = pair.Item2;

        var extensionMethod = methodSymbol.ReducedFrom;

        if (extensionMethod is null)
        {
            return default;
        }

        if (SymbolEqualityComparer.Default.Equals(extensionMethod, specialSymbols.CreateHubProxySymbol))
        {
            return new ValidatedSourceSymbol(methodSymbol, location);
        }

        return default;
    }

    private static ValidatedSourceSymbol ValidateRegisterMethod((SourceSymbol, SpecialSymbols) pair, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourceSymbol = pair.Item1;

        if (sourceSymbol.IsNone())
        {
            return default;
        }

        var methodSymbol = sourceSymbol.MethodSymbol;
        var location = sourceSymbol.Location;
        var specialSymbols = pair.Item2;

        var extensionMethod = methodSymbol.ReducedFrom;

        if (extensionMethod is null)
        {
            return default;
        }

        if (SymbolEqualityComparer.Default.Equals(extensionMethod, specialSymbols.RegisterSymbol))
        {
            return new ValidatedSourceSymbol(methodSymbol, location);
        }

        return default;
    }

    private static void GenerateHubInvokerSource(SourceProductionContext context, (ImmutableArray<ValidatedSourceSymbol>, SpecialSymbols) data)
    {
        var sourceSymbols = data.Item1;
        var specialSymbols = data.Item2;

        try
        {
            var hubTypes = ExtractHubTypesFromCreateHubProxyMethods(context, sourceSymbols, specialSymbols);

            var template = new HubConnectionExtensionsHubInvokerTemplate()
            {
                HubTypes = hubTypes,
            };

            var source = template.TransformText();

            Debug.WriteLine(source);

            context.AddSource("TypedSignalR.Client.HubConnectionExtensions.HubInvoker.Generated.cs", source);
        }
        catch (Exception exception)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.UnexpectedException,
                Location.None,
                exception));
        }
    }

    private static void GenerateBinderSource(SourceProductionContext context, (ImmutableArray<ValidatedSourceSymbol>, SpecialSymbols) data)
    {
        var sourceSymbols = data.Item1;
        var specialSymbols = data.Item2;

        try
        {
            var receiverTypes = ExtractReceiverTypesFromRegisterMethods(context, sourceSymbols, specialSymbols);

            var template = new HubConnectionExtensionsBinderTemplate()
            {
                ReceiverTypes = receiverTypes
            };

            var source = template.TransformText();

            Debug.WriteLine(source);

            context.AddSource("TypedSignalR.Client.HubConnectionExtensions.Binder.Generated.cs", source);
        }
        catch (Exception exception)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptorItems.UnexpectedException,
                Location.None,
                exception));
        }
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

    private static IReadOnlyList<HubTypeMetadata> ExtractHubTypesFromCreateHubProxyMethods(
        SourceProductionContext context,
        IReadOnlyList<ValidatedSourceSymbol> createHubProxyMethodSymbols,
        SpecialSymbols specialSymbols)
    {
        var hubTypeList = new List<HubTypeMetadata>(createHubProxyMethodSymbols.Count);

        foreach (var createHubProxyMethod in createHubProxyMethodSymbols)
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

            if (!hubTypeList.Any(hubType))
            {
                var (hubMethods, isValid) = MetadataUtilities.ExtractHubMethods(context, hubType, specialSymbols.TaskSymbol, specialSymbols.GenericTaskSymbol, location);

                if (isValid)
                {
                    var invoker = new HubTypeMetadata(hubType, hubMethods);
                    hubTypeList.Add(invoker);
                }
            }
        }

        return hubTypeList;
    }

    private static IReadOnlyList<ReceiverTypeMetadata> ExtractReceiverTypesFromRegisterMethods(
        SourceProductionContext context,
        IReadOnlyList<ValidatedSourceSymbol> registerMethodSymbols,
        SpecialSymbols specialSymbols)
    {
        var receiverTypeList = new List<ReceiverTypeMetadata>(registerMethodSymbols.Count);

        foreach (var registerMethod in registerMethodSymbols)
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

            if (SymbolEqualityComparer.Default.Equals(receiverType, specialSymbols.HubConnectionObserverSymbol))
            {
                continue;
            }

            if (!receiverTypeList.Any(receiverType))
            {
                var (receiverMethods, isValid) = MetadataUtilities.ExtractReceiverMethods(context, receiverType, specialSymbols.TaskSymbol, location);

                if (isValid)
                {
                    var receiverInfo = new ReceiverTypeMetadata(receiverType, receiverMethods);
                    receiverTypeList.Add(receiverInfo);
                }
            }
        }

        return receiverTypeList;
    }

    private readonly record struct SourceSymbol
    {
        public static SourceSymbol None => default;

        public readonly IMethodSymbol MethodSymbol;
        public readonly Location Location;

        public SourceSymbol(IMethodSymbol methodSymbol, Location location)
        {
            MethodSymbol = methodSymbol;
            Location = location;
        }

        public bool IsNone()
        {
            return this == default;
        }
    }

    private readonly record struct ValidatedSourceSymbol
    {
        public static ValidatedSourceSymbol None => default;

        public readonly IMethodSymbol MethodSymbol;
        public readonly Location Location;

        public ValidatedSourceSymbol(IMethodSymbol methodSymbol, Location location)
        {
            MethodSymbol = methodSymbol;
            Location = location;
        }

        public bool IsValid()
        {
            return this != default;
        }
    }

    private class SpecialSymbols
    {
        public readonly INamedTypeSymbol TaskSymbol;
        public readonly INamedTypeSymbol GenericTaskSymbol;
        public readonly INamedTypeSymbol HubConnectionObserverSymbol;
        public readonly IMethodSymbol CreateHubProxySymbol;
        public readonly IMethodSymbol RegisterSymbol;

        public SpecialSymbols(
            INamedTypeSymbol taskSymbol,
            INamedTypeSymbol genericTaskSymbol,
            INamedTypeSymbol hubConnectionObserverSymbol,
            IMethodSymbol createHubProxySymbol,
            IMethodSymbol registerSymbol
           )
        {
            TaskSymbol = taskSymbol;
            GenericTaskSymbol = genericTaskSymbol;
            HubConnectionObserverSymbol = hubConnectionObserverSymbol;
            CreateHubProxySymbol = createHubProxySymbol;
            RegisterSymbol = registerSymbol;
        }
    }
}
