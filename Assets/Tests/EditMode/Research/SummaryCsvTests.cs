using System.Globalization;
using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class SummaryCsvTests
    {
        private static VisitSummary Sample()
        {
            return new VisitSummary
            {
                ParticipantId = "P101", SessionId = "s1", Build = "abc1234", SchemaVersion = 2,
                Scene = "MixedPickingScene", SceneVisit = 1, EnterUtc = "2026-11-01T09:00:00.000Z",
                DurationS = 61.5, PausedS = 4.25, Pauses = 1, HmdRemovedCount = 1, UnityErrors = 0
            };
        }

        [Test]
        public void Header_MatchesDesignColumns()
        {
            StringAssert.StartsWith(
                "participant_id,session_id,build,schema_version,scene,scene_visit,enter_utc,duration_s,paused_s,",
                SummaryCsv.Header);
            Assert.AreEqual(
                "participant_id,session_id,build,schema_version,scene,scene_visit,enter_utc,duration_s,paused_s," +
                "confirm_count,confirm_ignored,first_attempt_correct,attempts_to_correct,time_to_first_confirm_s," +
                "time_to_correct_s,completed,error_codes_all,grabs,drops_floor,container_enters,container_exits," +
                "pauses,hmd_removed_count,fps_avg,dropped_frames,unity_errors",
                SummaryCsv.Header);
        }

        [Test]
        public void FormatRow_WritesOutcomeColumns()
        {
            var v = Sample();
            v.ConfirmCount = 3;
            v.ConfirmIgnored = 1;
            v.FirstAttemptCorrect = false;
            v.AttemptsToCorrect = 3;
            v.TimeToFirstConfirmS = 12.5;
            v.TimeToCorrectS = 40.25;
            v.Completed = true;
            v.ErrorCodesAll = "TOO_FEW:1;EMPTY:1";
            var cells = SummaryCsv.FormatRow(v).Split(',');
            Assert.AreEqual(new[] { "3", "1", "false", "3", "12.5", "40.25", "true", "TOO_FEW:1;EMPTY:1" },
                new[] { cells[9], cells[10], cells[11], cells[12], cells[13], cells[14], cells[15], cells[16] });
            Assert.AreEqual("1", cells[21]); // pauses
            Assert.AreEqual("0", cells[25]); // unity_errors
        }

        [Test]
        public void FormatRow_WritesInteractionCounts()
        {
            var v = Sample();
            v.Grabs = 7;
            v.ContainerEnters = 4;
            v.ContainerExits = 1;
            var cells = SummaryCsv.FormatRow(v).Split(',');
            Assert.AreEqual(new[] { "7", "", "4", "1" }, new[] { cells[17], cells[18], cells[19], cells[20] });
        }

        [Test]
        public void FormatRow_LeavesLaterColumnsEmptyAndUsesDotDecimal()
        {
            var old = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            try
            {
                var row = SummaryCsv.FormatRow(Sample());
                var cells = row.Split(',');
                Assert.AreEqual(SummaryCsv.Header.Split(',').Length, cells.Length);
                Assert.AreEqual("61.5", cells[7]);
                Assert.AreEqual("4.25", cells[8]);
                Assert.AreEqual("", cells[9]); // confirm_count: null = empty cell (WelcomeScene)
            }
            finally
            {
                CultureInfo.CurrentCulture = old;
            }
        }

        [Test]
        public void FormatRow_NonFiniteDoubleIsEmptyCell()
        {
            var v = Sample();
            v.DurationS = double.NaN;
            v.PausedS = double.PositiveInfinity;
            var cells = SummaryCsv.FormatRow(v).Split(',');
            Assert.AreEqual("", cells[7]);
            Assert.AreEqual("", cells[8]);
        }

        [Test]
        public void FormatRow_QuotesFieldsWithCommaOrQuote()
        {
            var v = Sample();
            v.Build = "a,b\"c";
            StringAssert.Contains(",\"a,b\"\"c\",", SummaryCsv.FormatRow(v));
        }
    }
}
