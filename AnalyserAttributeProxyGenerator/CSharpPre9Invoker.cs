namespace AnalyserAttributeProxyGenerator
{
    internal class CSharpPre9Invoker : ILanguageVersionInvoker
    {
        public LanguageVersionInvocation Get(string returnType, bool returnTypeIsNullableReferenceType, string invokeCode)
        {
            string privateVariable = $"    private {returnType}{(returnTypeIsNullableReferenceType ? "?" : string.Empty)} returnValue;";
            if (returnTypeIsNullableReferenceType)
            {
                privateVariable = NullableEnable.Apply(privateVariable);
            }

            privateVariable += "\r\n\r\n";
            return new(
string.Empty,
"void",
false,
$$"""
{{invokeCode}}
        return returnValue;
""",
privateVariable,
"Action<object[]>");
        }
    }
}
