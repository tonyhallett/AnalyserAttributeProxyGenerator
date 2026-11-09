using AnalyserAttributeProxy;
using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
    [AnalyserAttributeProxy(typeof(AnAttribute), typeof(string))]
    internal sealed partial class Proxy
    {
#pragma warning disable CA1822 // Mark members as static
        private partial string GetFromProxiedCtor(int i, string str, bool b) => $"i: {i}, str: {str}, b: {b}";

        private partial string GetFromProxiedCtor(ITypeSymbol type) => type.Name;

        private partial string GetFromProxiedCtor(AnAttribute.AnEnum anEnum) => anEnum.ToString();

        private partial string GetFromProxiedCtor(object anObject) => anObject.ToString()!;

        private partial string GetFromProxiedCtor(string[] strings) => string.Join(',', strings);
#pragma warning restore CA1822 // Mark members as static

    }
}
