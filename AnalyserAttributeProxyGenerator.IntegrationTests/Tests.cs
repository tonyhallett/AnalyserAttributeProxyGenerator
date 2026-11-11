namespace AnalyserAttributeProxyGenerator.IntegrationTests
{
    internal class Tests
    {
        [Test]
        public void Should_Work_With_Multiple_Primitive_Parameter_Types()
            => Assert.That(GetFromProxiedCtor(1, "test", true), Is.EqualTo("i: 1, str: test, b: True"));

        [Test]
        public void Should_Work_With_Nullable_Value_Type()
        {
            int? nullableInt = 42;
            Assert.That(GetFromProxiedCtorNullable(nullableInt, false, "int?"), Is.EqualTo("42"));
        }

        [Test]
        public void Should_Work_With_Nullable_Value_Type_Null()
        {
            int? nullableInt = null;
            Assert.That(GetFromProxiedCtorNullable(nullableInt, false, "int?"), Is.EqualTo("null"));
        }

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

        [Test]
        public void Should_Work_With_Parameter_Type_Nullable_Int_Array()
        {
            int[]? intArrayArg = [1, 2, 3];
            Assert.That(GetFromProxiedCtorNullable(intArrayArg, true, "int[]?"), Is.EqualTo("1,2,3"));
        }

        [Test]
        public void Should_Work_With_Parameter_Type_Nullable_Int_Array_Null()
        {
            int[]? intArrayArg = null;
            Assert.That(GetFromProxiedCtorNullable(intArrayArg, true, "int[]?"), Is.EqualTo("null"));
        }

        [Test]
        public void Should_Work_With_Parameter_Type_Array_Of_Arrays()
        {
            string[][] stringArrayArg = [["a", "b"], ["c", "d"]];
            Assert.That(GetFromProxiedCtor([stringArrayArg]), Is.EqualTo("a,b - c,d"));
        }

        private static string GetFromProxiedCtor(params object[] ctorArgs)
            => new Proxy().GetFromProxiedCtor(new ProxyAttributeData(ctorArgs));

        private static string GetFromProxiedCtorNullable<T>(T? nullable, bool isArray, string typeName)
            => new Proxy().GetFromProxiedCtor(new ProxyAttributeData(nullable, isArray, typeName));

        private static string GetFromProxiedCtorObjectArg(object ctorArg)
            => new Proxy().GetFromProxiedCtor(new ProxyAttributeData(ctorArg));
    }
}
