using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using SourceGenVerifier = AnalyserAttributeProxyGenerator.Tests.CSharpSourceGeneratorVerifier<AnalyserAttributeProxyGenerator.ProxyGenerator>;

namespace AnalyserAttributeProxyGenerator.Tests
{
    internal sealed class Tests
    {
        [Test]
        public Task Should_Generate_Non_Static()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"string", values => GetFromProxiedCtor((string)values[0]!)},
        };
""";
            string[] args = ["string name"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                string.Empty,
                args,
                args);
        }

        [Test]
        public Task Should_Generate_Static()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], bool>>
        {
           {"string", values => GetFromProxiedCtor((string)values[0]!)},
        };
""";
            string[] args = ["string name"];
            return GeneratedSourceTestAsync(
                true,
                "bool",
                false,
                expectedInvokers,
                string.Empty,
                args,
                args);
        }

        [Test]
        public Task Should_Generate_Custom_Method_Name()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"string", values => MyGen((string)values[0]!)},
        };
""";
            string[] args = ["string name"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "MyGen",
                args,
                args);
        }

        [Test]
        public Task Should_Work_With_Different_Return_Types()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], bool>>
        {
           {"string", values => GetFromProxiedCtor((string)values[0]!)},
        };
""";
            string[] args = ["string name"];
            return GeneratedSourceTestAsync(
                false,
                "bool",
                false,
                expectedInvokers,
                string.Empty,
                args,
                args);
        }

        [TestCase("int")]
        [TestCase("uint")]
        public Task Should_Work_With_Primitives(string primitive)
        {
            string expectedInvokers =
$$"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"{{primitive}}", values => GetFromProxiedCtor(({{primitive}})values[0]!)},
        };
""";
            string[] args = [$"{primitive} primitive"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "GetFromProxiedCtor",
                args,
                args,
                true);
        }

        [Test]
        public Task Should_Work_With_Enums()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"AnEnum", values => GetFromProxiedCtor((AnEnum)values[0]!)},
        };
""";
            string[] args = ["AnEnum anEnum"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "GetFromProxiedCtor",
                args,
                args,
                true);
        }

        [Test]
        public Task Should_Work_With_Object()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"object", values => GetFromProxiedCtor((object)values[0]!)},
        };
""";
            string[] args = ["object obj"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                string.Empty,
                args,
                args);
        }

        [Test]
        public Task Should_Work_With_Object_Nullable()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"object", values => GetFromProxiedCtor((object?)values[0])},
        };
""";
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "GetFromProxiedCtor",
                ["object obj"],
                ["object? obj"],
                true);
        }

        [TestCase("string")]
        [TestCase("int")]
        public Task Should_Work_With_Arrays(string elementType)
        {
            string expectedInvokers =
$$"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"{{elementType}}[]", values => GetFromProxiedCtor(GetArray<{{elementType}}>(values[0]!))},
        };
""";
            string[] args = [$"{elementType}[] array"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                string.Empty,
                args,
                args);
        }

        [Test]
        public Task Should_Work_With_Nullable_Arrays()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"string[]", values => GetFromProxiedCtor(GetOptionalArray<string?>(values[0]))},
        };
""";
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "GetFromProxiedCtor",
                ["string[] arr"],
                ["string?[]? arr"],
                true);
        }

        [Test]
        public Task Should_Provide_ITypeSymbol_For_Constructor_Parameter_Of_Type_Type()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"System.Type", values => MyGen((ITypeSymbol)values[0]!)},
        };
""";
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "MyGen",
                ["System.Type type"],
                ["ITypeSymbol type"]);
        }

        [Test]
        public Task Should_Work_With_Nullable_ITypeSymbol()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"System.Type", values => MyGen((ITypeSymbol?)values[0])},
        };
""";
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "MyGen",
                ["System.Type type"],
                ["ITypeSymbol? type"],
                true);
        }

        [Test]
        public Task Should_Work_With_Nullable_string()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"string", values => MyGen((string?)values[0])},
        };
