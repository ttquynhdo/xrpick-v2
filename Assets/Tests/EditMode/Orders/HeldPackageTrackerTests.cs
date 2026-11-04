using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #90, docs/decisions/026 (2): label tags follow the packages held right now.
    public class HeldPackageTrackerTests
    {
        [Test]
        public void Select_HoldsPackage_ReleaseLetsGo()
        {
            var tracker = new HeldPackageTracker();

            Assert.IsTrue(tracker.Select(1, 10, "stecker_c"));
            Assert.IsTrue(tracker.IsMaterialHeld("stecker_c"));
            Assert.IsTrue(tracker.Release(1, 10));
            Assert.IsFalse(tracker.IsMaterialHeld("stecker_c"));
            Assert.AreEqual(0, tracker.Held.Count);
        }

        [Test]
        public void TwoHandsOnOnePackage_HeldUntilBothLetGo()
        {
            var tracker = new HeldPackageTracker();
            tracker.Select(1, 10, "stecker_c");

            Assert.IsFalse(tracker.Select(1, 11, "stecker_c"));
            Assert.IsFalse(tracker.Release(1, 10));
            Assert.AreEqual(1, tracker.Held.Count);
            Assert.IsTrue(tracker.Release(1, 11));
        }

        [Test]
        public void TwoPackages_BothHeldInGrabOrder()
        {
            var tracker = new HeldPackageTracker();
            tracker.Select(2, 10, "screw_b");
            tracker.Select(1, 11, "stecker_c");

            CollectionAssert.AreEqual(new[] { 2, 1 }, tracker.Held);
            tracker.Release(2, 10);
            Assert.IsFalse(tracker.IsMaterialHeld("screw_b"));
            Assert.IsTrue(tracker.IsMaterialHeld("stecker_c"));
        }

        [Test]
        public void ReleaseWithoutSelect_ChangesNothing()
        {
            var tracker = new HeldPackageTracker();
            tracker.Select(1, 10, "ring_d");

            Assert.IsFalse(tracker.Release(1, 99));
            Assert.IsFalse(tracker.Release(5, 10));
            Assert.AreEqual(1, tracker.Held.Count);
        }

        [Test]
        public void TagSides_OnePackage_SideItIsOn()
        {
            CollectionAssert.AreEqual(new[] { -1 }, HeldPackageTracker.TagSides(new[] { -0.2f }));
            CollectionAssert.AreEqual(new[] { 1 }, HeldPackageTracker.TagSides(new[] { 0.2f }));
            CollectionAssert.AreEqual(new[] { 1 }, HeldPackageTracker.TagSides(new[] { 0f }));
        }

        [Test]
        public void TagSides_TwoPackages_OutwardEvenOnOneSide()
        {
            CollectionAssert.AreEqual(new[] { 1, -1 }, HeldPackageTracker.TagSides(new[] { 0.1f, -0.1f }));
            CollectionAssert.AreEqual(new[] { -1, 1 }, HeldPackageTracker.TagSides(new[] { 0.1f, 0.3f }));
        }
    }
}
