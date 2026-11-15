using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxyGenerator.Tests
{
    internal static class MetadataReferenceFromType
    {
        public static PortableExecutableReference CreateFromType<T>() => MetadataReference.CreateFromFile(typeof(T).Assembly.Location);
    }
}