""";
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                "MyGen",
                ["string str"],
                ["string? str"],
                true);
        }

        [Test]
        public Task Should_Work_With_Multiple_Constructors()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"string", values => GetFromProxiedCtor((string)values[0]!)},
           {"bool", values => GetFromProxiedCtor((bool)values[0]!)},
        };
""";
            string[] args = ["string name", "bool flag"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                string.Empty,
                args,
                args);
        }

        [Test]
        public Task Should_Work_With_Multiple_Constructor_Parameters()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"string,bool", values => GetFromProxiedCtor((string)values[0]!, (bool)values[1]!)},
        };
""";
            string[] args = ["string name, bool flag"];
            return GeneratedSourceTestAsync(
                false,
                "string",
                false,
                expectedInvokers,
                string.Empty,
                args,
                args);
        }

        [Test]
        public Task Should_Work_With_Global_Namespace()
        {
            const string expectedInvokers =
"""
        var invokers = new Dictionary<string, Func<object?[], string>>
        {
           {"string", values => GetFromProxiedCtor((string)values[0]!)},
        };
""";
            string[] args = ["string name"];
            return GeneratedSourceTestAsync(false, "string", true, expectedInvokers, string.Empty, args, args);
        }

        private static Task GeneratedSourceTestAsync(
            bool isStatic,
            string returnType,
            bool proxyIsGlobal,
            string invokers,
            string methodName,
            string[] attributeConstructorParams,
            string[] partialDeclarationParams,
            bool generateNullableReferenceTypes = false)
        {
            (string source, string expectedPartial) = GetSourceExpectedGenerated(isStatic, returnType, proxyIsGlobal, invokers, methodName, attributeConstructorParams, partialDeclarationParams, generateNullableReferenceTypes);
            return GeneratedSourceTestAsync(source, expectedPartial);
        }

        private static Task GeneratedSourceTestAsync(string source, string expectedPartial)
        {
            const string analyserAttributeProxyAttributeSource =
"""
using System;
using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxy
{
    [AttributeUsage(AttributeTargets.Class), Embedded]
    internal sealed class AnalyserAttributeProxyAttribute : Attribute
    {
        public AnalyserAttributeProxyAttribute(Type proxiedAttributeType, Type returnType, string methodName = "GetFromProxiedCtor", bool generateNullableReferenceTypes = false)
        {
            
        }
    }
}
""";
            var sourceGenTest = new SourceGenVerifier.Test()
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences =
                    {
                        MetadataReferenceFromType.CreateFromType<AttributeData>(),
                        MetadataReferenceFromType.CreateFromType<AttributeArgumentSyntax>(),
                    },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
                },
            };

            // although the matching of generated is independent of order, if there is another error it is reporting sources in the wrong order.
            sourceGenTest.TestState.GeneratedSources.AddEmbeddedAttribute<ProxyGenerator>();
            sourceGenTest.TestState.GeneratedSources.Add((typeof(ProxyGenerator), "AnalyserAttributeProxyAttribute.g.cs", SourceText.From(analyserAttributeProxyAttributeSource, Encoding.UTF8)));
            sourceGenTest.TestState.GeneratedSources.Add((typeof(ProxyGenerator), "Proxy.g.cs", SourceText.From(expectedPartial, Encoding.UTF8)));
            return sourceGenTest.RunAsync();
        }

        private static string GetSource(
            bool isStatic,
            string returnType,
            string proxyNamespace,
            string methodName,
            bool generateNullableReferenceTypes,
            params string[] attributeConstructorParams)
        {
            string namespaceStart = string.Empty;
            string namespaceEnd = string.Empty;
            if (!string.IsNullOrEmpty(proxyNamespace))
            {
                namespaceStart = $"namespace {proxyNamespace} {{";
                namespaceEnd = "}";
            }

            string staticKeyword = isStatic ? "static " : string.Empty;
            string attribute = $"[{ProxyGenerator.FullyQualifiedAttributeName}(typeof(AttributeNs.ProxiedAttribute), typeof({returnType})";
            if (string.IsNullOrEmpty(methodName))
            {
                attribute += ")]";
            }
            else
            {
                attribute += $", \"{methodName}\", {(generateNullableReferenceTypes ? "true" : "false")})]";
            }

            StringBuilder ctorsBuilder = new();
            foreach (string ctorParams in attributeConstructorParams)
            {
                ctorsBuilder.AppendLine(CultureInfo.InvariantCulture, $"        public ProxiedAttribute({ctorParams}) {{ }}");
            }

            return
$$"""
{{namespaceStart}}
    {{attribute}}
    {{staticKeyword}}partial class Proxy
    { }
{{namespaceEnd}}
namespace AttributeNs
{
    public class ProxiedAttribute : System.Attribute
    {
{{ctorsBuilder}}
    }
}
public enum AnEnum {}
""";
        }

        private static string GetExpectedGenerated(
            bool isStatic,
            string returnType,
            string proxyNamespace,
            string invokers,
            string methodName,
            params string[] partialDeclarationParams)
        {
            string staticKeyword = isStatic ? "static " : string.Empty;
            string genMethodName = string.IsNullOrEmpty(methodName) ? ProxyGenerator.DefaultGeneratedMethodName : methodName;
            string partialDeclarations = PartialMethodDeclarationsBuilder.Build([.. partialDeclarationParams.Select(ctorParams => $"    private {staticKeyword}partial {returnType} {{|CS8795:{genMethodName}|}}({ctorParams});")]);
            if (!string.IsNullOrEmpty(proxyNamespace))
            {
                proxyNamespace = $"namespace {proxyNamespace};";
            }

            return
$$"""
// <auto-generated/>
#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

