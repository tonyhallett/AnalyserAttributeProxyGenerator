using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
    internal sealed class Tests
    {
        [Test]
        public Task Should_Work_With_Multiple_Primitive_Parameter_Types()
            => AssertGetFromProxiedCtorAsync("1, \"test\", true", "i: 1, str: test, b: True");

        [Test]
        public Task Should_Work_With_Parameter_Type_Type()
           => AssertGetFromProxiedCtorAsync("typeof(bool)", "Boolean");

        [Test]
        public Task Should_Work_With_Parameter_Type_Enum()
            => AssertGetFromProxiedCtorAsync("System.DayOfWeek.Thursday", "DayOfWeek Thursday");

        [Test]
        public Task Should_Work_With_Parameter_Type_Object_String()
            => AssertGetFromProxiedCtorAsync("\"string object\"", "Object string object");

        [Test]
        public Task Should_Work_With_Parameter_Type_Object_Type()
            => AssertGetFromProxiedCtorAsync("(object)typeof(bool)", "Object bool");

        [Test]
        public Task Should_Work_With_Parameter_Type_StringArray()
            => AssertGetFromProxiedCtorAsync("new string[]{ \"a\", \"b\", \"c\"}", "a,b,c");

        [Test]
        public Task Should_Work_With_Parameter_Type_Type_Null()
            => AssertGetFromProxiedCtorAsync("(System.Type)null", "null");

        private static async Task AssertGetFromProxiedCtorAsync(string attributeArguments, string expected)
        {
            IEnumerable<PortableExecutableReference> references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path));

            var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "MainProject",
                "MainAssembly",
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: references);

            string anAttributePath = GetFilePathInProject("AnAttribute.cs");

            AdhocWorkspace workspace = new();
            Project project = workspace.AddProject(projectInfo);

            Document anAttributeDocument = project.AddDocumentFromFilePath(anAttributePath, "AnAttribute.cs");
            (string attributeName, string attributeFullyQualifiedTypeName) = await anAttributeDocument.GetClassNameAndFullyQualifiedTypeNameAsync();

            string sourceCode =
$$"""
[{{attributeFullyQualifiedTypeName}}({{attributeArguments}})]
public class Proxy
{
}
""";
            Document document = anAttributeDocument.Project.AddDocument(
"ForTest.cs",
sourceCode,
filePath: "filepath.cs");
            await ThrowForDiagnosticsAsync(document);
            AttributeData attribute = await document.GetAttributeAppliedToSingleTypeInDocumentAsync(attributeName);

            Assert.That(new Proxy().GetFromProxiedCtor(attribute), Is.EqualTo(expected));
        }

        private static async Task ThrowForDiagnosticsAsync(Document document)
        {
            SemanticModel? semanticModel = await document.GetSemanticModelAsync();
            ImmutableArray<Diagnostic> diagnostics = semanticModel!.Compilation.GetDiagnostics();
            if (diagnostics.Length > 0)
            {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                throw new InvalidOperationException(string.Join(Environment.NewLine, diagnostics.Select(d => d.GetMessage(CultureInfo.InvariantCulture))));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            }
        }

        private static string GetFilePathInProject(string fileName)
        {
            DirectoryInfo? dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            DirectoryInfo? projectDirectory = null;
            while (dir != null)
            {
                if (dir.GetFiles("*.csproj").Length > 0)
                {
                    projectDirectory = dir;
                    break;
                }

                dir = dir.Parent;
            }

            return Path.Combine(projectDirectory!.FullName, fileName);
        }
    }
}
