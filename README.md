# XR Pick

VR order-picking training for Meta Quest. The trainee grabs packages, places them in a container, checks the quantities against a WMS (warehouse management system) panel, and confirms the order.

## Requirements

- **Unity 6000.4.5f1** (install through Unity Hub with the **Android Build Support** module — the project targets Android / Meta Quest).
- Packages are resolved automatically from `Packages/manifest.json`: URP 17.4, Input System, OpenXR 1.17, Meta XR SDK Core and Interaction (OVR) 203.

Open the repository root as the Unity project in Unity Hub. The first import rebuilds `Library/` and takes a while.

## Project layout

- `Assets/Scripts/` — all project code (plain `Assembly-CSharp`, no assembly definitions).
- `Assets/Scenes/` — the five training scenes.
- Everything else under `Assets/` (`Oculus/`, `Samples/`, `SnapInteractionAssets/`, `TextMesh Pro/`, `GVOZDY/`, `SnowQ/`, `Storage_Solutions/`, `TV LED 30¨/`, `XR*/`, `TutorialInfo/`) is SDK samples or imported art.

## Scenes

Build order (File → Build Profiles):

1. `WelcomeScene`
2. `ConceptScene` — explains GG vs. VPE packages and ends with a short quiz.
3. `BasicPickingScene` — pick a single package type.
4. `MixedPickingScene` — pick two GG package types.
5. `ComplexScene` — pick whole packages plus loose pieces.

Every scene has a menu (`SceneMenuController`) whose toggles load the other scenes **by name**. If you rename a scene, update `SceneMenuController.cs` and the build profile as well.

## How a picking scenario works

Each picking scene pairs two components:

1. **Container counter** — a trigger collider on the target box. It tracks packages entering and leaving and updates the "PACKAGE PLACED" text.
2. **Validator** — a public method wired to the VR "Confirm" button. It compares the counts with the required order and shows green/red feedback.

| Scene | Counter | Validator | Packages identified by |
|---|---|---|---|
| ConceptScene | — | `PackageQuizController` | `PackageInfoOnGrab` updates the info panels on grab |
| BasicPickingScene | `PackageCounter`, `CalcForWMS` | `PackageCounter.CheckOrder()`, `OrderConfirm` | Tags `GGPackage` / `VPEPackage`; `BoxInfo.packageSize` |
| MixedPickingScene | `MixedPickingContainer` | `MixedPickingValidator` | `PackageData.packageType` |
| ComplexScene | `PiecesContainerCounter` | `PiecesOrderValidator` | `PackageData.packageType` (packages + loose pieces) |

BasicPickingScene uses Unity tags. The newer scenes use the `PackageData` component with the `PackageType` enum; prefer that for new work.

Most references (texts, panels, counters, button events) are assigned in the Inspector, not in code.

## Things to watch out for

- **Order quantities live in two places.** Counters have Inspector fields for the required amounts, but the validators also hard-code the numbers in their checks and messages. Change both when you change an order.
- **Don't delete or regenerate `.meta` files.** Scenes reference scripts and assets by the GUID stored there.
- **File name ≠ class name** for `MixPickingContainer.cs` (`MixedPickingContainer`) and `MixPickingValidator.cs` (`MixedPickingValidator`). Rename carefully, keeping the existing `.meta` files.
- **Fallback font.** `TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback` is a **Static** font asset holding the box-drawing characters `─ ┌ ┐ └ ┘` used in the WMS panels, which the main font lacks. If you add other special characters to UI text, regenerate it with *Window → TextMeshPro → Font Asset Creator* (custom characters), use **Save as…** onto the fallback asset, and keep it Static with *Clear Dynamic Data On Build* off. A dynamic fallback gets rewritten every Editor session and shows up as a git change.
- **Line endings.** `.gitattributes` checks Unity YAML assets (`.unity`, `.prefab`, `.asset`, `.meta`, …) out with LF, matching what the Editor writes. Keep those rules, or Windows checkouts will show line-ending-only changes after every save.
- `MenuSelectionController` is fully commented out, and `ScaleItemCounter` is not used by any scene.
- There are no automated tests; `com.unity.test-framework` is installed if you want to add some.
