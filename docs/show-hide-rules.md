# Show/hide rules per step (WelcomeScene, ConceptScene)

Backlog #156 (root C). One table per scene: every object whose visibility changes while the scene runs, who shows it, who hides it, and when. Update this file in the same change when a script starts or stops switching an object.

## Rules

1. **Decide before you activate.** A script never switches an object on and then decides to hide it in a later `Update` (it can be drawn for one frame). A marker that may refuse to show implements `IStepMarkerGate`; `WmsFieldTour` asks `MayShow` before it activates it.
2. **Set each object once per change.** A step change works out the final set and toggles only what differs. A marker shared by the old and the new step stays on (no `OnDisable`/`OnEnable`, so `MarkerBob` doesn't jump).
3. **One owner per object.** If two scripts must touch the same object, the second one only acts when the owner left it in a known state (see `LaneFlowDemo` below).
4. **Two panels at one spot swap at once.** When one panel takes another's place (`Caption step` ↔ `Caption question`), the leaving one hides at once (`PanelFade.HideNow`), the coming one fades in.
5. **Group hides list every member.** A step that hides a group hides all objects that only make sense with it (the `Welcome` banner belongs to the objectives).
6. **Test in the same frame.** MCP cases read `activeInHierarchy` in the same call as the press (testing.md C10).

## WelcomeScene

| Object | Shown by | Hidden by | When |
|---|---|---|---|
| `Participant Entry/EntryCanvas` | scene (active) | `ParticipantEntryController.Start` / `Begin` | session running or just started |
| Keypad / Confirm / Resume / Warning panels | `ParticipantEntryController.ShowOnly` | `ShowOnly` / `HideEntryPanels` | entry flow |
| Test mode badges | `ParticipantEntryController.SetBadges` | same | full test ID typed |
| `Welcome`, `Info Panel`, `Tasks Button` (`courseRoots`) | `SetCourseVisible(true)` | `Start` (no session); `OpenTasks` (all three = `objectiveRoots`, #156) | step 1 objectives, until „Zu den Aufgaben“ |
| `Menu`, `BigRedButton`, `Interactables Obj`, `InstructionShelf` (`taskRoots`) | `OpenTasks` | `SetCourseVisible` | step 2 tasks |
| Card `Done Check` ×2 | `WelcomePracticeFeedback` | never | first grab / first red-button press |
| Menu `Next Lesson Highlight` | `WelcomePracticeFeedback` | never | both actions done |
| `Step Frame` ×3 (`Image.enabled`) | `WelcomePracticeFeedback.ApplyFrame` | same | first card not done |
| Locked Menu items (alpha) | `MenuCurrentLesson.OnEnable` | — | research lock |
| Facilitator panel + ring | `FacilitatorEndControl` | same | 3 s hold |

Accepted as is: the scene saves every course/task root active and `Start` hides them before the first frame, so their `OnEnable` side effects run once more at `OpenTasks`. Nothing is drawn; changing the saved state would touch the Menu lock and Grabbable registration.

## ConceptScene: tour markers (`WmsFieldTour.steps[].markers`)

All switched only by `WmsFieldTour.ShowMarkersOf`: every marker off in `Start`, then per step only the differences.

| Step | Markers |
|---|---|
| 1 intro | — (text in `Caption step`) |
| 2 material | Marker Material, Caption material, STECKER C Marker (gate) |
| 3 verp_einh | Marker Verp.-Einh., Caption verp_einh, STECKER C Marker (gate) |
| 4 soll | Marker Soll, Caption soll |
| 5 cartons | Marker Soll, Marker Verp.-Einh., Caption cartons |
| 6 ist | Marker Ist, Caption ist |
| 7 rest | Marker Rest, Caption rest |
| 8 empf_kz_000 | Marker Empf Kz, Caption empf_kz_000, EKZ Sample 000 |
| 9 empf_kz_003 | Marker Empf Kz, EKZ Overview, EKZ Overview Arrow, Caption empf_kz_003, EKZ Sample 000, EKZ Sample 003 |
| 10 not_used | Marker Not used, EKZ Overview (`Caption step`) |
| 11 material_flow | EKZ Overview, Lane Signs (`Caption step`) |

`STECKER C Marker` has `MarkerHideOnGrab` (`IStepMarkerGate`): `MayShow` is false once a STECKER C carton was grabbed while the marker was shown, or one is in hand when the step is shown; then the tour never activates it again in this scene visit. Grabs before the marker was first shown don't count (the tour usually opens on the first grab).

## ConceptScene: other objects

| Object | Shown by | Hidden by | When |
|---|---|---|---|
| `PlasticBox_Storage/Box Sign`, `PlasticBox_Order/Box Sign` | scene (active, #156) | never by script; ride with their box | always, like Basic/Mixed/Complex |
| `Tour Nav Bar` | `WmsFieldTour.Open` / `SetBarVisible(true)`; `LaneFlowDemo.ShowViewBlockers` | `WmsFieldTour.Start`; `SetBarVisible(false)`; `LaneFlowDemo.HideViewBlockers` | card 1 done or 60 s; quiz open; lane ride |
| `NextButton` | `WmsFieldTour.ShowNextLabel` | same | hidden at step 11 before the follow-up |
| `BackButton` (alpha 0.5) | `WmsFieldTour.ShowStep` | same | step 1 |
| `Caption step` | `ShowTexts` / `SetBarVisible(true)` (fade) | `ShowTexts`; `SetBarVisible(false)` (at once, #156) | steps without own caption; quiz takes its place |
| `Action Row` | `PlaceActionRow` / `SetBarVisible` | same | every step; quiz |
| `AnswerOption` | `PackageQuizController.ShowQuestions`; `LaneFlowDemo.ShowViewBlockers` | `PackageQuizController.Start` / `ShowTour`; `LaneFlowDemo.HideViewBlockers` | „Zu den Fragen“ / „Zurück“ / after the 4th right answer; ride |
| `Tour Markers/Caption question` | `ShowQuestions` (fade) | `ShowLocked`; `ShowTour` (at once, #156) | with the cluster |
| Quiz Scroll View, progress, Toggle C | `PackageQuizController.Unlock` / `ShowQuestion` | `ShowLocked` / `ShowQuestion` | step 11 reached; 2-answer questions |
| Menu `Next Lesson Highlight` | `PackageQuizController` | never | 4th right answer |
| Card `Done Check` ×3, `Step Frame` | `ConceptCardProgress` | frames move forward only | grab / quiz open / quiz done |
| `Box Hints/Hint Tag` | `ConceptBoxHints.ShowHint` (placed in the same call) | same; `OnDisable` | wrong material / too many |
| `Carton Count Demo/Total 1–3` | `CartonCountDemo.ShowTotal` | same | step 5, count ≤ Soll |
| `PlasticBox_Storage`, `PlasticBox_Order` + riders | `LaneFlowDemo.ResetStation` | `LaneFlowDemo.RideAway` (out of sight) | step 11 ride; any reset |
| `PlasticBox_Storage_Next` | `LaneFlowDemo.Run` (moved to the hood first) | `ResetStation` | step 11 |
| STECKER C Karton 1–6 | `ResetStation` / `FillOrder` | `RideAway` | step 11 |

`LaneFlowDemo` and the bar/quiz owners: the ride hides `Tour Nav Bar` and `AnswerOption` only when it starts without waiting. If a hint kept it waiting, the questions were already offered (`ShowFollowUp`), the trainee may be answering one, so the ride leaves bar and quiz as their owners left them (#156).

## Review of the scripts that switch visibility (#156)

Changed:

| Site | Was | Now |
|---|---|---|
| `WmsFieldTour.ShowStep` | hide all markers, then show the step's | `ShowMarkersOf`: one pass, only differences, asks `IStepMarkerGate` |
| `MarkerHideOnGrab` | activated by the tour, deactivated in the next `Update` (one frame drawn) | `IStepMarkerGate.MayShow`; if activated anyway, canvases/renderers off in `OnEnable` |
| `WelcomeScene` `objectiveRoots` | `Info Panel`, `Tasks Button` | + `Welcome` |
| ConceptScene `Box Sign` ×2 | markers of steps 5, 6, 11 | always active, no marker |
| `WmsFieldTour.SetBarVisible(false)` / `PackageQuizController.ShowTour` | both captions at one spot cross-faded 0.25 s | leaving caption hides at once (`PanelFade.HideNow`) |
| `LaneFlowDemo.Run` | after a hint wait, faded out the open quiz but left `Caption question` | no hiding once the questions were offered |
| `ParticipantCodeLabel.OnEnable` | `SetActive(false)` while its parent was being activated (Unity refuses) | `label.enabled = false` |

Checked, no change (one owner, or decided before activating): `CartonCountDemo`, `ConceptBoxHints`, `ConceptCardProgress`, `PackageQuizController` (other sites), `PanelFade`, `WelcomePracticeFeedback`, `FacilitatorEndControl`, `MenuCurrentLesson`, `InstructionCardLook` / `InstructionStepHighlighter`, `PositionStrip`, `EkzPlacementCheck`, `MixPickingValidator`, `OrderConfirm`, `ResultCardStyle`, `PiecesOrderValidator` (activates with alpha 0 in the same call), `PositionStockSwitcher`, `ResultCardDeparture` (after the validator is `IsCompleted`), `RemainderQuizController`. Known and accepted: `WmsFieldTour.ShowTexts` switches `Caption step` with `SetActive` while a bar fade may run (only while the bar is shown, so no visible cut); `LaneFlowDemo.ResetStation` puts boxes back at once on „Von vorn“ / „Zurück“ (a jump, by design).
