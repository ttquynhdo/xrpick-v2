using System.Collections.Generic;
using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class PauseCausesTests
    {
        [Test]
        public void TwoFocusSources_PauseEndsOnlyWhenBothRegained()
        {
            var p = new PauseTracker(PauseCauses.Hmd);
            Assert.AreEqual(PauseTransition.Paused, p.SetCause(PauseCauses.AppFocus, true, 1));
            Assert.AreEqual(PauseTransition.None, p.SetCause(PauseCauses.InputFocus, true, 2));
            Assert.AreEqual(PauseTransition.None, p.SetCause(PauseCauses.AppFocus, false, 3));
            Assert.IsTrue(p.IsPaused);
            Assert.AreEqual(PauseTransition.Resumed, p.SetCause(PauseCauses.InputFocus, false, 4));
            CollectionAssert.AreEqual(new[] { "focus_lost" }, PauseCauses.Labels(p.CausesThisPause));
            Assert.AreEqual(3.0, p.PausedSeconds(10), 1e-9);
        }

        [Test]
        public void Labels_MapEverySourceAndKeepOrder()
        {
            var sources = new List<string> { PauseCauses.Hmd, PauseCauses.AppPause, PauseCauses.InputFocus };
            CollectionAssert.AreEqual(new[] { "hmd_unmounted", "pause", "focus_lost" }, PauseCauses.Labels(sources));
        }

        [Test]
        public void Labels_NullGivesEmptyList()
        {
            Assert.AreEqual(0, PauseCauses.Labels(null).Count);
        }
    }
}
