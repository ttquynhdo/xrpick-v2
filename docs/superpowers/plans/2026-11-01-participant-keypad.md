# Participant Keypad (#27) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Every Unity edit also follows the project skill `unity-safe-change`; every new text follows `localize-strings`.

**Goal:** The facilitator types `P` + 3 digits on a poke keypad in WelcomeScene, confirms it, and only then the course appears and the research session starts under that ID (with crash-resume and a warning for IDs that already finished), plus the end screen in ComplexScene shows the next step and the code.

**Architecture:** Pure logic (code buffer, auto-start rule, ended-session query, resume fields in `session_start`) goes into the tested `XRPick.Research` assembly. A scene-side `ParticipantEntryController` (Assembly-CSharp) drives the panels and calls the logger one way. The C#–R contract change (resume fields) is applied to spec, fake data and `analyse.R` in the same branch.

**Tech Stack:** Unity 6000.4.5f1, C#, TextMeshPro, Meta Interaction SDK poke (pattern of the existing `Menu` canvas), Unity Test Framework EditMode, UnityMCP 10.3.0, R 4.6.2 (`C:\Program Files\R\R-4.6.2\bin\Rscript.exe`, not on PATH) with tidyverse + jsonlite.

**Spec:** `docs/decisions/004-participant-keypad.md` (accepted 2026-11-01) and backlog item #27 *Done when* in `docs/backlog.md`. Log format: `docs/research-logging-spec.md`.

## Global Constraints

- Participant code: `P` + exactly 3 digits; the keypad can produce nothing else. Test codes `P000`, `P900`–`P999` (`ResearchIds.IsTestId`).
- No auto-start when the app starts in the first scene of the build order (build index 0, WelcomeScene); any other start scene in the Editor still auto-starts `P000`. No new asset or build switch.
- Course content opens only when `ResearchLogger.StartSession` returned `true` **and** `ResearchLogger.ParticipantId` equals the typed code.
- Hidden until confirmed in WelcomeScene: `Menu`, `BigRedButton`, `Interactables Obj`, `Info Panel`, `Welcome`. When a session is already active on scene load, the keypad never shows.
- Keypad buttons: digits in phone layout (1-2-3 top row), `Löschen`, `OK` (never `Bestätigen`); `OK` not interactable until 3 digits. Confirm panel buttons `Korrigieren` (left) and `Ja` (right); `Ja` must not sit where `OK` was.
- "Testmodus" label with an icon (not colour only) only on keypad and confirm panel, only for test codes.
- Resume: `TryFindResumePoint` before `StartSession`; scene loaded through a new method added to `SceneMenuController` (existing methods unchanged); unknown scene name = no resume point.
- New log fields are additive (schema_version stays 1): `session_start.data.resume_offered_scene` (string or null), `session_start.data.resume_accepted` (bool or null when nothing was offered). Spec, `make_fake_data.R`, `analyse.R`, `selftest.R` change in the same branch.
- Displayed texts are German (formal *Sie*) now (conventions §6: no new English text shown); every string has a key, EN and DE in `docs/i18n/strings-todo.csv`, a `// key` comment next to its const, and goes through `german-reviewer`. New glossary terms `Code`, `Sitzung`, `Testmodus`, `Kurs` and the 5 lesson names with status `proposed`.
- Lesson display names (D1, team answer; German pending `german-reviewer`): WelcomeScene `1. Einführung`, ConceptScene `2. Grundlagen`, BasicPickingScene `3. Einzelpick nach EKZ 3`, MixedPickingScene `4. Mehrere Positionen`, ComplexScene `5. Volle Kartons + Einzelteile`.
- Keypad placement inside poke reach, centred in front: y 1.15–1.30, z 0.35–0.45, tilted up towards the eye (0, 1.6, 0).
- Conventions `docs/coding-conventions.md`: new scene scripts in `Assets/Scripts/ParticipantEntry/`, one public class per file, `[SerializeField] private`, `TMP_Text`, `RequiredReference.IsMissing` for required refs, English comments. Never hand-edit or delete `.meta`; don't rename existing methods/fields; no edits to SDK folders.

