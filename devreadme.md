# Source generator docs

[Incremental Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md)
[Incremental Generators - in depth](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)

[Testing](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators)
This is the old source generator cookbook, there is no mention of testing for incremental source generators.
Note that the test framework specific verifier packages are now deprecated [e.g NUnit](Microsoft.CodeAnalysis.Testing.Verifiers.NUnit), as such DefaultVerifier is used.  
Also note that the test framework has `CSharpSourceGeneratorVerifier` and `SourceGeneratorVerifier`  but unlike the verifiers for analyzers there are no static methods for Verify
and as such are completely pointless.

# New lines in source generators
[False positive RS1035 error for Environment.NewLine](https://github.com/dotnet/roslyn-analyzers/issues/6467)

[How should source generators emit newlines?](https://github.com/dotnet/roslyn/issues/51437)


