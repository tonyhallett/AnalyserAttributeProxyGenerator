namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
    internal class Tests
    {
        [Test]
        public void Should_Work_With_Multiple_Primitive_Parameter_Types()
            => Assert.That(GetFromProxiedCtor(1, "test", true), Is.EqualTo("i: 1, str: test, b: True"));

        [Test]
        public void Should_Work_With_Parameter_Type_Type()
           => Assert.That(GetFromProxiedCtor(typeof(bool)), Is.EqualTo(ProxyAttributeData.MockTypeSymbolName));

        [Test]
        public void Should_Work_With_Parameter_Type_Enum()
            => Assert.That(GetFromProxiedCtor(AnAttribute.AnEnum.One), Is.EqualTo("One"));

        [Test]
        public void Should_Work_With_Parameter_Type_StringArray()
        {
            string[] stringArrayArg = ["a", "b", "c"];
            Assert.That(GetFromProxiedCtor([stringArrayArg]), Is.EqualTo("a,b,c"));
        }

        private static string GetFromProxiedCtor(params object[] ctorArgs)
            => new Proxy().GetFromProxiedCtor(new ProxyAttributeData(ctorArgs));
    }
}
