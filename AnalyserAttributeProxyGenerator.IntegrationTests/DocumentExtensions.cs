using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
    internal static class DocumentExtensions
    {
        public static async Task<(string ClassName, string FullyQualifiedTypeName)> GetClassNameAndFullyQualifiedTypeNameAsync(this Document document)
        {
            SyntaxNode? syntaxRoot = await document.GetSyntaxRootAsync();
            ClassDeclarationSyntax classDeclaration = syntaxRoot!.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            SemanticModel? semanticModel = await document.GetSemanticModelAsync();
            INamedTypeSymbol? classSymbol = semanticModel!.GetDeclaredSymbol(classDeclaration);
            string className = classSymbol!.Name;
            string fullyQualifiedTypeName = classSymbol!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return (className, fullyQualifiedTypeName);
        }

        public static async Task<AttributeData> GetAttributeAppliedToSingleTypeInDocumentAsync(this Document document, string attributeName)
        {
            SyntaxNode? syntaxRoot = await document.GetSyntaxRootAsync();
            ClassDeclarationSyntax proxyDeclaration = syntaxRoot!.DescendantNodes().OfType<ClassDeclarationSyntax>().First(cd => cd.Identifier.Text == "Proxy");
            SemanticModel? semanticModel = await document.GetSemanticModelAsync();
            INamedTypeSymbol? proxySymbol = semanticModel!.GetDeclaredSymbol(proxyDeclaration);
            ImmutableArray<AttributeData> attributes = proxySymbol!.GetAttributes();
            return attributes.First(a => a.AttributeClass!.Name == attributeName);
        }
    }
}
