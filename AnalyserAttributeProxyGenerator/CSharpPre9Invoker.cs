namespace AnalyserAttributeProxyGenerator
{
    internal class CSharpPre9Invoker : ILanguageVersionInvoker
    {
        public LanguageVersionInvocation Get(string returnType, bool returnTypeIsNullableReferenceType, string invokeCode)
            => new(
string.Empty,
"void",
$$"""
{{invokeCode}}
        return returnValue;
""",
$"    private {returnType}{(returnTypeIsNullableReferenceType ? "?" : string.Empty)} returnValue;",
"Action<object[]>");
    }
}
