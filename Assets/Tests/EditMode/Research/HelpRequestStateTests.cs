using NUnit.Framework;

namespace XRPick.Research.Tests
{
    // #182: the „Hilfe“ button takes one press, then waits until an attempt, a scene change or 60 s.
    public class HelpRequestStateTests
    {
        [Test]
        public void FirstPress_IsTaken_AndWaits()
        {
            var state = new HelpRequestState();

            Assert.IsTrue(state.TryRequest(10));
            Assert.IsTrue(state.IsWaiting);
        }

        [Test]
        public void PressWhileWaiting_IsNotTaken()
        {
            var state = new HelpRequestState();
            state.TryRequest(10);

            Assert.IsFalse(state.TryRequest(12));
            Assert.IsTrue(state.IsWaiting);
        }

        [Test]
        public void Reset_OpensOnce_ThenPressIsTakenAgain()
        {
            var state = new HelpRequestState();
            state.TryRequest(10);

            Assert.IsTrue(state.Reset());
            Assert.IsFalse(state.Reset());
            Assert.IsFalse(state.IsWaiting);
            Assert.IsTrue(state.TryRequest(20));
        }

        [Test]
        public void ResetWithoutPress_DoesNothing()
        {
            Assert.IsFalse(new HelpRequestState().Reset());
        }

        [Test]
        public void TimesOut_AfterSixtySeconds_NotBefore()
        {
            var state = new HelpRequestState();
            state.TryRequest(10);

            Assert.IsFalse(state.IsTimedOut(69.9));
            Assert.IsTrue(state.IsTimedOut(70));
        }

        [Test]
        public void NotWaiting_NeverTimesOut()
        {
            Assert.IsFalse(new HelpRequestState().IsTimedOut(1000));
        }

        [Test]
        public void HelpEvents_HaveTheSpecShapes()
        {
            Assert.AreEqual(
                "{\"tour_step\":\"soll\",\"question_id\":null,\"position\":null,\"repeat\":false,\"time_since_scene_enter_s\":12.346}",
                OutcomeEvents.HelpRequested("soll", null, null, 12.3456).ToJson());
            Assert.AreEqual(
                "{\"tour_step\":null,\"question_id\":\"ekz\",\"position\":1,\"repeat\":false,\"time_since_scene_enter_s\":3}",
                OutcomeEvents.HelpRequested(null, "ekz", 1, 3).ToJson());
            Assert.AreEqual("{\"reason\":\"timeout\"}", OutcomeEvents.HelpReset(OutcomeEvents.HelpResetTimeout).ToJson());
        }
    }
}
