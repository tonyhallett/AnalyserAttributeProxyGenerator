using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AnalyserAttributeProxyGenerator
{
    internal static class NamespaceWithType
    {
        public static string GetSource(LanguageVersion languageVersion, string namespaceName, string typeCode)
        {
            bool globalNamespace = string.IsNullOrEmpty(namespaceName);
            if (globalNamespace)
            {
                return typeCode;
            }

            bool fileScopedNamespace = languageVersion >= LanguageVersion.CSharp10;
            if (fileScopedNamespace)
            {
                return
$$"""
namespace {{namespaceName}};

{{typeCode}}
""";
            }

            return
$$"""
namespace {{namespaceName}}
{
{{Indent(typeCode)}}
}
""";
        }

        private static string Indent(string code, int indentLevel = 1)
        {
            string indent = new(' ', indentLevel * 4);
            return string.Join("\r\n", code.Replace("\r\n", "\n").Split('\n')
                .Select(line =>
                {
                    if (line.Length == 0)
                    {
                        return line;
                    }

                    return indent + line;
                }));
        }
    }
}
