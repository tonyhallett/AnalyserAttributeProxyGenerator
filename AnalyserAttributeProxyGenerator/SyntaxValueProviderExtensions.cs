using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyserAttributeProxyGenerator
{
    internal static class SyntaxValueProviderExtensions
    {
        public static IncrementalValuesProvider<T?> ForPartialClassWithAttributeWithMetadataName<T>(
            this SyntaxValueProvider syntaxValueProvider,
            string fullyQualifiedMetadataName,
            Func<INamedTypeSymbol, GeneratorAttributeSyntaxContext, CancellationToken, T> transform)
        {
            return syntaxValueProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName,
                static (node, _) => node is ClassDeclarationSyntax,
                (context, _) =>
                {
                    if (!IsPartial(context))
                    {
                        return default;
                    }

                    var partialClassSymbol = (INamedTypeSymbol)context.TargetSymbol;
                    return transform(partialClassSymbol, context, _);
                });
        }

        private static bool IsPartial(GeneratorAttributeSyntaxContext context) => ((ClassDeclarationSyntax)context.TargetNode).Modifiers.Any(SyntaxKind.PartialKeyword);
    }
}
