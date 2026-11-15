using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AnalyserAttributeProxyGenerator
{
    internal readonly struct ConstructorParameters : IEquatable<ConstructorParameters>
    {
        private readonly string _cachedStringRepresentation;

        public ConstructorParameters(INamedTypeSymbol attributeTypeToProxy)
        {
            Value = [.. attributeTypeToProxy.Constructors.Select(attributeConstructor => attributeConstructor.Parameters.Select(p => new ConstructorParameter(p.Type.ToDisplayString(), p.Name)).ToList())];

            // Compute and cache the string representation ONCE during construction
            _cachedStringRepresentation = SerializeToString(Value);
        }

        // Expose your original nested data structure
        public List<List<ConstructorParameter>> Value { get; }

        // Direct performance shortcut for the Roslyn Cache
        public bool Equals(ConstructorParameters other) =>
            _cachedStringRepresentation == other._cachedStringRepresentation;

        public override bool Equals(object? obj) =>
            obj is ConstructorParameters other && Equals(other);

        public override int GetHashCode() => _cachedStringRepresentation.GetHashCode();

        public override string ToString() => _cachedStringRepresentation;

        private static string SerializeToString(List<List<ConstructorParameter>> list)
        {
            // Deterministically flatten the nested hierarchy into a unique string signature
            IEnumerable<string> lines = list.Select(innerList =>
                $"[{string.Join(", ", innerList.Select(p => $"{p.Type} {p.Name}"))}]");
            return string.Join("|", lines);
        }
    }
}
