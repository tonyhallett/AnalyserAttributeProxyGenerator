using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AnalyserAttributeProxyGenerator
{
    internal static class AnalyserAttributeProxyAttributeSource
    {
        internal const string DefaultGeneratedMethodName = "GetFromProxiedCtor";
        internal const string AttributeNamespace = "AnalyserAttributeProxy";
        internal const string AnalyserAttributeProxyAttributeName = "AnalyserAttributeProxyAttribute";

        internal static string FullyQualifiedAttributeName => $"{AttributeNamespace}.{AnalyserAttributeProxyAttributeName}";

        public static void AddSource(IncrementalGeneratorPostInitializationContext context)
        {
            var sourceText = SourceText.From(
$$"""
using System;
using Microsoft.CodeAnalysis;

namespace {{AttributeNamespace}}
{
    [AttributeUsage(AttributeTargets.Class), Embedded]
    internal sealed class {{AnalyserAttributeProxyAttributeName}} : Attribute
    {
        public {{AnalyserAttributeProxyAttributeName}}(Type proxiedAttributeType, Type returnType, bool returnTypeIsNullableReferenceType = false, string methodName = "{{DefaultGeneratedMethodName}}", bool generateNullableReferenceTypes = false)
        {
            
        }
    }
}
""",
Encoding.UTF8);
            context.AddEmbeddedAttributeDefinition();
            context.AddSource($"{AnalyserAttributeProxyAttributeName}.g.cs", sourceText);
        }
    }
}
