# analysis/: R analysis of the research logs (backlog #26, log contract v2 since #28)

The xpick app writes one events file and one summary file per session
(`docs/research-logging-spec.md`, design `docs/superpowers/specs/2026-11-01-research-logger-design.md`,
log contract v2: `docs/decisions/021-research-log-catalogue-v2.md`).
The scripts here turn those files into charts and tables for research questions
1 (improvement per skill), 2 (hard scenes, common errors), 3 (where the time
of an order position goes, from the #29 interaction events), 4 and 5 (paper
SSQ and NASA-TLX, once the typed-in paper forms are there, #31) and 6 (data
quality), plus a wide per-participant file for SPSS, joined with the paper forms.
The logs hold **no** pose, head-movement, frame-rate or tracking data (decision
022, `docs/decisions/022-no-in-app-pose-perf.md`): question 4 compares the SSQ
change only with headset removals, pauses, an early end and active time.

| File | What it is |
|---|---|
| `analyse.R` | The script you run. It runs `analyse_set.R` once for the study set and once for the pilot set (see *Participant sets*). |
| `analyse_set.R` | The analysis of one participant set, run top to bottom, `tidyverse` + `jsonlite` only. Not run directly. |
| `selftest.R` | Makes fake data and checks `analyse.R` against every edge case. Run after any change. |
| `make_fake_data.R` | Writes fake logs in exactly the logger's format (base R). The contract between C# and R. |
| `scenes.csv` | `scene, build_order`: scene order in every chart (survives scene renames). |
| `skill_map.csv` | `error_code, skill, description`: which skill each error code tests. |
| `scene_skills.csv` | `scene, skill`: which skills each scene tests (an error-free tested skill shows 0 %, not missing). |
| `paper_forms_template.csv` | Empty paper-form table (header row only) to copy for each wave; see *Paper forms*. |
| `item_skills.csv` | `scene, item, skill, scaffolded`: the skill of each quiz question (`question_id`) and order position (`position_<n>`), and which items are scaffolded (Complex `position_1`). A position with an empty skill tests every skill of its scene. |
| `data/` | Copied logs. **Git-ignored, never commit.** |
| `output/` | Charts and tables of the study set; `output/pilot/` the same for the pilot set. Git-ignored. |

The study lead may edit the four config CSVs (`scenes`, `skill_map`, `scene_skills`,
`item_skills`) without touching code. The header of `paper_forms_template.csv` is a
contract with `analyse_set.R` (the script stops if they differ): change both together.

## Setup (once)

1. Install R from CRAN: <https://cran.r-project.org/> (Windows: "Download R for Windows" → "base").
2. Install RStudio Desktop: <https://posit.co/download/rstudio-desktop/>.
3. In the R console: `install.packages(c("tidyverse", "jsonlite"))`.

## Where to put the logs

Copy the headset folder `/sdcard/Android/data/com.xrise.xpick/files/research/`
(Editor: `%USERPROFILE%/AppData/LocalLow/XRISE/xpick/research/`) into
`analysis/data/<wave>/`, e.g. `analysis/data/wave1/research/`. Keep the file
names as they are (`P101_<session_id>_events.jsonl`, `..._summary.csv`).
Test IDs (`P000`, `P900`–`P999`) are dropped wherever they are, in
`practice/` or by mistake in `research/` (the latter with a warning). A study
or pilot ID found under `practice/` is **kept**, with a warning, so no
participant's data is lost silently: check it in `warnings.txt`.

## Participant sets (decision 023, backlog #75)

| IDs | Set | Folder on the headset | Language | Outputs |
|---|---|---|---|---|
| `P001`–`P799` | study | `research/` | always `DE` (else warned) | `output_dir` (`analysis/output/`) |
| `P800`–`P899` | pilot | `research/pilot/` | the one chosen on the keypad (`EN` or `DE`), no warning | `output_dir/pilot/` (`analysis/output/pilot/`) |
| `P000`, `P900`–`P999` | test | `research/practice/` | any | none (dropped) |

`analyse.R` runs the same analysis on each set and writes **all** outputs (charts,
`items.csv`, `tour_steps.csv`, `concept_hints*.csv`, `concept_containers_by_visit.csv`, `station_waits.csv`, `wide.csv`, `wide_with_paper.csv`, `data_quality.*`, `warnings.txt`)
into the set's folder. The ID range decides the set, the folder only checks it:
a pilot ID outside `research/pilot/` stays in the pilot set and is warned in
`output/pilot/warnings.txt` ("pilot ID outside research/pilot/"); a study ID inside
`research/pilot/` stays in the study set and is warned in `output/warnings.txt`
("study ID in research/pilot/"). Paper-form rows join the set of their
`participant_id` (one `paper_forms.csv` for both is fine). The study
`warnings.txt` lists the pilot IDs it left out; the pilot `warnings.txt` ends with
a "PILOT SET" validity line. Without pilot logs, `output/pilot/` is deleted so no
old pilot result looks current. After `source("analysis/analyse.R")` the study
tables are in the workspace (`wide`, `visits`, …), the pilot tables in
`pilot_set` (e.g. `pilot_set$wide`). Pilot results are a try-out of the course
and the analysis, **never** study results: never pool the two sets.

