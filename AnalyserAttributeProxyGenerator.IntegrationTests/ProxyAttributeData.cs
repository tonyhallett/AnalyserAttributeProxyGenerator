using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Moq;

namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal class ProxyAttributeData : AttributeData
    {
        public const string MockTypeSymbolName = "MockedTypeSymbol";

        private static readonly ConstructorInfo s_nonArrayConstructor;
        private static readonly ConstructorInfo s_arrayConstructor;

        private static readonly Dictionary<Type, string> s_typeAliases = new()
        {
            [typeof(string)] = "string",
            [typeof(int)] = "int",
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort",
            [typeof(uint)] = "uint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(char)] = "char",
            [typeof(object)] = "object",
            [typeof(Type)] = "System.Type",
        };

        static ProxyAttributeData()
        {
            var mockTypeSymbol = new Mock<ITypeSymbol>();
            mockTypeSymbol.SetupGet(ts => ts.Name).Returns(MockTypeSymbolName);
            mockTypeSymbol.Setup(ts => ts.ToString()).Returns(MockTypeSymbolName);
            MockedTypeSymbol = mockTypeSymbol.Object;

            ConstructorInfo[] ctors = typeof(TypedConstant).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            s_nonArrayConstructor = ctors.First(c => c.GetParameters().Length == 3);
            s_arrayConstructor = ctors.First(c => c.GetParameters().Length == 2);
        }

        public ProxyAttributeData(params object[] constructorArgs)
        {
            CommonConstructorArguments = [.. constructorArgs.Select(GetTypedConstant)];
            var parameters = constructorArgs.Select(arg => GetParameterSymbol(GetParameterTypeName(arg))).ToImmutableArray();
            var mockAttributeConstructor = new Mock<IMethodSymbol>();
            mockAttributeConstructor.SetupGet(c => c.Parameters).Returns(parameters);
            CommonAttributeConstructor = mockAttributeConstructor.Object;
        }

        public ProxyAttributeData(object obj)
        {
            if (obj.GetType().IsArray)
            {
                CommonConstructorArguments = [CreateNonArrayTypedConstant(obj)];
            }
            else
            {
                CommonConstructorArguments = [GetTypedConstant(obj)];
            }

            ImmutableArray<IParameterSymbol> parameters = [GetParameterSymbol("object")];
            var mockAttributeConstructor = new Mock<IMethodSymbol>();
            mockAttributeConstructor.SetupGet(c => c.Parameters).Returns(parameters);
            CommonAttributeConstructor = mockAttributeConstructor.Object;
        }

        protected override INamedTypeSymbol? CommonAttributeClass { get; }
        protected override IMethodSymbol? CommonAttributeConstructor { get; }
        protected override SyntaxReference? CommonApplicationSyntaxReference { get; } = new AttributeSyntaxReference();
        protected override ImmutableArray<TypedConstant> CommonConstructorArguments { get; }
        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments { get; } = [];

        private static ITypeSymbol MockedTypeSymbol { get; }

        private static TypedConstant GetTypedConstant(object value)
        {
            if (value is Type)
            {
                value = MockedTypeSymbol;
            }

            if (value.GetType().IsArray)
            {
                var array = (Array)value;
                List<TypedConstant> arrayTypedConstants = [];
                foreach (object? entry in array)
                {
                    arrayTypedConstants.Add(CreateNonArrayTypedConstant(entry));
                }

                ImmutableArray<TypedConstant> typedConstantsArg = [.. arrayTypedConstants];

                return (TypedConstant)s_arrayConstructor.Invoke([null, typedConstantsArg]);
            }

            return CreateNonArrayTypedConstant(value);
        }

        private static TypedConstant CreateNonArrayTypedConstant(object value) => (TypedConstant)s_nonArrayConstructor.Invoke([null, TypedConstantKind.Primitive, value]);

        private static IParameterSymbol GetParameterSymbol(string typeName)
        {
            var mockParameterSymbol = new Mock<IParameterSymbol>();
            mockParameterSymbol.SetupGet(p => p.Type).Returns(new DisplayStringTypeSymbol(typeName));
            return mockParameterSymbol.Object;
        }

        private static string GetParameterTypeName(object ctorArg)
        {
            if (ctorArg is Type)
            {
                return "System.Type";
            }

            Type ctorArgType = ctorArg.GetType();
            if (ctorArgType.IsArray)
            {
                Type elementType = ctorArgType.GetElementType()!;
                string elementAlias = s_typeAliases[elementType];
                return $"{elementAlias}[]";
            }

            return s_typeAliases.TryGetValue(ctorArgType, out string? alias) ? alias : ctorArgType.FullName!.Replace('+', '.');
        }

        private sealed class AttributeSyntaxReference : SyntaxReference
        {
            public override SyntaxTree SyntaxTree { get; }

            public override TextSpan Span { get; }

            public override SyntaxNode GetSyntax(CancellationToken cancellationToken = default)
            {
                return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(string.Empty), SyntaxFactory.AttributeArgumentList());
            }
        }

