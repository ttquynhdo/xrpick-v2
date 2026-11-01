# Facilitator End-Session Control (#44) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Every Unity edit also follows the project skill `unity-safe-change`; every new text follows `localize-strings`.

**Goal:** In every scene, while a research session runs, the facilitator can hold a hidden control for ~3 s, confirm `Sitzung wirklich beenden?`, and the session ends with reason `facilitator` and WelcomeScene opens with the keypad (#27).

**Architecture:** A pure `HoldGesture` timer in the tested `XRPick.Research` assembly decides "held long enough". A scene-side `FacilitatorEndControl` (Assembly-CSharp) on one prefab, `FacilitatorEndSession.prefab`, receives pointer down/up from a poke-enabled world-space canvas (same canvas pattern as the #27 keypad, which works with `PointableCanvasModule`), shows the confirm panel, calls `ResearchLogger.EndSession("facilitator")` and loads WelcomeScene through the scene's `SceneMenuController`. The prefab is placed once in each of the 5 scenes.

**Tech Stack:** Unity 6000.4.5f1, C#, TextMeshPro, Meta Interaction SDK poke via world-space canvas, Unity Test Framework EditMode, UnityMCP 10.3.0.

**Spec:** `docs/decisions/004-participant-keypad.md` §9 and backlog #44 *Done when* in `docs/backlog.md`.

## Global Constraints

- Active only while `ResearchLogger.IsSessionActive`; with no session the control does nothing and shows nothing.
- A quick touch does nothing visible. Holding ≥ 3.0 s (`HoldSeconds = 3.0`) opens the confirm panel; releasing or leaving earlier cancels with nothing shown. A thin progress indicator may appear only after 0.5 s of continuous hold.
- Confirm panel: `Sitzung wirklich beenden?` with `Abbrechen` and `Beenden`. `Abbrechen` closes the panel and the session keeps running. `Beenden` calls `ResearchLogger.EndSession("facilitator")` exactly once, then loads WelcomeScene via the scene's `SceneMenuController.OpenWelcome(true)`.
- After `Beenden`, the last line of the events file is `session_end` with reason `facilitator` (ResearchLogger guarantees ordering) and WelcomeScene shows the keypad.
- Note: after german-reviewer the cancel button became `Fortsetzen` (not `Abbrechen`); the strings and scenes use `Fortsetzen`.
- Placement, same in all 5 scenes: outside the trainee's reach around the table and the Menu — on the trainee's left side, about x −0.80, y 1.40, z 0.10, facing the rig origin; nothing within ~0.6 m of the table front, the red button or `Menu` (0.66, 0.86, 0.45). The idle control is a small dark disc with no text.
- The confirm panel's `Beenden` must not sit where the hold disc is (a continued press must not confirm).
- Strings German (formal *Sie*), keys + EN + DE in `docs/i18n/strings-todo.csv`, `german-reviewer` before commit: `end.confirm.question` "End the session?" / "Sitzung wirklich beenden?", `end.confirm.cancel` "Cancel" / "Abbrechen", `end.confirm.end` "End" / "Beenden".
- Conventions `docs/coding-conventions.md`; scene scripts in `Assets/Scripts/ParticipantEntry/`; new prefab `Assets/Prefabs/FacilitatorEndSession.prefab`; required refs via `RequiredReference.IsMissing`; no `.meta` hand edits; no SDK edits; per-frame work only while holding (no allocation in `Update`).

## Review Focus

1. **Release just before 3 s / pointer leaves the disc while held**: cancelled, nothing shown. → tests in Task 1 (`HoldGesture`) and Task 3 (Play).
2. **`Beenden` pressed twice** (bounce): one `EndSession`, one scene load. → test in Task 3.
3. **Control used with no session** (Editor scene opened directly after auto-start ended, or after `EndSession`): nothing happens. → test in Task 3.
4. **Hold continues after the panel opens** (finger stays on the disc): panel stays open, nothing confirmed. → Task 1 test + Task 3.
5. **Scene change while the panel is open** (trainee chose a Menu item): no stale state carried over; next scene starts idle. → Task 3 (prefab per scene, so it's fresh; verify).

---

### Task 1: HoldGesture (pure timer)

**Files:**
- Create: `Assets/Scripts/Research/HoldGesture.cs`
- Test: `Assets/Tests/EditMode/Research/HoldGestureTests.cs`

**Interfaces:**
- Produces: `public sealed class HoldGesture { HoldGesture(double holdSeconds, double showProgressAfterSeconds); void Press(double t); void Release(); bool IsHolding { get; } double Progress(double t) /* 0..1 */; bool ShouldShowProgress(double t); bool Completed(double t) /* true exactly once per press, when t - pressT >= holdSeconds */; }`

- [ ] **Step 1: Failing tests.**

```csharp
[Test] public void QuickTap_NeverCompletes()
{
    var h = new HoldGesture(3.0, 0.5);
    h.Press(10); Assert.IsFalse(h.Completed(10.2)); h.Release();
    Assert.IsFalse(h.Completed(14)); Assert.IsFalse(h.ShouldShowProgress(10.2));
}
[Test] public void ReleaseJustBeforeThree_Cancels()
{
    var h = new HoldGesture(3.0, 0.5);
    h.Press(0); Assert.IsFalse(h.Completed(2.99)); h.Release(); Assert.IsFalse(h.Completed(3.5));
}
[Test] public void HoldThree_CompletesOnce()
{
    var h = new HoldGesture(3.0, 0.5);
    h.Press(0); Assert.IsTrue(h.ShouldShowProgress(0.6)); Assert.IsTrue(h.Completed(3.0));
    Assert.IsFalse(h.Completed(3.5)); // continued hold does not fire again
    Assert.AreEqual(1.0, h.Progress(4.0), 1e-9);
}
[Test] public void NewPressAfterRelease_StartsOver()
{
    var h = new HoldGesture(3.0, 0.5);
    h.Press(0); h.Completed(3.0); h.Release(); h.Press(10);
    Assert.IsFalse(h.Completed(12)); Assert.IsTrue(h.Completed(13));
}
```

- [ ] **Step 2:** run (UnityMCP `run_tests` EditMode `XRPick.Research.Tests`) → compile failure.
- [ ] **Step 3:** implement (plain C#, no UnityEngine).
- [ ] **Step 4:** run → all pass (75 + new).
- [ ] **Step 5:** commit `feat: hold gesture timer for facilitator control (#44)`.

### Task 2: FacilitatorEndControl script and strings

**Files:**
- Create: `Assets/Scripts/ParticipantEntry/FacilitatorEndControl.cs`, `Assets/Scripts/ParticipantEntry/HoldPointerRelay.cs`
- Modify: `docs/i18n/strings-todo.csv`

**Interfaces:**
- Consumes: `HoldGesture`; `ResearchLogger.IsSessionActive`, `ResearchLogger.EndSession(string)`; `SceneMenuController.OpenWelcome(bool)`.
- Produces:
  - `HoldPointerRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler` on the disc; serialized `FacilitatorEndControl target`; forwards to `target.HoldStart()` / `target.HoldEnd()`.
  - `FacilitatorEndControl` (MonoBehaviour): serialized `confirmPanel` (GameObject, inactive at start), `progressIndicator` (UnityEngine.UI.Image, fillAmount; hidden until `ShouldShowProgress`), `sceneMenu` (SceneMenuController); public `HoldStart()`, `HoldEnd()`, `PressCancel()`, `PressEnd()`. `Update` does work only while holding. `PressEnd`: guard flag so it acts once; `EndSession("facilitator")` then `sceneMenu.OpenWelcome(true)`. All methods no-op without a session or when required refs are missing (fail closed, like `ParticipantEntryController`).

- [ ] **Step 1: Strings** (`localize-strings`): add the three keys from Global Constraints with context and `max_chars`; status `proposed`. List them in the report for `german-reviewer` (the controller dispatches it).
- [ ] **Step 2: Implement** both scripts; German consts with `// key` comments.
- [ ] **Step 3: Compile check** — `read_console` clean; EditMode suite green.
- [ ] **Step 4: Commit** `feat: facilitator end-session control script (#44)`.

### Task 3: Prefab, placement in 5 scenes, Play tests

**Files:**
- Create: `Assets/Prefabs/FacilitatorEndSession.prefab`
- Modify: the 5 scenes in `Assets/Scenes/` (one prefab instance each, `sceneMenu` wired to the scene's `SceneMenuController` holder)
- Create: `docs/test-reports/bl44.md`, screenshots `TestEvidence/<Scene>/bl44/`

Build decisions: build the canvas from the #27 `Participant Entry/EntryCanvas` pattern (poke surface + `PointableCanvas`), not a new interaction type. Disc ~6 cm, dark, no text; progress ring as a radial-fill Image around it; confirm panel beside the disc (not over it) with `Abbrechen` nearer the disc and `Beenden` farther away. Place per Global Constraints and verify per scene that nothing else (Environment props, boxes) overlaps; adjust only the instance transform if a scene differs.

- [ ] **Step 1: Test plan** in `docs/test-reports/bl44.md` (Vietnamese, `unity-safe-change`): one row per #44 *Done when* sentence × scene (the quick-tap, hold-cancel, hold-confirm rows at least in WelcomeScene and ComplexScene; presence/placement/no-session rows in all 5), Review Focus rows, regression (all 5 scenes open, #27 keypad flow K5 still works, Console clean). `before`: no control exists.
- [ ] **Step 2: Build the prefab, place the 5 instances, wire refs; save every scene; confirm with `git diff` that each scene only gained the instance.**
- [ ] **Step 3: Play cases via UnityMCP** (`execute_code` drives `HoldPointerRelay`/`FacilitatorEndControl` methods with simulated time where needed; delete `persistentDataPath/research` first; test codes P9xx): quick tap → nothing; hold 2.9 s release → nothing; hold 3 s → panel; `Abbrechen` → panel closed, session still active; hold 3 s → `Beenden` → events file last line `session_end` reason `facilitator`, WelcomeScene loaded with keypad visible; `Beenden` twice → one `session_end`; no session → nothing; panel open then load another scene → next scene idle. Screenshot views: a new `facilitator` view at (0, 1.6, 0) → (−0.80, 1.40, 0.10), max_resolution 1280, plus `overview` per scene to show the disc is out of the work area.
- [ ] **Step 4: Commit** `feat: facilitator end-session control in all scenes (#44)`.

### Task 4: Docs

- [ ] **Step 1:** complete `docs/test-reports/bl44.md` with the manual Quest checklist (hold with a real hand, reach of the disc by turning left, not triggered while doing the lessons). #44 Status `verified-mcp (x/y ✅, chờ test tay …)`, Test report line. CLAUDE.md (local): the control, prefab path, `facilitator` view row.
- [ ] **Step 2:** commit `docs: test report and status for #44`.
