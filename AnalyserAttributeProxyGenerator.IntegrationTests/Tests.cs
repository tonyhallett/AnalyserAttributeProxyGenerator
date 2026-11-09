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
        public void Should_Work_With_Parameter_Type_Object_String()
        {
            object obj = "string object";
            Assert.That(GetFromProxiedCtorObjectArg(obj), Is.EqualTo("string object"));
        }

        [Test]
        public void Should_Work_With_Parameter_Type_Object_Type()
        {
            object obj = typeof(Type);
            Assert.That(GetFromProxiedCtorObjectArg(obj), Is.EqualTo(ProxyAttributeData.MockTypeSymbolName));
        }

        [Test]
        public void Should_Work_With_Parameter_Type_Object_StringArray()
        {
            string[] stringArrayArg = [];
            object obj = stringArrayArg;
            Assert.That(GetFromProxiedCtorObjectArg(obj), Is.EqualTo(stringArrayArg.ToString()));
        }

        [Test]
        public void Should_Work_With_Parameter_Type_StringArray()
        {
            string[] stringArrayArg = ["a", "b", "c"];
            Assert.That(GetFromProxiedCtor([stringArrayArg]), Is.EqualTo("a,b,c"));
        }

        private static string GetFromProxiedCtor(params object[] ctorArgs)
            => new Proxy().GetFromProxiedCtor(new ProxyAttributeData(ctorArgs));

        private static string GetFromProxiedCtorObjectArg(object ctorArg)
            => new Proxy().GetFromProxiedCtor(new ProxyAttributeData(ctorArg));
    }
}