## Paper forms (SSQ, NASA-TLX, Abbruchgrund)

Each participant fills the SSQ **before** putting on the headset and **again at the
end**, and the NASA-TLX once at the end (design, *Study setup*). The facilitator adds
the end time and, if the session was stopped, the Abbruchgrund. Type them in like this:

1. Copy `analysis/paper_forms_template.csv` to `analysis/data/<wave>/paper_forms.csv`
   (next to `research/`, e.g. `analysis/data/wave1/paper_forms.csv`).
   The file name must start with `paper_forms` and end in `.csv`. It is under
   `data/`, so it is git-ignored: **never commit it** (SSQ answers are health data).
2. One row per participant, one column per answer. Leave a cell **empty** when the
   answer is missing (never type 0 for "not answered": 0 means "none").
3. Save as CSV. Excel on a German PC saves semicolons and Windows-1252
   ("CSV (Trennzeichen-getrennt)"); "CSV UTF-8" with commas works too. Both are read.
   An `.xlsx` alone is not read (the script warns). The template has commas: a German
   Excel may show it all in column A; then use *Daten → Aus Text/CSV* (separator comma)
   or type into a new sheet with the same header and save as CSV.

| Column | Values |
|---|---|
| `participant_id` | `P` + 3 digits as printed on the paper set, e.g. `P017`. |
| `session_date` | `YYYY-MM-DD` (or `DD.MM.YYYY`, as German Excel rewrites it). |
| `facilitator` | Short facilitator code, e.g. `F1` (not a name). |
| `end_time_local` | `HH:MM`, local time, as written down at the end (Congratulations screen with `Code: P…`, or the clock when the session was stopped). |
| `ssq_pre_01_general_discomfort` … `ssq_pre_16_burping` | SSQ before, items 1–16 in the standard order (Kennedy et al. 1993), whole numbers 0 (none) – 3 (severe). |
| `ssq_post_01_…` … `ssq_post_16_…` | SSQ at the end, same items. |
| `tlx_mental`, `tlx_physical`, `tlx_temporal`, `tlx_performance`, `tlx_effort`, `tlx_frustration` | NASA-TLX raw, 0–100 in steps of 5 (the 21 marks of the scale). `tlx_performance`: 0 = good, 100 = poor (as on the form). No pairwise weighting. |
| `abbruchgrund` | Empty (not stopped) or one of `mued_schwindlig`, `zeit`, `fehlbedienung`, `andere`. |
| `note` | Optional free text for the study lead (no names, no health details beyond the reason). Never copied into any output. |

The 16 SSQ item columns, in order: `01_general_discomfort`, `02_fatigue`,
`03_headache`, `04_eye_strain`, `05_difficulty_focusing`, `06_salivation_increased`,
`07_sweating`, `08_nausea`, `09_difficulty_concentrating`, `10_fullness_of_head`,
`11_blurred_vision`, `12_dizzy_eyes_open`, `13_dizzy_eyes_closed`, `14_vertigo`,
`15_stomach_awareness`, `16_burping`. The German paper form must list the items in
this order.

Why raw TLX: the pairwise weighting adds 15 comparisons per person at the end of a
VR session and changes the score little (raw and weighted TLX correlate highly);
with a small N the extra form time is not worth it.

**How the join works.** `analyse.R` reads every `paper_forms*.csv` under `data_dir`.
Without one it writes `wide.csv` as before, with no error. With one it also writes
`wide_with_paper.csv`: every row of `wide.csv` plus participants who have a paper form
but no log, joined on `participant_id`. `wide.csv` itself never gets paper columns,
so its columns don't depend on whether paper exists, and the SSQ answers live in
one file you can guard and delete. Checks (all in `warnings.txt`, nothing dropped
silently; the IDs are listed under "Paper forms to check"):

- an ID not in `P` + 3 digits is not joined (row number named); test IDs are dropped;
- an ID typed twice: neither row is joined (`paper_status` = `duplicate`), keep one;
- a value out of range (SSQ not 0–3, TLX not 0–100) becomes empty and is named
  (`paper_problems` counts them); a TLX value between two marks is only warned;
- empty answers: a missing SSQ item makes its subscale(s) and the total NA (no
  guessing), a missing TLX scale makes `tlx_raw` NA; a form not filled at all is named;
- an `abbruchgrund` outside the list becomes empty (upper case and spaces are
  ignored); a facilitator-ended session without one, and an `abbruchgrund` for
  someone whose log shows the course finished, are warned;
- IDs with a paper form but no log, and with a log but no paper form;
- `end_time_diff_min` = paper end time minus the participant's last logged event
  (time zone `paper_time_zone`, default `Europe/Berlin`; set it before sourcing if
  the study runs elsewhere); more than 15 min is warned (swapped form? wrong ID?).

