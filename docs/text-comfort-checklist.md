# Text comfort checklist (VR, Quest 3)

Review checklist for every world-space text and poke panel in the five scenes. Written 2026-11-17 for backlog #125 (team comment C19: "text floats sometimes near, sometimes far, makes me dizzy", lesson 2).
- Use it before handing a build to testers.
- Use it whenever a panel is added, moved or resized.

The numbers come from sources of different strength. Each threshold says where it comes from. **[META]** marks official Meta developer docs, **[MS]** official Microsoft Mixed Reality docs (written for HoloLens), **[PR]** peer-reviewed studies and **[GREY]** blogs and theses. A threshold marked *project rule* is our own choice where no source gives a number.

## How to measure

- **Eye:** standing at the origin, eye at (0, 1.6, 0), facing +z (the rig start pose in every scene).
- **Distance:** from the eye to the centre of the text rect, in metres. Dioptres = 1 / distance.
- **Size:**
  - Size is the TMP font size (em) in world metres, `fontSize × lossyScale.y`, given as a visual angle: `2·atan(em / 2d)`.
  - Cap height is about 0.7 em.
  - This works for UI `TextMeshProUGUI` only. The 3D `TextMeshPro` on package labels, `F6` and `Storage Label` uses another size unit, so judge those in the headset.
- **Yaw and pitch:** angle of the text centre from straight ahead. Negative pitch means below eye height.
- **Facing:** angle between the text's normal and the line from the eye. 0° means the text faces the eye.
- **Tool:**
  - The MCP `execute_code` snippet that wrote `TestEvidence/bl125/texts.tsv` lists every TMP in all 5 scenes (active and inactive) with these columns.
  - Rerun it after layout changes.
  - The curved `Menu` renders a flat source canvas onto a cylinder, so its rows are approximate.

## Checks

| # | Check | Threshold | Source |
|---|---|---|---|
| T1 | Text that is only read (not touched) stands far enough away | ≥ 1.0 m; never in 0.5–0.8 m | [META] *Hands UI best practices*: indirect UI 0.8–3 m, "avoid … the middle distance, roughly 0.5m to 0.8m … push it well into raycast range, 1m or more" |
| T2 | Poke panels are in clear touch range | 42–46 cm from the user; not in 0.5–0.8 m | [META] same page: "Position UI between 42cm and 46cm … to encourage touch" |
| T3 | Few near/far jumps between texts read one after another in the same step | *Project rule:* flag a step ≥ 0.5 D (for example 0.7 m → 1.2 m) | [MS] *Comfort*: switching gaze between depths and near/far refocus cause fatigue (no number). No sourced dioptre limit found; Shibata et al. 2011 (J Vision 11(8):11) still to be read |
| T4 | Text is big enough | Hard floor: em ≥ 0.5°. Target: em ≥ 0.8° for sentences the learner must read | [MS] *Typography*: minimum legible 0.35–0.5°, comfortable 0.6–0.8° (HoloLens). [GREY]/[PR]: 1.2–2.3° preferred for longer reading (Solum 2019, Dingler et al. 2018, sources disagree). Kilpeläinen & Häkkinen 2023 [PR]: Quest 2 legibility varies most between people |
| T5 | Primary content is reachable without turning the neck far | yaw ≤ 45° from straight ahead | [MS] *Comfort*: "avoid … more than 45° off-centre" |
| T6 | Content does not make the learner look far down or up | best 0° to −35°; avoid below −60° or above +10°. Frequent reading: 0° to −15° | [MS] *Comfort*. [PR] Penumudi et al. 2020 (Applied Ergonomics 84): −30° and +15° gave the highest neck load. [META]: "about 1 m, slightly below line of sight" |
| T7 | Text is not read at a steep angle | facing ≤ 45° (*project rule*); ≥ 60° needs clearly larger text | [PR] Büttner et al. 2020 (IEEE VRW, abstract only) |
| T8 | Nothing head-locked; text never turns with every small head movement | No head-locked text. A billboard turns on yaw only, or turns once when it appears, or is smoothed | [META] VRC.Quest.Functional.10: head-locked UI "generally uncomfortable". [META] MR design guideline: world-locked preferred, follow content smoothed. [MS] *Billboarding and tag-along* |
| T9 | The same role is in the same place in every scene | Same distance (± 5 cm), yaw and size for the instruction shelf, LVS, Menu, red button, result card, objectives | *Project rule* (consistency, decision 011) |
| T10 | Text does not change size or pop in and out at the reading spot | No `enableAutoSizing` on texts whose content changes. Panels near the reading spot fade in and out (no sudden swap) | [MS] *Comfort*: content moving in depth and pop-ups are fatiguing. Fade length: no sourced number |
| T11 | Readable contrast and weight | Body contrast ≥ 4.5:1, headlines ≥ 3:1; no Light or Thin weights | [META] *Typography* (weights). Contrast numbers to be confirmed on the Meta accessibility page |
| T12 | Poke targets are easy to hit | Hit area ≥ about 22 mm (48 dp), ≥ 2.5–3° visual angle, ≥ 12 mm between targets | [META] *Hands UI best practices* |

What only the headset can judge: T3, T4 for wearers of reading glasses or varifocals, T8 (does a turning tag feel like swimming), T10. Ask testers *where* they were looking when it felt uncomfortable.

## Status on 2026-11-17 (main `f6dc0df`, static scene poses)

