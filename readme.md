# AnalyzerAttributeProxyGenerator

A Source generator that generates partial declaring methods with the same or similar arguments as the constructors of a specified attribute.

If your project is using C# 9 or higher then the generated partial methods will be private and have a return type that you specify.
It is an error to not implement such methods.

Below C# 9 the generated partial methods are void and for every generated method you should implement and from the body set the private returnValue variable.

The generator is to be used for attributes applied for reading by an analyzer as the public generated method takes `AttributeData` as a parameter and will invoke the corresponding partial implementing proxy constructor method.

e.g with return type string
`public string GetFromProxiedCtor(AttributeData attributeData)`

## Example C# 9

Given an attribute

```C#
    internal sealed class AnAttribute : Attribute
    {
        public AnAttribute(int i, string str, bool b)
        {
        }

        public AnAttribute(Type type)
        {
        }

        public AnAttribute(DayOfWeek dayOfWeek)
        {
        }

        public AnAttribute(object anObject)
        {
        }

        public AnAttribute(string[] strings)
        {
        }
    }

    public enum AnEnum
    {
        One = 0,
        Two = 1,
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

the generated partial proxy creates partial method declarations for each attribute constructor.

When the parameter type is Type ITypeSymbol will be used instead.

The implementation just uses the constructor IMethodSymbol parameter types to determine which partial method to invoke.

```
partial class Proxy
{
    private SeparatedSyntaxList<AttributeArgumentSyntax> arguments;

    private partial string GetFromProxiedCtor(int i, string str, bool b);

    private partial string GetFromProxiedCtor(ITypeSymbol type);

    private partial string GetFromProxiedCtor(System.DayOfWeek dayOfWeek);

    private partial string GetFromProxiedCtor(object anObject);

    private partial string GetFromProxiedCtor(string[] strings);

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
           {"System.DayOfWeek", values => GetFromProxiedCtor((System.DayOfWeek)values[0])},
           {"object", values => GetFromProxiedCtor((object)values[0])},
           {"string[]", values => GetFromProxiedCtor((string[])values[0])},
        };
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
}
```

The `arguments` field is provided if you need to look at the syntax for the arguments to your attribute constructor.

## Additional arguments

`public AnalyserAttributeProxyAttribute(Type proxiedAttributeType, Type returnType, string methodName = "GetFromProxiedCtor",  bool generateNullableReferenceTypes = false)`

Just provide a third argument to change the generated method name.

If the fourth argument is true then instead of e.g

`private partial string GetFromProxiedCtor(ITypeSymbol type);`

The parameter will be a nullable reference type.

`private partial string GetFromProxiedCtor(ITypeSymbol? type);`








