using System.Globalization;
using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class JsonFieldsTests
    {
        [Test]
        public void ToJson_EscapesQuotesBackslashNewlineAndKeepsUmlauts()
        {
            var json = new JsonFields().Add("message", "say \"hi\"\\\nÄÖÜß").ToJson();
            Assert.AreEqual("{\"message\":\"say \\\"hi\\\"\\\\\\nÄÖÜß\"}", json);
            Assert.IsFalse(json.Contains("\n"));
        }

        [Test]
        public void ToJson_NullsAreJsonNull()
        {
            Assert.AreEqual("{\"a\":null,\"b\":null}",
                new JsonFields().Add("a", (int?)null).Add("b", (string)null).ToJson());
        }

        [Test]
        public void ToJson_UsesDotDecimalUnderGermanCulture()
        {
            var old = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            try
            {
                Assert.AreEqual("{\"t\":41.25}", new JsonFields().Add("t", 41.25).ToJson());
            }
            finally
            {
                CultureInfo.CurrentCulture = old;
            }
        }

        [Test]
        public void ToJson_StringListAndRaw()
        {
            Assert.AreEqual("{\"causes\":[\"focus_lost\",\"hmd_unmounted\"],\"x\":{\"k\":1}}",
                new JsonFields()
                    .AddStringList("causes", new[] { "focus_lost", "hmd_unmounted" })
                    .AddRaw("x", "{\"k\":1}")
                    .ToJson());
        }
    }
}
