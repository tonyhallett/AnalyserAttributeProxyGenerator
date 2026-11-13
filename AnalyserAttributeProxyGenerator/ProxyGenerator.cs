using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        private readonly string[] _referenceTypes = ["ITypeSymbol", "object", "string"];

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
        public {{AnalyserAttributeProxyAttributeName}}(Type proxiedAttributeType, Type returnType, bool returnTypeIsNullableReferenceType = false, string methodName = "{{DefaultGeneratedMethodName}}", bool generateNullableReferenceTypes = false)
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
                    INamedTypeSymbol returnType = attributeClass.GetValue<INamedTypeSymbol>(1);
                    string returnTypeString = returnType.ToDisplayString();
                    bool returnTypeIsNullableReferenceType = false;
                    if (returnType.IsReferenceType)
                    {
                        returnTypeIsNullableReferenceType = attributeClass.GetValue<bool>(2);
                    }

                    return new ProxyAttributeModel(
                        GetNamespaceName(partialClassSymbol),
                        partialClassSymbol.Name,
                        partialClassSymbol.IsStatic,
                        returnTypeString,
                        returnTypeIsNullableReferenceType,
                        attributeClass.GetValue<string>(3),
                        attributeClass.GetValue<bool>(4),
                        new ConstructorParameters(attributeClass.GetValue<INamedTypeSymbol>(0)));
                });

            IncrementalValuesProvider<(ProxyAttributeModel? Left, Compilation Right)> combined = pipeline.Combine(context.CompilationProvider);

            context.RegisterSourceOutput(combined, (context, result) =>
            {
                (ProxyAttributeModel? model, Compilation compilation) = result;
                LanguageVersion languageVersion = ((CSharpCompilation)compilation).LanguageVersion;
                bool nullableAnnotationsEnabled = languageVersion >= LanguageVersion.CSharp8 && compilation.Options.NullableContextOptions.AnnotationsEnabled();
                if (model != null)
                {
                    ILanguageVersionInvoker languageVersionInvoker = languageVersion >= LanguageVersion.CSharp9 ? new CSharp9Invoker() : new CSharpPre9Invoker();
                    LanguageVersionInvocation languageVersionInvocation = languageVersionInvoker.Get(
                        model.ReturnType,
                        model.ReturnTypeIsNullableReferenceType,
                        "invokers[overload](attributeData.ConstructorArguments.Select(GetValue).ToArray());");
                    string partialDeclarationsReturnType = languageVersionInvocation.ReturnType;
                    string privateKeyword = languageVersionInvocation.DeclarationPrivateKeyword;
                    bool generatedNullableParameters = false;
                    bool generateNullableReferenceTypes = nullableAnnotationsEnabled && model.GenerateNullableReferenceTypes;
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

                    bool applyNullableAnnotationToDeclarations = generatedNullableParameters || (model.ReturnTypeIsNullableReferenceType && partialDeclarationsReturnType != "void");
                    string partialDeclarationsNullableEnable = applyNullableAnnotationToDeclarations ? "\r\n#nullable enable" : string.Empty;
                    if (applyNullableAnnotationToDeclarations)
                    {
                        stringBuilders.PartialMethods.AppendLine("#nullable disable");
                    }

                    string privateVariable = languageVersionInvocation.PrivateVariable;
                    if (privateVariable != string.Empty)
                    {
                        if (model.ReturnTypeIsNullableReferenceType)
                        {
                            privateVariable =
$$"""
#nullable enable
{{privateVariable}}
#nullable disable


""";
                        }
                        else
                        {
                            privateVariable += "\r\n\r\n";
                        }
                    }

                    string mainReturnType = model.ReturnTypeIsNullableReferenceType ? model.ReturnType + "?" : model.ReturnType;
                    string mainSignature = $"    public {staticKeyword}{mainReturnType} {generatedMethodName}(AttributeData attributeData)";
                    if (model.ReturnTypeIsNullableReferenceType)
                    {
                        mainSignature =
$$"""
#nullable enable
{{mainSignature}}
#nullable disable
""";
                    }

                    string classSource =
    $$"""
{{staticKeyword}}partial class {{model.PartialClassName}}
{
{{privateVariable}}    private {{staticKeyword}}SeparatedSyntaxList<AttributeArgumentSyntax> arguments;
{{partialDeclarationsNullableEnable}}
{{stringBuilders.PartialMethods}}
{{mainSignature}}
    {
        var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax();
        arguments = attribute.ArgumentList.Arguments;
        IMethodSymbol ctor = attributeData.AttributeConstructor;
        var overload = string.Join(",", ctor.Parameters.Select(p => p.Type.ToDisplayString()));
        var invokers = new Dictionary<string, {{languageVersionInvocation.DictionaryValueType}}>
        {
{{stringBuilders.InvokerEntries}}
        };
        {{languageVersionInvocation.ReturnCode}}
    }

    private static object GetValue(TypedConstant typedConstant)
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

    private static T[] GetArray<T>(object array) => ((object[])array).Cast<T>().ToArray();
}
""";
                    string namespaceEnd = string.Empty;
                    string namespaceStart = string.Empty;
                    if (!string.IsNullOrEmpty(model.PartialNamespaceName))
                    {
                        namespaceStart =
    $$"""
namespace {{model.PartialNamespaceName}}
{
""";
                        namespaceEnd = "}";
                    }

                    string source =
$$"""
// <auto-generated/>
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

{{namespaceStart}}
{{classSource}}
{{namespaceEnd}}
""";

                    context.AddSource($"{model.PartialClassName}.g.cs", SourceText.From(source, Encoding.UTF8));

                    string CreateProxiedGetter(List<ConstructorParameter> constructorParameters)
                    {
                        StringBuilder sb = ConstructorParametersCommaDelimitingBuilder(
                            string.Empty,
                            constructorParameters,
                            (sb, pType, _, parameter) => sb.Append($"{GetParameterType(pType)} {parameter.Name}"));
                        return
    $$"""
    {{privateKeyword}}{{staticKeyword}}partial {{partialDeclarationsReturnType}} {{generatedMethodName}}({{sb}});

""";
                    }

                    string CreateInvokerEntry(List<ConstructorParameter> constructorParameters)
                    {
                        string lookupKey = string.Join(",", constructorParameters.Select(p => p.Type));
                        return ConstructorParametersCommaDelimitingBuilder(
                            $"            {{\"{lookupKey}\", values => {generatedMethodName}(",
                            constructorParameters,
                            (sb, pType, i, _) =>
                            {
                                if (pType.EndsWith("[]", StringComparison.InvariantCulture))
                                {
                                    sb.Append($"GetArray<{pType[..^2]}>(values[{i}])");
                                }
                                else
                                {
                                    sb.Append($"({pType})values[{i}]");
                                }
                            })
                            .Append(")},").ToString();
                    }

                    StringBuilder ConstructorParametersCommaDelimitingBuilder(
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

                    string GetParameterType(string parameterType)
                    {
                        if (!generateNullableReferenceTypes)
                        {
                            return parameterType;
                        }

                        if (parameterType.EndsWith("[]", StringComparison.InvariantCulture))
                        {
                            generatedNullableParameters = true;
                            return GetParameterType(parameterType[..^2]) + "[]?";
                        }

                        if (_referenceTypes.Contains(parameterType))
                        {
                            generatedNullableParameters = true;
                            return parameterType + "?";
                        }

                        return parameterType;
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

        private record ProxyAttributeModel(
            string PartialNamespaceName,
            string PartialClassName,
            bool IsStatic,
            string ReturnType,
            bool ReturnTypeIsNullableReferenceType,
            string GeneratedMethodName,
            bool GenerateNullableReferenceTypes,
            ConstructorParameters ConstructorParameters);

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
