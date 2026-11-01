using System.IO;
using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class ResearchIdsTests
    {
        [TestCase("P017", true)]
        [TestCase("P000", true)]
        [TestCase("P999", true)]
        [TestCase("17", false)]
        [TestCase("p017", false)]
        [TestCase("P1000", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsValid_AcceptsOnlyPThreeDigits(string id, bool expected)
        {
            Assert.AreEqual(expected, ResearchIds.IsValid(id));
        }

        [TestCase("P000", true)]
        [TestCase("P900", true)]
        [TestCase("P999", true)]
        [TestCase("P001", false)]
        [TestCase("P899", false)]
        public void IsTestId_IsP000AndP900To999(string id, bool expected)
        {
            Assert.AreEqual(expected, ResearchIds.IsTestId(id));
        }

        [Test]
        public void FolderFor_TestIdGoesToPractice()
        {
            Assert.AreEqual(Path.Combine("root", "practice"), ResearchIds.FolderFor("root", "P950"));
        }
    }
}
