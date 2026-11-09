using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using SourceGenTest = Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorTest<AnalyserAttributeProxyGenerator.ProxyGenerator, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace AnalyserAttributeProxyGenerator.Tests
{
    internal sealed class Tests
    {
        [Test]
        public Task Should_Generate_Non_Static_With_Custom_Method_Name()
        {
            string sourceCode =
$$"""
namespace SourceNs
{
    [{{ProxyGenerator.FullyQualifiedAttributeName}}(typeof(AttributeNs.ProxiedAttribute), typeof(string), "MyGen")]
    partial class Proxy
    {
        
    }
}
namespace AttributeNs
{
    public class ProxiedAttribute : System.Attribute
    {
        public ProxiedAttribute(string name) { }
        public ProxiedAttribute(System.Type type, bool flag) { }
        public ProxiedAttribute(string[] strings) { }
    }
}
""";
            const string expectedGenerated =
"""
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceNs;

partial class Proxy
{
    private SeparatedSyntaxList<AttributeArgumentSyntax> arguments;

    private partial string {|CS8795:MyGen|}(string name);

    private partial string {|CS8795:MyGen|}(ITypeSymbol type, bool flag);

    private partial string {|CS8795:MyGen|}(string[] strings);

    public string MyGen(AttributeData attributeData)
    {
        var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax();
        arguments = attribute.ArgumentList.Arguments;
        IMethodSymbol ctor = attributeData.AttributeConstructor;
        var overload = string.Join(",", ctor.Parameters.Select(p => p.Type.ToDisplayString()));
        var invokers = new Dictionary<string, Func<object[], string>>
        {
           {"string", values => MyGen((string)values[0])},
           {"System.Type,bool", values => MyGen((ITypeSymbol)values[0], (bool)values[1])},
           {"string[]", values => MyGen(GetArray<string>(values[0]))},
        };
        return invokers[overload](attributeData.ConstructorArguments.Select(GetValue).ToArray());
    }

    private static object GetValue(TypedConstant typedConstant)
    {
        if (typedConstant.Kind == TypedConstantKind.Array)
        {
            return typedConstant.Values.Select(GetValue).ToArray();
        }

        return typedConstant.Value;
    }

    private static T[] GetArray<T>(object array)
        => ((object[])array).Cast<T>().ToArray();
}
""";
            return GeneratedSourceTestAsync(sourceCode, expectedGenerated);
        }

        [Test]
        public Task Should_Generate_Static_With_Default_Method_Name()
        {
            string sourceCode =
$$"""
namespace SourceNs
{
    [{{ProxyGenerator.FullyQualifiedAttributeName}}(typeof(AttributeNs.ProxiedAttribute), typeof(bool))]
    static partial class Proxy
    {
        
    }
}
namespace AttributeNs
{
    public class ProxiedAttribute : System.Attribute
    {
        public ProxiedAttribute(int i) { }
    }
}
""";
            const string expectedGenerated =
"""
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceNs;

static partial class Proxy
{
    private static SeparatedSyntaxList<AttributeArgumentSyntax> arguments;

    private static partial bool {|CS8795:GetFromProxiedCtor|}(int i);

    public static bool GetFromProxiedCtor(AttributeData attributeData)
    {
        var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax();
        arguments = attribute.ArgumentList.Arguments;
        IMethodSymbol ctor = attributeData.AttributeConstructor;
        var overload = string.Join(",", ctor.Parameters.Select(p => p.Type.ToDisplayString()));
        var invokers = new Dictionary<string, Func<object[], bool>>
        {
           {"int", values => GetFromProxiedCtor((int)values[0])},
        };
        return invokers[overload](attributeData.ConstructorArguments.Select(GetValue).ToArray());
    }

    private static object GetValue(TypedConstant typedConstant)
    {
        if (typedConstant.Kind == TypedConstantKind.Array)
        {
            return typedConstant.Values.Select(GetValue).ToArray();
        }

        return typedConstant.Value;
    }

    private static T[] GetArray<T>(object array)
        => ((object[])array).Cast<T>().ToArray();
}
""";
            return GeneratedSourceTestAsync(sourceCode, expectedGenerated);
        }

        [Test]
        public Task Should_Work_With_Global_Namespace()
        {
            string sourceCode =
$$"""

[{{ProxyGenerator.FullyQualifiedAttributeName}}(typeof(ProxiedAttribute), typeof(string))]
partial class Proxy
{
        
}

public class ProxiedAttribute : System.Attribute
{
    public ProxiedAttribute(string name) { }
}
""";
            const string expectedGenerated =
"""
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;



partial class Proxy
{
    private SeparatedSyntaxList<AttributeArgumentSyntax> arguments;

    private partial string {|CS8795:GetFromProxiedCtor|}(string name);

    public string GetFromProxiedCtor(AttributeData attributeData)
    {
        var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax();
        arguments = attribute.ArgumentList.Arguments;
        IMethodSymbol ctor = attributeData.AttributeConstructor;
        var overload = string.Join(",", ctor.Parameters.Select(p => p.Type.ToDisplayString()));
        var invokers = new Dictionary<string, Func<object[], string>>
        {
           {"string", values => GetFromProxiedCtor((string)values[0])},
        };
        return invokers[overload](attributeData.ConstructorArguments.Select(GetValue).ToArray());
    }

    private static object GetValue(TypedConstant typedConstant)
    {
        if (typedConstant.Kind == TypedConstantKind.Array)
        {
            return typedConstant.Values.Select(GetValue).ToArray();
        }

        return typedConstant.Value;
    }

    private static T[] GetArray<T>(object array)
        => ((object[])array).Cast<T>().ToArray();
}
""";
            return GeneratedSourceTestAsync(sourceCode, expectedGenerated);
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
        public AnalyserAttributeProxyAttribute(Type proxiedAttributeType, Type returnType, string methodName = "GetFromProxiedCtor")
        {
            
        }
    }
}
""";
            var sourceGenTest = new SourceGenTest()
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
    }
}
