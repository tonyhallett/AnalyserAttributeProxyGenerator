using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AnalyserAttributeProxyGenerator
{
    /// <summary>
    /// Generator for creating partial class with proxy methods for attribute constructors.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public class ProxyGenerator : IIncrementalGenerator
    {
        internal const string DefaultGeneratedMethodName = "GetFromProxiedCtor";
        internal const string AttributeNamespace = "AnalyserAttributeProxy";
        internal const string AnalyserAttributeProxyAttributeName = "AnalyserAttributeProxyAttribute";

        internal static string FullyQualifiedAttributeName => $"{AttributeNamespace}.{AnalyserAttributeProxyAttributeName}";

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static postInitializationContext =>
            {
                postInitializationContext.AddEmbeddedAttributeDefinition();
                postInitializationContext.AddSource($"{AnalyserAttributeProxyAttributeName}.g.cs", SourceText.From(
$$"""
using System;
using Microsoft.CodeAnalysis;

namespace {{AttributeNamespace}}
{
    [AttributeUsage(AttributeTargets.Class), Embedded]
    internal sealed class {{AnalyserAttributeProxyAttributeName}} : Attribute
    {
        public {{AnalyserAttributeProxyAttributeName}}(Type proxiedAttributeType, Type returnType, string methodName = "{{DefaultGeneratedMethodName}}")
        {
            
        }
    }
}
""",
Encoding.UTF8));
            });

            IncrementalValuesProvider<ProxyAttributeModel?> pipeline = context.SyntaxProvider.ForPartialClassWithAttributeWithMetadataName(
                fullyQualifiedMetadataName: FullyQualifiedAttributeName,
                transform: static (partialClassSymbol, _, _) =>
                {
                    AttributeData attributeClass = GetAttributeData(partialClassSymbol);
                    return new ProxyAttributeModel(
                        GetNamespaceName(partialClassSymbol),
                        partialClassSymbol.Name,
                        partialClassSymbol.IsStatic,
                        attributeClass.GetValue<INamedTypeSymbol>(1).ToDisplayString(),
                        attributeClass.GetValue<string>(2),
                        new ConstructorParameters(attributeClass.GetValue<INamedTypeSymbol>(0)));
                });

            context.RegisterSourceOutput(pipeline, (context, model) =>
            {
                if (model != null)
                {
                    string generatedMethodName = model.GeneratedMethodName;
                    string staticKeyword = model.IsStatic ? "static " : string.Empty;
                    var stringBuilders = new StringBuilders();
                    List<List<ConstructorParameter>> allConstructorParameters = model.ConstructorParameters.Value;
                    for (int i = 0; i < allConstructorParameters.Count; i++)
                    {
                        bool isLast = i == allConstructorParameters.Count - 1;
                        List<ConstructorParameter> constructorParameters = allConstructorParameters[i];
                        stringBuilders.Append(CreateProxiedGetter(constructorParameters), CreateInvokerEntry(constructorParameters), isLast);
                    }

                    string classSource =
$$"""
{{staticKeyword}}partial class {{model.PartialClassName}}
{
    private {{staticKeyword}}SeparatedSyntaxList<AttributeArgumentSyntax> arguments;

{{stringBuilders.PartialMethods}}
    public {{staticKeyword}}{{model.ReturnType}} {{generatedMethodName}}(AttributeData attributeData)
    {
        var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax();
        arguments = attribute.ArgumentList.Arguments;
        IMethodSymbol ctor = attributeData.AttributeConstructor;
        var overload = string.Join(",", ctor.Parameters.Select(p => p.Type.ToDisplayString()));
        var invokers = new Dictionary<string, Func<object[], {{model.ReturnType}}>>
        {
{{stringBuilders.InvokerEntries}}
        };
        return invokers[overload](attributeData.ConstructorArguments.Select(arg => arg.Value).ToArray());
    }
}
""";
                    string namespaceSource = string.IsNullOrEmpty(model.PartialNamespaceName) ? string.Empty : $"namespace {model.PartialNamespaceName};";
                    string source =
$$"""
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

{{namespaceSource}}

{{classSource}}
""";
                    context.AddSource($"{model.PartialClassName}.g.cs", SourceText.From(source, Encoding.UTF8));

                    string CreateProxiedGetter(List<ConstructorParameter> constructorParameters)
                    {
                        StringBuilder sb = StringBuilderCommon(
                            string.Empty,
                            constructorParameters,
                            (sb, pType, _, parameter) => sb.Append($"{pType} {parameter.Name}"));
                        return
$$"""
    private {{staticKeyword}}partial {{model.ReturnType}} {{generatedMethodName}}({{sb}});

""";
                    }

                    string CreateInvokerEntry(List<ConstructorParameter> constructorParameters)
                    {
                        string lookupKey = string.Join(",", constructorParameters.Select(p => p.Type));
                        return StringBuilderCommon(
                            $"           {{\"{lookupKey}\", values => {generatedMethodName}(",
                            constructorParameters,
                            (sb, pType, i, _) => sb.Append($"({pType})values[{i}]"))
                        .Append(")},").ToString();
                    }

                    StringBuilder StringBuilderCommon(
                        string sbStart,
                        List<ConstructorParameter> constructorParameters,
                        Action<StringBuilder, string, int, ConstructorParameter> parameterAction)
                    {
                        StringBuilder sb = new(sbStart);
                        for (int i = 0; i < constructorParameters.Count; i++)
                        {
                            ConstructorParameter parameter = constructorParameters[i];
                            if (i > 0)
                            {
                                sb.Append(", ");
                            }

                            string parameterType = parameter.Type == "System.Type" ? "ITypeSymbol" : parameter.Type;
                            parameterAction(sb, parameterType, i, parameter);
                        }

                        return sb;
                    }
                }
            });
        }

        private static AttributeData GetAttributeData(INamedTypeSymbol partialClassSymbol)
            => partialClassSymbol.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == FullyQualifiedAttributeName);

        private static string GetNamespaceName(INamedTypeSymbol partialClassSymbol)
        {
            INamespaceSymbol partialNamespaceSymbol = partialClassSymbol.ContainingNamespace;
            string partialNamespaceName = string.Empty;
            if (!partialNamespaceSymbol.IsGlobalNamespace)
            {
                // not looking at nested
                partialNamespaceName = partialNamespaceSymbol.ToDisplayString();
            }

            return partialNamespaceName;
        }

        private record ProxyAttributeModel(string PartialNamespaceName, string PartialClassName, bool IsStatic, string ReturnType, string GeneratedMethodName, ConstructorParameters ConstructorParameters);

        private class StringBuilders
        {
            public StringBuilder PartialMethods { get; } = new();
            public StringBuilder InvokerEntries { get; } = new();

            public void Append(string partialMethod, string invokerEntry, bool isLast)
            {
                Action<string, string> appendAction = isLast ? Append : AppendLine;
                appendAction(partialMethod, invokerEntry);
            }

            private void Append(string partialMethod, string invokerEntry)
            {
                PartialMethods.Append(partialMethod);
                InvokerEntries.Append(invokerEntry);
            }

            private void AppendLine(string partialMethod, string invokerEntry)
            {
                PartialMethods.AppendLine(partialMethod);
                InvokerEntries.AppendLine(invokerEntry);
            }
        }
    }
}
