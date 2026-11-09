using AnalyserAttributeProxy;
using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
    [AnalyserAttributeProxy(typeof(AnAttribute), typeof(string))]
    internal partial class Proxy
    {
#pragma warning disable CA1822 // Mark members as static
        private partial string GetFromProxiedCtor(int i, string str, bool b) => $"i: {i}, str: {str}, b: {b}";

        private partial string GetFromProxiedCtor(ITypeSymbol type) => type.Name;

        private partial string GetFromProxiedCtor(AnAttribute.AnEnum anEnum) => anEnum.ToString();
#pragma warning restore CA1822 // Mark members as static

    }
}
