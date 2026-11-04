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
| `analyse.R` | The analysis. One script, run top to bottom, `tidyverse` + `jsonlite` only. |
| `selftest.R` | Makes fake data and checks `analyse.R` against every edge case. Run after any change. |
| `make_fake_data.R` | Writes fake logs in exactly the logger's format (base R). The contract between C# and R. |
| `scenes.csv` | `scene, build_order`: scene order in every chart (survives scene renames). |
| `skill_map.csv` | `error_code, skill, description`: which skill each error code tests. |
| `scene_skills.csv` | `scene, skill`: which skills each scene tests (an error-free tested skill shows 0 %, not missing). |
| `paper_forms_template.csv` | Empty paper-form table (header row only) to copy for each wave; see *Paper forms*. |
| `item_skills.csv` | `scene, item, skill, scaffolded`: the skill of each quiz question (`question_id`) and order position (`position_<n>`), and which items are scaffolded (Complex `position_1`). A position with an empty skill tests every skill of its scene. |
| `data/` | Copied logs. **Git-ignored, never commit.** |
| `output/` | Charts and tables. Git-ignored. |

The study lead may edit the four config CSVs (`scenes`, `skill_map`, `scene_skills`,
`item_skills`) without touching code. The header of `paper_forms_template.csv` is a
contract with `analyse.R` (the script stops if they differ): change both together.

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
`practice/` or by mistake in `research/` (the latter with a warning). A real
ID (`P001`–`P899`) found under `practice/` is **kept**, with a warning, so no
participant's data is lost silently: check it in `warnings.txt`.

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

`analyse.R` prints `data_dir` and `output_dir` first; check them. It refuses
any `data_dir` whose path contains `fake` unless `allow_fake <- TRUE` is set
(only `selftest.R` does that). `selftest.R` runs in its own environment and
writes to `analysis/output/selftest/`, so it leaves no settings behind in
RStudio. Without Rscript on PATH use the full path, e.g.
`"C:\Program Files\R\R-4.6.2\bin\Rscript.exe" analysis/analyse.R`.

## What each output answers