## Review Focus

1. **Double press** (`OK` or `Ja` hit twice by a shaky poke): exactly one session starts, no false error text. → test in Task 4 (call `PressYes` twice).
2. **Typing past 3 digits / deleting on empty**: extra digits ignored, delete on empty does nothing, display stays `P _ _ _`-shaped. → tests in Task 1 (`ParticipantCodeBuffer`).
3. **Resume scene not in the build** (renamed or removed scene in an old log): no resume panel, course starts in Welcome, `resume_offered_scene` null. → test in Task 3/4.
4. **A session already running when `Ja` is pressed** (e.g. auto `P000` in an Editor run): error text in German, course stays hidden, no second session. → test in Task 4.
5. **Test code with an earlier finished session** (`P950` reused all day): no "already finished" warning for test codes. → test in Task 1 (query) and Task 4 (flow).

---

### Task 1: Research assembly — auto-start rule, ended-session query, resume fields, code buffer

**Files:**
- Modify: `Assets/Scripts/Research/ResearchLogger.cs` (auto-start in `CreateAtAppStart`, new `StartSession` overload, `HasEndedSession`), `Assets/Scripts/Research/SessionFiles.cs` (new static query), `Assets/Scripts/Research/ResearchEventData.cs` (session_start data)
- Create: `Assets/Scripts/Research/ParticipantCodeBuffer.cs`, `Assets/Scripts/Research/AutoStartRule.cs`
- Test: `Assets/Tests/EditMode/Research/ParticipantCodeBufferTests.cs`, `AutoStartRuleTests.cs`, add cases to `SessionFilesTests.cs`

**Interfaces:**
- Produces:
  - `public sealed class ParticipantCodeBuffer { bool AddDigit(int digit); bool DeleteLast(); void Clear(); bool IsComplete { get; } string Code { get; } /* "P017" or null when incomplete */ string Display { get; } /* "P 0 1 _" */ int Length { get; } }`
  - `public static class AutoStartRule { bool ShouldAutoStart(bool autoStartSetting, int activeSceneBuildIndex); }` → false when setting off or build index 0; true otherwise (also for -1, a scene not in the build).
  - `public static bool SessionFiles.HasEndedSession(string researchRoot, string participantId, string excludeSessionId)` → true when any earlier events file of that ID contains a parseable `session_end` line.
  - `public static bool ResearchLogger.HasEndedSession(string participantId)` (excludes the running session; false for invalid IDs).
  - `public static bool ResearchLogger.StartSession(string participantId, string resumeOfferedScene, bool resumeAccepted)`; the existing `StartSession(string)` stays and writes `resume_offered_scene: null, resume_accepted: null`. With the overload, `session_start.data` = `{"resume_offered_scene": <scene or null>, "resume_accepted": <bool, or null when resumeOfferedScene is null>}`.

- [ ] **Step 1: Write the failing tests.**

