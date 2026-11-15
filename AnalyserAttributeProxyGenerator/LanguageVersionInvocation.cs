namespace AnalyserAttributeProxyGenerator
{
    internal record LanguageVersionInvocation(
        string DeclarationPrivateKeyword,
        string PartialReturnType,
        bool HasReturnType,
        string ReturnCode,
        string PrivateVariable,
        string DictionaryValueType);
}
