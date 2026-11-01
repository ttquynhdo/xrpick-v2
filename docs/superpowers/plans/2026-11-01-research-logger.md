# Research Logger Core (#26) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Every Unity edit also follows the project skill `unity-safe-change` (test plan, UnityMCP runs, `docs/test-reports/bl26.md`).

**Goal:** A null-safe, crash-tolerant session logger on Quest that writes `events.jsonl` + `summary.csv` per session, plus an R pipeline that proves on fake data that the format answers research questions 1, 2 and 6.

**Architecture:** Pure C# classes (IDs, JSON, pause state, error folding, CSV, file layout) live in a new `XRPick.Research` assembly so they get EditMode tests. One `ResearchLogger` MonoBehaviour, created at app start and kept across scenes, wires them to Unity callbacks and exposes a static API. The R side (`analysis/`) generates fake sessions in the same format and turns any set of sessions into charts and a wide file.

**Tech Stack:** Unity 6000.4.5f1, C# (`System.IO`, no JSON package), Unity Test Framework 1.6 (EditMode, NUnit), Meta XR Core `Oculus.VR` (pause/HMD events), UnityMCP 10.3.0 for Play-mode checks, R with `tidyverse` + `jsonlite`.

**Spec:** `docs/superpowers/specs/2026-11-01-research-logger-design.md` (approved 2026-11-01). Event catalogue: `docs/research-logging-spec.md`.

## Global Constraints

- App identity: Company `XRISE`, Product `xpick`, Android package id `com.xrise.xpick`.
- Participant ID format `P` + 3 digits. Test IDs: `P000` and `P900`–`P999` → `research/practice/`. Real: `P001`–`P899` → `research/`.
- Files: `<participant_id>_<session_id>_events.jsonl`, `<participant_id>_<session_id>_summary.csv`; `session_id` = UUID; file names contain nothing but the IDs.
- Every event line: `t`, `utc`, `participant_id`, `session_id`, `scene`, `scene_visit`, `event`, `data`. Line 1 is the header with `"event":"header"`.
- `t` from `Time.realtimeSinceStartupAsDouble` minus session start; `utc` ISO 8601 with `Z`.
- Time/length fields carry units (`_s`, `_m`) and are active time unless named `_wall_s`. "Not measured" is empty/`null`, never 0.
- Numbers in JSON and CSV use invariant culture (dot decimal); CSV comma-separated, UTF-8 without BOM, header row.
- `schema_version` = `1`. Renaming/removing a field bumps it; adding doesn't.
- Write each line to disk immediately. No per-frame allocation or disk write from logging.
- Logging never changes what the trainee sees or how scenes flow. With no session, every API call is a silent no-op.
- No personal data: no names, no free text; file paths in error messages cut to the file name.
- Follow `docs/coding-conventions.md`: new scripts in `Assets/Scripts/Research/`, one public class per file, file name = class name, Allman braces, English comments, `Debug.Log*` in English. No scene edits in this item.
- Never create, delete or hand-edit `.meta` files outside what Unity generates for new files.
- `analysis/data/` and `analysis/output/` are git-ignored; real logs are never committed.

## Review Focus

1. **Disk write fails** (storage full, permission): the logger must not throw into gameplay. It stops writing for that session and prints one `Debug.LogWarning`. → test in Task 4.
2. **Truncated last line after power-off** (half-written JSON): the crash scan and `TryFindResumePoint` must not throw; the file counts as crashed. → test in Task 4.
3. **Error text with quotes, backslashes, newlines, umlauts** must stay one valid JSON line. → test in Task 2.
4. **German system culture** (`de-DE`, decimal comma) must not change number formatting in JSON or CSV. → tests in Task 2 and Task 4.
5. **API used before any session exists, or with a malformed ID** (`"17"`, `"p017"`, `"P1000"`): no file, no exception; `StartSession` returns false and logs an error. → tests in Task 2 and Task 5.

---

### Task 0: Branches and backlog start

