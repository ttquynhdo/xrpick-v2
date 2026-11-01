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
                ParticipantId = "P101", SessionId = "s1", Build = "abc1234", SchemaVersion = 1,
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
            Assert.AreEqual(27, SummaryCsv.Header.Split(',').Length);
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
                Assert.AreEqual("", cells[9]); // confirm_count: not measured in #26
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
