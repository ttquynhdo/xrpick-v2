# #19 ComplexScene: hai position, ví dụ phép chia có dư — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Every script/scene edit also follows the project skill `unity-safe-change` (test plan, before/after screenshots in `TestEvidence/ComplexScene/bl19/`, report `docs/test-reports/bl19.md`).

**Goal:** ComplexScene becomes one order with two positions (RING D 55/25, then SCHLAUCHSCHELLE B 156/50), with a worked division example and a division question before position 1, and loose pieces taken from an opened carton.

**Architecture:** Reuse the Mixed pattern (decision 012): `PiecesContainerCounter` gets an `OrderSequence`, confirmed packages stop counting, `LineAdvanced` drives `PositionStockSwitcher` and new per-position UI. Pure logic (package form, division question options) lives in `XRPick.Orders` with EditMode tests; scene work is done through UnityMCP `execute_code` in the open Editor.

**Tech Stack:** Unity 6000.4.5f1, C#, Meta XR Interaction SDK (poke toggles, Grabbable), TMP, NUnit EditMode, UnityMCP.

**Spec:** `docs/decisions/016-complex-two-positions-remainder.md` + **Done when** of #19 in `docs/backlog.md`. Patterns: `docs/decisions/012-mixed-sequential-positions.md`, `015-concept-buttons-next-to-reading.md`.

## Global Constraints

