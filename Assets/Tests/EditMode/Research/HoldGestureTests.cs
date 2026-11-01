using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class HoldGestureTests
    {
        [Test]
        public void QuickTap_NeverCompletes()
        {
            var h = new HoldGesture(3.0, 0.5);
            h.Press(10);
            Assert.IsFalse(h.Completed(10.2));
            h.Release();
            Assert.IsFalse(h.Completed(14));
            Assert.IsFalse(h.ShouldShowProgress(10.2));
        }

        [Test]
        public void ReleaseJustBeforeThree_Cancels()
        {
            var h = new HoldGesture(3.0, 0.5);
            h.Press(0);
            Assert.IsFalse(h.Completed(2.99));
            h.Release();
            Assert.IsFalse(h.Completed(3.5));
        }

        [Test]
        public void HoldThree_CompletesOnce()
        {
            var h = new HoldGesture(3.0, 0.5);
            h.Press(0);
            Assert.IsTrue(h.ShouldShowProgress(0.6));
            Assert.IsTrue(h.Completed(3.0));
            Assert.IsFalse(h.Completed(3.5)); // continued hold does not fire again
            Assert.AreEqual(1.0, h.Progress(4.0), 1e-9);
        }

        [Test]
        public void NewPressAfterRelease_StartsOver()
        {
            var h = new HoldGesture(3.0, 0.5);
            h.Press(0);
            h.Completed(3.0);
            h.Release();
            h.Press(10);
            Assert.IsFalse(h.Completed(12));
            Assert.IsTrue(h.Completed(13));
        }
    }
}
