namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class AnAttribute : Attribute
    {
        public AnAttribute(int i, string str, bool b)
        {
        }

        public AnAttribute(Type type)
        {
        }

        public AnAttribute(AnEnum anEnum)
        {
        }

        public AnAttribute(object anObject)
        {
        }

        public AnAttribute(string[] strings)
        {
        }

        public enum AnEnum
        {
            One = 0,
            Two = 1,
        }
    }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter
}