- Branch `feat/19-complex-two-steps`. Decision 016, backlog and plan are committed with Task 1.
- Order data only in `PiecesContainerCounter.orderLines`: line 1 `ring_d` / RING D / 55 / 25 / 264857 / EKZ 000 (unchanged); line 2 `training_b` / SCHLAUCHSCHELLE B / 156 / 50 / Mat.-Nr 443890 / EKZ 000 (copy `weightKg` style from line 1; same material data as BasicPickingScene, #17).
- Worked example numbers are fixed text: Soll 64, VPE 20 → 3 volle Kartons + 4 lose Teile. Never print the real answers (2 Kartons / 5 Teile, 3 Kartons / 6 Teile) anywhere.
- Opened carton of position 1 holds exactly 12 RING D rings; opened carton of position 2 holds 8 SCHLAUCHSCHELLE B pieces.
- The red button stays F6 = confirm order only; it never rates the division question (decision 008).
- All trainee text German, formal *Sie*, through skill `localize-strings` (key + EN + DE in `docs/i18n/strings-todo.csv`), reviewed by `german-reviewer`. WMS labels never translated.
- No new research logging (#28 does it). No `ResearchLogger.Log` calls added.
- Never delete/recreate `.meta` files; renamed/retyped serialized fields keep their name (or `[FormerlySerializedAs]`).
- Code style: `docs/coding-conventions.md`, reference `PiecesContainerCounter.cs`; comments cite `#19, decision 016`.
- `OrderRule` does not change. Menu keeps 5 lessons; build order, `LessonNames`, `SceneMenuController` untouched.
- Red button (F6), `Instruction Table/Result`, `InstructionShelf`, `Menu` keep their positions; all R2 indicators stay off.

## Review Focus

1. Trainee answers the quiz, then confirms position 1 before picking anything → `Result` shows the first error (EMPTY/TOO_FEW), stays at position 1 (Task 3 test case M-empty).
2. A ring grabbed from the opened carton and dropped back into the storage box, not the order box → Ist unchanged, ring stays grabbable at full size (Task 5, MCP case R-drop).
3. Order box already holds position-1 items when the stock switches → those items neither shrink nor count for position 2 (Task 2 `Contains` skip + Task 7 case P2-old).
4. Trainee keeps pressing toggles after both questions are answered → nothing changes: toggles are disabled, stock stays visible, no second `RevealFirstSet` (Task 4 behaviour, checked by MCP in Task 7).
5. Scene opened, quiz never answered, red button pressed → `Result` says the order is empty, stock stays hidden, no exception (Task 7 case Q-skip).

---

### Task 1: Test plan, baseline evidence, commit decision

**Files:**
- Create: `docs/test-reports/bl19.md` (test plan section only)
- Commit: `docs/decisions/016-complex-two-positions-remainder.md`, `docs/backlog.md`, this plan

- [ ] **Step 1:** Follow `unity-safe-change`: one test row per Done-when sentence of #19 plus the Review Focus rows, regression rows for ComplexScene from `testing.md`, each marked `MCP`, `code` or `Bạn` (Quest: ring grab, reach, Q-cluster reach).
- [ ] **Step 2:** Open ComplexScene in Play, take the before screenshots `overview`, `wms`, `instructions`, `container`, `confirm` into `TestEvidence/ComplexScene/bl19/before_<view>.png` (views from CLAUDE.md; the old `views/` set is outdated). Record that the before state fails the new cases (no example, no quiz, one line).
- [ ] **Step 3: Commit**

```bash
git add docs/decisions/016-complex-two-positions-remainder.md docs/backlog.md docs/superpowers/plans/2026-11-02-bl19-complex-two-positions.md docs/test-reports/bl19.md
git commit -m "docs: decision 016 and test plan for #19 (Complex two positions)"
```

### Task 2: Pure logic — package form and division question options

**Files:**
- Create: `Assets/Scripts/Orders/PackageForm.cs`, `Assets/Scripts/Orders/RemainderQuiz.cs`
- Modify: `Assets/Scripts/Orders/ContainerContents.cs`
- Test: `Assets/Tests/EditMode/Orders/RemainderQuizTests.cs`, `Assets/Tests/EditMode/Orders/ContainerContentsFormTests.cs`

**Interfaces:**
- Produces: `public enum PackageForm { FullCarton, LoosePiece, OpenedCarton }`; `ContainerContents.Add(string materialKey, PackageForm form)` (FullCarton → `AddFull`, LoosePiece → `AddLoose`, OpenedCarton → nothing); `public static class RemainderQuiz { int FullCartons(OrderLine line); int LoosePieces(OrderLine line); IReadOnlyList<int> CartonOptions(OrderLine line); IReadOnlyList<int> LooseOptions(OrderLine line); }`.

- [ ] **Step 1: Write the failing tests**

```csharp
// ContainerContentsFormTests
[Test] public void OpenedCarton_IsNeverAFullCarton()
{ var c = new ContainerContents(); c.Add("ring_d", PackageForm.OpenedCarton);
  Assert.AreEqual(0, c.FullOf("ring_d")); Assert.AreEqual(0, c.LooseOf("ring_d")); Assert.IsTrue(c.IsEmpty); }
[Test] public void TwoCartonsFiveLoose_IsCorrect()   // 55 / 25 via OrderRule.Check(lines, contents, true)
  → result.Correct == true
[Test] public void ThreeCartons_IsTooMany()          → first error Code == OrderErrorCode.TooMany
[Test] public void OpenedCartonInBox_DoesNotMakeTooMany() // 2 full + 5 loose + 1 OpenedCarton → Correct

// RemainderQuizTests (line = new OrderLine("ring_d","RING D",55,25); line2 = ("training_b","SCHLAUCHSCHELLE B",156,50))
Assert.AreEqual(2, RemainderQuiz.FullCartons(line));  Assert.AreEqual(5, RemainderQuiz.LoosePieces(line));
Assert.AreEqual(3, RemainderQuiz.FullCartons(line2)); Assert.AreEqual(6, RemainderQuiz.LoosePieces(line2));
CollectionAssert.AreEqual(new[] { 1, 2, 3 },   RemainderQuiz.CartonOptions(line));
CollectionAssert.AreEqual(new[] { 5, 20, 55 }, RemainderQuiz.LooseOptions(line));
// every line in {55/25, 156/50, 50/20, 25/25, 7/25}: options are 3 distinct, ascending, positive or zero, contain the correct value
```

Error codes: `OrderErrorCode.TooMany`, `.Empty`, `.WrongMaterial`.

- [ ] **Step 2:** Run `run_tests` EditMode, assembly `XRPick.Orders.Tests` → new tests FAIL (types missing).
- [ ] **Step 3:** Implement. `CartonOptions`: values `q − 1, q, q + 1` with `q = soll / vpe`; when `q == 0` use `0, 1, 2`. `LooseOptions`: first three distinct values of `[r, vpe − r, soll, r + vpe, r + 1]` (`r = soll % vpe`), sorted ascending. Distractors stand for: rounding up, the rest left in the carton, all as loose pieces.
- [ ] **Step 4:** Run `XRPick.Orders.Tests` → all PASS (old `OrderRuleTests`, `OrderSequenceTests` too).
- [ ] **Step 5: Commit** `feat: pure package form and division question options (#19)`

### Task 3: Counter and validator work position by position

**Files:**
- Create: `Assets/Scripts/IPositionedOrder.cs`
- Modify: `Assets/Scripts/OrderContents.cs` (`From`), `Assets/Scripts/PiecesContainerCounter.cs`, `Assets/Scripts/PiecesOrderValidator.cs`, `Assets/Scripts/MixPickingContainer.cs` (implements the interface only), `Assets/Scripts/PositionStockSwitcher.cs`

**Interfaces:**
- Consumes: `OrderSequence` (`Position`, `Count`, `IsLast`, `CurrentLine`, `CheckCurrent`, `TryAdvance`), `PackageForm`, `ContainerContents.Add`.
- Produces:
  - `public interface IPositionedOrder { IReadOnlyList<OrderLine> OrderLines { get; } int CurrentPosition { get; } bool Contains(PackageData package); event Action<int> LineAdvanced; }` — implemented by `MixedPickingContainer` and `PiecesContainerCounter`.
  - `PiecesContainerCounter`: `CurrentPosition`, `PositionCount`, `IsLastPosition`, `CurrentLine`, `bool AdvanceToNextLine()`, `event Action<int> LineAdvanced`, `bool Contains(PackageData)`, existing `ContentsChanged`, `CheckOrder()` rates the current position only (`Sequence.CheckCurrent(..., true)`), `IWmsOrderSource.CurrentLine` = current line, Ist/Rest of the current position.
  - `OrderContents.Form(PackageType type)`: `LooseItemDisplay → LoosePiece`, `VPEOpenedDisplay → OpenedCarton`, else `FullCarton`; `From` uses it.
  - `PositionStockSwitcher`: field `container` retyped `MonoBehaviour` (same field name, existing Mixed reference keeps working), used as `IPositionedOrder`; new `[SerializeField] bool hideFirstUntilRevealed` (default false) and `public void RevealFirstSet()` (grow-in of set 0 with the same scale/kinematic/`ResetGrabScale` handling as a switch; no-op if already visible).

- [ ] **Step 1:** In ComplexScene note the current behaviour of 2 cartons + 5 rings → correct (baseline for regression).
- [ ] **Step 2:** Implement `PiecesContainerCounter` like `MixedPickingContainer`: `confirmedPackages`, `CurrentPackages()` (drops destroyed/disabled), Ist = Σ over current packages (FullCarton → `Vpe`, LoosePiece → 1, OpenedCarton → 0), `RestPieces` recomputed per position after `LineAdvanced` listeners ran, `CartonCount`/`TotalPieces` keep their meaning for the current position. Keep `piecesPlacedText`/`packagesPlacedText`/`orderLineTexts` fields (cleanup is #46).
- [ ] **Step 3:** `PiecesOrderValidator`: copy the Mixed flow. Correct and not last → `AdvanceToNextLine()`, then result card text `PositionDoneFormat` (green); correct and last → existing `CorrectText`, congratulations, `IsCompleted`, `OrderCompleted` once. Wrong → as today; empty later position → `OrderFeedback.ExplainEmptyPosition`. Draft German, key `complex.feedback.position_done`, `{0}` finished, `{1}` next:
  „Richtig! Position {0} ist erledigt.\nDie Restmenge im angebrochenen Karton bleibt im Modul im Lagerbehälter.\nLesen Sie im LVS die Angaben von Position {1} ab.“
- [ ] **Step 4:** Mixed regression in Play via MCP: position 1 → 2 → done still works, storage switch still grows/shrinks, console clean.
- [ ] **Step 5:** ComplexScene still has one order line at this point: 2 cartons + 5 rings → correct, conveyor runs (`OrderCompleted` once); 3 cartons → TOO_MANY card, Review Focus 1 (confirm with empty box → error, stays). Console clean.
- [ ] **Step 6: Commit** `feat: Complex counter and validator work per position (#19)`

### Task 4: Division question and per-position texts

**Files:**
- Create: `Assets/Scripts/Complex/RemainderQuizController.cs`, `Assets/Scripts/PositionTexts.cs`

**Interfaces:**
- Consumes: `RemainderQuiz`, `PiecesContainerCounter` (as `IPositionedOrder` + `CurrentLine`), `PositionStockSwitcher.RevealFirstSet()`.
- Produces:
  - `RemainderQuizController : MonoBehaviour` — serialized: `PiecesContainerCounter container`, `PositionStockSwitcher stock`, 3 `Toggle` + 3 `TMP_Text` labels (same shape as `PackageQuizController`), `TMP_Text questionText`, `TMP_Text progressText`, `TMP_Text feedbackText`, `GameObject[] hideWhenPositionDone` (quiz cluster root, worked-example board). Public read-only `bool IsAnswered`, `string LastAnswerKey` (answer value as invariant integer string, e.g. "2"; for #28). Event `Action<string, string, bool> AnswerChosen` (questionId `cartons`/`loose`, key, correct) — no logging.
  - `PositionTexts : MonoBehaviour` — serialized `MonoBehaviour container` (as `IPositionedOrder`), `TMP_Text target`, `string[] textPerPosition` (index 0 = position 1); writes `textPerPosition[position − 1]` in `Start` and on `LineAdvanced`; logs an error when the count ≠ `OrderLines.Count`.

Behaviour of `RemainderQuizController`:
- Question 1 `cartons` from `RemainderQuiz.CartonOptions(container.CurrentLine)`, then question 2 `loose` from `LooseOptions`. Each toggle label shows letter + answer (decision 015 style, selected = bold + „» “).
- A toggle switched **on** rates at once. Wrong → feedback = wrong text of that question, selection stays, trainee picks again. Right on question 1 → show question 2. Right on question 2 → `IsAnswered = true`, feedback = done text, toggles disabled, `stock.RevealFirstSet()`.
- On `LineAdvanced` → every object in `hideWhenPositionDone` set inactive.
- Never touches the red button.

Draft German (keys `complex.quiz.*`, via `localize-strings`):
  - `q_cartons`: „Wie viele volle Kartons nehmen Sie für diese Position?“
  - `q_loose`: „Wie viele lose Teile nehmen Sie zusätzlich?“
  - `option_cartons`: „{0} Kartons“ (1 → „1 Karton“); `option_loose`: „{0} Teile“ (1 → „1 Teil“)
  - `wrong_cartons`: „Nicht richtig. Teilen Sie Soll durch VPE. Nur volle Kartons zählen.“
  - `wrong_loose`: „Nicht richtig. Lose Teile = Soll minus (volle Kartons × VPE).“
  - `next`: „Richtig! Beantworten Sie jetzt die zweite Frage.“
  - `done`: „Richtig! Nehmen Sie jetzt die vollen Kartons und die losen Teile aus dem Lagerbehälter.“
  - progress: reuse `concept.quiz.progress` „Frage {0} von {1}“.

- [ ] **Step 1:** Implement both components; `RequiredReference.IsMissing` for every required field.
- [ ] **Step 2:** Compile, console clean (`read_console`).
- [ ] **Step 3: Commit** `feat: division question and per-position texts for Complex (#19)`

### Task 5: Scene — two stock sets, opened cartons with loose pieces

**Files:** Modify `Assets/Scenes/ComplexScene.unity` (via MCP `execute_code`, Undo + `SetDirty` + save; inspect with `.claude/tools/scene_inventory.py` before and after).

- [ ] **Step 1:** `PiecesContainerCounter.orderLines`: add line 2 (Global Constraints).
- [ ] **Step 2:** Stock set 1 = existing `PlasticBox_Storage` with its RING D cartons, the KABELVERBINDER A cartons and `VPE_OpenedPackage`. Move `VPE_OpenedPackage` into the storage box, give it wall/bottom colliders (no `Grabbable`, no `Rigidbody` or kinematic) so the rings rest inside, keep 12 of the 13 rings inside it (delete one), each ring grabbable on its own (own `Rigidbody` + `Grabbable`, `PackageData` `LooseItemDisplay`, `ring_d`). The opened carton keeps `PackageType.VPEOpenedDisplay`.
- [ ] **Step 3:** Stock set 2 = `PlasticBox_Storage_2`, a copy of the storage box at the same place, inactive at start: 4 SCHLAUCHSCHELLE B cartons (copy a RING D carton, change `PackageData` to `training_b`/443890/VPE 50, label 3 lines `SCHLAUCHSCHELLE B` / `Mat.-Nr: 443890` / `VPE: 50 ST`), 2 KABELVERBINDER A cartons as distractors, one opened SCHLAUCHSCHELLE B carton (`VPEOpenedDisplay`, `training_b`) with 8 loose pieces (copies of the ring mesh in a metallic grey material — a hose clamp is a ring band; `LooseItemDisplay`, `training_b`).
- [ ] **Step 4:** Add `PositionStockSwitcher` to `PiecesOrderManager` with `container` = counter, sets = [set 1, set 2] listing every package (rings included), `hideFirstUntilRevealed = true`.
- [ ] **Step 5:** Play via MCP: set 1 hidden at start; call `RevealFirstSet()` → set 1 visible at full scale. Place 2 RING D cartons + 5 rings into the order box by code, confirm → advances, set 2 grows in, Ist 0, Rest = 4 × 50 + 8 − 156 = 52. Review Focus 2: move a ring from the order box back into the storage box → Ist −1, ring scale unchanged. Console clean.
- [ ] **Step 6: Commit** `feat: Complex stock per position with opened cartons (#19)`

### Task 6: Scene — worked example, question cluster, cards, German review

**Files:** Modify `Assets/Scenes/ComplexScene.unity`; `docs/i18n/strings-todo.csv`.

- [ ] **Step 1:** Question cluster: open ConceptScene additively, duplicate `Interactables/AnswerOption` (question text, progress, A/B/C borderless toggles with `Plate`), move it into ComplexScene with `EditorSceneManager.MoveGameObjectToScene`, place at (0.04, 1.23, 0.50), remove the copy's Concept references, add a feedback TMP line under the buttons, wire `RemainderQuizController` (on `PiecesOrderManager`). Close ConceptScene without saving it.
- [ ] **Step 2:** Worked-example board `Worked Example` right of the monitor at about (0.70, 1.30, 0.98), styled like Concept's `Sample Description`, text (draft, key `complex.example.*`):
  „Beispiel“ / „Soll 64 ST, VPE 20 ST“ / „1. 64 : 20 = 3, Rest 4“ / „2. 3 volle Kartons = 60 ST“ / „3. Rest 4 = 4 lose Teile“ / „Kontrolle: 60 + 4 = 64“.
  Add it and the cluster root to `hideWhenPositionDone`. Check ≥ 15 cm from Menu edge and red button (decision 015).
- [ ] **Step 3:** Shelf cards: keep the template `Aufgabe` / `1. Angaben im LVS ablesen` / `2. …` / `3. Auftrag bestätigen`. `Aufgabe` says the order has two positions worked one after the other. Card 2 via `PositionTexts`: position 1 „2. Beantworten Sie die Fragen unter dem Bildschirm. Nehmen Sie dann volle Kartons und lose Teile aus dem angebrochenen Karton.“, position 2 „2. Rechnen Sie selbst: volle Kartons und lose Teile. Nehmen Sie sie aus dem Lagerbehälter.“ Card 3 keeps „Drücken Sie … den roten Knopf (F6)“.
- [ ] **Step 4:** Run `localize-strings` for every new/changed string (keys + EN + DE in `docs/i18n/strings-todo.csv`), then `german-reviewer` on the German; apply its fixes in code/scene.
- [ ] **Step 5:** Play screenshots `overview`, `wms`, `instructions`, `confirm` + a `quiz` view (0.04, 1.45, 0.05) → (0.04, 1.23, 0.50): cluster and board do not cover the dialog, shelf or the reach to both boxes.
- [ ] **Step 6: Commit** `feat: Complex worked example, division question and position cards (#19)`

### Task 7: Full verification, report, docs

- [ ] **Step 1:** Run every `MCP`/`code` row of `docs/test-reports/bl19.md` in Play (packages placed by code; read box position in a later call), screenshots `TestEvidence/ComplexScene/bl19/after_*.png`. Includes Review Focus 3–5 and: wrong carton answer → reason, stock hidden; both right → stock shown; position 1 with KABELVERBINDER A → wrong material; position 2 with 3 SCHLAUCHSCHELLE B cartons + 6 pieces → whole order correct, `Congratulations` shows `Code: P<nnn>`, box rides the order lane; R2 indicators off; Menu 5 lessons.
- [ ] **Step 2:** `run_tests` EditMode `XRPick.Orders.Tests` and `XRPick.Research.Tests` → all PASS.
- [ ] **Step 3:** Regression: MixedPickingScene full order once more (switcher retype).
- [ ] **Step 4:** `unity-reviewer` on the branch diff; fix confirmed findings.
- [ ] **Step 5:** Update CLAUDE.md (Complex row of the table, `PositionStockSwitcher` shared by Mixed/Complex, `RemainderQuizController`, `PositionTexts`, Complex views) and set the sync marker; backlog #19 status `verified-mcp (x/y ✅, chờ test tay …)` with the report link; Vietnamese report + "Việc bạn cần làm trong Unity Editor / trên kính" (ring grab one at a time, no shrink, reach of the cluster, readability).
- [ ] **Step 6: Commit** `test: #19 verified in Editor, report and docs`

If the opened-carton grab fails on the Quest, fall back as decision 016 says: rings lie next to the opened carton, open a new backlog item, #19 stays `verified-mcp`.
