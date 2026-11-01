using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class SessionFilesTests
    {
        private string root;
        private int headerCounter;

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);
            headerCounter = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }

        // Writes valid lines via EventLine. "scene_enter:X" sets scene = X; the header utc grows per call.
        private void WriteEvents(string rootDir, string id, string session, params string[] events)
        {
            var files = new SessionFiles(rootDir, id, session);
            headerCounter++;
            var utc = "2026-11-01T09:" + headerCounter.ToString("00") + ":00.000Z";
            double t = 0;
            foreach (var e in events)
            {
                string name = e;
                string scene = null;
                int? visit = null;
                int colon = e.IndexOf(':');
                if (colon >= 0)
                {
                    name = e.Substring(0, colon);
                    scene = e.Substring(colon + 1);
                    visit = 1;
                }

                Assert.IsTrue(files.AppendEvent(SessionFiles.EventLine(t, utc, id, session, scene, visit, name, null)));
                t += 1.5;
            }
        }

        [Test]
        public void TestId_WritesUnderPractice()
        {
            var files = new SessionFiles(root, "P901", "s1");
            Assert.IsTrue(files.AppendEvent("{\"event\":\"header\"}"));
            Assert.IsTrue(File.Exists(Path.Combine(root, "practice", "P901_s1_events.jsonl")));
        }

        [Test]
        public void ForceToDisk_AppendsLikeNormalWrite()
        {
            var files = new SessionFiles(root, "P101", "s1");
            Assert.IsTrue(files.AppendEvent("{\"a\":1}"));
            Assert.IsTrue(files.AppendEvent("{\"b\":2}", true));
            Assert.IsTrue(files.AppendEvent("{\"c\":3}"));
            Assert.AreEqual("{\"a\":1}\n{\"b\":2}\n{\"c\":3}\n", File.ReadAllText(files.EventsPath));
        }

        [Test]
        public void Summary_WritesHeaderOnce()
        {
            var files = new SessionFiles(root, "P101", "s1");
            files.AppendSummary("a");
            files.AppendSummary("b");
            var lines = File.ReadAllLines(files.SummaryPath);
            Assert.AreEqual(new[] { SummaryCsv.Header, "a", "b" }, lines);
        }

        [Test]
        public void EventLine_KeepsFieldOrderAndRoundsT()
        {
            var line = SessionFiles.EventLine(1.23456, "2026-11-01T09:00:00.000Z", "P101", "s1", "ConceptScene", 2, "x",
                new JsonFields().Add("k", 1));
            Assert.AreEqual(
                "{\"t\":1.235,\"utc\":\"2026-11-01T09:00:00.000Z\",\"participant_id\":\"P101\",\"session_id\":\"s1\"," +
                "\"scene\":\"ConceptScene\",\"scene_visit\":2,\"event\":\"x\",\"data\":{\"k\":1}}", line);
        }

        [Test]
        public void CrashScan_MarksOnlyUnendedFiles_AndOnlyOnce()
        {
            WriteEvents(root, "P101", "old", "header", "scene_enter:MixedPickingScene");
            WriteEvents(root, "P102", "done", "header", "session_end");
            Assert.AreEqual(1, SessionFiles.MarkCrashedSessions(root, "new", "2026-11-01T10:00:00.000Z"));
            Assert.AreEqual(0, SessionFiles.MarkCrashedSessions(root, "newer", "2026-11-01T11:00:00.000Z"));
            StringAssert.Contains("\"recovered_by_session_id\":\"new\"",
                File.ReadAllLines(Path.Combine(root, "P101_old_events.jsonl")).Last());
        }

        [Test]
        public void CrashScan_ScansPracticeFolderAndWritesDesignFields()
        {
            WriteEvents(root, "P901", "old", "header", "scene_enter:ConceptScene");
            Assert.AreEqual(1, SessionFiles.MarkCrashedSessions(root, "new", "2026-11-01T10:00:00.000Z"));
            var last = File.ReadAllLines(Path.Combine(root, "practice", "P901_old_events.jsonl")).Last();
            StringAssert.Contains("\"t\":null", last);
            StringAssert.Contains("\"utc\":\"2026-11-01T10:00:00.000Z\"", last);
            StringAssert.Contains("\"participant_id\":\"P901\"", last);
            StringAssert.Contains("\"session_id\":\"old\"", last);
            StringAssert.Contains("\"event\":\"crash_recovered\"", last);
            StringAssert.Contains("\"last_event_t\":1.5", last);
        }

        [Test]
        public void ResumePoint_ReturnsLastSceneOfCrashedSession()
        {
            WriteEvents(root, "P101", "old", "header", "scene_enter:ConceptScene", "scene_enter:MixedPickingScene");
            Assert.IsTrue(SessionFiles.TryFindResumePoint(root, "P101", "current", out var scene));
            Assert.AreEqual("MixedPickingScene", scene);
        }

        [Test]
        public void ResumePoint_UsesNewestFileAndIgnoresExcluded()
        {
            WriteEvents(root, "P101", "a", "header", "scene_enter:ConceptScene");
            WriteEvents(root, "P101", "b", "header", "scene_enter:ComplexScene");
            WriteEvents(root, "P101", "cur", "header", "scene_enter:BasicPickingScene");
            Assert.IsTrue(SessionFiles.TryFindResumePoint(root, "P101", "cur", out var scene));
            Assert.AreEqual("ComplexScene", scene);
        }

        [Test]
        public void ResumePoint_NothingForCleanOrUnknown()
        {
            WriteEvents(root, "P102", "done", "header", "scene_enter:BasicPickingScene", "session_end");
            Assert.IsFalse(SessionFiles.TryFindResumePoint(root, "P102", "current", out _));
            Assert.IsFalse(SessionFiles.TryFindResumePoint(root, "P555", "current", out _));
        }

        [Test]
        public void TruncatedLastLine_CountsAsCrash_WithoutThrowing()
        {
            WriteEvents(root, "P103", "cut", "header", "scene_enter:BasicPickingScene");
            var path = Path.Combine(root, "P103_cut_events.jsonl");
            File.AppendAllText(path, "{\"t\":12.0,\"event\":\"scene_ex");
            Assert.DoesNotThrow(() => SessionFiles.MarkCrashedSessions(root, "new", "2026-11-01T10:00:00.000Z"));
            Assert.IsTrue(SessionFiles.TryFindResumePoint(root, "P103", "current", out var scene));
            Assert.AreEqual("BasicPickingScene", scene);

            // The marker must sit on its own line, never glued to the cut line.
            var lines = File.ReadAllLines(path);
            StringAssert.Contains("\"event\":\"crash_recovered\"", lines.Last());
            StringAssert.StartsWith("{\"t\":null", lines.Last());
            Assert.AreEqual(0, SessionFiles.MarkCrashedSessions(root, "again", "2026-11-01T11:00:00.000Z"));
        }

        [Test]
        public void WriteFailure_ReturnsFalseAndStaysFailed()
        {
            var blocker = Path.Combine(root, "blocked");
            File.WriteAllText(blocker, "a file where a folder must be");
            var files = new SessionFiles(blocker, "P101", "s1");
            Assert.IsFalse(files.AppendEvent("{}"));
            Assert.IsFalse(files.AppendSummary("x"));
        }
    }
}
