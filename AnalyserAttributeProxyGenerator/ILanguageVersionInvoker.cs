namespace AnalyserAttributeProxyGenerator
{
    internal interface ILanguageVersionInvoker
    {
        LanguageVersionInvocation Get(string returnType, string invokeCode);
    }
}
