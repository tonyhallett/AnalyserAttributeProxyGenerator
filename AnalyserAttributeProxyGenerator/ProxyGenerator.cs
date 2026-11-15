using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AnalyserAttributeProxyGenerator
{
    /// <summary>
    /// Generator for creating partial class with proxy methods for attribute constructors.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public class ProxyGenerator : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(AnalyserAttributeProxyAttributeSource.AddSource);

            IncrementalValuesProvider<ProxyAttributeModel?> modelProvider = context.SyntaxProvider.ForPartialClassWithAttributeWithMetadataName(
                fullyQualifiedMetadataName: AnalyserAttributeProxyAttributeSource.FullyQualifiedAttributeName,
                transform: static (partialClassSymbol, _, _) => ProxyAttributeModelProvider.Provide(partialClassSymbol));

            IncrementalValuesProvider<(ProxyAttributeModel? Left, Compilation Right)> combined = modelProvider.Combine(context.CompilationProvider);

            context.RegisterSourceOutput(combined, (context, result) =>
            {
                (ProxyAttributeModel? model, Compilation compilation) = result;
                if (model != null)
                {
                    string source = ProxySourceGenerator.Generate(compilation, model);
                    context.AddSource($"{model.PartialClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            });
        }
    }
}
