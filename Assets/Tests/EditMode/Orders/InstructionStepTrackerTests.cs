using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #40, docs/decisions/017: the current instruction card follows actions only.
    public class InstructionStepTrackerTests
    {
        [Test]
        public void Starts_AtRead_WithoutHint()
        {
            var tracker = new InstructionStepTracker();

            Assert.AreEqual(InstructionStep.Read, tracker.Step);
            Assert.IsFalse(tracker.ConfirmReady);
        }

        [Test]
        public void FirstGrab_MovesToPick_LaterGrabsChangeNothing()
        {
            var tracker = new InstructionStepTracker();

            Assert.IsTrue(tracker.PackageGrabbed());
            Assert.AreEqual(InstructionStep.Pick, tracker.Step);
            Assert.IsFalse(tracker.PackageGrabbed());
        }

        [Test]
        public void PackageInBox_KeepsPick_AndShowsHint()
        {
            var tracker = new InstructionStepTracker();
            tracker.PackageGrabbed();

            Assert.IsTrue(tracker.PackagesInBoxChanged(1));
            Assert.AreEqual(InstructionStep.Pick, tracker.Step);
            Assert.IsTrue(tracker.ConfirmReady);
            Assert.IsFalse(tracker.PackagesInBoxChanged(2));
        }

        [Test]
        public void EmptyBoxAgain_RemovesHint_KeepsPick()
        {
            var tracker = new InstructionStepTracker();
            tracker.PackageGrabbed();
            tracker.PackagesInBoxChanged(1);

            Assert.IsTrue(tracker.PackagesInBoxChanged(0));
            Assert.AreEqual(InstructionStep.Pick, tracker.Step);
            Assert.IsFalse(tracker.ConfirmReady);
        }

        [Test]
        public void PackageInBoxWithoutGrabEvent_MovesToPick()
        {
            var tracker = new InstructionStepTracker();

            Assert.IsTrue(tracker.PackagesInBoxChanged(1));
            Assert.AreEqual(InstructionStep.Pick, tracker.Step);
        }

        [Test]
        public void NextPosition_BackToRead_WithoutHint()
        {
            var tracker = new InstructionStepTracker();
            tracker.PackageGrabbed();
            tracker.PackagesInBoxChanged(2);

            Assert.IsTrue(tracker.PositionStarted());
            Assert.AreEqual(InstructionStep.Read, tracker.Step);
            Assert.IsFalse(tracker.ConfirmReady);

            // The counter reports 0 after the position change: packages of position 1 no longer count.
            Assert.IsFalse(tracker.PackagesInBoxChanged(0));
            Assert.AreEqual(InstructionStep.Read, tracker.Step);
        }

        [Test]
        public void OrderCompleted_EndsTracking()
        {
            var tracker = new InstructionStepTracker();
            tracker.PackageGrabbed();
            tracker.PackagesInBoxChanged(6);

            Assert.IsTrue(tracker.OrderCompleted());
            Assert.AreEqual(InstructionStep.Done, tracker.Step);
            Assert.IsFalse(tracker.ConfirmReady);

            Assert.IsFalse(tracker.PackageGrabbed());
            Assert.IsFalse(tracker.PackagesInBoxChanged(0));
            Assert.IsFalse(tracker.PositionStarted());
            Assert.IsFalse(tracker.OrderCompleted());
            Assert.AreEqual(InstructionStep.Done, tracker.Step);
        }
    }
}
