namespace AnalyserAttributeProxyGenerator
{
    internal interface ILanguageVersionInvoker
    {
        LanguageVersionInvocation Get(string returnType, bool returnTypeIsNullableReferenceType, string invokeCode);
    }
}
