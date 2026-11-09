# AnalyzerAttributeProxyGenerator

A Source generator that generates private partial declaring methods with the same or similar arguments as the constructors of a specified attribute with return type that you specify.

The generator is to be used for attributes applied for reading by an analyzer as the public generated method takes `AttributeData` as a parameter and will invoke the corresponding partial implementing proxy constructor method.

e.g with return type string
`public string GetFromProxiedCtor(AttributeData attributeData)`

## Example

Given an attribute

```C#
    [AttributeUsage(AttributeTargets.All)]
    internal class AnAttribute : Attribute
    {
        public AnAttribute(int i, string str, bool b)
        {
        }

        public AnAttribute(Type type)
        {
        }
    }

```

and a proxy for it attributed with the `AnalyserAttributeProxy` attribute, here with return type string

```
using AnalyserAttributeProxy;
using Microsoft.CodeAnalysis;


[AnalyserAttributeProxy(typeof(AnAttribute), typeof(string))]
internal partial class Proxy
{

}
```

the generated partial proxy creates 2 partial method declarations.

The first has parameters matching the constructor arguments whereas the second has ITypeSymbol instead of Type.

The implementation just uses the constructor IMethodSymbol parameter types to determine which partial method to invoke.

```
partial class Proxy
{
    private SeparatedSyntaxList<AttributeArgumentSyntax> arguments;

    private partial string GetFromProxiedCtor(int i, string str, bool b);

    private partial string GetFromProxiedCtor(ITypeSymbol type);

    public string GetFromProxiedCtor(AttributeData attributeData)
    {
        var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax();
        arguments = attribute.ArgumentList.Arguments;
        IMethodSymbol ctor = attributeData.AttributeConstructor;
        var overload = string.Join(",", ctor.Parameters.Select(p => p.Type.ToDisplayString()));
        var invokers = new Dictionary<string, Func<object[], string>>
        {
           {"int,string,bool", values => GetFromProxiedCtor((int)values[0], (string)values[1], (bool)values[2])},
           {"System.Type", values => GetFromProxiedCtor((ITypeSymbol)values[0])},
        };
        return invokers[overload](attributeData.ConstructorArguments.Select(arg => arg.Value).ToArray());
    }
}
```

## Changing the generated method names

`public AnalyserAttributeProxyAttribute(Type proxiedAttributeType, Type returnType, string methodName = "GetFromProxiedCtor")`

Just provide a third argument to change the generated method name.

## Supported attribute constructor parameter types

Just primitives and enums for now.  Object and arrays are not supported.  Type is supported but the generated partial method will have ITypeSymbol instead of Type.