```csharp
[Test] public void Buffer_ThreeDigitsMakeCode()
{
    var b = new ParticipantCodeBuffer();
    b.AddDigit(0); b.AddDigit(1); b.AddDigit(7);
    Assert.IsTrue(b.IsComplete); Assert.AreEqual("P017", b.Code); Assert.AreEqual("P 0 1 7", b.Display);
}
[Test] public void Buffer_FourthDigitIgnored()
{
    var b = new ParticipantCodeBuffer();
    b.AddDigit(1); b.AddDigit(2); b.AddDigit(3);
    Assert.IsFalse(b.AddDigit(4)); Assert.AreEqual("P123", b.Code);
}
[Test] public void Buffer_DeleteOnEmptyIsNoOp()
{
    var b = new ParticipantCodeBuffer();
    Assert.IsFalse(b.DeleteLast()); Assert.AreEqual("P _ _ _", b.Display); Assert.IsNull(b.Code);
}
[Test] public void Buffer_RejectsNonDigits() { Assert.IsFalse(new ParticipantCodeBuffer().AddDigit(10)); Assert.IsFalse(new ParticipantCodeBuffer().AddDigit(-1)); }
[Test] public void Buffer_PartialDisplay() { var b = new ParticipantCodeBuffer(); b.AddDigit(5); Assert.AreEqual("P 5 _ _", b.Display); Assert.IsFalse(b.IsComplete); }

[TestCase(true, 0, false)] [TestCase(true, 1, true)] [TestCase(true, -1, true)] [TestCase(false, 3, false)]
public void AutoStart_NotInFirstBuildScene(bool setting, int index, bool expected) =>
    Assert.AreEqual(expected, AutoStartRule.ShouldAutoStart(setting, index));

[Test] public void HasEndedSession_TrueOnlyWithSessionEnd()
{
    WriteEvents(root, "P101", "done", "header", "scene_enter:WelcomeScene", "session_end");
    WriteEvents(root, "P102", "crash", "header", "scene_enter:WelcomeScene");
    Assert.IsTrue(SessionFiles.HasEndedSession(root, "P101", "current"));
    Assert.IsFalse(SessionFiles.HasEndedSession(root, "P102", "current"));
    Assert.IsFalse(SessionFiles.HasEndedSession(root, "P555", "current"));
    Assert.IsFalse(SessionFiles.HasEndedSession(root, "P101", "done")); // excluded session
}
[Test] public void HasEndedSession_TestIdLooksInPractice()
{
    WriteEvents(root, "P950", "done", "header", "session_end"); // helper writes test IDs under practice/
    Assert.IsTrue(SessionFiles.HasEndedSession(root, "P950", "current"));
}
```
(`WriteEvents` is the existing helper in `SessionFilesTests.cs`; extend it if it doesn't route test IDs to `practice/` yet.)

- [ ] **Step 2: Run, expect compile failure.** UnityMCP `refresh_unity` → `read_console` → `run_tests` EditMode `assembly_names ["XRPick.Research.Tests"]` → `get_test_job`. Expected: errors for the missing types/methods.
- [ ] **Step 3: Implement** the two new classes, `SessionFiles.HasEndedSession` (reuse the existing line parsing), the `StartSession` overload and `ResearchLogger.HasEndedSession`; in `CreateAtAppStart` replace the plain `autoStart` check with `AutoStartRule.ShouldAutoStart(settings.autoStart, SceneManager.GetActiveScene().buildIndex)`.
- [ ] **Step 4: Run, expect all PASS** (58 existing + new). Then two Play checks via UnityMCP (delete `persistentDataPath/research` first): Play with WelcomeScene open → no folder/file created after 3 s; Play with ConceptScene open → a `practice/P000_…` file with `session_start_mode` `auto` and `session_start.data` `{"resume_offered_scene":null,"resume_accepted":null}`.
- [ ] **Step 5: Commit** `feat: no auto-start in first build scene, resume fields, code buffer (#27)`.

### Task 2: Log contract for resume fields (spec + R)

**Files:**
- Modify: `docs/research-logging-spec.md` (§4.1 `session_start` data), `analysis/make_fake_data.R`, `analysis/analyse.R`, `analysis/selftest.R`, `analysis/README.md`

**Interfaces:**
- Consumes: the `session_start.data` shape from Task 1.
- Produces: `analyse.R` columns `resume_offered` (TRUE when any session of the participant had `resume_offered_scene` non-null), `resume_declined` (TRUE when an offer was declined) in `data_quality.csv` and `wide.csv`; a participant with a declined resume is listed in `warnings.txt` (the trainee repeated already-seen lessons).

- [ ] **Step 1: Fake data.** Every fake `session_start` carries the two fields (null/null by default). P103's second session: `resume_offered_scene` `MixedPickingScene`, `resume_accepted` true. Add one fake participant (next free real ID, e.g. P111) whose crash is followed by a session with `resume_accepted` false that restarts at WelcomeScene. Keep `set.seed(26)`.
- [ ] **Step 2: Selftest assertions (write first, run, expect failure):** `wide$resume_offered[P103]` TRUE and `resume_declined[P103]` FALSE; `resume_declined[P111]` TRUE; a declined participant appears in `warnings.txt`; all earlier assertions still hold (participant count goes from 10 to 11 — update that assertion).
- [ ] **Step 3: Implement in `analyse.R`**; older logs without the fields → NA, never an error.
- [ ] **Step 4: Run** `"C:\Program Files\R\R-4.6.2\bin\Rscript.exe" analysis/selftest.R` from the repo root → `selftest OK`.
- [ ] **Step 5: Spec** §4.1: document both fields and their null rules. Commit `feat: resume decision in session_start, R reads it (#27)`.

### Task 3: Scene-side scripts and strings

**Files:**
- Modify: `Assets/Scripts/SceneMenuController.cs` (add one method only)
- Create: `Assets/Scripts/ParticipantEntry/ParticipantEntryController.cs`, `Assets/Scripts/ParticipantEntry/LessonNames.cs`, `Assets/Scripts/ParticipantEntry/ParticipantCodeLabel.cs`
- Modify: `docs/i18n/strings-todo.csv`, `docs/i18n/glossary.csv`

**Interfaces:**
- Consumes: Task 1 API (`ParticipantCodeBuffer`, `ResearchLogger.StartSession(id)`, `StartSession(id, scene, accepted)`, `TryFindResumePoint`, `HasEndedSession`, `IsSessionActive`, `ParticipantId`, `ResearchIds.IsTestId`).
- Produces:
  - `public bool SceneMenuController.OpenScene(string sceneName)` → loads only if `Application.CanStreamedLevelBeLoaded(sceneName)`, else returns false and loads nothing.
  - `public static class LessonNames { static bool TryGetDisplayName(string sceneName, out string displayName); }` (the 5 names from Global Constraints; unknown → false).
  - `ParticipantEntryController` (MonoBehaviour) with serialized refs: `keypadPanel`, `confirmPanel`, `resumePanel`, `warningPanel` (GameObjects), `codeDisplay`, `confirmText`, `resumeText`, `warningText`, `errorText` (TMP_Text), `testModeBadges` (GameObject[] — one on keypad, one on confirm panel), `okButton` (UnityEngine.UI.Button or the ISDK equivalent used by the template, exposing `interactable`), `courseRoots` (GameObject[] — the 5 roots), `sceneMenu` (SceneMenuController). Public no-arg or single-int methods for UnityEvents: `PressDigit(int digit)`, `PressDelete()`, `PressOk()`, `PressCorrect()`, `PressYes()`, `PressResumeContinue()`, `PressResumeRestart()`, `PressWarningCorrect()`, `PressWarningContinue()`.
  - `ParticipantCodeLabel` (MonoBehaviour, on a TMP in the Congratulations panel): `OnEnable` → shows `Code: {participant_id}` from `ResearchLogger.ParticipantId`, hides its GameObject when null.

Flow (the part the signatures don't fix):
```
Start(): if IsSessionActive → hide all entry panels, show courseRoots; else show keypad only, hide courseRoots.
PressOk (complete code)   → confirm panel "Ist {code} richtig?"; badges per IsTestId.
PressYes:
  if busy/started → ignore (double press)
  if TryFindResumePoint(code, out s) && LessonNames.TryGetDisplayName(s, out n) && Application.CanStreamedLevelBeLoaded(s) → resume panel
  else if !IsTestId(code) && HasEndedSession(code) → warning panel
  else Begin(code, null, false, loadScene: null)
PressResumeContinue → Begin(code, s, true, loadScene: s);  PressResumeRestart → Begin(code, s, false, loadScene: null)
PressWarningContinue → Begin(code, null, false, null);   PressWarningCorrect / PressCorrect → keypad (code kept)
Begin: ok = (offered == null ? StartSession(code) : StartSession(code, offered, accepted)) && ParticipantId == code
  ok  → hide entry panels; loadScene != null ? sceneMenu.OpenScene(loadScene) : show courseRoots
  !ok → errorText = start error (German), courseRoots stay hidden, no retry loop
```

- [ ] **Step 1: Strings first** (`localize-strings`): add keys with EN + DE + `max_chars` to `strings-todo.csv` — drafts below — and glossary rows (`Code`, `Sitzung`, `Testmodus`, `Kurs`, 5 lesson names, `proposed`). Run `german-reviewer` on the keys (give EN, DE, context; not your opinion); apply its fixes.

| key | EN | DE (draft) |
|---|---|---|
| entry.keypad.title | Enter code | Code eingeben |
| entry.keypad.delete | Delete | Löschen |
| entry.keypad.ok | OK | OK |
| entry.testmode | Test mode – not analysed | Testmodus – wird nicht ausgewertet |
| entry.confirm.question | Is {participant_id} correct? | Ist {participant_id} richtig? |
| entry.confirm.correct | Correct | Korrigieren |
| entry.confirm.yes | Yes | Ja |
| entry.resume.question | Continue the course of {participant_id}? | Kurs von {participant_id} fortsetzen? |
| entry.resume.where | Interrupted in: {scene_name} | Abgebrochen in: {scene_name} |
| entry.resume.continue | Continue | Fortsetzen |
| entry.resume.restart | Start over | Neu beginnen |
| entry.warning.finished | {participant_id} already has a finished session. | Für {participant_id} gibt es bereits eine abgeschlossene Sitzung. |
| entry.warning.continue | Continue anyway | Trotzdem fortfahren |
| entry.error.start | The session could not be started. Please restart the app. | Die Sitzung konnte nicht gestartet werden. Bitte starten Sie die App neu. |
| complex.end.next | Please take off the headset and tell the course leader. | Bitte nehmen Sie die Brille ab und melden Sie sich bei der Kursleitung. |
| complex.end.code | Code: {participant_id} | Code: {participant_id} |
| lesson.welcome … lesson.complex | the 5 names | see Global Constraints |

- [ ] **Step 2: Implement** the three scripts and `OpenScene` (strings as `const` with `// key` comments, German displayed).
- [ ] **Step 3: Compile check.** `refresh_unity` → `read_console` has no errors; EditMode suite still passes.
- [ ] **Step 4: Commit** `feat: participant entry controller, lesson names, OpenScene (#27)`.

### Task 4: WelcomeScene keypad and panels

**Files:**
- Modify: `Assets/Scenes/WelcomeScene.unity` (via the Editor / UnityMCP, not hand-edited YAML)
- Create/modify: `docs/test-reports/bl27.md`, screenshots under `TestEvidence/WelcomeScene/bl27/`

Build decisions:
- Make the keypad by duplicating the existing `Menu` object's canvas setup (its `CurvedUnityCanvas`/`Unity Canvas` + poke wiring already works on Quest) into a new root `Participant Entry`, then replace its content: code display above a 3×4 phone grid (1-2-3 / 4-5-6 / 7-8-9 / Löschen-0-OK), title line, test-mode badge (text + icon). Confirm, resume and warning panels are children at the same place; confirm buttons are offset so `Ja` is not at `OK`'s position (put the two buttons on a row above the old `OK` row, `Korrigieren` left, `Ja` right). If the Menu canvas is curved, a flat canvas with the same components is fine.
- Place the root inside y 1.15–1.30, z 0.35–0.45, centred at x 0, tilted up towards (0, 1.6, 0).
- Wire every button's UnityEvent to the controller method; `courseRoots` = the 5 roots; `sceneMenu` = the existing `SceneMenuManager` component; add a key-click sound by reusing the Menu's `AudioTrigger` if trivial.

- [ ] **Step 1: Test plan** in `docs/test-reports/bl27.md` per `unity-safe-change`: one row per #27 *Done when* sentence (WelcomeScene ones here; ComplexScene in Task 5; R in Task 2), the Review Focus rows 1, 3, 4, 5, regression rows (all 5 scenes open in Play, Console clean, Menu buttons still load scenes once a session runs). Record `before` (keypad absent; Play from WelcomeScene currently auto-starts P000 — after Task 1 it doesn't).
- [ ] **Step 2: Build the canvas and wiring.** Save the scene.
- [ ] **Step 3: Play-mode cases via UnityMCP** (`execute_code` calls the controller's public methods in place of pokes; delete `research` first; screenshots `overview`, plus a new view `entry` at position (0, 1.6, -0.05) → target (0, 1.22, 0.40), max_resolution 1280, saved to `TestEvidence/WelcomeScene/bl27/`): only keypad visible at start; `OK` not interactable with 2 digits; `017` → confirm text `Ist P017 richtig?`; `Korrigieren` keeps the code; `P101` + `Ja` → course roots visible, `research/P101_…` with `manual`, no `P000` file in this run; `P950` → badges visible, file under `practice/`, badges gone after loading ConceptScene; double `PressYes` → one session, no error; session already running (start one via `ResearchLogger.StartSession("P900")` in `execute_code` before `PressYes`) → German error, course hidden; crash case (`devSkipSessionEndOnQuit` on, P101 to MixedPickingScene, stop, restore setting, Play, P101) → resume panel with `4. Mehrere Positionen`, Continue loads MixedPickingScene and the new log's `session_start.data` has `resume_accepted: true`; repeat with Restart → Welcome course, `false`; a resume point whose scene is not in the build (edit the old log's scene name in the copied file to `OldScene`) → no resume panel; finished real code → warning; finished `P950` → no warning; with a session active, load WelcomeScene via Menu → no keypad. Restore `ResearchSettings` defaults afterwards.
- [ ] **Step 4: Copy the P101 keypad session** (events + summary) to `analysis/data/real/` replacing the old one, and run `analyse.R` on it (output_dir `analysis/output/bl27_check`) → no errors, `session_start_mode` `manual`, no `auto` warning.
- [ ] **Step 5: Commit** `feat: participant keypad in WelcomeScene (#27)` (scene + test report).

### Task 5: ComplexScene end screen

**Files:**
- Modify: `Assets/Scenes/ComplexScene.unity` (`[Congratulations]`)

- [ ] **Step 1:** `[Congratulations]/TXT_Description` text → `complex.end.next` (German); add a smaller TMP below with `ParticipantCodeLabel`; keep `TXT_Title (1)` as is (it belongs to #12). Check it fits inside the panel (font auto-size off, no overflow).
- [ ] **Step 2: Play cases via UnityMCP:** with session `P101` active, `SetActive(true)` on `Congratulations` (or a correct order via the validator) → main line German, code line `Code: P101` (screenshot `wms` view + a `congrats` view at (0, 1.45, 0.5) → (0, 1.36, 1.1), saved to `TestEvidence/ComplexScene/bl27/`); with no session → no code line. Regression: correct/incorrect order feedback as before (#38 rows).
- [ ] **Step 3: Commit** `feat: end screen shows next step and participant code (#27)`.

### Task 6: Docs and close-out

- [ ] **Step 1:** Complete `docs/test-reports/bl27.md` (Vietnamese, with "Việc bạn cần làm trong Unity Editor": poke every key on Quest, reach and readability of the keypad, `Ja` not hit by a repeated `OK` poke, sound). Set #27 to `verified-mcp (x/y ✅, chờ test tay …)`.
- [ ] **Step 2:** CLAUDE.md (local): `Assets/Scripts/ParticipantEntry/`, the no-auto-start-in-WelcomeScene rule, the new `entry` view in the screenshot table, `SceneMenuController.OpenScene`. Note in the backlog #28 entry: activate the Congratulations panel before `EndSession("completed")` so `ParticipantCodeLabel` still reads the code.
- [ ] **Step 3: Commit** `docs: test report and status for #27`.
