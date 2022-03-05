using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypedSignalR.Client.CodeAnalysis;
using TypedSignalR.Client.Templates;

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
            .Select(ValidateCreateHubProxyMethodSymbol)
            .Where(static x => x.IsValid())
            .Collect();

        var registerMethodSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(WhereRegisterMethod, TransformToSourceSymbol)
            .Combine(specialSymbols)
            .Select(ValidateRegisterMethodSymbol)
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

        var node = context.Node as InvocationExpressionSyntax;
        var target = node?.Expression as MemberAccessExpressionSyntax;

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

    private static ValidatedSourceSymbol ValidateCreateHubProxyMethodSymbol((SourceSymbol, SpecialSymbols) pair, CancellationToken cancellationToken)
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

        var extensionMethodSymbol = methodSymbol.ReducedFrom;

        if (extensionMethodSymbol is null)
        {
            return default;
        }

        if (SymbolEqualityComparer.Default.Equals(extensionMethodSymbol, specialSymbols.CreateHubProxyMethodSymbol))
        {
            return new ValidatedSourceSymbol(methodSymbol, location);
        }

        return default;
    }

    private static ValidatedSourceSymbol ValidateRegisterMethodSymbol((SourceSymbol, SpecialSymbols) pair, CancellationToken cancellationToken)
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

        var extensionMethodSymbol = methodSymbol.ReducedFrom;

        if (extensionMethodSymbol is null)
        {
            return default;
        }

        if (SymbolEqualityComparer.Default.Equals(extensionMethodSymbol, specialSymbols.RegisterMethodSymbol))
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
        var memberSymbols = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubConnectionExtensions")!.GetMembers();

        IMethodSymbol? createHubProxyMethodSymbol = null;
        IMethodSymbol? registerMethodSymbol = null;

        foreach (var methodSymbol in memberSymbols.OfType<IMethodSymbol>())
        {
            if (methodSymbol.Name is "CreateHubProxy")
            {
                if (methodSymbol.MethodKind is MethodKind.Ordinary)
                {
                    createHubProxyMethodSymbol = methodSymbol;

                    if (registerMethodSymbol is not null)
                    {
                        break;
                    }
                }
            }
            else if (methodSymbol.Name is "Register")
            {
                if (methodSymbol.MethodKind is MethodKind.Ordinary)
                {
                    registerMethodSymbol = methodSymbol;

                    if (createHubProxyMethodSymbol is not null)
                    {
                        break;
                    }
                }
            }
        }

        return new SpecialSymbols(taskSymbol!, genericTaskSymbol!, hubConnectionObserverSymbol!, createHubProxyMethodSymbol!, registerMethodSymbol!);
    }

    private static IReadOnlyList<TypeMetadata> ExtractHubTypesFromCreateHubProxyMethods(
        SourceProductionContext context,
        IReadOnlyList<ValidatedSourceSymbol> createHubProxyMethodSymbols,
        SpecialSymbols specialSymbols)
    {
        var hubTypeList = new List<TypeMetadata>(createHubProxyMethodSymbols.Count);

        foreach (var createHubProxyMethodSymbol in createHubProxyMethodSymbols)
        {
            var methodSymbol = createHubProxyMethodSymbol.MethodSymbol;
            var location = createHubProxyMethodSymbol.Location;

            ITypeSymbol hubTypeSymbol = methodSymbol.TypeArguments[0];

            var isValid = TypeValidator.ValidateHubTypeRule(context, hubTypeSymbol, specialSymbols.TaskSymbol, specialSymbols.GenericTaskSymbol, location);

            if (isValid && !hubTypeList.Any(hubTypeSymbol))
            {
                hubTypeList.Add(new TypeMetadata(hubTypeSymbol));
            }
        }

        return hubTypeList;
    }

    private static IReadOnlyList<TypeMetadata> ExtractReceiverTypesFromRegisterMethods(
        SourceProductionContext context,
        IReadOnlyList<ValidatedSourceSymbol> registerMethodSymbols,
        SpecialSymbols specialSymbols)
    {
        var receiverTypeList = new List<TypeMetadata>(registerMethodSymbols.Count);

        foreach (var registerMethodSymbol in registerMethodSymbols)
        {
            var methodSymbol = registerMethodSymbol.MethodSymbol;
            var location = registerMethodSymbol.Location;

            ITypeSymbol receiverTypeSymbol = methodSymbol.TypeArguments[0];

            if (SymbolEqualityComparer.Default.Equals(receiverTypeSymbol, specialSymbols.HubConnectionObserverSymbol))
            {
                continue;
            }

            var isValid = TypeValidator.ValidateReceiverTypeRule(context, receiverTypeSymbol, specialSymbols.TaskSymbol, location);

            if (isValid && !receiverTypeList.Any(receiverTypeSymbol))
            {
                receiverTypeList.Add(new TypeMetadata(receiverTypeSymbol));
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
        public readonly IMethodSymbol CreateHubProxyMethodSymbol;
        public readonly IMethodSymbol RegisterMethodSymbol;

        public SpecialSymbols(
            INamedTypeSymbol taskSymbol,
            INamedTypeSymbol genericTaskSymbol,
            INamedTypeSymbol hubConnectionObserverSymbol,
            IMethodSymbol createHubProxyMethodSymbol,
            IMethodSymbol registerMethodSymbol
           )
        {
            TaskSymbol = taskSymbol;
            GenericTaskSymbol = genericTaskSymbol;
            HubConnectionObserverSymbol = hubConnectionObserverSymbol;
            CreateHubProxyMethodSymbol = createHubProxyMethodSymbol;
            RegisterMethodSymbol = registerMethodSymbol;
        }
    }
}
