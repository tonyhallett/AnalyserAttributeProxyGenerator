namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal sealed class AnAttribute : System.Attribute
    {
        public AnAttribute(int i, string str, bool b)
        {
        }

        public AnAttribute(System.Type type)
        {
        }

        public AnAttribute(System.DayOfWeek dayOfWeek)
        {
        }

        public AnAttribute(object anObject)
        {
        }

        public AnAttribute(string[] strings)
        {
        }
    }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter
}
