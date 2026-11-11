using System.Globalization;
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

        private partial string GetFromProxiedCtor(int? i) => i.HasValue ? i.Value.ToString(CultureInfo.InvariantCulture) : "null";

        private partial string GetFromProxiedCtor(string[] strings) => string.Join(',', strings);

        private partial string GetFromProxiedCtor(int[]? maybeInts) => maybeInts != null ? string.Join(',', maybeInts) : "null";

        private partial string GetFromProxiedCtor(string[][] arrayOfArrays)
        {
            IEnumerable<string> result = arrayOfArrays.Select(arr => string.Join(',', arr));
            return string.Join(" - ", result);
        }
#pragma warning restore CA1822 // Mark members as static

    }
}
