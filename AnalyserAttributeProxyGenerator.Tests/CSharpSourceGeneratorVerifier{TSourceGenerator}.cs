using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyserAttributeProxyGenerator.Tests
{
    internal static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : new()
    {
        public sealed class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
        {
            public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;
            public bool NullableEnabled { get; set; } = true;

            protected override CompilationOptions CreateCompilationOptions()
            {
                CSharpCompilationOptions compilationOptions = (CSharpCompilationOptions)base.CreateCompilationOptions();
                if (NullableEnabled)
                {
                    compilationOptions = compilationOptions.WithNullableContextOptions(NullableContextOptions.Enable);
                }

                return compilationOptions.WithSpecificDiagnosticOptions(
                     compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
            }

            protected override ParseOptions CreateParseOptions()
            {
                return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
            }

            private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
            {
                string[] args = ["/warnaserror:nullable"];
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                CSharpCommandLineArguments commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
                return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
            }
        }
    }
}
