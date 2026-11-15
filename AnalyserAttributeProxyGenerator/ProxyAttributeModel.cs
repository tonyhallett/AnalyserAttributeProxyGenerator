namespace AnalyserAttributeProxyGenerator
{
    internal record ProxyAttributeModel(
        string PartialNamespaceName,
        string PartialClassName,
        bool IsStatic,
        string ReturnType,
        bool ReturnTypeIsNullableReferenceType,
        string GeneratedMethodName,
        bool GenerateNullableReferenceTypes,
        ConstructorParameters ConstructorParameters);
}
