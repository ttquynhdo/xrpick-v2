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

        [TestCase("P800", true)]
        [TestCase("P850", true)]
        [TestCase("P899", true)]
        [TestCase("P799", false)]
        [TestCase("P900", false)]
        [TestCase("P000", false)]
        [TestCase("p850", false)]
        public void IsPilotId_IsP800To899(string id, bool expected)
        {
            Assert.AreEqual(expected, ResearchIds.IsPilotId(id));
        }

        [Test]
        public void FolderFor_TestIdGoesToPractice()
        {
            Assert.AreEqual(Path.Combine("root", "practice"), ResearchIds.FolderFor("root", "P950"));
        }

        [Test]
        public void FolderFor_PilotIdGoesToPilot()
        {
            Assert.AreEqual(Path.Combine("root", "pilot"), ResearchIds.FolderFor("root", "P801"));
        }

        [Test]
        public void FolderFor_StudyIdStaysInRoot()
        {
            Assert.AreEqual("root", ResearchIds.FolderFor("root", "P017"));
        }

        [TestCase("P017", "EN", "DE")]
        [TestCase("P799", "EN", "DE")]
        [TestCase("P801", "EN", "EN")]
        [TestCase("P801", "DE", "DE")]
        [TestCase("P950", "EN", "EN")]
        public void SessionLanguage_OnlyStudyIdsAreForcedToGerman(string id, string chosen, string expected)
        {
            Assert.AreEqual(expected, ResearchIds.SessionLanguage(id, chosen));
        }
    }
}
