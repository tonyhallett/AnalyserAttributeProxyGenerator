namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter
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
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter
}
