# XR Pick

VR order-picking training for Meta Quest, built for a research study. The trainee reads an order on a WMS (warehouse management system) screen, grabs cartons, bags or loose pieces from a storage container, puts them into an order container, and confirms the order with a red button (it stands for the foot pedal / `F6 Bestätigen` of the real station). The app rates each confirm, gives feedback, and writes a research log per participant.

**Language.** The end users are native German warehouse staff who don't read English, so the whole UI is German, using formal *Sie*. English exists only as a developer/pilot mode. Developers work in English; some old code comments are in Vietnamese.

## Requirements and first start

- **Unity 6000.4.5f1** with **Android Build Support** (the project targets Android / Meta Quest).
- Packages come from `Packages/manifest.json`: URP 17.4, Input System, OpenXR 1.17, Meta XR SDK Core and Interaction (OVR) 203, Unity Localization 1.5 (+ Addressables), Test Framework.
- Open the repository root in Unity Hub. The first import rebuilds `Library/` and takes a while.
- Without a headset, use the **Meta XR Simulator** for a realistic view. Without the simulator or a headset, XR does not start and the camera stays at floor height.
- Research analysis needs **R 4.6** (see [`analysis/README.md`](analysis/README.md)).

## The course

Five scenes, played in this build order (`ProjectSettings/EditorBuildSettings.asset`). Every scene has the same station layout and a Menu on the right that shows the five lesson names (`1. Einführung` … `5. Volle Kartons und loses Material`).

| # | Scene | What the trainee does |
|---|---|---|
| 1 | `WelcomeScene` | The facilitator enters the participant code on a keypad (and, for pilots and tests, picks Deutsch/English). The trainee then practises the two basic actions: grab the box and press the red button. |
| 2 | `ConceptScene` | A guided tour of the WMS fields (Mat.-Nr, Bez., Verp.-Einh., Soll, Ist, Rest, Empf Kz) with a nav bar (`Zurück` / `Weiter`) and on-screen markers. The trainee moves STECKER C cartons from the storage container into the order container, sees Soll : Verp.-Einh. = number of cartons, the Empf Kz 003 vs 000 samples, and the three-lane container flow. Ends with a 4-question A/B/C quiz. |
| 3 | `BasicPickingScene` | One order line: 6 full cartons, each placed in its own snap place (Empf Kz 003). |
| 4 | `MixedPickingScene` | Two order lines, one after the other. Line 1: 3 cartons, each placed in its own marked spot (Empf Kz 003 = place neatly). Line 2: 3 bags, dropped in anywhere in the free half (Empf Kz 000 = no placement rule). |
| 5 | `ComplexScene` | Two positions with remainders, for example 55 pieces = 2 full cartons + 5 loose pieces taken from an opened carton. A short quiz on full cartons and loose pieces comes before position 1. Ends with a summary panel that shows the participant code. |

In research sessions the Menu opens only the next lesson, and only after the current one is done.

### The station (same in every scene)

- **Three-lane roller conveyor.** The storage container comes in on the left lane and stops at the work spot. The order container stands on the right lane. Both leave over the middle lane, which runs back under the rear bench. After a correct position, the boxes wait 4 s, then the storage container rides out and the next one rides in. The order container rides away when the whole order is done. A larger static conveyor network and two empty stations in the background are scenery only.
- **WMS monitor** (`WMS_Monitor` prefab with the `WMS_Kommissionierung` dialog) on the rear bench: a rebuild of the real "Kommissionierung" dialog with fake data. WMS labels are never translated.
- **Instruction shelf** left of the monitor: a goal card plus three step cards (read the WMS → pick → confirm). The current step is framed in blue, and a finished step gets a check mark.
- **Result card** at the front wall of the order container after each confirm: a bold title (`Richtig!` / `Nicht richtig`) and one sentence with the first error and what to do.
- **Facilitator control** (dark disc, top left): hold it for 3 s to end the session.

## Code layout

All project code is in `Assets/Scripts/`. Everything else under `Assets/` (`Oculus/`, `Samples/`, `SnapInteractionAssets/`, `TextMesh Pro/`, `GVOZDY/`, `SnowQ/`, `Storage_Solutions/`, `TV LED 30¨/`, `XR*/`, `TutorialInfo/`) is SDK samples or imported art. Don't edit it.

