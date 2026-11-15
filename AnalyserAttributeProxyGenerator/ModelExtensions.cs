namespace AnalyserAttributeProxyGenerator
{
    internal static class ModelExtensions
    {
        public static string StaticKeyword(this ProxyAttributeModel model) => model.IsStatic ? "static " : string.Empty;
    }
}