**Fails or doubtful:**

| Check | Where | Measured |
|---|---|---|
| T1, T2 | Concept `Tour Nav Bar` (step text + buttons) | 0.68–0.71 m, middle zone |
| T1, T2 | Concept `AnswerOption`, Complex `Remainder Quiz` | 0.62–0.66 m, middle zone |
| T1 | Complex `Worked Example` (read only) | 0.64–0.66 m, middle zone |
| T2 | `Menu` in all scenes | 0.74–0.76 m, middle zone |
| T2 | Welcome keypad, confirm, resume and warning panels | 0.55–0.57 m, middle zone |
| T3 | Concept tour: bar 0.69 m (1.45 D) → caption 1.23 m (0.81 D) and LVS 1.10–1.20 m | 0.6 D jump in each of 11 steps |
| T3 | Complex position 1: quiz 0.62 m (1.61 D) → LVS 1.15 m → shelf 1.53 m (0.65 D) | about 1.0 D |
| T4 | LVS smallest fields 0.49–0.50° | at the floor; kept as the real WMS (decision 008) |
| T4 | Complex `Opened Carton Note` 0.54° | near the floor |
| T4 | Shelf cards 0.64° (Basic/Mixed/Complex), 0.71–0.73° (Welcome/Concept); quiz texts 0.60–0.64°; Concept `EKZ Overview` 0.62°, `Tour Markers` captions 0.68° | below target |
| T5 | Instruction shelf | Basic/Mixed/Complex −58°, Concept −60° |
| T5 | Result card | 47° |
| T5 | Menu | about 44–62° |
| T5 | Concept `EKZ Overview` | 51–63° |
| T6 | Welcome keypad | −57° |
| T6 | Complex `Worked Example` | −54° |
| T6 | Concept tour bar | −47° |
| T6 | Result card and Menu | about −44° |
| T7 | Complex `Opened Carton Note` | 54° |
| T7 | Result card | 34° |
| T7 | `Box Sign` | 42° |
| T8 | Concept `Box Hints/Hint Tag` and `Carton Count Demo/Total 1–3` | full `LookRotation` to the head every `LateUpdate` |
| T8 | `EkzPlacementCheck` loose-carton marks (Basic, Mixed) | full `LookRotation` to the head every `LateUpdate` |
| T9 | Instruction shelf | Welcome 1.36 m / −39°, Concept 1.37 m / −60°, Basic/Mixed/Complex 1.52–1.59 m / −58° |
| T9 | Course objectives | Welcome `Info Panel` 1.11–1.17 m, Complex `Congratulations` 0.95–0.97 m |
| T10 | `enableAutoSizing` | Concept `Tour Nav Bar/Progress/TXT_Progress`, A/B/C answer titles (Concept, Complex), Complex `Worked Example` body |
| T10 | Concept `LaneFlowDemo.hideWhileRiding` | bar and quiz switch off and on at 0.7 m |
| T10 | Concept quiz | takes the bar's place without a fade |
| T11, T12 | — | not measured yet |

**Passes:**
- LVS dialog, red button, Menu and result card have the same pose in every scene that has them.
- No text is closer than 0.5 m.
- Within one picking step in Basic, Mixed and Complex, the shelf, LVS, labels and result card stay within about 0.4 D.

**Note on T2:**
- Meta gives "distance from the user" without saying from where.
- The tour bar is 0.48 m in front of the eye horizontally, but 0.71 m away along the line of sight because it sits about 0.5 m lower.
- Reading comfort depends on the line-of-sight distance. Reach depends on the hand.
- So a poke panel can be both easy to reach and in the middle zone for the eyes. Decide per panel whether reading or touching matters more.

## Sources (opened 2026-11-17)

**Official Meta and Microsoft guidance**
- Meta, *Hands UI best practices* (updated 2026-09-24), https://developers.meta.com/horizon/design/hands-ui-best-practices/
- Meta, *MR design guideline*, https://developers.meta.com/horizon/design/mr-design-guideline/
- Meta, *Typography*, https://developers.meta.com/horizon/design/styles_typography/
- Meta, VRC.Quest.Functional.10, https://developers.meta.com/vr/resources/vrc-quest-functional-10/
- Microsoft, *Comfort*, https://learn.microsoft.com/en-us/windows/mixed-reality/design/comfort
- Microsoft, *Typography*, https://learn.microsoft.com/en-us/windows/mixed-reality/design/typography
- Microsoft, *Billboarding and tag-along*, https://learn.microsoft.com/en-us/windows/mixed-reality/design/billboarding-and-tag-along

**Studies**
- Penumudi, Kuppam, Kim & Hwang (2020), Applied Ergonomics 84, 103010, doi:10.1016/j.apergo.2019.103010
- Kilpeläinen & Häkkinen (2023), Frontiers in Virtual Reality 4, doi:10.3389/frvir.2023.1243387
- Büttner, Grünvogel & Fuhrmann (2020), IEEE VRW, doi:10.1109/VRW50115.2020.00182 (abstract only)
- Sanders & Chen (2025), Proc. HFES 69(1), doi:10.1177/10711813251369860

**Not found:**
- An official Quest 3 lens focal distance. The often quoted 1.3 m is John Carmack's figure for Quest 1/2.
- A sourced dioptre limit between consecutive reading targets.
- An angular text size from Meta.

The often quoted "0.75–3.5 m" Oculus range was not found in any Meta document.
