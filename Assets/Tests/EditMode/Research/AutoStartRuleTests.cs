using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class AutoStartRuleTests
    {
        [TestCase(true, 0, false)]
        [TestCase(true, 1, true)]
        [TestCase(true, -1, true)]
        [TestCase(false, 3, false)]
        public void AutoStart_NotInFirstBuildScene(bool setting, int index, bool expected)
        {
            Assert.AreEqual(expected, AutoStartRule.ShouldAutoStart(setting, index));
        }
    }
}
