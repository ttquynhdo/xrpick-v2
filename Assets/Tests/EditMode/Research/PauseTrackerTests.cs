using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class PauseTrackerTests
    {
        [Test]
        public void OverlappingCauses_GiveOnePauseAndOneResume()
        {
            var p = new PauseTracker();
            Assert.AreEqual(PauseTransition.Paused, p.SetCause("hmd_unmounted", true, 10));
            Assert.AreEqual(PauseTransition.None, p.SetCause("focus_lost", true, 10.1));
            Assert.AreEqual(PauseTransition.None, p.SetCause("hmd_unmounted", false, 14));
            Assert.AreEqual(PauseTransition.Resumed, p.SetCause("focus_lost", false, 15));
            Assert.AreEqual(1, p.PauseCount);
            Assert.AreEqual(1, p.HmdRemovedCount);
            Assert.AreEqual(5.0, p.PausedSeconds(20), 1e-9);
        }

        [Test]
        public void OpenPause_CountsUpToNow()
        {
            var p = new PauseTracker();
            p.SetCause("pause", true, 3);
            Assert.AreEqual(4.0, p.PausedSeconds(7), 1e-9);
        }

        [Test]
        public void CustomHmdCause_CountsOncePerPause()
        {
            var p = new PauseTracker("hmd");
            p.SetCause("app_focus", true, 1);
            p.SetCause("hmd", true, 2);
            p.SetCause("hmd", false, 3);
            p.SetCause("hmd", true, 4);
            p.SetCause("hmd", false, 5);
            p.SetCause("app_focus", false, 6);
            Assert.AreEqual(1, p.HmdRemovedCount);
            Assert.AreEqual(1, p.PauseCount);
        }

        [Test]
        public void CausesThisPause_KeepsCausesAddedLaterAndResetsOnNextPause()
        {
            var p = new PauseTracker();
            p.SetCause("focus_lost", true, 1);
            p.SetCause("hmd_unmounted", true, 2);
            p.SetCause("hmd_unmounted", false, 3);
            p.SetCause("focus_lost", false, 4);
            CollectionAssert.AreEqual(new[] { "focus_lost", "hmd_unmounted" }, p.CausesThisPause);

            p.SetCause("pause", true, 5);
            CollectionAssert.AreEqual(new[] { "pause" }, p.CausesThisPause);
        }

        [Test]
        public void SameCauseTwice_IsIdempotent()
        {
            var p = new PauseTracker();
            p.SetCause("pause", true, 1);
            Assert.AreEqual(PauseTransition.None, p.SetCause("pause", true, 2));
            Assert.AreEqual(PauseTransition.None, p.SetCause("focus_lost", false, 2));
        }
    }
}
