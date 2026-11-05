using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class ResearchEventDataTests
    {
        [Test]
        public void NErrors_FirstOccurrence_IsOne()
        {
            var folder = new ErrorFolder(5, 200);
            ErrorDecision first = folder.Decide("boom", 0);
            Assert.IsTrue(first.Write);
            Assert.AreEqual(0, first.FoldedCount);
            Assert.AreEqual(1, first.ErrorCount);
            StringAssert.Contains("\"folded_count\":0,\"n_errors\":1",
                ResearchEventData.UnityError("error", "boom", "stack", first.FoldedCount, first.ErrorCount).ToJson());
        }

        [Test]
        public void NErrors_AfterWindow_IsFoldedPlusOne()
        {
            var folder = new ErrorFolder(5, 200);
            folder.Decide("boom", 0);
            for (int i = 1; i <= 60; i++)
            {
                Assert.AreEqual(0, folder.Decide("boom", i * 0.05).ErrorCount);
            }

            ErrorDecision afterWindow = folder.Decide("boom", 5.5);
            Assert.IsTrue(afterWindow.Write);
            Assert.AreEqual(60, afterWindow.FoldedCount);
            Assert.AreEqual(61, afterWindow.ErrorCount);
            StringAssert.Contains("\"folded_count\":60,\"n_errors\":61",
                ResearchEventData.UnityError("error", "boom", "stack", afterWindow.FoldedCount, afterWindow.ErrorCount).ToJson());
        }

        [Test]
        public void NErrors_DrainLine_IsFolded()
        {
            var folder = new ErrorFolder(5, 200);
            folder.Decide("boom", 0);
            folder.Decide("boom", 1);
            folder.Decide("boom", 2);
            var pending = folder.DrainPending();
            Assert.AreEqual(1, pending.Count);
            string json = ResearchEventData.DrainedError("error", pending[0].Key, pending[0].Value).ToJson();
            Assert.AreEqual(
                "{\"type\":\"error\",\"message\":\"boom\",\"stack\":null,\"folded_count\":2,\"n_errors\":2}", json);
        }

        [Test]
        public void NErrors_SumOfLines_EqualsErrorsRaised()
        {
            // 1 first + 60 folded + 1 after the window + 3 folded and drained at visit close = 65.
            var folder = new ErrorFolder(5, 200);
            int raised = 0;
            int logged = 0;
            void Raise(double t)
            {
                raised++;
                logged += folder.Decide("boom", t).ErrorCount;
            }

            Raise(0);
            for (int i = 1; i <= 60; i++)
            {
                Raise(i * 0.05);
            }

            Raise(5.5);
            Raise(6);
            Raise(7);
            Raise(8);
            foreach (var pending in folder.DrainPending())
            {
                logged += pending.Value;
            }

            Assert.AreEqual(65, raised);
            Assert.AreEqual(raised, logged);
        }

        [TestCase(ResearchEventData.StartModeAuto, "\"session_start_mode\":\"auto\"")]
        [TestCase(ResearchEventData.StartModeManual, "\"session_start_mode\":\"manual\"")]
        public void Header_CarriesSessionStartMode(string mode, string expected)
        {
            string json = ResearchEventData.Header("0.1.0", "868fea7", "Quest", "Android", "P101", "s1", mode,
                "DE", "controllers", 72).ToJson();
            StringAssert.Contains(expected, json);
            StringAssert.StartsWith("{\"schema_version\":2,\"layout_version\":\"" + ResearchEventData.LayoutVersion + "\",", json);
        }

        [Test]
        public void Header_ListsEmittedEventGroups()
        {
            string json = ResearchEventData.Header("0.1.0", "868fea7", "Quest", "Android", "P101", "s1",
                ResearchEventData.StartModeManual, "DE", "controllers", 72).ToJson();
            StringAssert.Contains("\"event_groups\":[\"session\",\"outcome\",\"interaction\"]", json);
        }

        [Test]
        public void SessionStart_ResumeAccepted_WritesSceneAndTrue()
        {
            Assert.AreEqual(
                "{\"resume_offered_scene\":\"MixedPickingScene\",\"resume_accepted\":true,\"ended_session_warning\":false}",
                ResearchEventData.SessionStart("MixedPickingScene", true).ToJson());
        }

        [Test]
        public void SessionStart_ResumeDeclined_WritesSceneAndFalse()
        {
            Assert.AreEqual(
                "{\"resume_offered_scene\":\"MixedPickingScene\",\"resume_accepted\":false,\"ended_session_warning\":false}",
                ResearchEventData.SessionStart("MixedPickingScene", false).ToJson());
        }

        [Test]
        public void SessionStart_NothingOffered_AcceptedIsNullEvenIfTrue()
        {
            Assert.AreEqual(
                "{\"resume_offered_scene\":null,\"resume_accepted\":null,\"ended_session_warning\":false}",
                ResearchEventData.SessionStart(null, true).ToJson());
        }

        [TestCase(true, "\"ended_session_warning\":true")]
        [TestCase(false, "\"ended_session_warning\":false")]
        public void SessionStart_EndedSessionWarning_IsWritten(bool warning, string expected)
        {
            string json = ResearchEventData.SessionStart(null, false, warning).ToJson();
            StringAssert.Contains(expected, json);
            StringAssert.EndsWith(expected + "}", json);
        }

        [Test]
        public void SessionStart_TwoArgOverload_DefaultsWarningToFalse()
        {
            Assert.AreEqual(ResearchEventData.SessionStart("ConceptScene", true, false).ToJson(),
                ResearchEventData.SessionStart("ConceptScene", true).ToJson());
        }

        [TestCase("DE")]
        [TestCase("EN")]
        public void Header_CarriesLanguage(string language)
        {
            string json = ResearchEventData.Header("0.1.0", "868fea7", "Quest", "Android", "P901", "s1",
                ResearchEventData.StartModeManual, language, "hands", 72).ToJson();
            StringAssert.Contains("\"language\":\"" + language + "\"", json);
        }

        [TestCase("P101", "EN", "DE")]
        [TestCase("P101", "DE", "DE")]
        [TestCase("P901", "EN", "EN")]
        [TestCase("P000", "EN", "EN")]
        [TestCase("P901", "DE", "DE")]
        public void SessionLanguage_ResearchCodesAreGerman(string id, string chosen, string expected)
        {
            Assert.AreEqual(expected, ResearchIds.SessionLanguage(id, chosen));
        }

        [Test]
        public void Header_NullsStayNull()
        {
            string json = ResearchEventData.Header("0.1.0", null, "Quest", "Android", "P101", "s1",
                ResearchEventData.StartModeAuto, null, null, null).ToJson();
            StringAssert.Contains("\"build\":null", json);
            StringAssert.Contains("\"language\":null", json);
            StringAssert.Contains("\"input_mode_at_start\":null", json);
            StringAssert.Contains("\"target_refresh_rate\":null", json);
        }
    }
}
