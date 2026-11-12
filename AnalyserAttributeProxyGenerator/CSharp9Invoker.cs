namespace AnalyserAttributeProxyGenerator
{
    internal class CSharp9Invoker : ILanguageVersionInvoker
    {
        public LanguageVersionInvocation Get(string returnType, string invokeCode)
            => new("private ", returnType, $"return {invokeCode}", string.Empty, $"Func<object[], {returnType}>");
    }
}
