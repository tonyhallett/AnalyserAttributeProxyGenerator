using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
    internal static class ProjectExtensions
    {
        public static Document AddDocumentFromFilePath(this Project project, string filePath, string documentName)
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            return project.AddDocument(
 documentName,
 File.ReadAllText(filePath),
 filePath: filePath);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        }
    }
}
