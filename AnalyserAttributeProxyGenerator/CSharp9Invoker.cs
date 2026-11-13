namespace AnalyserAttributeProxyGenerator
{
    internal class CSharp9Invoker : ILanguageVersionInvoker
    {
        public LanguageVersionInvocation Get(string returnType, bool returnTypeIsNullableReferenceType, string invokeCode)
            => new("private ", returnType + (returnTypeIsNullableReferenceType ? "?" : string.Empty), $"return {invokeCode}", string.Empty, $"Func<object[], {returnType}>");
    }
}
