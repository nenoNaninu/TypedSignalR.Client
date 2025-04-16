using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace TypedSignalR.Client.SourceGeneratorTests;

using System.Collections.Generic;
using global::TypedSignalR.Client;

public static class CompilationHelper
{
    public static (ImmutableArray<Diagnostic> diagnostics, Dictionary<string, string> outputs) GetGeneratedOutput(string source)
    {
        const LanguageVersion LanguageVersion = LanguageVersion.CSharp12;

        var generator = new SourceGenerator();

        var sourceSyntaxTree = CSharpSyntaxTree.ParseText(source, path: "source.cs");
        var parsingOptions = new CSharpParseOptions(LanguageVersion);
        var sourceSyntaxTreeWithOptions = sourceSyntaxTree.WithRootAndOptions(sourceSyntaxTree.GetRoot(), parsingOptions);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat([
                MetadataReference.CreateFromFile(generator.GetType().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.SignalR.HubMethodNameAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.SignalR.Client.HubConnection).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).Assembly.Location)
            ]);

        var compilation = CSharpCompilation.Create(
            "generator",
            [sourceSyntaxTreeWithOptions],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


        GeneratorDriver driver =
            CSharpGeneratorDriver.Create(
                [generator.AsSourceGenerator()],
                driverOptions: new GeneratorDriverOptions(
                    disabledOutputs: IncrementalGeneratorOutputKind.None,
                    trackIncrementalGeneratorSteps: true),
                optionsProvider: null,
                parseOptions: new CSharpParseOptions(LanguageVersion));

        driver = driver.RunGenerators(compilation);

        var runResult = driver.GetRunResult();

        return (runResult.Diagnostics, runResult.Results.SelectMany(r => r.GeneratedSources).ToDictionary(s => s.HintName, s => s.SourceText.ToString()));
    }
}