#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
        private sealed class DisplayStringTypeSymbol(string displayString) : ITypeSymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
        {
            public TypeKind TypeKind { get; }
            public INamedTypeSymbol? BaseType { get; }
            public ImmutableArray<INamedTypeSymbol> Interfaces { get; }
            public ImmutableArray<INamedTypeSymbol> AllInterfaces { get; }
            public bool IsReferenceType { get; }
            public bool IsValueType { get; }
            public bool IsAnonymousType { get; }
            public bool IsTupleType { get; }
            public bool IsNativeIntegerType { get; }
            public bool IsExtension { get; }
            public IParameterSymbol? ExtensionParameter { get; }
            public ITypeSymbol OriginalDefinition { get; }
            public SpecialType SpecialType { get; }
            public bool IsRefLikeType { get; }
            public bool IsUnmanagedType { get; }
            public bool IsReadOnly { get; }
            public bool IsRecord { get; }
            public bool IsUnion { get; }
            public bool IsClosed { get; }
            public NullableAnnotation NullableAnnotation { get; }
            public bool IsNamespace { get; }
            public bool IsType { get; }
            public SymbolKind Kind { get; }
            public string Language { get; }
            public string Name { get; }
            public string MetadataName { get; }
            public int MetadataToken { get; }
            public ISymbol ContainingSymbol { get; }
            public IAssemblySymbol ContainingAssembly { get; }
            public IModuleSymbol ContainingModule { get; }
            public INamedTypeSymbol ContainingType { get; }
            public INamespaceSymbol ContainingNamespace { get; }
            public bool IsDefinition { get; }
            public bool IsStatic { get; }
            public bool IsVirtual { get; }
            public bool IsOverride { get; }
            public bool IsAbstract { get; }
            public bool IsSealed { get; }
            public bool IsExtern { get; }
            public bool IsImplicitlyDeclared { get; }
            public bool CanBeReferencedByName { get; }
            public ImmutableArray<Location> Locations { get; }
            public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }
            public Accessibility DeclaredAccessibility { get; }
            public bool HasUnsupportedMetadata { get; }
            ISymbol ISymbol.OriginalDefinition { get; }

            public void Accept(SymbolVisitor visitor) => throw new NotImplementedException();

            public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor) => throw new NotImplementedException();

            public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument) => throw new NotImplementedException();

            public bool Equals([NotNullWhen(true)] ISymbol? other, SymbolEqualityComparer equalityComparer) => throw new NotImplementedException();

            public bool Equals(ISymbol? other) => throw new NotImplementedException();

            public ISymbol? FindImplementationForInterfaceMember(ISymbol interfaceMember) => throw new NotImplementedException();

            public ImmutableArray<AttributeData> GetAttributes() => throw new NotImplementedException();

#pragma warning disable RSEXPERIMENTAL006 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            public ClosedDerivedTypeInfo GetClosedDerivedTypeInfo(CancellationToken cancellationToken) => throw new NotImplementedException();

#pragma warning restore RSEXPERIMENTAL006 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            public string? GetDocumentationCommentId() => throw new NotImplementedException();

            public string? GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public ImmutableArray<ISymbol> GetMembers() => throw new NotImplementedException();

            public ImmutableArray<ISymbol> GetMembers(string name) => throw new NotImplementedException();

            public ImmutableArray<INamedTypeSymbol> GetTypeMembers() => throw new NotImplementedException();

            public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name) => throw new NotImplementedException();

            public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name, int arity) => throw new NotImplementedException();

            public ImmutableArray<SymbolDisplayPart> ToDisplayParts(NullableFlowState topLevelNullability, SymbolDisplayFormat? format = null) => throw new NotImplementedException();

            public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat? format = null) => throw new NotImplementedException();

            public string ToDisplayString(NullableFlowState topLevelNullability, SymbolDisplayFormat? format = null) => throw new NotImplementedException();

            public string ToDisplayString(SymbolDisplayFormat? format = null) => displayString;

            public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat? format = null) => throw new NotImplementedException();

            public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null) => throw new NotImplementedException();

            public string ToMinimalDisplayString(SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat? format = null) => throw new NotImplementedException();

            public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null) => throw new NotImplementedException();

            public ITypeSymbol WithNullableAnnotation(NullableAnnotation nullableAnnotation) => throw new NotImplementedException();
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