| Output | Question | Reads as |
|---|---|---|
| `first_attempt_by_item.png` | 1 (**main chart**) | Per item (quiz question or order position) in build order: share of participants right at their **first attempt** at that item, with a 95 % Wilson interval and `right/participants`. One panel per skill. Hollow point = scaffolded (Complex position 1). Guess rate 33 % (A/B/C) or 50 % (EKZ questions): a quiz share near that level says little. |
| `items.csv` | 1 | One row per participant, scene and item (the first visit with an attempt at that item). `attempts` = all touches / red-button presses at that item in that visit (rated ones only; ignored presses are never attempts). `first_attempt_correct` = the first of them was right (right only on a later touch counts as wrong). `reached_correct` = any of them was right. `scaffolded` = the app gave the numbers (from `item_skills.csv`). `repeat_visit` = this visit was not the participant's first visit of the scene (e.g. after a crash): a second exposure. `first_attempt_codes` = error codes of the first attempt. |
| `error_rate_by_skill.png` | 1 (secondary) | Per skill and scene: share of the attempts **at items of that skill** that had an error of that skill (see *Per-skill rule* below). Dot = one participant (first visit of the scene with ≥ 1 counted attempt), diamond = mean, `n` under each scene. |
| `error_rate_by_skill_first_attempt.png` | 1 (secondary) | Same skills, first attempt at an item of that skill only: `k/n` participants made that error there. |
| `error_codes_by_scene.png` | 2 | Which error codes occur in which scene (counting visits). Unknown codes are marked `(unknown)`. |
| `time_by_phase.png` | 3 | Per scene and order position (completed positions of counting visits): active seconds of the three phases `read` (position start → first `grab_start`), `pick` (first grab → last `container_enter`/`_exit` before `position_completed`) and `check` (last box change → the correct press). Position start = scene enter for position 1 (Complex: storage box 1 arrived after the remainder quiz), the `station_move_arrived` of the new storage box for later positions, so the box-change wait (`wait_s`) is never inside. Point = one participant, bar = median, `n` per position. |
| `process_by_position.csv` | 3 | One row per participant, picking-scene visit and position: `read_s`, `pick_s`, `check_s` (they add up to `position_s`), `presses`, `completed`, `scaffolded`, `counting`. A position never completed keeps `read_s` (if a grab happened) and NA for the rest. Only sessions whose header lists the `interaction` group. |
| `time_to_correct_by_scene.png` | 2 | Active time to the first correct confirm per scene; `never` counts the people who didn't get there (survivor bias). |
| `data_quality.csv` / `.png` | 6 | One row per participant: `end_status` (completed / facilitator / quit / crash), `course_finished` (empty = no outcome events logged), `multi_session`, `mixed_build`, `builds`, `gap_h`, `resume_offered` / `resume_declined` (#27: a resume after a crash was offered / declined in any session; empty = log from before #27), `ended_session_override` (a session was started with "Trotzdem starten" although the ID already had a finished session; empty = log from before the #27 fix wave), `duration_censored`, repeat visits, `paused_s` (seconds), pauses, headset removals, Unity errors, `error_cap_reached`, invariant breaks, unknown codes, mismatches with `summary.csv`, `exit_disagreements` (visits whose `scene_exit` numbers differ from their events), `duplicate_files` (copies of a session's files that were dropped), `interaction_logged` (#29 group in the header of any session), `end_control_opens` (facilitator end control opened; more than 1 = opened by mistake; empty = not measured), `menu_blocked` (touches on a locked Menu item, #42), `open_grabs` (a package still held at a crash / scene change: no hold time), `facilitator_tail_s` (active seconds from the last `end_control_opened` to the end of a session the facilitator ended: not the trainee's time). Orange = check. |
| `wide.csv` | all | One row per real participant for SPSS. Per-scene columns (`active_s_<Scene>`, `paused_s_<Scene>`, `time_to_correct_s_<Scene>`, `confirms_<Scene>`, `error_rate_<Scene>`, `error_rate_<Scene>_<skill>`, …) come from the counting visit; scenes without a confirm or quiz (WelcomeScene) have no outcome columns. `confirm_ignored_<Scene>` = red-button presses the app ignored (box still riding, order already done, Complex quiz not answered yet): counted, never an attempt, never in a rate. `wait_s_<Scene>` = seconds the trainee was blocked by a box change **before** `task_completed`: one interval per `storage_in` ride, from its trigger (the previous position's `position_completed`; for Complex box 1 the last remainder-quiz touch before the ride) to that ride's `station_move_arrived` (the visit's last event if it never arrived), intervals unioned, cut at `task_completed`, minus pauses. It includes the 0.6 s before the box starts and leaves out the old box still riding away; Basic has no `storage_in` ride, so 0. `wait_after_done_s_<Scene>` = union of all rides after `task_completed` (the boxes leaving at the end; empty when the task was never completed). These three exist only for the red-button scenes (Basic, Mixed, Complex). #29: `grabs_<Scene>` (= `grab_start` lines; Concept and the picking scenes, WelcomeScene's box has no `PackageData`), `regrabs_<Scene>` (grabs of a package already grabbed in the visit), `time_to_first_grab_s_<Scene>`, `container_enters_<Scene>` / `container_exits_<Scene>` (picking scenes only; an exit = a package taken out of the order container again), and `read_s_` / `pick_s_` / `check_s_<Scene>_p<n>` per position. All NA (not 0) for a session whose header `event_groups` lacks `interaction`. Join the paper forms (SSQ, NASA-TLX, #31) on `participant_id`. |
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
skill of the scene. Presses at the scaffolded Complex `position_1` are left
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

## Validity limits (state them with every result)

- Fixed scene order: a lower error rate in a later scene mixes learning with scene difficulty. It is an indication of improvement, not proof (no control group, no pre/post test).
- Quizzes (ConceptScene, Complex remainder quiz): a guess is right 33 % of the time (A/B/C; 50 % for the two EKZ questions) and a touch is rated at once, so later touches are trial-and-error. Use the first attempt per item (`items.csv`, `first_attempt_by_item.png`), not the number of touches.
- Complex position 1 is scaffolded (see above): don't pool it with position 2 as one calculation measure.
- Schema 1 logs have no outcome (NA): don't read them as 0 errors.
- Data recorded before backlog #14 (feedback printing target numbers) measure trial-and-error, not skill. Keep builds before and after #14 apart (`builds` column).
- Small N, one run per person: describe distributions first; any test is exploratory.
- Time-to-correct only covers visits that reached correct; read it with the `never` counts.
- Q4: no frame-rate, frame-drop, head-movement or tracking data exist (decision 022). The SSQ change can only be set against headset removals, pauses, an early end (`end_status`, `abbruchgrund`) and active time, for the whole session; sickness per scene can't be analysed. Pre-existing symptoms are why the change (post − pre) is used, not post alone.
- Q5: NASA-TLX is asked once at the end (raw, unweighted). Who stopped early did fewer scenes, so their attempts and time are not comparable with finishers.

## If the logger format changes

Change `make_fake_data.R` and `analyse.R` in the same commit, run
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
6. Kết quả nằm trong `analysis/output/`. Đọc `warnings.txt` trước, rồi
   `data_quality.png` (ai có dữ liệu cần kiểm tra lại) và dòng "Paper forms to check"
   (phiếu giấy nhập sai hoặc thiếu), sau đó mới xem các biểu đồ.
   Biểu đồ chính cho câu hỏi 1 là `first_attempt_by_item.png` (lần thử đầu tiên ở
   mỗi câu hỏi / mỗi vị trí); hai biểu đồ `error_rate_by_skill*.png` chỉ là phụ.
7. Khi trình bày kết quả, luôn ghi kèm các giới hạn ở mục *Validity limits*: chỉ là
   **dấu hiệu** tiến bộ, không chứng minh khoá học gây ra tiến bộ.
