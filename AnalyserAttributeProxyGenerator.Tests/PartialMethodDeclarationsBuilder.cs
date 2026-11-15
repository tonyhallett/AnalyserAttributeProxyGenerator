using System.Text;

namespace AnalyserAttributeProxyGenerator.Tests
{
    internal static class PartialMethodDeclarationsBuilder
    {
        public static string Build(string[] partialMethodDeclarations)
        {
            StringBuilder sb = new();
            for (int i = 0; i < partialMethodDeclarations.Length; i++)
            {
                sb.AppendLine(partialMethodDeclarations[i]);
                bool isLast = i == partialMethodDeclarations.Length - 1;
                if (!isLast)
                {
                    sb.AppendLine(string.Empty);
                }
            }

            return sb.ToString();
        }
    }
}