Columns added in `wide_with_paper.csv`: `has_log`, `paper_status` (`joined` /
`no_paper` / `duplicate`), `session_date`, `facilitator`, `end_time_local`,
`end_time_diff_min`, `abbruchgrund`, `paper_note_present`, the 32 SSQ items, per form
`ssq_<pre|post>_nausea`, `_oculomotor`, `_disorientation`, `_total` (weighted:
raw sums × 9.54, 7.58, 13.92; total = sum of the three raw sums × 3.74),
`ssq_change_<scale>` = post − pre, the 6 `tlx_*` scales, `tlx_raw` (mean of the 6),
`paper_problems`.

## Run

From the repo root (in RStudio: *Session → Set Working Directory* to the repo folder):

```bash
Rscript analysis/selftest.R      # expected last line: selftest OK
Rscript analysis/analyse.R       # all logs under analysis/data/ (fake/ is skipped)
```

To analyse one wave only, set both folders first (in RStudio always set both,
so nothing from an earlier run is reused):

```r
data_dir <- "analysis/data/wave1"
output_dir <- "analysis/output"
source("analysis/analyse.R")
```

`analyse.R` prints `set`, `data_dir` and `output_dir` for each set; check them. It refuses
any `data_dir` whose path contains `fake` unless `allow_fake <- TRUE` is set
(only `selftest.R` does that). `selftest.R` runs in its own environment and
writes to `analysis/output/selftest/`, so it leaves no settings behind in
RStudio. Without Rscript on PATH use the full path, e.g.
`"C:\Program Files\R\R-4.6.2\bin\Rscript.exe" analysis/analyse.R`.

## What each output answers

