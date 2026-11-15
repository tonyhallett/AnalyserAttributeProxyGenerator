namespace AnalyserAttributeProxyGenerator
{
    internal static class NullableEnable
    {
        public const string Enable = "#nullable enable";
        public const string Disable = "#nullable disable";

        public static string Apply(string code)
        {
            return
$$"""
{{Enable}}
{{code}}
{{Disable}}
""";
        }
    }
}
