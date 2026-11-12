namespace AnalyserAttributeProxyGenerator
{
    internal class CSharpPre9Invoker : ILanguageVersionInvoker
    {
        public LanguageVersionInvocation Get(string returnType, string invokeCode)
            => new(
string.Empty,
"void",
$$"""
{{invokeCode}}
        return returnValue;
""",
$"    private {returnType} returnValue;\r\n\r\n",
"Action<object[]>");
    }
}