**Files:**
- Modify: `docs/backlog.md` (#26 *Done when*, **Status**)

- [ ] **Step 1: Commit the pending docs on their own branch and merge.** `git switch -c docs/backlog-review`, commit `docs/backlog.md` and `docs/superpowers/specs/2026-11-01-research-logger-design.md` and this plan as `docs: review backlog, add #42/#43, design and plan for #26`, then switch to `main` and merge it (`--no-ff`, as earlier branches).
- [ ] **Step 2: Branch** `git switch -c feat/26-research-logger`.
- [ ] **Step 3: Merge the design's §9 *Done when* into #26.** Replace the line "Dừng Play giữa chừng rồi chạy lại thì có `crash_recovered`" with the dev-option case, remove "(đặt tạm trong Inspector)" (the test ID now comes from `ResearchSettings`, Task 5). Set **Status** `in progress`. Commit `docs: start #26`.

### Task 1: App identity and ignore rules

**Files:**
- Modify: `ProjectSettings/ProjectSettings.asset` (via the Editor, not by hand)
- Modify: `.gitignore`

- [ ] **Step 1: Set the identity through UnityMCP `execute_code`:** `PlayerSettings.companyName = "XRISE"; PlayerSettings.productName = "xpick"; PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.xrise.xpick"); PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.xrise.xpick"); AssetDatabase.SaveAssets();`
- [ ] **Step 2: Verify:** `git diff ProjectSettings/ProjectSettings.asset` shows only `companyName: XRISE`, `productName: xpick` and the two identifier lines changed; `execute_code` returning `Application.persistentDataPath` ends in `/XRISE/xpick`.
- [ ] **Step 3: Append to `.gitignore`:** `/analysis/data/`, `/analysis/output/`, `/Assets/Resources/ResearchBuildInfo.txt`, `/Assets/Resources/ResearchBuildInfo.txt.meta`.
- [ ] **Step 4: Commit** `chore: app identity XRISE/xpick/com.xrise.xpick, ignore research outputs (#26)`.

### Task 2: Research assembly, IDs and JSON lines

**Files:**
- Create: `Assets/Scripts/Research/XRPick.Research.asmdef` (name `XRPick.Research`, references `Oculus.VR`, auto-referenced so `Assembly-CSharp` scripts can call it)
- Create: `Assets/Scripts/Research/ResearchIds.cs`
- Create: `Assets/Scripts/Research/JsonFields.cs`
- Create: `Assets/Tests/EditMode/Research/XRPick.Research.Tests.asmdef` (Editor platform only, references `XRPick.Research`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, `nunit.framework.dll` precompiled, define constraint `UNITY_INCLUDE_TESTS`)
- Test: `Assets/Tests/EditMode/Research/ResearchIdsTests.cs`, `JsonFieldsTests.cs`

Why an asmdef here: `docs/coding-conventions.md` §10 says test assemblies can't reference `Assembly-CSharp`. Putting only the new `Research/` folder in its own assembly makes it testable without touching the old scripts. The `Research` code never references `Assembly-CSharp` types.

**Interfaces:**
- Produces:
  - `public static class ResearchIds { bool IsValid(string id); bool IsTestId(string id); string FolderFor(string researchRoot, string id); }`. `FolderFor` returns `researchRoot/practice` for test IDs, else `researchRoot`.
  - `public sealed class JsonFields`: fluent `Add(string key, string value)`, `Add(string key, double? value)`, `Add(string key, int? value)`, `Add(string key, bool? value)`, `AddRaw(string key, string json)`, `AddStringList(string key, IReadOnlyList<string> values)`, each returning `this`; `string ToJson()` → `{...}` with keys in insertion order, `null` for null values.
  - `public static string JsonFields.Escape(string s)`: JSON string escaping (quotes, backslash, control chars as `\n`, `\r`, `\t`, `\uXXXX`; non-ASCII kept as UTF-8).

- [ ] **Step 1: Write the failing tests.**

```csharp
[TestCase("P017", true)] [TestCase("P000", true)] [TestCase("P999", true)]
[TestCase("17", false)] [TestCase("p017", false)] [TestCase("P1000", false)] [TestCase("", false)] [TestCase(null, false)]
public void IsValid_AcceptsOnlyPThreeDigits(string id, bool expected) => Assert.AreEqual(expected, ResearchIds.IsValid(id));

[TestCase("P000", true)] [TestCase("P900", true)] [TestCase("P999", true)] [TestCase("P001", false)] [TestCase("P899", false)]
public void IsTestId_IsP000AndP900To999(string id, bool expected) => Assert.AreEqual(expected, ResearchIds.IsTestId(id));

[Test] public void FolderFor_TestIdGoesToPractice() =>
    Assert.AreEqual(Path.Combine("root", "practice"), ResearchIds.FolderFor("root", "P950"));

[Test] public void ToJson_EscapesQuotesBackslashNewlineAndKeepsUmlauts()
{
    var json = new JsonFields().Add("message", "say \"hi\"\\\nÄÖÜß").ToJson();
    Assert.AreEqual("{\"message\":\"say \\\"hi\\\"\\\\\\nÄÖÜß\"}", json);
    Assert.IsFalse(json.Contains("\n"));
}

[Test] public void ToJson_NullsAreJsonNull() =>
    Assert.AreEqual("{\"a\":null,\"b\":null}", new JsonFields().Add("a", (int?)null).Add("b", (string)null).ToJson());

[Test] public void ToJson_UsesDotDecimalUnderGermanCulture()
{
    var old = CultureInfo.CurrentCulture;
    CultureInfo.CurrentCulture = new CultureInfo("de-DE");
    try { Assert.AreEqual("{\"t\":41.25}", new JsonFields().Add("t", 41.25).ToJson()); }
    finally { CultureInfo.CurrentCulture = old; }
}

[Test] public void ToJson_StringListAndRaw() =>
    Assert.AreEqual("{\"causes\":[\"focus_lost\",\"hmd_unmounted\"],\"x\":{\"k\":1}}",
        new JsonFields().AddStringList("causes", new[] { "focus_lost", "hmd_unmounted" }).AddRaw("x", "{\"k\":1}").ToJson());
```

- [ ] **Step 2: Run, expect compile failure.** UnityMCP `run_tests` (mode EditMode, filter `XRPick.Research.Tests`), or with the Editor closed: `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml -testFilter XRPick.Research.Tests`. Expected: errors "ResearchIds/JsonFields does not exist".
- [ ] **Step 3: Implement** `ResearchIds` (regex `^P\d{3}$`) and `JsonFields` (StringBuilder, `ToString("R", CultureInfo.InvariantCulture)` for doubles).
- [ ] **Step 4: Run, expect all PASS.** `read_console` shows no compile errors in `Assembly-CSharp` (the old scripts must still compile with the new auto-referenced assembly).
- [ ] **Step 5: Commit** `feat: research assembly with participant IDs and JSON lines (#26)`.

### Task 3: Pause state and error folding

**Files:**
- Create: `Assets/Scripts/Research/PauseTracker.cs`, `Assets/Scripts/Research/ErrorFolder.cs`
- Test: `Assets/Tests/EditMode/Research/PauseTrackerTests.cs`, `ErrorFolderTests.cs`

**Interfaces:**
- Produces:
  - `public enum PauseTransition { None, Paused, Resumed }`
  - `public sealed class PauseTracker`: `PauseTransition SetCause(string cause, bool active, double t)` (cause ∈ `focus_lost`, `hmd_unmounted`, `pause`); `IReadOnlyList<string> ActiveCauses`; `bool IsPaused`; `int PauseCount`; `int HmdRemovedCount` (pauses whose causes ever included `hmd_unmounted`); `double PausedSeconds(double now)` (total, including a still-open pause up to `now`).
  - `public sealed class ErrorFolder`: ctor `(double windowSeconds = 5, int cap = 200)`; `ErrorDecision Decide(string message, double t)`; `static string StripPaths(string text)`. `public readonly struct ErrorDecision { bool Write; int FoldedCount; bool CapReachedNow; }`: `Write` is true for the first occurrence and for the first occurrence after the window; `FoldedCount` is how many identical messages were swallowed since the last written one; `CapReachedNow` is true exactly once, on the first message past the cap.

- [ ] **Step 1: Write the failing tests.**

```csharp
[Test] public void OverlappingCauses_GiveOnePauseAndOneResume()
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

[Test] public void OpenPause_CountsUpToNow()
{
    var p = new PauseTracker();
    p.SetCause("pause", true, 3);
    Assert.AreEqual(4.0, p.PausedSeconds(7), 1e-9);
}

[Test] public void SameCauseTwice_IsIdempotent()
{
    var p = new PauseTracker();
    p.SetCause("pause", true, 1);
    Assert.AreEqual(PauseTransition.None, p.SetCause("pause", true, 2));
    Assert.AreEqual(PauseTransition.None, p.SetCause("focus_lost", false, 2));
}

[Test] public void RepeatedError_IsFoldedWithinWindow()
{
    var f = new ErrorFolder(5, 200);
    Assert.IsTrue(f.Decide("boom", 0).Write);
    for (int i = 1; i <= 60; i++) Assert.IsFalse(f.Decide("boom", i * 0.016).Write);
    var next = f.Decide("boom", 5.5);
    Assert.IsTrue(next.Write);
    Assert.AreEqual(60, next.FoldedCount);
}

[Test] public void Cap_IsReportedOnce()
{
    var f = new ErrorFolder(5, 2);
    f.Decide("a", 0); f.Decide("b", 0);
    var third = f.Decide("c", 0);
    Assert.IsFalse(third.Write); Assert.IsTrue(third.CapReachedNow);
    Assert.IsFalse(f.Decide("d", 0).CapReachedNow);
}

[Test] public void StripPaths_KeepsOnlyFileNames() =>
    Assert.AreEqual("at Foo.cs:12 and Bar.cs:3",
        ErrorFolder.StripPaths(@"at C:\Users\anna\proj\Foo.cs:12 and /home/x/Bar.cs:3"));
```

- [ ] **Step 2: Run, expect compile failure** (same command as Task 2).
- [ ] **Step 3: Implement both classes** (plain C#, no `UnityEngine`).
- [ ] **Step 4: Run, expect all PASS.**
- [ ] **Step 5: Commit** `feat: pause state and error folding for research log (#26)`.

### Task 4: Summary rows and session files

**Files:**
- Create: `Assets/Scripts/Research/SummaryCsv.cs`, `Assets/Scripts/Research/SessionFiles.cs`
- Test: `Assets/Tests/EditMode/Research/SummaryCsvTests.cs`, `SessionFilesTests.cs` (each test uses its own `Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())` root and deletes it in `TearDown`)

**Interfaces:**
- Consumes: `ResearchIds.FolderFor`, `JsonFields` (Task 2).
- Produces:
  - `public static class SummaryCsv { const string Header; string FormatRow(VisitSummary v); }`. `Header` is exactly the design §5 column list, in order: `participant_id,session_id,build,schema_version,scene,scene_visit,enter_utc,duration_s,paused_s,confirm_count,first_attempt_correct,attempts_to_correct,time_to_first_confirm_s,time_to_correct_s,completed,error_codes_all,grabs,drops_floor,container_enters,container_exits,dwell_wms_s,dwell_instructions_s,pauses,hmd_removed_count,fps_avg,dropped_frames,unity_errors`. Columns #26 doesn't fill are empty. Fields containing `,` or `"` are quoted.
  - `public sealed class VisitSummary` with the #26 fields: `ParticipantId, SessionId, Build, SchemaVersion, Scene, SceneVisit, EnterUtc, DurationS, PausedS, Pauses, HmdRemovedCount, UnityErrors`.
  - `public sealed class SessionFiles`: ctor `(string researchRoot, string participantId, string sessionId)`; `string EventsPath`, `string SummaryPath`; `bool AppendEvent(string jsonLine)`, `bool AppendSummary(string csvRow)` (writes the CSV header first when the file is new; returns false on IO failure and stays failed for this session; never throws); `static int MarkCrashedSessions(string researchRoot, string recoveredBySessionId, string utcNow)` (scans `research/` and `research/practice/`; every events file whose last parseable line is neither `session_end` nor `crash_recovered` gets one `crash_recovered` line; returns how many); `static bool TryFindResumePoint(string researchRoot, string participantId, string excludeSessionId, out string scene)`.
  - Event line helper: `public static string SessionFiles.EventLine(double? t, string utc, string participantId, string sessionId, string scene, int? sceneVisit, string eventName, JsonFields data)` producing the common-field order from Global Constraints, `t` rounded to 3 decimals.

`TryFindResumePoint` takes the participant's newest events file by header `utc` (ignoring `excludeSessionId`). It returns true with the `scene` of its last `scene_enter` only if that file has no `session_end`. Lines that don't parse are skipped.

- [ ] **Step 1: Write the failing tests.**

```csharp
[Test] public void Header_MatchesDesignColumns() =>
    StringAssert.StartsWith("participant_id,session_id,build,schema_version,scene,scene_visit,enter_utc,duration_s,paused_s,", SummaryCsv.Header);

[Test] public void FormatRow_LeavesLaterColumnsEmptyAndUsesDotDecimal()
{
    var old = CultureInfo.CurrentCulture; CultureInfo.CurrentCulture = new CultureInfo("de-DE");
    try
    {
        var row = SummaryCsv.FormatRow(new VisitSummary { ParticipantId = "P101", SessionId = "s1", Build = "abc1234", SchemaVersion = 1,
            Scene = "MixedPickingScene", SceneVisit = 1, EnterUtc = "2026-11-01T09:00:00.000Z", DurationS = 61.5, PausedS = 4.25,
            Pauses = 1, HmdRemovedCount = 1, UnityErrors = 0 });
        var cells = row.Split(',');
        Assert.AreEqual(SummaryCsv.Header.Split(',').Length, cells.Length);
        Assert.AreEqual("61.5", cells[7]); Assert.AreEqual("4.25", cells[8]);
        Assert.AreEqual("", cells[9]); // confirm_count: not measured in #26
    }
    finally { CultureInfo.CurrentCulture = old; }
}

[Test] public void TestId_WritesUnderPractice()
{
    var files = new SessionFiles(root, "P901", "s1");
    Assert.IsTrue(files.AppendEvent("{\"event\":\"header\"}"));
    Assert.IsTrue(File.Exists(Path.Combine(root, "practice", "P901_s1_events.jsonl")));
}

[Test] public void Summary_WritesHeaderOnce()
{
    var files = new SessionFiles(root, "P101", "s1");
    files.AppendSummary("a"); files.AppendSummary("b");
    var lines = File.ReadAllLines(files.SummaryPath);
    Assert.AreEqual(new[] { SummaryCsv.Header, "a", "b" }, lines);
}

[Test] public void CrashScan_MarksOnlyUnendedFiles_AndOnlyOnce()
{
    WriteEvents(root, "P101", "old", "header", "scene_enter:MixedPickingScene");      // crashed
    WriteEvents(root, "P102", "done", "header", "session_end");                         // clean
    Assert.AreEqual(1, SessionFiles.MarkCrashedSessions(root, "new", "2026-11-01T10:00:00.000Z"));
    Assert.AreEqual(0, SessionFiles.MarkCrashedSessions(root, "newer", "2026-11-01T11:00:00.000Z"));
    StringAssert.Contains("\"recovered_by_session_id\":\"new\"", File.ReadAllLines(Path.Combine(root, "P101_old_events.jsonl")).Last());
}

[Test] public void ResumePoint_ReturnsLastSceneOfCrashedSession()
{
    WriteEvents(root, "P101", "old", "header", "scene_enter:ConceptScene", "scene_enter:MixedPickingScene");
    Assert.IsTrue(SessionFiles.TryFindResumePoint(root, "P101", "current", out var scene));
    Assert.AreEqual("MixedPickingScene", scene);
}

[Test] public void ResumePoint_NothingForCleanOrUnknown()
{
    WriteEvents(root, "P102", "done", "header", "scene_enter:BasicPickingScene", "session_end");
    Assert.IsFalse(SessionFiles.TryFindResumePoint(root, "P102", "current", out _));
    Assert.IsFalse(SessionFiles.TryFindResumePoint(root, "P555", "current", out _));
}

[Test] public void TruncatedLastLine_CountsAsCrash_WithoutThrowing()
{
    WriteEvents(root, "P103", "cut", "header", "scene_enter:BasicPickingScene");
    File.AppendAllText(Path.Combine(root, "P103_cut_events.jsonl"), "{\"t\":12.0,\"event\":\"scene_ex");
    Assert.DoesNotThrow(() => SessionFiles.MarkCrashedSessions(root, "new", "2026-11-01T10:00:00.000Z"));
    Assert.IsTrue(SessionFiles.TryFindResumePoint(root, "P103", "current", out var scene));
    Assert.AreEqual("BasicPickingScene", scene);
}

[Test] public void WriteFailure_ReturnsFalseAndStaysFailed()
{
    var blocker = Path.Combine(root, "blocked");
    File.WriteAllText(blocker, "a file where a folder must be");
    var files = new SessionFiles(blocker, "P101", "s1");
    Assert.IsFalse(files.AppendEvent("{}"));
    Assert.IsFalse(files.AppendSummary("x"));
}
```

`WriteEvents(root, id, session, params string[] events)` is a test helper: it writes valid lines via `SessionFiles.EventLine`; `"scene_enter:X"` sets `scene` = X; the header line gets a `utc` that increases with each call.

- [ ] **Step 2: Run, expect compile failure.**
- [ ] **Step 3: Implement** `SummaryCsv`, `VisitSummary`, `SessionFiles` (append with `File.AppendAllText` + UTF-8 without BOM per line, or a `StreamWriter` with `AutoFlush = true`; catch `IOException`/`UnauthorizedAccessException` only, set a failed flag). For parsing, use a minimal field lookup (`"event":"…"`, `"scene":"…"`, header `"utc":"…"`); no JSON package.
- [ ] **Step 4: Run, expect all PASS.**
- [ ] **Step 5: Commit** `feat: summary rows, session files, crash scan and resume point (#26)`.

### Task 5: ResearchLogger runtime

**Files:**
- Create: `Assets/Scripts/Research/ResearchLogger.cs` (MonoBehaviour + static API)
- Create: `Assets/Scripts/Research/ResearchSettings.cs` (ScriptableObject, `[CreateAssetMenu]`)
- Create: `Assets/Resources/ResearchSettings.asset` (via UnityMCP `manage_scriptable_object` or `execute_code`; the logger falls back to defaults when it's missing)
- Create: `Assets/Scripts/Editor/ResearchBuildInfoWriter.cs` (`IPreprocessBuildWithReport`: writes `git rev-parse --short HEAD` into `Assets/Resources/ResearchBuildInfo.txt`; `Assembly-CSharp-Editor`)

**Interfaces:**
- Consumes: everything from Tasks 2–4.
- Produces (static, all no-ops without a session, none throws):
  - `bool ResearchLogger.StartSession(string participantId)`: false + `Debug.LogError` for an invalid ID or an already running session. Creates the `session_id`, runs `MarkCrashedSessions`, writes the header and `session_start`, then `scene_enter` for the active scene with `via: "start"`.
  - `void ResearchLogger.EndSession(string reason)`: reason ∈ `completed`, `facilitator`, `quit`. Writes the open scene's `scene_exit` + summary row, then `session_end`. Further calls do nothing.
  - `void ResearchLogger.Log(string eventName, JsonFields data = null)`: for #27–#30.
  - `bool ResearchLogger.TryFindResumePoint(string participantId, out string scene)`: excludes the running session.
  - `bool ResearchLogger.IsSessionActive`, `string ResearchLogger.ParticipantId`.
  - `ResearchSettings` fields: `bool autoStart = true`, `string autoStartParticipantId = "P000"`, `bool devSkipSessionEndOnQuit = false` (Editor only, ignored in builds).

Wiring decisions:
- Creation: `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` makes one `DontDestroyOnLoad` GameObject `ResearchLogger`. If `autoStart`, it calls `StartSession(autoStartParticipantId)`.
- Scenes: `SceneManager.activeSceneChanged` → `scene_exit` (data `duration_s`, `paused_s`) + summary row for the old scene, then `scene_enter` (data `from_scene`, `via: null`; #29 will set `menu`). `scene_visit` counts per scene name within the session.
- Pause: `OnApplicationPause(bool)` → cause `pause`; `OnApplicationFocus(bool)` → `focus_lost`; `OVRManager.HMDUnmounted/HMDMounted` → `hmd_unmounted`; `OVRManager.InputFocusLost/InputFocusAcquired` → `focus_lost`. All go through one `PauseTracker`; `app_pause` (data `causes`) / `app_resume` (data `paused_s` of this pause) only on transitions. Unsubscribe OVR events in `OnDestroy`.
- Visit times: snapshot `PausedSeconds` at `scene_enter`; at exit `paused_s` = difference, `duration_s` = wall difference − `paused_s`.
- Errors: `Application.logMessageReceived` for `Error`, `Exception`, `Assert` → `ErrorFolder` → `unity_error` (data `type`, `message` stripped, `stack` first 5 lines stripped, `folded_count`). A re-entry guard drops messages raised while the logger itself is writing. Noise filter: a `static readonly string[]` of substrings (start with the OVRPlugin/XR start-up errors seen in `read_console` when Play starts without a headset).
- Quit: `OnApplicationQuit` → `EndSession("quit")` unless `devSkipSessionEndOnQuit` in the Editor.
- Header data: `schema_version` 1, `app_version` = `Application.version`, `build` = `ResearchBuildInfo.txt` text or `"editor"`, `device_model` = `SystemInfo.deviceModel`, `os_version` = `SystemInfo.operatingSystem`, `language` = `null` (no localization until #11, which adds it and `language_change`), `input_mode_at_start` = `hands`/`controllers`/`none` from `OVRInput.GetActiveController()`, `target_refresh_rate` from `OVRManager.display.displayFrequency` (or `null` without a headset). Verify each OVR member with UnityMCP `unity_reflect` before using it.
- Disk failure: when an append returns false, print one `Debug.LogWarning` (warnings aren't captured, so no loop) and stop writing for this session.

- [ ] **Step 1: Write the Play-mode test plan** in `docs/test-reports/bl26.md` per `unity-safe-change`: one row per design §9 sentence that needs Unity, plus regression rows "every scene still runs, no new Console errors". `before` results: no `research/` folder exists.
- [ ] **Step 2: Implement** `ResearchSettings`, `ResearchLogger`, `ResearchBuildInfoWriter`; create `ResearchSettings.asset` with the defaults.
- [ ] **Step 3: Compile check.** `read_console` shows no errors; EditMode tests from Tasks 2–4 still PASS.
- [ ] **Step 4: Play-mode cases via UnityMCP** (`manage_editor` play/stop, `execute_code` to change scenes, set settings, raise errors and read files under `Application.persistentDataPath + "/research"`):
  - Default settings → files under `…/XRISE/xpick/research/practice/`, line 1 has `"event":"header"`, then `session_start`, `scene_enter` for WelcomeScene with `via:"start"`.
  - `autoStartParticipantId = "P101"` → files under `research/`.
  - `SceneManager.LoadScene("ConceptScene")` → `scene_exit` for WelcomeScene, one summary row with `duration_s`/`paused_s` filled and `confirm_count` empty, `scene_enter` ConceptScene with `scene_visit` 1. Load WelcomeScene again → `scene_visit` 2.
  - Call `Debug.LogError("x")` from `execute_code` 100 times in one frame → one `unity_error` line, a later one with `folded_count`.
  - `ResearchLogger.Log("x")` with `autoStart = false` and no session → no file, no exception (Review Focus 5). `StartSession("17")` → false + one error.
  - Simulate focus loss for ~3 s (`execute_code` calling the logger's focus handler, since Editor focus can't be scripted reliably) → `app_pause`/`app_resume`; the visit's `paused_s` ≈ 3 and `duration_s` excludes it.
  - Stop Play → last line `session_end` reason `quit`. Set `devSkipSessionEndOnQuit = true`, `P101`, load MixedPickingScene, stop, set it back, start Play → the old file ends with `crash_recovered`, and `TryFindResumePoint("P101")` from `execute_code` returns MixedPickingScene. After a clean stop, it returns false. For `P555`, it returns false.
  - Regression: open each of the five scenes in Play; Console has no new errors.
- [ ] **Step 5: Copy one real session** (`P101`, several scenes) to `analysis/data/real/` for Task 7.
- [ ] **Step 6: Commit** `feat: ResearchLogger session, scene, pause and error logging (#26)` (scripts, asset, `.meta` files Unity created, test report).

### Task 6: Analysis config and fake data

**Files:**
- Create: `analysis/skill_map.csv` (`error_code,skill,description`): rows for `WRONG_TYPE`, `WRONG_MATERIAL` → `check_material`; `TOO_FEW`, `TOO_MANY`, `EMPTY` → `calculate_quantity`; `MISSING_LOOSE`, `EXTRA_LOOSE` → `calculate_loose`; `NO_ANSWER`, `WRONG_ANSWER` → `package_concept`.
- Create: `analysis/scene_skills.csv` (`scene,skill`): ConceptScene–package_concept; BasicPickingScene–calculate_quantity; MixedPickingScene–calculate_quantity, check_material; ComplexScene–calculate_quantity, check_material, calculate_loose.
- Create: `analysis/scenes.csv` (`scene,build_order`): WelcomeScene 1 … ComplexScene 5.
- Create: `analysis/make_fake_data.R`

**Interfaces:**
- Produces: `analysis/data/fake/research/` (+ `practice/`) with `events.jsonl` and `summary.csv` files in exactly the logger's format, including design §6 events (`confirm_pressed`, `quiz_answer`, `task_completed`) and `session_end` reasons.

Required fake participants: `P101`–`P110` real. Edge cases: `P103` crashes in MixedPickingScene (no `session_end`, one open `app_pause` at the end) and then has a second session that resumes there; `P105` crashes with no later session; `P106` has an `app_pause` whose `causes` list has two entries; `P107` has a ComplexScene visit with 0 confirms; `P108` and `P103`'s second session are on build `bbb2222`, everything else on `aaa1111`; `P109` has one `confirm_pressed` with error code `FOO_BAR`; `P110` has one `confirm_pressed` with `correct: true` and a non-empty `error_codes`; `P950` sits in `research/` (not `practice/`). Use `set.seed(26)`.

- [ ] **Step 1: Write the three CSVs and `make_fake_data.R`.**
- [ ] **Step 2: Run** `Rscript analysis/make_fake_data.R` (needs R; if `Rscript` isn't found, mark this and Task 7's runs `chưa chạy` and move them to the user's checklist). Expected: 12 events files (P103 has two) and their summary files, no errors.
- [ ] **Step 3: Format check against the real file:** the first line of a fake and the real `analysis/data/real/` events file have the same header keys; a fake `scene_exit` line and a real one have the same keys in the same order.
- [ ] **Step 4: Commit** `feat: analysis config and fake research data generator (#26)`.

### Task 7: Analysis script, self-test, README

**Files:**
- Create: `analysis/analyse.R`, `analysis/selftest.R`, `analysis/README.md`

**Interfaces:**
- Consumes: Task 6 files; any folder of logger files.
- Produces in `analysis/output/`: `error_rate_by_skill.png`, `error_rate_by_skill_first_attempt.png`, `error_codes_by_scene.png`, `time_to_correct_by_scene.png` (box plot with the count of visits that never reached correct per scene), `data_quality.csv` + `data_quality.png`, `wide.csv`, `warnings.txt`.

`analyse.R` runs top to bottom. It uses `data_dir` if it already exists, else `"analysis/data"`. It implements design §8 exactly: rebuild visits from events; drop test IDs wherever found; merge sessions per participant by `utc`; `end_status` and `course_finished` derived; rates per visit (NA when 0 confirms) → mean per participant → per scene; first visit with ≥ 1 confirm counts; flags `repeat_visit`, `repeat_after_crash`, `multi_session`, `mixed_build`, `duration_censored`, `gap_h`; compare rebuilt visits with `summary.csv`; warnings for unknown codes and invariant breaks; validity limits printed at the end and written into `warnings.txt`.

- [ ] **Step 1: Write `selftest.R`** that sets `data_dir <- "analysis/data/fake"`, sources `make_fake_data.R` then `analyse.R`, and asserts with `stopifnot`:
  - `nrow(wide) == 10` and `!"P950" %in% wide$participant_id`
  - `wide$multi_session[wide$participant_id == "P103"]` is TRUE
  - `wide$end_status[wide$participant_id == "P105"] == "crash"` and `course_finished` FALSE
  - the ComplexScene error rate of P107's visit is `NA`
  - `wide$mixed_build` is TRUE for P103 only, and `data_quality` shows both builds
  - `warnings.txt` contains `FOO_BAR` and a line naming P110
  - all seven output files exist
- [ ] **Step 2: Run** `Rscript analysis/selftest.R`. Expected: fails (no `analyse.R`).
- [ ] **Step 3: Write `analyse.R`** (English step comments for a beginner; `tidyverse` + `jsonlite` only; scene order from `scenes.csv`; colour-blind-safe palette; `n` shown per group).
- [ ] **Step 4: Run** `Rscript analysis/selftest.R`. Expected: no error, prints `selftest OK`.
- [ ] **Step 5: Real file:** set `data_dir <- "analysis/data/real"` and run `analyse.R`. Expected: no error; `data_quality.csv` shows P101 with `end_status` `quit`; outcome charts are empty (no #28 events yet) without failing.
- [ ] **Step 6: Write `README.md`:** install R (CRAN) + RStudio, `install.packages(c("tidyverse", "jsonlite"))`, where to put copied logs (`analysis/data/<wave>/`), how to run `analyse.R` and `selftest.R`, what each output answers (question 1, 2, 6), and the validity limits.
- [ ] **Step 7: Commit** `feat: R analysis with self-test for research logs (#26)`.

### Task 8: Docs, review, close-out

**Files:**
- Modify: `docs/research-logging-spec.md`, `docs/backlog.md`, `docs/test-reports/bl26.md`, `CLAUDE.md` (local only, not committed)

- [ ] **Step 1: Spec cleanup** exactly as design §4 *Spec cleanup*, plus the §6 contract, the §7 rules and the summary columns. Verify: `grep -n "condition\|lh_px\|knowledge test\|rule_package_counter\|crash-recovered" docs/research-logging-spec.md` finds nothing.
- [ ] **Step 2: data-analyst review** (review mode) on `git diff main...HEAD` and on the real file in `analysis/data/real/`. Fix every **chặn** finding, re-run the affected task's tests, and record the review in the test report.
- [ ] **Step 3: `unity-reviewer`** on the C# diff (asmdef, OVR wiring, quit/pause callbacks).
- [ ] **Step 4: CLAUDE.md:** add `Assets/Scripts/Research/` (own asmdef `XRPick.Research`, tests in `Assets/Tests/EditMode/Research/`, how to run them), `analysis/`, the app identity, and the gotcha "Research code can't reference `Assembly-CSharp` types; scene scripts call `ResearchLogger` one way only". Set the `claude-md-synced` marker after the final commit.
- [ ] **Step 5: (code) per-frame check.** `grep -n "void Update\|void LateUpdate\|void FixedUpdate" Assets/Scripts/Research/` finds nothing; every write path starts from an event callback.
- [ ] **Step 6: Test report and status.** Complete `docs/test-reports/bl26.md` (MCP/code rows, manual rows for the user: R-dependent rows if R wasn't installed, and the Quest checks: files land under `/sdcard/Android/data/com.xrise.xpick/files/research/`, and the system-menu pause shows up as `app_pause`). Set #26 to `verified-mcp (x/y ✅, chờ test tay …)`.
- [ ] **Step 7: Commit** `docs: spec cleanup, test report and status for #26`, then merge `feat/26-research-logger` into `main` after the user agrees.