| Folder | Assembly | What it holds |
|---|---|---|
| `Assets/Scripts/` (root) | `Assembly-CSharp` | Scene components: counters, validators, WMS view, conveyor rides, instruction cards, Menu, package data |
| `Assets/Scripts/WS Scripts/` | `Assembly-CSharp` | BasicPickingScene's counter (`CalcForWMS`) and validator (`OrderConfirm`) |
| `Assets/Scripts/Concept/` | `Assembly-CSharp` | ConceptScene: field tour, sample WMS order, demos, box hints, card progress |
| `Assets/Scripts/Complex/` | `Assembly-CSharp` | ComplexScene remainder quiz |
| `Assets/Scripts/Welcome/` | `Assembly-CSharp` | WelcomeScene practice feedback |
| `Assets/Scripts/ParticipantEntry/` | `Assembly-CSharp` | Keypad, resume panel, facilitator end control, lesson names |
| `Assets/Scripts/Localization/` | `Assembly-CSharp` | `Loc` (text lookup) and the language buttons |
| `Assets/Scripts/Orders/` | `XRPick.Orders` (pure C#, no Unity references) | Order rules and other logic that is unit-tested: `OrderRule`, `OrderSequence`, `RemainderQuiz`, step trackers |
| `Assets/Scripts/Research/` | `XRPick.Research` | Research logger: session files, events, pauses, summary CSV |
| `Assets/Scripts/Editor/` | Editor only | Tools under the **XRPick** menu (see below) and the build-hash writer |
| `Assets/Tests/EditMode/` | `XRPick.Orders.Tests`, `XRPick.Research.Tests` | EditMode unit tests |

Reusable scene parts are prefabs in `Assets/Prefabs/`: `WMS_Monitor`, `WMS_Kommissionierung`, `InstructionShelf`, `RearBench`, `RollerConveyorThreeLanes`, `WarehouseConveyorNetworkThreeLanes`, `FacilitatorEndSession`, `DoneCheck`, `NoGrabSymbol`. `RollerConveyor` and `WarehouseConveyorNetwork` are older two-lane versions that no scene uses.

## How a picking scene works

Each picking scene pairs two components:

1. **Container counter.** A trigger collider on the order container that keeps the packages inside in a `HashSet` (a package with several colliders counts once). It holds the order as `orderLines` (material, name, Soll pieces, VPE), so **an order is changed in one place only**. It raises `ContentsChanged`, and the WMS dialog (`WmsDialogView`, through `IWmsOrderSource`) shows Soll / Ist / Rest from it.
2. **Validator.** Its confirm method is wired to the red button in the Inspector. It passes the container contents to the pure `OrderRule.Check`, which returns an ordered list of `{Code, Item}` errors. The result card shows the first one (`OrderFeedback`). On a correct confirm the scene moves to the next position, or completes the order (`IOrderCompletion.OrderCompleted`).

| Scene | Counter | Validator | Packages identified by |
|---|---|---|---|
| BasicPickingScene | `CalcForWMS` | `OrderConfirm` | Unity tags `GGPackage` / `VPEPackage` + `BoxInfo` (older scheme) |
| MixedPickingScene | `MixedPickingContainer` (in `MixPickingContainer.cs`) | `MixedPickingValidator` (in `MixPickingValidator.cs`) | `PackageData.MaterialKey` |
| ComplexScene | `PiecesContainerCounter` | `PiecesOrderValidator` | `PackageData.MaterialKey`; `PackageType` tells full carton / loose piece / opened carton apart |
| ConceptScene | none (`ConceptBoxHints` only reads both containers) | `PackageQuizController` (quiz) | `PackageData` |

Every package carries a `PackageData` with `MaterialKey`, `MaterialNumber` and `Vpe`. This is fake data, the same per material in every scene. Prefer `PackageData` for new work. The Empf Kz 003 rule (each carton in its own snap place) is checked by `EkzPlacementCheck` on the order container.

Most references (texts, panels, counters, button events) are wired in the Inspector. The three validators report a missing one in the Console (`RequiredReference`). Other scripts fail only at runtime.

## Texts and languages

- Unity Localization, one string table `UI` (`de`, `en`) in `Assets/Localization/`.
- The source to edit is **`docs/i18n/strings-todo.csv`** (key, context, en, de). Import it with **XRPick → Localization → Import strings CSV into UI table**.
- Code reads texts with `Loc.Get("key", ("name", value))`. Fixed scene texts get a `LocalizeStringEvent` from **XRPick → Localization → Bind texts in open scene**. Never add one to a text that a script writes.
- Style guide, glossary and the native-speaker review workflow are in [`docs/i18n/README.md`](docs/i18n/README.md).

## Research logging and analysis

- `ResearchLogger` (in `XRPick.Research`) starts with the app and writes, per session, a JSON-lines event log and a `summary.csv` to `persistentDataPath/research/`. On the Quest that is `/sdcard/Android/data/com.xrise.xpick/files/research/`.
- Participant IDs decide where files go. Study `P001`–`P799` → `research/`. Pilot `P800`–`P899` → `research/pilot/`. Test `P000`, `P900`–`P999` → `research/practice/`.
- **The log format is a contract between C# and R.** Any change updates [`docs/research-logging-spec.md`](docs/research-logging-spec.md), `analysis/make_fake_data.R` and `analysis/analyse_set.R` together, and `analysis/selftest.R` must still print `selftest OK`. The header field `layout_version` (now `"034"`) changes whenever the station changes in a way that makes data incomparable.
- Analysis in R: [`analysis/README.md`](analysis/README.md). Copying data off the headset: [`docs/research-data-guide.md`](docs/research-data-guide.md).

## Editor tools (menu **XRPick**)

| Menu | What it does | When to run it |
|---|---|---|
| Localization → Import strings CSV / Bind texts in open scene | Fills the string table from the CSV; binds fixed TMP texts | After editing `strings-todo.csv` |
| Packages → Check / Standardize open scene | Carton/bag sizes, label texts and font size from `PackageData` | After changing a package or its label |
| Instruction Shelf → Check / Lay out cards in open scene | Card heights from the longest text (DE and EN), card spacing, board size | After changing any card text, in every affected scene |

## Tests

EditMode tests cover the pure logic (`XRPick.Orders`) and the logger (`XRPick.Research`). Run them in **Window → General → Test Runner**, or headless with the Editor closed:

```bash
Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml
```

Scene behaviour is checked in Play mode (the UnityMCP server can drive the Editor) and on the Quest. Test cases and results per backlog item are in `docs/test-reports/`.

## Where things are documented

| Document | Content |
|---|---|
| [`docs/backlog.md`](docs/backlog.md) | All work items, their status and the recommended order (in Vietnamese) |
| [`docs/decisions/`](docs/decisions/) | Accepted design decisions, numbered (`NNN-slug.md`) |
| [`docs/test-reports/`](docs/test-reports/) | One test report per backlog item (`bl<nn>.md`) |
| [`docs/learning-objectives.md`](docs/learning-objectives.md) | Course objectives and how each is measured |
| [`docs/research-logging-spec.md`](docs/research-logging-spec.md) | Log format (events, fields, summary columns) |
| [`docs/headset-test-session.md`](docs/headset-test-session.md) | Checklist for a test session on the Quest |
| [`docs/coding-conventions.md`](docs/coding-conventions.md) | C# conventions for new code |
| `docs/review-notes-p8xx.md`, `docs/pilot-P802-summary.md` | Notes from pilot headset sessions (dated records) |
| `docs/requirements/` | Confidential business requirements. **Local only, never committed.** Ask the project lead. |

## Things to watch out for

- **Never delete or regenerate `.meta` files.** Scenes reference scripts and assets by the GUID stored there.
- **File name ≠ class name** for `MixPickingContainer.cs` (`MixedPickingContainer`) and `MixPickingValidator.cs` (`MixedPickingValidator`). If you rename one, keep the existing `.meta`.
- **Scenes load by name.** `SceneMenuController` calls `SceneManager.LoadScene("…")`. Renaming a scene means updating that script and the build profile.
- **`XRPick.Research` can't see `Assembly-CSharp`.** Scene scripts call the logger, never the other way round. The research code has no per-frame methods (decision 022: no pose or performance logging).
- **Prefab instances and RectTransforms.** When moving UI under a prefab instance by code, set `anchoredPosition3D` and mark it dirty, or the change reverts on save.
- **Fallback font.** `TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback` is a **Static** font asset with the box-drawing characters `─ ┌ ┐ └ ┘` used in the WMS dialog. Keep it Static with *Clear Dynamic Data On Build* off. A dynamic fallback is rewritten every Editor session.
- **Line endings.** `.gitattributes` keeps Unity YAML assets (`.unity`, `.prefab`, `.asset`, `.meta`, …) at LF, as the Editor writes them. Keep those rules.
- `ScaleItemCounter` and `SampleFormulaDisplay` are not used by any scene.
