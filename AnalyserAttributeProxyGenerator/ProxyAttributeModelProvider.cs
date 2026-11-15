using System.Linq;
using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxyGenerator
{
    internal static class ProxyAttributeModelProvider
    {
        public static ProxyAttributeModel Provide(INamedTypeSymbol partialClassSymbol)
        {
            AttributeData attributeClass = GetAttributeData(partialClassSymbol);
            INamedTypeSymbol returnType = attributeClass.GetValue<INamedTypeSymbol>(1);
            string returnTypeString = returnType.ToDisplayString();
            bool returnTypeIsNullableReferenceType = false;
            if (returnType.IsReferenceType)
            {
                returnTypeIsNullableReferenceType = attributeClass.GetValue<bool>(2);
            }

            return new ProxyAttributeModel(
                GetNamespaceName(partialClassSymbol),
                partialClassSymbol.Name,
                partialClassSymbol.IsStatic,
                returnTypeString,
                returnTypeIsNullableReferenceType,
                attributeClass.GetValue<string>(3),
                attributeClass.GetValue<bool>(4),
                new ConstructorParameters(attributeClass.GetValue<INamedTypeSymbol>(0)));
        }

        private static AttributeData GetAttributeData(INamedTypeSymbol partialClassSymbol)
            => partialClassSymbol.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == AnalyserAttributeProxyAttributeSource.FullyQualifiedAttributeName);

        private static string GetNamespaceName(INamedTypeSymbol partialClassSymbol)
        {
            INamespaceSymbol partialNamespaceSymbol = partialClassSymbol.ContainingNamespace;
            string partialNamespaceName = string.Empty;
            if (!partialNamespaceSymbol.IsGlobalNamespace)
            {
                // not looking at nested
                partialNamespaceName = partialNamespaceSymbol.ToDisplayString();
            }

            return partialNamespaceName;
        }
    }
}