| Output | Question | Reads as |
|---|---|---|
| `first_attempt_by_item.png` | 1 (**main chart**) | Per item (quiz question or order position) in build order: share of participants right at their **first attempt** at that item, with a 95 % Wilson interval and `right/participants`. One panel per skill. Hollow point = scaffolded (Complex position 1). Guess rate 33 % (A/B/C) or 50 % (EKZ questions): a quiz share near that level says little. |
| `items.csv` | 1 | One row per participant, scene and item (the first visit with an attempt at that item). `attempts` = all touches / red-button presses at that item in that visit (rated ones only; ignored presses are never attempts). `first_attempt_correct` = the first of them was right (right only on a later touch counts as wrong). `reached_correct` = any of them was right. `scaffolded` = the app gave the numbers (from `item_skills.csv`). `repeat_visit` = this visit was not the participant's first visit of the scene (e.g. after a crash): a second exposure. `first_attempt_codes` = error codes of the first attempt. `layout_version` = header layout of the item's session (empty = not in the header); in a mixed-layout set compare ConceptScene items only within one layout. `scaffold_level` (Complex `position_1` only, #93) = `quiz_visible` (layout up to `"025"`: remainder quiz and Worked Example visible the whole position) or `result_on_card` (`"029"` on: they hide after both answers are right, shelf card 2 shows the trainee's result); empty for every other item. `empf_kz` (order positions, #94) = the position's Empf Kz: `line.empf_kz` of its first press; logs without the field are from builds before #94: Basic `003`, every other position `000`; empty for quiz items. |
| `error_rate_by_skill.png` | 1 (secondary) | Per skill and scene: share of the attempts **at items of that skill** that had an error of that skill (see *Per-skill rule* below). Dot = one participant (first visit of the scene with ≥ 1 counted attempt), diamond = mean, `n` under each scene. |
| `error_rate_by_skill_first_attempt.png` | 1 (secondary) | Same skills, first attempt at an item of that skill only: `k/n` participants made that error there. |
| `error_codes_by_scene.png` | 2 | Which error codes occur in which scene (counting visits). Unknown codes are marked `(unknown)`. |
| `time_by_phase.png` | 3 | Per scene and order position (completed positions of counting visits): active seconds of the three phases `read` (position start → first `grab_start`), `pick` (first grab → last `container_enter`/`_exit` before `position_completed`) and `check` (last box change → the correct press). Position start = scene enter for position 1 (Complex: storage box 1 arrived after the remainder quiz), the `station_move_arrived` of the new storage box for later positions, so the box-change wait (`wait_s`) is never inside. Point = one participant, bar = median, `n` per position. |
| `process_by_position.csv` | 3 | One row per participant, picking-scene visit and position: `read_s`, `pick_s`, `check_s` (they add up to `position_s`), `presses`, `completed`, `scaffolded`, `scaffold_level` and `empf_kz` (as in `items.csv`), `layout_version`, `counting`. A position never completed keeps `read_s` (if a grab happened) and NA for the rest. Only sessions whose header lists the `interaction` group. |
| `time_to_correct_by_scene.png` | 2 | Active time to the first correct confirm per scene; `never` counts the people who didn't get there (survivor bias). ConceptScene: includes 3 × 1.5 s app hold after the correct answers to questions 1–3 (#78, decision 024); compare Concept times only within builds that have the hold. ComplexScene: the remainder quiz adds 1 × 1.5 s after the right carton answer (#85); compare Complex times only within builds that have it. R splits only ConceptScene by `layout_version`; for Complex a mixed set is only flagged (`data_quality.mixed_layout`), so filter by `layout_version` yourself. |
| `data_quality.csv` / `.png` | 6 | One row per participant (the picture shows a subset of the columns): `layout_versions`, `mixed_layout` (TRUE on every row when the set holds more than one `layout_version`, see *Layout versions*), `end_status` (completed / facilitator / quit / crash), `course_finished` (empty = no outcome events logged), `multi_session`, `mixed_build`, `builds`, `gap_h`, `resume_offered` / `resume_declined` (#27: a resume after a crash was offered / declined in any session; empty = log from before #27), `ended_session_override` (a session was started with "Trotzdem starten" although the ID already had a finished session; empty = log from before the #27 fix wave), `duration_censored`, repeat visits, `paused_s` (seconds), pauses, headset removals, Unity errors, `error_cap_reached`, invariant breaks, unknown codes, mismatches with `summary.csv`, `exit_disagreements` (visits whose `scene_exit` numbers differ from their events), `duplicate_files` (copies of a session's files that were dropped), `interaction_logged` (#29 group in the header of any session), `end_control_opens` (facilitator end control opened; more than 1 = opened by mistake; empty = not measured), `menu_blocked` (touches on a locked Menu item, #42), `open_grabs` (a package still held at a crash / scene change: no hold time), `facilitator_tail_s` (active seconds from the last `end_control_opened` to the end of a session the facilitator ended: not the trainee's time). Orange = check. |
| `wide.csv` | all | One row per real participant for SPSS. Per-scene columns (`active_s_<Scene>`, `paused_s_<Scene>`, `time_to_correct_s_<Scene>`, `confirms_<Scene>`, `error_rate_<Scene>`, `error_rate_<Scene>_<skill>`, …) come from the counting visit; scenes without a confirm or quiz (WelcomeScene) have no outcome columns. `confirm_ignored_<Scene>` = red-button presses the app ignored (box still riding, order already done, Complex quiz not answered yet): counted, never an attempt, never in a rate. `wait_s_<Scene>` = seconds the trainee was blocked by a box change **before** `task_completed`: one interval per `storage_in` ride, from its trigger (the previous position's `position_completed`; for Complex box 1 the last remainder-quiz touch before the ride) to that ride's `station_move_arrived` (the visit's last event if it never arrived), intervals unioned, cut at `task_completed`, minus pauses. It includes the start delay before the box starts, taken from the log (0.6 s up to layout `"032"`, 4 s from `"033"`, decision 033; never pool the two, see `station_waits.csv`) and leaves out the old box still riding away; Basic has no `storage_in` ride, so 0. `wait_after_done_s_<Scene>` = union of all rides after `task_completed` (the boxes leaving at the end; empty when the task was never completed). These three exist only for the red-button scenes (Basic, Mixed, Complex). #29: `grabs_<Scene>` (= `grab_start` lines; Concept and the picking scenes, WelcomeScene's box has no `PackageData`), `regrabs_<Scene>` (grabs of a package already grabbed in the visit), `time_to_first_grab_s_<Scene>`, `container_enters_<Scene>` / `container_exits_<Scene>` (picking scenes only; an exit = a package taken out of the order container again), and `read_s_` / `pick_s_` / `check_s_<Scene>_p<n>` per position. All NA (not 0) for a session whose header `event_groups` lacks `interaction`. #82: `tour_active_s_ConceptScene`, `tour_back_presses_ConceptScene`, `tour_quick_next_back_ConceptScene`, `tour_restarts_ConceptScene` = totals of `tour_steps.csv` over the first Concept visit with a tour (Back columns empty for layout `"019"` / none). Join the paper forms (SSQ, NASA-TLX, #31) on `participant_id`. |
| `tour_steps.csv` | 2, 3 | ConceptScene field tour (#82): one row per participant, Concept visit (`session_id`, `scene_visit`, `visit_no`, `repeat_visit`) and tour step 1 … n (`step_index`, `step_id`; n = highest `step_index` in the set unless `tour_n_steps` is set before sourcing). `layout_version` of the session's header (empty = not in the header), `zurueck_available` (TRUE from layout `"025"` on). `shows` = how often the step was shown; `active_s` = active seconds on the step over all showings (from its `tour_step` to the next one, the first `quiz_answer` of the visit or the visit end, pauses removed; empty when never shown before the quiz); `shows_after_quiz` = showings after the first quiz answer (no time); `time_censored` = the step was on screen when a crashed visit stopped. Presses are booked on the step the trainee **left**, from the change of `step_index` between consecutive `tour_step` lines of the visit in file line order (not `t`): `back_presses` (−1 = „Zurück“), `quick_next_back` („Weiter“ to this step, then „Zurück“ within < 2 active seconds: a likely mis-press), `restarts` (last step → 1 = „Von vorn“). `back_presses` and `quick_next_back` are empty (NA, not 0) for layout `"019"` or no layout: those builds had no „Zurück“. A step never reached has `shows` 0 and empty `active_s`. Other jumps are warned. |
| `tour_time_by_step.png` | 3 | Active seconds per tour step, one dot per participant (first Concept visit with a tour), diamond = median, `n` per step; one panel per `layout_version`. |
| `tour_back_by_step.png` | 2 | „Zurück“ presses per tour step (the step left), split into < 2 s after „Weiter“ (likely mis-press) and later (rereading), label = participants with ≥ 1 press / participants; layouts with „Zurück“ only. |
| `concept_hints.csv` | 2, 6 | ConceptScene hint tags (#100, decision 030, layout `"029"` on): one row per tag shown (`concept_hint_shown` paired with the next `concept_hint_cleared` of the visit in file line order). `container` (`order` / `storage`), `reason` (`WRONG_MATERIAL` / `TOO_MANY`), `object_id`, `material_key`, `tour_step_id` (step on screen when it appeared; empty = before the tour bar opened), `cleared_step_id`, `shown_t` / `end_t` (visit time), `shown_s` (active seconds, pauses removed), `ended_by` (`cleared`; `scene_exit` = still shown when the scene was left; `crash` = still shown at the last event of a crashed visit, a lower bound; `next_shown_without_cleared` = contract break), `removed_by_grab` (TRUE = the trainee picked that package up while it was shown; FALSE = the station cleared it, e.g. `TOO_MANY` when the demo cartons left with the step; empty when not cleared). A cleared without a shown before it is warned and ignored. **Not attempts**: hints never enter `confirm_count`, `items.csv` or an error rate. |
| `concept_hints_by_visit.csv` | 2, 6 | One row per Concept visit of a session with the `interaction` group: `hints_logged` (TRUE from layout `"029"` on), `hints_shown`, `hints_order_wrong_material`, `hints_order_too_many`, `hints_storage_wrong_material`, `hints_removed_by_grab`, `hints_open_at_exit`, `hint_shown_s` (total active seconds), `hint_max_shown_s`, `material_flow_demo_due` / `_held` (showings of step `material_flow` that lasted past the lane demo's start, `lane_demo_delay_s` = 4 s, and those where a tag held it), `material_flow_wait_max_s` / `material_flow_wait_s` (active seconds the demo waited for the station to be clear: from the demo start until the chain of tags ends, cut at the next step), `material_flow_wait_censored`, `hint_contract_breaks`. Every hint column is empty (NA, not 0) for layouts before `"029"`; the demo columns are empty when the demo was never due. Counts, never a rate: on `"029"` putting packages into the Concept containers is voluntary and unlogged (no `container_*` in Concept); from `"032"` (decision 032) `WRONG_MATERIAL` cannot happen (only STECKER C cartons) and `TOO_MANY` only comes from a 4th carton the trainee laid in. Never pool `"029"` and `"032"`. |
| `station_waits.csv` | 3, 6 | One row per station ride (`station_move_start`), #99 / decision 033: `what`, `position`, `trigger_t` (the `position_completed` the ride follows; Complex box 1: last remainder-quiz touch), `start_delay_s` (active seconds trigger → `station_move_start`, from the log, never assumed), `ride_s` (start → `station_move_arrived`), `station_wait_group` (`before_033` = 0.6 s builds, `033_on` = 4 s builds), `expected_delay_s` (checked only for `storage_out` and `storage_in` p > 1; NA for the order box and Complex box 1), `delay_unexpected` (more than 0.5 s off; also in `warnings.txt`). `wait_s`, `wait_after_done_s` and `active_s` of the picking scenes include the delay, so the two groups are never pooled (`data_quality.csv` `mixed_station_wait`); `time_to_correct_by_scene.png` uses `time_to_correct_s` minus `wait_s` for the picking scenes. |
| `concept_containers_by_visit.csv` | 2, 3 | ConceptScene carton moves (#92, decision 032, layout `"032"` on), one row per Concept visit of a session with the `interaction` group: `containers_logged` (TRUE from `"032"`), `storage_exits` / `storage_enters`, `order_enters` / `order_exits` (`container_*` lines), `order_max_cartons`, `soll_reached` (the trainee's own cartons made 3 = Soll at least once), `time_to_soll_s` (active seconds from scene enter to that carton's `container_enter`), `over_soll` / `order_over_soll_enters` (cartons laid in above Soll: the chances for a `TOO_MANY` tag), `demo_resets` (steps shown after `material_flow`: the station puts all 6 cartons back, unlogged), `container_count_breaks` (segments whose `count_after` does not follow storage 6 / order 0 after each reset), `container_unlinked_lines` (lines with no `grab_start` / `grab_end` of the same carton within 0.1 s). Every column empty (NA, not 0) before `"032"`. The lane demo's own moves are not logged. **Not attempts**; Concept `container_*` never enter `container_enters_<Scene>` or `summary.csv`. |
| `wide_with_paper.csv` | 4, 5 | Only with a paper file: `wide.csv` + the paper columns (see *Paper forms*). The file to give SPSS for questions 4 and 5. |
| `ssq_pre_post.png` | 4 | SSQ subscales and total before and after, one line per participant with both forms, `n` per panel. Colour = the log shows a headset removal, a pause or an end other than `completed`. No frame-rate or head-movement data (decision 022). |
| `tlx_vs_attempts.png` | 5 | Raw NASA-TLX against rated attempts over the whole course and the trainee's active time (minutes, facilitator tail removed), `n` per panel. Hollow = did not finish (fewer scenes). |
| `warnings.txt` | 6 | Everything the script found odd, then the validity limits. Read it first. |

Rules the script applies (design §8, decision 021): `events.jsonl` is the source of truth;
`confirm_count` (and every per-visit rate) counts only **rated attempts**: in
ConceptScene the quiz touches, in Basic/Mixed/Complex the red-button presses
(`confirm_pressed`); the Complex remainder quiz touches are not in
`confirm_count` (they are in `items.csv` as items `cartons` and `loose`), and
ignored presses (`confirm_ignored`) never count;
`completed` = the visit has a `task_completed` event (Mixed and Complex only
after the **last** position, so position 1 right is not "completed");
per-visit rates are NA (never 0 %) when a visit has 0 confirms; the counting
visit of a scene is the first one with ≥ 1 confirm (scenes without confirms, or
logs without outcome events: the first visit); a participant's sessions are
merged by time and visits renumbered: `repeat_visit` means `visit_no > 1`
for that scene across all sessions (so the counting visit itself can carry it,
e.g. when the first visit had no confirm), `repeat_after_crash` means a repeat
visit after a crashed session; times are active time (pauses removed).
`course_finished` is TRUE when ComplexScene has a `task_completed` **or** a
confirm with `correct: true` in any session, even if that session later
crashed or ended without `session_end`. A session with no `confirm_pressed` /
`quiz_answer` at all counts as "outcome not logged" (NA), because logs from
before #28 have no outcome events. A file with `schema_version` 1 (before
decision 021) is warned, not fatal: it stays in the data-quality table, but
its outcome columns are NA (not measured) and it is not in `items.csv`.

**Per-skill rule** (`error_rate_by_skill*.png`, `error_rate_<Scene>_<skill>`):
an attempt tests the skill of its item in `item_skills.csv`; an order position
without a skill there tests every skill its scene lists in `scene_skills.csv`.
The denominator of a skill in a visit = the counted attempts that tested it;
the numerator = those of them with an error code of that skill; the "first
attempt" = the first of them. Example: ConceptScene `check_material` counts
only the touches on question `material_by_matnr`, and its first attempt is the
first touch on that question (the quiz always starts with `cartons_from_soll`,
so "the visit's first touch" would make the other skills 0 % by
construction). In Basic, Mixed and Complex every counted press tests every
skill of the scene, except `ekz_placement` (#94, decision 028): a press tests
it only when it is an **EKZ-3 opportunity**, i.e. its position has Empf Kz
`003` and its `error_codes` hold nothing but `EKZ_PLACEMENT` (the validator
looks at the snap places only once the quantity is right; a `TOO_FEW` press
was never rated for placement). So Mixed `position_2`, Mixed `position_1` of
builds before #94 and wrong-quantity presses never enter its denominator;
a Mixed visit without an opportunity has no `ekz_placement` rate (NA, not 0).
`EKZ_PLACEMENT` on a press that is no opportunity is warned and not counted. Presses at the scaffolded Complex `position_1` are left
out of the skill rates (numerator and denominator): there the remainder quiz
gave the trainee the numbers, so it measures picking, not calculation. A
Complex visit that never reached position 2 has no Complex skill rate.

Data checks (all end in `warnings.txt`): the same `session_id` in two files
(e.g. a backup copy) keeps the longest file and names both paths; Unity
errors are summed from each line's `n_errors` (older logs without it: inferred
from `folded_count` and `stack`); a pause counts in the visit where it started,
its time is split over the visits it spans, and a pause never resumed ends at
the session's last event; sessions that hit the error cap are not
cross-checked for `unity_errors`; `session_start_mode` `auto` for a real ID,
`build` null / `unknown`, and a gap over 1 s between `utc` and `t` are warned.
A declined resume (`session_start.resume_accepted` false) is warned and the
participant flagged: they restarted at WelcomeScene and repeated lessons
already seen, so their later visits are second exposures. Also warned: resume
fields that break the null rule, and a resume offer whose previous session of
that ID in the data is not a crash (files missing from the copy?). A session
started despite the "already finished" warning
(`session_start.ended_session_warning` true) is warned and the participant
flagged: check it is the same person. The header `language` (#11, fixed per
session, no `language_change` event) is listed per participant in
`data_quality.csv` (`languages`, `NA` = log from before #11, allowed); a real
ID with a language other than `DE` is warned and flagged (`language_not_de`):
that trainee saw non-German texts.
Interaction checks (#29): a session counts as "interaction logged" only when
its header `event_groups` lists `interaction` (interaction events in a session
without it are warned and not read); `grab_end.duration_s` is compared with
the active time between the paired `grab_start` and `grab_end` of the same
`object_id` in the visit (a difference usually means a grab spanned a pause
and wall time was logged); `container_*.count_after` must equal the running
count of enters minus exits in the visit (every package in the container,
finished positions included); `scene_enter.via` must be `start` on the first
entry of a session and `menu` / `resume` later (`resume` only in a session
whose `session_start` accepted a resume), and a `menu` entry needs a
`menu_select` for that scene just before it. `summary.csv` columns `grabs`,
`container_enters`, `container_exits` are compared with the events; a 0 where
the scene can't produce the event (Welcome grabs, Welcome/Concept container
counts) is reported as a mismatch.

**Layout versions (decision 024, #82).** ConceptScene changed with layout
`"025"` (captions, tour with „Zurück“, quiz hidden until step 11, 1.5 s hold),
so its numbers are never pooled with a `"019"` build (the pilot P811) or a log
without `layout_version`. When a set holds more than one layout (schema 1 logs
not counted: none of their Concept data enters a figure), `warnings.txt` says
"set mixes layout_version …", `data_quality.csv` has `mixed_layout` TRUE on
every row, `items.csv` and `tour_steps.csv` carry `layout_version`, and the
charts show ConceptScene once per layout (`ConceptScene [019]`,
`ConceptScene [025]`; tour charts: one panel per layout). `wide.csv` keeps one
Concept column per measure: split it by `layout_versions` before comparing.

**Complex position 1 scaffold (#93).** Up to layout `"025"` (and `"019"`)
the remainder quiz and the Worked Example stay visible during the whole of
position 1; from `"029"` they hide once both answers are right and shelf
card 2 shows the trainee's result. When a set holds both levels,
`warnings.txt` says "set mixes Complex position_1 scaffold …" and
`first_attempt_by_item.png` and `time_by_phase.png` show Complex
`position_1` once per level (`position_1 [019/025]`, `position_1 [029]`).
Position 2 and the other picking scenes stay pooled; the per-scene Complex
charts (`time_to_correct_by_scene.png`, `error_codes_by_scene.png`) still
pool both levels.

**Mixed position 1 Empf Kz (#94, decision 028).** Builds before #94 had
Empf Kz `000` at Mixed `position_1` (cartons put in any way, never rated);
from #94 it is `003` with 3 snap places and can get `EKZ_PLACEMENT`. The #94
build logs `line.empf_kz` on every `confirm_pressed` but kept layout
`"029"`, which an earlier build without the change also wrote, so R tells
the builds apart by the field, not the layout: a log without it is read as
the old orders (Basic `003`, every other position `000`) and a `"029"` log
without it is warned (`items.csv` / `process_by_position.csv` column
`empf_kz`). When a set holds both levels, `warnings.txt` says "set mixes
Mixed position_1 Empf Kz …" and `first_attempt_by_item.png` and
`time_by_phase.png` show Mixed `position_1` once per level
(`position_1 [Empf Kz 000]`, `position_1 [Empf Kz 003]`). The per-scene
Mixed numbers still pool both, and an Empf Kz `003` visit can have more
presses (EKZ retries): filter by `empf_kz` before comparing them. A logged
`empf_kz` that differs from the current orders is warned (R uses the
logged one).

## Validity limits (state them with every result)

- Fixed scene order: a lower error rate in a later scene mixes learning with scene difficulty. It is an indication of improvement, not proof (no control group, no pre/post test).
- Quizzes (ConceptScene, Complex remainder quiz): a guess is right 33 % of the time (A/B/C; 50 % for the two EKZ questions) and a touch is rated at once, so later touches are trial-and-error. Use the first attempt per item (`items.csv`, `first_attempt_by_item.png`), not the number of touches.
- Complex position 1 is scaffolded (see above): don't pool it with position 2 as one calculation measure, and don't pool its `quiz_visible` and `result_on_card` rows (#93).
- Mixed position 1 (#94): don't pool its Empf Kz `000` (builds before #94) and `003` rows. The `ekz_placement` rate has a small denominator (only presses with the right quantity at a `003` position, usually 1–2 per visit): read it as counts (`k/n`), not as a smooth percentage.
- Schema 1 logs have no outcome (NA): don't read them as 0 errors.
- Data recorded before backlog #14 (feedback printing target numbers) measure trial-and-error, not skill. Keep builds before and after #14 apart (`builds` column).
- Small N, one run per person: describe distributions first; any test is exploratory.
- Time-to-correct only covers visits that reached correct; read it with the `never` counts.
- Q4: no frame-rate, frame-drop, head-movement or tracking data exist (decision 022). The SSQ change can only be set against headset removals, pauses, an early end (`end_status`, `abbruchgrund`) and active time, for the whole session; sickness per scene can't be analysed. Pre-existing symptoms are why the change (post − pre) is used, not post alone.
- Q5: NASA-TLX is asked once at the end (raw, unweighted). Who stopped early did fewer scenes, so their attempts and time are not comparable with finishers.
- Concept tour (`tour_steps.csv`): a „Zurück“ < 2 s after „Weiter“ is read as a likely mis-press, not proven; step time runs until the next step, so looking at the label board or packages during a step is in it. Never pool Concept numbers of different `layout_version` (`"025"` and `"029"` are split too: decision 030 changed the Concept station).
- Concept hint tags (`concept_hints*.csv`): one tag at a time, so a second wrong package gets its tag only when the first is gone; hint counts are not errors or attempts (on `"029"` placing packages in Concept is not logged; from `"032"` the carton moves are in `concept_containers_by_visit.csv`, `order_over_soll_enters` = chances for `TOO_MANY`).

## If the logger format changes

Change `make_fake_data.R` and `analyse_set.R` (the analysis) in the same commit, run
`selftest.R`, then run `analyse.R` on one real Play-mode log. A renamed or
removed field bumps `schema_version`.

## Cách chạy (cho người mới học R)

1. Cài R (CRAN) và RStudio theo mục *Setup*, rồi gõ trong cửa sổ Console của RStudio:
   `install.packages(c("tidyverse", "jsonlite"))` (chỉ làm một lần).
2. Chép thư mục `research/` từ kính (hoặc từ máy chạy Editor) vào
   `analysis/data/<đợt>/`, ví dụ `analysis/data/wave1/research/`. Không đổi tên file.
   Không bao giờ commit thư mục `analysis/data/`.
3. Trong RStudio: *Session → Set Working Directory → Choose Directory…* và chọn
   thư mục gốc của repo (`xrpick-v2`).
4. Chạy kiểm tra trước: mở `analysis/selftest.R`, bấm **Source**. Dòng cuối phải là
   `selftest OK`. Nếu báo lỗi, đừng dùng kết quả phân tích; hãy báo lại cho developer.
5. Chạy phân tích: gõ trong Console, đủ cả ba dòng:
   `data_dir <- "analysis/data/wave1"`
   `output_dir <- "analysis/output"`
   `source("analysis/analyse.R")`
   Hai dòng đầu script in ra là `data_dir = …` và `output_dir = …`: kiểm tra đúng
   thư mục. Nếu `data_dir` chứa chữ `fake`, script dừng và báo lỗi (dữ liệu giả).
   Nếu có phiếu giấy: nhập vào bản sao của `analysis/paper_forms_template.csv`, lưu thành
   `analysis/data/wave1/paper_forms.csv` (xem mục *Paper forms*; ô trống = chưa trả lời,
   không gõ 0). Script ghi thêm `wide_with_paper.csv`, `ssq_pre_post.png`,
   `tlx_vs_attempts.png`. Không có file phiếu thì script vẫn chạy bình thường.
6. Kết quả của nhóm nghiên cứu (`P001`–`P799`) nằm trong `analysis/output/`; kết quả
   của nhóm pilot (`P800`–`P899`, log trong `research/pilot/`) nằm riêng trong
   `analysis/output/pilot/`, cùng các file như trên. Không bao giờ gộp hai nhóm.
   Đọc `warnings.txt` trước, rồi
   `data_quality.png` (ai có dữ liệu cần kiểm tra lại) và dòng "Paper forms to check"
   (phiếu giấy nhập sai hoặc thiếu), sau đó mới xem các biểu đồ.
   Biểu đồ chính cho câu hỏi 1 là `first_attempt_by_item.png` (lần thử đầu tiên ở
   mỗi câu hỏi / mỗi vị trí); hai biểu đồ `error_rate_by_skill*.png` chỉ là phụ.
   Tour ở ConceptScene: `tour_steps.csv`, `tour_time_by_step.png` (thời gian mỗi
   bước) và `tour_back_by_step.png` (bấm „Zurück“ ở bước nào). Thẻ nhắc vàng ở
   ConceptScene (từ layout "029"): `concept_hints.csv` (mỗi thẻ một dòng) và
   `concept_hints_by_visit.csv` (đếm theo thùng × lý do, thời gian demo ba làn phải
   chờ ở bước `material_flow`); đây không phải lượt thử hay tỉ lệ lỗi. Nếu `warnings.txt`
   báo "set mixes layout_version", số liệu ConceptScene đã được tách theo
   `layout_version`; không gộp lại.
7. Khi trình bày kết quả, luôn ghi kèm các giới hạn ở mục *Validity limits*: chỉ là
   **dấu hiệu** tiến bộ, không chứng minh khoá học gây ra tiến bộ.
