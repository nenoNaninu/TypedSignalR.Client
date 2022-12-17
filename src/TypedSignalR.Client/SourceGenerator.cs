using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
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

            ctx.AddSource("TypedSignalR.Client.Components.Generated.cs", NormalizeNewLines(new ComponentsTemplate().TransformText()));
            ctx.AddSource("TypedSignalR.Client.HubConnectionExtensions.Generated.cs", NormalizeNewLines(new HubConnectionExtensionsTemplate().TransformText()));
        });

        var specialSymbols = context.CompilationProvider
            .Select(static (compilation, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return GetSpecialSymbols(compilation);
            });

        var createHubProxyMethodSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(WhereCreateHubProxyMethod, TransformToSourceMethodSymbol)
            .Combine(specialSymbols)
            .Select(ValidateCreateHubProxyMethodSymbol)
            .Where(static x => x.IsValid())
            .Collect();

        var registerMethodSymbols = context.SyntaxProvider
            .CreateSyntaxProvider(WhereRegisterMethod, TransformToSourceMethodSymbol)
            .Combine(specialSymbols)
            .Select(ValidateRegisterMethodSymbol)
            .Where(static x => x.IsValid())
            .Collect();

        var hubInterfaces = context.CompilationProvider
            .Select(GetHubSourceTypeSymbol);

        var receiverInterfaces = context.CompilationProvider
            .Select(GetReceiverSourceTypeSymbol);

        context.RegisterSourceOutput(createHubProxyMethodSymbols.Combine(hubInterfaces).Combine(specialSymbols), GenerateHubInvokerSource);
        context.RegisterSourceOutput(registerMethodSymbols.Combine(receiverInterfaces).Combine(specialSymbols), GenerateBinderSource);
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

    private static SourceMethodSymbol TransformToSourceMethodSymbol(GeneratorSyntaxContext context, CancellationToken cancellationToken)
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

        return new SourceMethodSymbol(methodSymbol, target.GetLocation());
    }

    private static ValidatedSourceMethodSymbol ValidateCreateHubProxyMethodSymbol((SourceMethodSymbol, SpecialSymbols) pair, CancellationToken cancellationToken)
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

        var extensionMethodSymbol = methodSymbol.ReducedFrom ?? methodSymbol.ConstructedFrom;

        if (SymbolEqualityComparer.Default.Equals(extensionMethodSymbol, specialSymbols.CreateHubProxyMethodSymbol))
        {
            return new ValidatedSourceMethodSymbol(methodSymbol, location);
        }

        return default;
    }

    private static ValidatedSourceMethodSymbol ValidateRegisterMethodSymbol((SourceMethodSymbol, SpecialSymbols) pair, CancellationToken cancellationToken)
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

        var extensionMethodSymbol = methodSymbol.ReducedFrom ?? methodSymbol.ConstructedFrom;

        if (SymbolEqualityComparer.Default.Equals(extensionMethodSymbol, specialSymbols.RegisterMethodSymbol))
        {
            return new ValidatedSourceMethodSymbol(methodSymbol, location);
        }

        return default;
    }

    private static ImmutableArray<SourceTypeSymbol> GetHubSourceTypeSymbol(Compilation compilation, CancellationToken cancellationToken)
    {
        var attribute = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubAttribute");

        if (attribute is null)
        {
            return ImmutableArray.Create<SourceTypeSymbol>();
        }

        return ImmutableArray.Create(GetSourceTypeSymbol(compilation, attribute, cancellationToken));
    }

    private static ImmutableArray<SourceTypeSymbol> GetReceiverSourceTypeSymbol(Compilation compilation, CancellationToken cancellationToken)
    {
        var attribute = compilation.GetTypeByMetadataName("TypedSignalR.Client.ReceiverAttribute");

        if (attribute is null)
        {
            return ImmutableArray.Create<SourceTypeSymbol>();
        }

        return ImmutableArray.Create(GetSourceTypeSymbol(compilation, attribute, cancellationToken));
    }

    private static SourceTypeSymbol[] GetSourceTypeSymbol(Compilation compilation, INamedTypeSymbol attributeTypeSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var typeCollector = new AttributeAnnotatedTypeCollector(attributeTypeSymbol);
        typeCollector.VisitNamespace(compilation.GlobalNamespace);

        return typeCollector.ToSourceTypeSymbolArray();
    }

    private static void GenerateHubInvokerSource(
        SourceProductionContext context,
        ((ImmutableArray<ValidatedSourceMethodSymbol>, ImmutableArray<SourceTypeSymbol>), SpecialSymbols) data)
    {
        var sourceMethodSymbols = data.Item1.Item1;
        var sourceTypeSymbols = data.Item1.Item2;
        var specialSymbols = data.Item2;

        try
        {
            var hubTypes = new List<TypeMetadata>(16);

            AddHubTypesFromAttributeAnnotatedTypes(hubTypes, context, sourceTypeSymbols, specialSymbols);
            AddHubTypesFromCreateHubProxyMethods(hubTypes, context, sourceMethodSymbols, specialSymbols);

            var template = new HubConnectionExtensionsHubInvokerTemplate()
            {
                HubTypes = hubTypes,
                SpecialSymbols = specialSymbols
            };

            var source = NormalizeNewLines(template.TransformText());

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

    private static void GenerateBinderSource(
        SourceProductionContext context,
        ((ImmutableArray<ValidatedSourceMethodSymbol>, ImmutableArray<SourceTypeSymbol>), SpecialSymbols) data)
    {
        var sourceMethodSymbols = data.Item1.Item1;
        var sourceTypeSymbols = data.Item1.Item2;
        var specialSymbols = data.Item2;

        try
        {
            var receiverTypes = new List<TypeMetadata>(16);

            AddReceiverTypesFromAttributeAnnotatedTypes(receiverTypes, context, sourceTypeSymbols, specialSymbols);
            AddReceiverTypesFromRegisterMethods(receiverTypes, context, sourceMethodSymbols, specialSymbols);

            var template = new HubConnectionExtensionsBinderTemplate()
            {
                ReceiverTypes = receiverTypes
            };

            var source = NormalizeNewLines(template.TransformText());

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
        var cancellationTokenSymbol = compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        var asyncEnumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");
        var channelReaderSymbol = compilation.GetTypeByMetadataName("System.Threading.Channels.ChannelReader`1");
        var hubConnectionObserverSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.IHubConnectionObserver");
        var memberSymbols = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubConnectionExtensions")!.GetMembers();

        var hubAttributeSymbols = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubAttribute");
        var receiverAttributeSymbols = compilation.GetTypeByMetadataName("TypedSignalR.Client.ReceiverAttribute");

        IMethodSymbol? createHubProxyMethodSymbol = null;
        IMethodSymbol? registerMethodSymbol = null;

        foreach (var memberSymbol in memberSymbols)
        {
            if (memberSymbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

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

        return new SpecialSymbols(
            taskSymbol!,
            genericTaskSymbol!,
            cancellationTokenSymbol!,
            asyncEnumerableSymbol!,
            channelReaderSymbol!,
            hubConnectionObserverSymbol!,
            hubAttributeSymbols,
            receiverAttributeSymbols,
            createHubProxyMethodSymbol!,
            registerMethodSymbol!
        );
    }

    private static void AddHubTypesFromCreateHubProxyMethods(
        ICollection<TypeMetadata> hubTypeBuffer,
        SourceProductionContext context,
        IReadOnlyList<ValidatedSourceMethodSymbol> createHubProxyMethodSymbols,
        SpecialSymbols specialSymbols)
    {
        foreach (var createHubProxyMethodSymbol in createHubProxyMethodSymbols)
        {
            var methodSymbol = createHubProxyMethodSymbol.MethodSymbol;
            var location = createHubProxyMethodSymbol.Location;

            ITypeSymbol hubTypeSymbol = methodSymbol.TypeArguments[0];

            var isValid = TypeValidator.ValidateHubTypeRule(context, hubTypeSymbol, specialSymbols, location);

            if (isValid && !hubTypeBuffer.Contains(hubTypeSymbol))
            {
                hubTypeBuffer.Add(new TypeMetadata(hubTypeSymbol));
            }
        }
    }

    private static void AddReceiverTypesFromRegisterMethods(
        ICollection<TypeMetadata> receiverTypes,
        SourceProductionContext context,
        IReadOnlyList<ValidatedSourceMethodSymbol> registerMethodSymbols,
        SpecialSymbols specialSymbols)
    {
        foreach (var registerMethodSymbol in registerMethodSymbols)
        {
            var methodSymbol = registerMethodSymbol.MethodSymbol;
            var location = registerMethodSymbol.Location;

            ITypeSymbol receiverTypeSymbol = methodSymbol.TypeArguments[0];

            if (SymbolEqualityComparer.Default.Equals(receiverTypeSymbol, specialSymbols.HubConnectionObserverSymbol))
            {
                continue;
            }

            var isValid = TypeValidator.ValidateReceiverTypeRule(context, receiverTypeSymbol, specialSymbols, location);

            if (isValid && !receiverTypes.Contains(receiverTypeSymbol))
            {
                receiverTypes.Add(new TypeMetadata(receiverTypeSymbol));
            }
        }
    }

    private static void AddHubTypesFromAttributeAnnotatedTypes(
        ICollection<TypeMetadata> hubTypes,
        SourceProductionContext context,
        IReadOnlyList<SourceTypeSymbol> sourceTypeSymbols,
        SpecialSymbols specialSymbols)
    {
        foreach (var sourceTypeSymbol in sourceTypeSymbols)
        {
            var hubTypeSymbol = sourceTypeSymbol.TypeSymbol;
            var location = sourceTypeSymbol.Location;

            var isValid = TypeValidator.ValidateHubTypeRule(context, hubTypeSymbol, specialSymbols, location);

            if (isValid && !hubTypes.Contains(hubTypeSymbol))
            {
                hubTypes.Add(new TypeMetadata(hubTypeSymbol));
            }
        }
    }

    private static void AddReceiverTypesFromAttributeAnnotatedTypes(
        ICollection<TypeMetadata> receiverTypes,
        SourceProductionContext context,
        IReadOnlyList<SourceTypeSymbol> sourceTypeSymbols,
        SpecialSymbols specialSymbols)
    {
        foreach (var sourceTypeSymbol in sourceTypeSymbols)
        {
            var receiverTypeSymbol = sourceTypeSymbol.TypeSymbol;
            var location = sourceTypeSymbol.Location;

            var isValid = TypeValidator.ValidateReceiverTypeRule(context, receiverTypeSymbol, specialSymbols, location);

            if (isValid && !receiverTypes.Contains(receiverTypeSymbol))
            {
                receiverTypes.Add(new TypeMetadata(receiverTypeSymbol));
            }
        }
    }

    private static string NormalizeNewLines(string source)
    {
        return source.Replace("\r\n", "\n");
    }

    private readonly record struct SourceMethodSymbol
    {
        public static SourceMethodSymbol None => default;

        public readonly IMethodSymbol MethodSymbol;
        public readonly Location Location;

        public SourceMethodSymbol(IMethodSymbol methodSymbol, Location location)
        {
            MethodSymbol = methodSymbol;
            Location = location;
        }

        public bool IsNone()
        {
            return this == default;
        }
    }

    private readonly record struct ValidatedSourceMethodSymbol
    {
        public static ValidatedSourceMethodSymbol None => default;

        public readonly IMethodSymbol MethodSymbol;
        public readonly Location Location;

        public ValidatedSourceMethodSymbol(IMethodSymbol methodSymbol, Location location)
        {
            MethodSymbol = methodSymbol;
            Location = location;
        }

        public bool IsValid()
        {
            return this != default;
        }
    }

    private readonly record struct SourceTypeSymbol
    {
        public static SourceTypeSymbol None => default;

        public readonly INamedTypeSymbol TypeSymbol;
        public readonly Location Location;

        public SourceTypeSymbol(INamedTypeSymbol typeSymbol)
        {
            TypeSymbol = typeSymbol;
            Location = typeSymbol.Locations[0];
        }
    }

    private sealed class AttributeAnnotatedTypeCollector : SymbolVisitor
    {
        // Ignore the widely used lib namespace
        private static readonly string[] IgnoreNamespaces = new[]
        {
            "System",
            "Microsoft",
            "Dapper",
            "MySqlConnector",
            "Npgsql",
            "MimeKit",
            "StackExchange",
            "Azure",
            "Google",
            "Amazon",
            "UnityEngine"
        };

        private readonly INamedTypeSymbol _targetAttributeTypeSymbol;

        private readonly HashSet<INamedTypeSymbol> _namedTypeSymbols = new(SymbolEqualityComparer.Default);

        public AttributeAnnotatedTypeCollector(INamedTypeSymbol targetAttributeTypeSymbol)
        {
            _targetAttributeTypeSymbol = targetAttributeTypeSymbol;
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var ignoreNamespace in IgnoreNamespaces)
            {
                if (symbol.Name.StartsWith(ignoreNamespace))
                {
                    return;
                }
            }

            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                return;
            }

            var attributes = symbol.GetAttributes();

            foreach (var attribute in attributes)
            {
                var attributeSymbol = attribute.AttributeClass;

                if (attributeSymbol is null)
                {
                    continue;
                }

                if (SymbolEqualityComparer.Default.Equals(_targetAttributeTypeSymbol, attributeSymbol))
                {
                    _namedTypeSymbols.Add(symbol);
                }
            }
        }

        public INamedTypeSymbol[] ToArray() => _namedTypeSymbols.ToArray();

        public SourceTypeSymbol[] ToSourceTypeSymbolArray() => _namedTypeSymbols
            .Select(x => new SourceTypeSymbol(x))
            .ToArray();
    }
}
