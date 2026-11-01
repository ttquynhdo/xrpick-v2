# analysis/: R analysis of the research logs (backlog #26)

The xpick app writes one events file and one summary file per session
(`docs/research-logging-spec.md`, design `docs/superpowers/specs/2026-11-01-research-logger-design.md`).
The scripts here turn those files into charts and tables for research questions
1 (improvement per skill), 2 (hard scenes, common errors) and 6 (data quality),
plus a wide per-participant file for SPSS and the paper questionnaires.

| File | What it is |
|---|---|
| `analyse.R` | The analysis. One script, run top to bottom, `tidyverse` + `jsonlite` only. |
| `selftest.R` | Makes fake data and checks `analyse.R` against every edge case. Run after any change. |
| `make_fake_data.R` | Writes fake logs in exactly the logger's format (base R). The contract between C# and R. |
| `scenes.csv` | `scene, build_order`: scene order in every chart (survives scene renames). |
| `skill_map.csv` | `error_code, skill, description`: which skill each error code tests. |
| `scene_skills.csv` | `scene, skill`: which skills each scene tests (an error-free tested skill shows 0 %, not missing). |
| `data/` | Copied logs. **Git-ignored, never commit.** |
| `output/` | Charts and tables. Git-ignored. |

The study lead may edit the three CSVs without touching code.

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
| `error_rate_by_skill.png` | 1 | Per skill and scene: share of confirm presses with an error of that skill. Dot = one participant (first visit of the scene with ≥ 1 confirm), diamond = mean, `n` under each scene. |
| `error_rate_by_skill_first_attempt.png` | 1 | Same skills, first press only: `k/n` participants made that error on attempt 1. |
| `error_codes_by_scene.png` | 2 | Which error codes occur in which scene (counting visits). Unknown codes are marked `(unknown)`. |
| `time_to_correct_by_scene.png` | 2 | Active time to the first correct confirm per scene; `never` counts the people who didn't get there (survivor bias). |
| `data_quality.csv` / `.png` | 6 | One row per participant: `end_status` (completed / facilitator / quit / crash), `course_finished` (empty = no outcome events logged), `multi_session`, `mixed_build`, `builds`, `gap_h`, `resume_offered` / `resume_declined` (#27: a resume after a crash was offered / declined in any session; empty = log from before #27), `ended_session_override` (a session was started with "Trotzdem starten" although the ID already had a finished session; empty = log from before the #27 fix wave), `duration_censored`, repeat visits, `paused_s` (seconds), pauses, headset removals, Unity errors, `error_cap_reached`, invariant breaks, unknown codes, mismatches with `summary.csv`, `exit_disagreements` (visits whose `scene_exit` numbers differ from their events), `duplicate_files` (copies of a session's files that were dropped). Orange = check. |
| `wide.csv` | all | One row per real participant for SPSS. Per-scene columns (`active_s_<Scene>`, `paused_s_<Scene>`, `time_to_correct_s_<Scene>`, `confirms_<Scene>`, `error_rate_<Scene>`, `error_rate_<Scene>_<skill>`, …) come from the counting visit; scenes without a confirm or quiz (WelcomeScene) have no outcome columns. Join the paper forms (SSQ, NASA-TLX, #31) on `participant_id`. |
| `warnings.txt` | 6 | Everything the script found odd, then the validity limits. Read it first. |

Rules the script applies (design §8): `events.jsonl` is the source of truth;
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
before #28 have no outcome events.

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
flagged: check it is the same person.

## Validity limits (state them with every result)

- Fixed scene order: a lower error rate in a later scene mixes learning with scene difficulty. It is an indication of improvement, not proof (no control group, no pre/post test).
- ConceptScene quiz: the feedback reveals the answer and a guess is right 33 % of the time.
- Data recorded before backlog #14 (feedback printing target numbers) measure trial-and-error, not skill. Keep builds before and after #14 apart (`builds` column).
- Small N, one run per person: describe distributions first; any test is exploratory.
- Time-to-correct only covers visits that reached correct; read it with the `never` counts.

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
6. Kết quả nằm trong `analysis/output/`. Đọc `warnings.txt` trước, rồi
   `data_quality.png` (ai có dữ liệu cần kiểm tra lại), sau đó mới xem các biểu đồ.
7. Khi trình bày kết quả, luôn ghi kèm các giới hạn ở mục *Validity limits*: chỉ là
   **dấu hiệu** tiến bộ, không chứng minh khoá học gây ra tiến bộ.
