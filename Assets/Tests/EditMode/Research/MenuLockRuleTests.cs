using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class MenuLockRuleTests
    {
        [TestCase(0, 1, true)]
        [TestCase(0, 0, false)]
        [TestCase(0, 2, false)]
        [TestCase(2, 3, true)]
        [TestCase(2, 1, false)]
        [TestCase(2, 2, false)]
        [TestCase(4, 0, false)]
        [TestCase(-1, 0, false)]
        public void ResearchId_LessonDone_OnlyNextLessonOpen(int current, int target, bool expected)
        {
            Assert.AreEqual(expected, MenuLockRule.IsOpen("P017", current, target, true));
        }

        [TestCase(0, 1)]
        [TestCase(2, 3)]
        [TestCase(2, 1)]
        public void ResearchId_LessonNotDone_EverythingLocked(int current, int target)
        {
            Assert.IsFalse(MenuLockRule.IsOpen("P017", current, target, false));
        }

        [TestCase(null)]
        [TestCase("P000")]
        [TestCase("P950")]
        public void NoSessionOrTestId_EveryItemOpen(string id)
        {
            Assert.IsTrue(MenuLockRule.IsOpen(id, 2, 0, false));
            Assert.IsTrue(MenuLockRule.IsOpen(id, 2, 2, false));
            Assert.IsTrue(MenuLockRule.IsOpen(id, 0, 4, true));
        }
    }
}