{{proxyNamespace}}

{{staticKeyword}}partial class Proxy
{
    private {{staticKeyword}}SeparatedSyntaxList<AttributeArgumentSyntax> arguments;

{{partialDeclarations}}
    public {{staticKeyword}}{{returnType}} {{genMethodName}}(AttributeData attributeData)
    {
        var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference!.GetSyntax();
        arguments = attribute.ArgumentList!.Arguments;
        IMethodSymbol ctor = attributeData.AttributeConstructor!;
        var overload = string.Join(",", ctor.Parameters.Select(p => p.Type.ToDisplayString()));
{{invokers}}
        return invokers[overload](attributeData.ConstructorArguments.Select(GetValue).ToArray());
    }

    private static object? GetValue(TypedConstant typedConstant)
    {
        if (typedConstant.IsNull)
        {
            return null;
        }

        if (typedConstant.Kind == TypedConstantKind.Array)
        {
            return typedConstant.Values.Select(GetValue).ToArray();
        }

        return typedConstant.Value;
    }

    private static T[]? GetOptionalArray<T>(object? array)
        => array == null ? null : GetArray<T>(array);

    private static T[] GetArray<T>(object array)
        => ((object[])array).Cast<T>().ToArray();
}
""";
        }

        private static SourceExpectedGenerated GetSourceExpectedGenerated(
            bool isStatic,
            string returnType,
            bool proxyIsGlobal,
            string invokers,
            string methodName,
            string[] attributeConstructorParams,
            string[] partialDeclarationParams,
            bool generateNullableReferenceTypes = false)
        {
            string proxyNamespace = proxyIsGlobal ? string.Empty : "SourceNs";
            return new(
                GetSource(isStatic, returnType, proxyNamespace, methodName, generateNullableReferenceTypes, attributeConstructorParams),
                GetExpectedGenerated(isStatic, returnType, proxyNamespace, invokers, methodName, partialDeclarationParams));
        }

        private sealed record SourceExpectedGenerated(string Source, string ExpectedGenerated);
    }
}
