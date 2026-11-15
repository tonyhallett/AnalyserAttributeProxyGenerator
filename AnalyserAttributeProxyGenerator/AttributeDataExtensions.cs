using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxyGenerator
{
    internal static class AttributeDataExtensions
    {
        public static T GetValue<T>(this AttributeData attributeData, int index)
            => (T)attributeData.ConstructorArguments[index].Value!;
    }
}
