# Research log contract v2 (#28 bước 1) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the v2 log format (spec, skill CSVs, fake data, R analysis) to match the current exercises, before any C# logging code is written.

**Architecture:** `docs/research-logging-spec.md` is the contract; `make_fake_data.R` stands in for the C# logger and writes files in exactly that format; `analyse.R` reads them and draws the Q1/Q2/Q6 outputs; `selftest.R` runs both and asserts. No Unity file changes in this step (C# is #28 bước 2).

**Tech Stack:** Markdown, CSV, R 4.6.2 (`tidyverse`, `jsonlite` only). Run R as `"/c/Program Files/R/R-4.6.2/bin/Rscript.exe"` (not on PATH).

**Spec:** `docs/decisions/021-research-log-catalogue-v2.md` (+ `docs/backlog.md` #28 *Done when*, design `docs/superpowers/specs/2026-11-01-research-logger-design.md` §6–§8).

## Global Constraints

- `schema_version` = 2. Every event keeps the common fields of spec §3 (`t, utc, participant_id, session_id, scene, scene_visit, event, data`).
- Header adds `layout_version` (string, `"019"` = the three-lane station of decision 019) and `event_groups` (array of `"session"`, `"outcome"`, `"interaction"`, `"pose"`, `"perf"`: the groups this build emits; R treats a missing group as NA, not 0).
- Times carry units (`_s`, `_m`) and are active time unless `_wall_s`. A key that doesn't apply is `null`, not 0.
- Item keys are material keys: `connector_a`, `screw_b`, `ring_d`, `training_b` (CLAUDE.md, #17).
- Orders: Basic 1 position `training_b` 300/50; Mixed pos 1 `connector_a` 150/50, pos 2 `screw_b` 60/20; Complex pos 1 `ring_d` 55/25, pos 2 `training_b` 156/50. Only Complex counts loose pieces.
- Error codes logged: `EMPTY, WRONG_MATERIAL, TOO_FEW, TOO_MANY, MISSING_LOOSE, EXTRA_LOOSE, WRONG_ANSWER, EKZ_PLACEMENT`. Never logged: `WRONG_TYPE`, `NO_ANSWER`. Invariant `correct == (error_codes empty)`.
- R packages: `tidyverse`, `jsonlite` only; beginner-readable, commented step by step (design §4).
- `analysis/` is owned by `data-analyst`; it reviews the whole diff before commit. Spec + fake data + R change in one commit (CLAUDE.md gotcha).

## Review Focus

1. A v1 log (schema 1, real dev logs in `analysis/data/real/`) next to v2 logs: analyse.R must not crash; it warns "schema 1 file" and leaves v1 outcome columns NA. → test in Task 4.
2. A `confirm_ignored` must never change `confirm_count`, `attempt_no` or any rate denominator. → test in Task 4.
3. Mixed/Complex `task_completed` comes only after the last position; a correct position 1 alone is not "completed". → test in Task 4.
4. A quiz question answered right on the 3rd touch must count as first-attempt wrong (guessing through A/B/C). → test in Task 4.
5. A skill listed in `scene_skills.csv` for a scene but never erred shows 0 %, not missing; a skill not listed (e.g. `check_material` in Mixed) never appears for that scene. → test in Task 4.

---

### Task 1: Spec v2

**Files:**
- Modify: `docs/research-logging-spec.md` (status line, §3 header, §4.1–4.7, §5, §6, §7)

**Interfaces:**
- Produces: the event and field names below, used verbatim by Tasks 3–4 and by #28 bước 2 / #29 / #30.

- [ ] **Step 1: Rewrite the event catalogue to decision 021**
  - §3 header: add `layout_version`, `event_groups`; `schema_version` 2.
  - §4.1: `scene_enter.via` enum `start | menu | resume | facilitator_end`; `menu_select {target_scene, blocked}` (no `item_label`); new `end_control_opened`, `end_control_cancelled` (data `{}`).
  - §4.2: `confirm_pressed {attempt_no, position, position_count, attempt_in_position, line: {item, soll, vpe, required_full, required_loose, placed_full, placed_loose}, foreign: [{item, count}], correct, error_codes, time_since_scene_enter_s}` (only rated presses, written before the position advances); `confirm_ignored {reason: station_moving | completed | quiz_first, position, time_since_scene_enter_s}`; `position_completed {position, attempts_in_position}`; `task_completed {attempts_to_correct, time_to_first_confirm_s, time_to_correct_s}` (picking: at order completion; Concept: when all 4 questions are right); `quiz_answer {attempt_no, quiz: concept | complex_remainder, question_id, selected, correct, attempt_in_question, error_codes, time_since_scene_enter_s}` (every rated touch); `quiz_unlocked {}`; `tour_step {step_id, step_index}`; `station_move_start` / `station_move_arrived {what: storage_out | storage_in | order_box, position}`; `welcome_practice_done {action: grab | button}`. Order rule: `confirm_pressed` → `position_completed` → `station_move_start` → `station_move_arrived`.
  - §4.3: table of the 8 logged codes; `EMPTY` at position > 1 = nothing of the current position in the box; `EKZ_PLACEMENT` after the `OrderRule` codes; `WRONG_TYPE`/`NO_ANSWER` reserved, never logged.
  - §4.4: `grab_start {object_id, material_key, hand, input}`, `grab_end {object_id, material_key, hand, input, duration_s}` (`hand`/`input` may be `null` until #74), `container_enter/exit {container: order, object_id, material_key, count_after}`; `object_dropped_floor`, `input_mode_change`, `tracking_*` marked #74. Remove `button_touch`, `info_shown`.
  - §4.5: replace `gaze_target_*` with "head-direction zones are assigned in R from `_pose.csv` per `layout_version`" and the zone list of 021.
  - §4.6/§4.7: `perf_sample` unchanged; pose stream via an open stream, sampler outside `Research/`.
  - §5 summary columns: remove `dwell_wms_s`, `dwell_instructions_s`; add `confirm_ignored` after `confirm_count`; `confirm_count` = rated presses only.
  - §6: replace both JSON examples with v2 ones (Mixed pos 1, 2 of 3 cartons → `TOO_FEW` item `connector_a`; Complex `quiz_answer` `cartons` selected `"1"` wrong); skill mapping now via `skill_map.csv` + `scene_skills.csv` + `item_skills.csv`.
  - §7: replace "feedback reveals the answer" by "Complex position 1 is scaffolded by the remainder quiz"; keep the other validity notes.

- [ ] **Step 2: Check no stale names remain**

Run: `grep -nE "gga|ggb|\"gg\"|package_type|button_touch|info_shown|gaze_target|item_label|dwell_wms" docs/research-logging-spec.md`
Expected: only lines that say these are removed / reserved.

### Task 2: Skill CSVs

**Files:**
- Modify: `analysis/skill_map.csv`, `analysis/scene_skills.csv`
- Create: `analysis/item_skills.csv`

**Interfaces:**
- Produces: `item_skills.csv` columns `scene,item,skill,scaffolded` (item = `question_id` for quizzes, `position_<n>` for picking positions; `skill` empty = the position measures every skill of the scene; `scaffolded` `true`/`false`/empty).

- [ ] **Step 1: Write the three files**
  - `skill_map.csv` rows: `EMPTY,calculate_quantity`, `WRONG_MATERIAL,check_material` (description by material key), `TOO_FEW`/`TOO_MANY,calculate_quantity`, `MISSING_LOOSE`/`EXTRA_LOOSE,calculate_loose`, `EKZ_PLACEMENT,ekz_placement`, `WRONG_ANSWER,` (empty skill: taken from `item_skills.csv` by `question_id`).
  - `scene_skills.csv`: Concept `calculate_quantity, check_material, ekz_placement`; Basic `calculate_quantity, ekz_placement`; Mixed `calculate_quantity`; Complex `calculate_quantity, calculate_loose`.
  - `item_skills.csv`: the table of decision 021 (`cartons_from_soll`, `material_by_matnr`, `ekz_003`, `ekz_000`, `cartons`, `loose`, `ComplexScene,position_1,,true`, `ComplexScene,position_2,,false`).

- [ ] **Step 2: Verify**

Run: `grep -c "WRONG_TYPE\|NO_ANSWER\|package_concept" analysis/skill_map.csv analysis/scene_skills.csv`
Expected: 0 in both.

### Task 3: Fake data v2

**Files:**
- Modify: `analysis/make_fake_data.R` (§4 course content, §5 `start_session`/`press_confirm`/`record_outcome`/`exit_scene`, §6 scene runners, §7 participants)

**Interfaces:**
- Consumes: Task 1 names, Task 2 CSVs.
- Produces: files under `analysis/data/fake/` in v2 format; `make_fake_data.R` keeps its entry point (sourced by `selftest.R`).

- [ ] **Step 1: Course content.** Replace `order_required`/`counts_obj`/`total_pieces` with `order_positions` (list per scene of positions `{item, soll, vpe}` from Global Constraints) and `line_obj(pos, placed_full, placed_loose)`; `required_loose`/`placed_loose` `null` outside Complex. Replace `scene_skill_list` and `skill_base_logit` by the v2 skills (`calculate_quantity, check_material, calculate_loose, ekz_placement`).
- [ ] **Step 2: Header** `schema_version = 2`, `layout_version = "019"`, `event_groups = ["session","outcome"]`.
- [ ] **Step 3: Picking runner.** Loop over positions; per press draw `TOO_FEW/TOO_MANY` (item = position's material), Complex `MISSING/EXTRA_LOOSE`, Basic `EKZ_PLACEMENT` (only when quantity is right), ~3 % `EMPTY` alone; no `WRONG_TYPE`, no `WRONG_MATERIAL` (no foreign material in any scene). Emit `confirm_pressed` → on correct `position_completed` → `station_move_start`/`station_move_arrived` (storage_out + storage_in between positions; storage_out + order_box at the end) with ~3 s ride; `task_completed` only after the last position. Emit `confirm_ignored` with `reason = station_moving` for ~20 % of position changes (press during the ride) and, in Complex, `quiz_first` for ~15 % of visits before the quiz is done.
- [ ] **Step 4: Quiz runners.** Concept: 11 `tour_step` events, `quiz_unlocked` at `material_flow`, then 4 questions in order, each touched until right (answer keys and correct keys: `cartons_from_soll` {2, 4✓, 25}, `material_by_matnr` {connector_a, screw_b✓, ring_d}, `ekz_003` {ordered✓, poured}, `ekz_000` {ordered, poured✓}); `task_completed` after the 4th right. Complex: `cartons` {"1","2"✓,"3"} and `loose` {"5"✓,"20","55"} before position 1, then picking. Each wrong touch carries `WRONG_ANSWER`.
- [ ] **Step 5: Welcome** emits `welcome_practice_done` `grab` then `button`.
- [ ] **Step 6: Summary rows** write `confirm_ignored` and drop the two dwell columns (same order as spec §5).
- [ ] **Step 7: Keep the existing edge-case participants** (crash + resume, crash without resume, overlapping pauses, 0-confirm visit, `P9xx` in `research/`, two builds, unknown code `FOO_BAR`, invariant break P110) and add: one participant with a Mixed position-2 `EMPTY`; one with a correct Complex position 1 then crash (not completed); one v1-format session file copied from the current generator's output shape (schema 1) under a `P1xx` ID for the mixed-schema warning.
- [ ] **Step 8: Run**

Run: `"/c/Program Files/R/R-4.6.2/bin/Rscript.exe" analysis/make_fake_data.R`
Expected: finishes without error; `grep -rlE '"WRONG_TYPE"|"NO_ANSWER"|"gga"' analysis/data/fake` prints nothing.

### Task 4: analyse.R v2 + selftest

**Files:**
- Modify: `analysis/analyse.R` (§2 config, §5 header fields, §7 outcome events, §9 summary compare, §11–§14 charts and wide file, §15 validity text)
- Modify: `analysis/selftest.R`

**Interfaces:**
- Consumes: Task 2 CSVs, Task 3 files.
- Produces: outputs in `analysis/output/`: existing files plus `first_attempt_by_item.png` and `items.csv` (one row per participant × scene × item: `first_attempt_correct`, `attempts`, `skill`, `scaffolded`); `wide.csv` gains `confirm_ignored_<Scene>` and `wait_s_<Scene>` (sum of station rides).

- [ ] **Step 1: Write the failing selftest assertions** in `selftest.R` (new numbered block after 8c):

```r
items <- read_csv(file.path(output_dir, "items.csv"), show_col_types = FALSE)
stopifnot(file.exists(file.path(output_dir, "first_attempt_by_item.png")))
# Review Focus 4: right only on a later touch counts as first-attempt wrong.
stopifnot(all(!items$first_attempt_correct[items$attempts > 1]))
# Review Focus 2: ignored presses never count as attempts.
stopifnot(all(visits_out$confirm_count == visits_out$rated_presses, na.rm = TRUE))
# Review Focus 3: Mixed/Complex completed only after the last position.
stopifnot(!any(wide$completed_ComplexScene[wide$participant_id == "<crash-after-pos1 id>"] %in% TRUE))
# Review Focus 5: no check_material rate for Mixed/Complex; tested skills present.
stopifnot(!any(skill_rates$scene %in% c("MixedPickingScene", "ComplexScene") & skill_rates$skill == "check_material"))
# Review Focus 1: the schema-1 file is warned, not fatal.
stopifnot(any(grepl("schema_version 1", warn_lines)))
```
(`visits_out`/`skill_rates` = the objects analyse.R already builds; use the real names.)

- [ ] **Step 2: Run to see it fail**

Run: `"/c/Program Files/R/R-4.6.2/bin/Rscript.exe" analysis/selftest.R`
Expected: FAIL at the first new assertion (`items.csv` missing).

- [ ] **Step 3: Implement in `analyse.R`**
  - Read `item_skills.csv`; read `layout_version`, `event_groups`; warn per file with `schema_version` 1 and set its outcome columns NA.
  - Parse `confirm_pressed` v2 (`position`, `attempt_in_position`, `line.*`) and `quiz_answer` v2 (`quiz`, `question_id`, `attempt_in_question`); `confirm_ignored` counted separately; `WRONG_ANSWER` skill from `item_skills.csv` by `question_id`.
  - Build `items`: per participant × scene × item (position or question) the first attempt's `correct` and the attempt count; write `items.csv` and `first_attempt_by_item.png` (share of first attempts correct per skill across scenes in build order, scaffolded items drawn hollow). This is the primary Q1 chart; `error_rate_by_skill*.png` stay as secondary.
  - `completed` per visit = a `task_completed` exists; `wait_s` = sum of `station_move_arrived.t − station_move_start.t` (paused time subtracted) per visit.
  - Summary compare: `confirm_ignored` column; tolerate missing dwell columns.
  - §15 validity text: replace "feedback reveals the answer / pre-#14" by the 021 §7 wording; keep fixed-order and 33 % guess notes.
- [ ] **Step 4: Run selftest to pass**

Run: `"/c/Program Files/R/R-4.6.2/bin/Rscript.exe" analysis/selftest.R`
Expected: ends with the selftest's success line, no `stop()`; no seam warnings.

- [ ] **Step 5: data-analyst review** of the full diff (Tasks 1–4); fix blocking findings.

- [ ] **Step 6: Commit** (with the decision and backlog files from solution-decision)

```bash
git add docs/research-logging-spec.md analysis/skill_map.csv analysis/scene_skills.csv analysis/item_skills.csv analysis/make_fake_data.R analysis/analyse.R analysis/selftest.R docs/decisions/021-research-log-catalogue-v2.md docs/decisions/011-workstation-layout-conveyor.md docs/backlog.md docs/superpowers/plans/2026-11-04-research-log-v2-contract.md
git commit -m "docs: research log contract v2 (spec, skill maps, fake data, R) (#28 step 1, decision 021)"
```
