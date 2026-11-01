# Design: research logger core (backlog #26)

Status: draft for review, 2026-11-01. Builds on `docs/research-logging-spec.md` (event catalogue) and backlog #26–#31.

## 1. Intent

The course will be run with real trainees once, and the logs are the only quantitative record of how they did: there is **no paper knowledge test** and no control group. The data must therefore be designed backwards from the charts we want, and proven usable before the first real participant, because a format mistake found after the study can't be fixed.

The user ranked the research questions (1 = most important):

| # | Question | Chart | Data from |
|---|---|---|---|
| 1 | Do trainees improve across the course? | Error rate per **skill** across scenes; attempts within a scene | #28 (outcome), skill map (this design) |
| 2 | Which scene is hard, which errors are common? | Stacked bar: error codes per scene; box plot time-to-correct per scene | #28 |
| 3 | Where does time go (reading WMS, picking, confirming)? | Per-trainee timeline | #29, #30 |
| 4 | Does VR discomfort (paper SSQ) relate to frame drops / head movement? | Scatter | #30 + paper |
| 5 | Does perceived load (paper NASA-TLX) match attempts? | Scatter | #28 + paper |
| 6 | Whose data is affected by errors, pauses, headset removal? | Data-quality table | **#26** |

#26 directly answers only question 6 and scene durations. Its real job is to fix the **shape** of the data so #28–#30 slot in without format changes, and to prove that shape with an R analysis run on fake data.

### Measuring "improvement" without a test
Each error code is mapped to the skill it tests (e.g. `WRONG_MATERIAL` → check material, `TOO_FEW`/`TOO_MANY` → calculate quantity, `MISSING_LOOSE`/`EXTRA_LOOSE` → calculate loose pieces). A skill that reappears in a later scene gives an error-rate-per-skill-across-scenes chart. This is an **indication** of improvement, not proof (no control group, no pre/post test); reports must say so. A short in-VR pre/post quiz is parked as backlog #43.

## 2. Goals, non-goals, anti-goals

**Goals**
- A session/scene/event log on the headset that R reads with no manual cleaning.
- Every event carries the keys needed to join later events: participant, session, scene, scene visit, monotonic time.
- A `summary.csv` with one row per scene visit whose columns already include those needed for questions 1–2 (filled by #28; empty until then).
- A runnable R script that turns logs into the charts for questions 1, 2 and 6 and a wide per-participant file for SPSS.
- The app has its final identity before any data exists.

**Non-goals (for #26)**
- Outcome events (`confirm_pressed`, `quiz_answer`, `task_completed`, error codes): #28.
- Grab/container/button/input/tracking events: #29. Head-direction, pose stream, perf samples: #30.
- Participant ID keypad: #27. Retrieval guide and merge with paper forms: #31. Menu lock: #42.
- Real data collection. It waits for #14 (feedback no longer prints target numbers) and for #1/#15 to be `done` (spec §7).

**Anti-goals (a failure even if it "works")**
- Logging changes what the trainee sees, hears, or how scenes flow.
- Any personal data on the headset (names, free text). Only `P` + 3 digits.
- A scene throws or behaves differently when the logger is absent.
- Measurable frame-time cost on Quest from logging (no per-frame allocation, no per-frame disk write).
- A format that needs hand-editing in Excel before R/SPSS can read it.
- Data lost on crash or power-off beyond the last line.

## 3. Constraints

- Quest 3, offline, Unity 6000.4. The research code is its own assembly `XRPick.Research` (`Assets/Scripts/Research/XRPick.Research.asmdef`, references `Oculus.VR`, auto-referenced so `Assembly-CSharp` scene scripts can call it; it never references `Assembly-CSharp`), so EditMode tests in `Assets/Tests/EditMode/Research/` (`XRPick.Research.Tests`) can test it; the build-info writer stays in `Assembly-CSharp-Editor`. Follow `docs/coding-conventions.md` (new scripts in `Assets/Scripts/Research/`, one public class per file, English comments, no scene edits).
- Null-safe static API: `ResearchLogger.Log(...)` is a no-op when no session runs, so scene scripts call it unconditionally.
- Write each line to disk immediately (flush per line; spec §1).
- Schema is versioned (`schema_version`); adding fields is fine, renaming/removing bumps the version.
- Pseudonymous; DSGVO handling per spec §1.

## 4. Decided

**App identity** (user, 2026-11-01), set in `ProjectSettings` as part of #26:

| Field | Value |
|---|---|
| Company Name | `XRISE` |
| Product Name | `xpick` |
| Android package id | `com.xrise.xpick` |

Consequences: log folder `…/LocalLow/XRISE/xpick/research/` in the Editor and `/sdcard/Android/data/com.xrise.xpick/files/research/` on Quest. Quest treats it as a new app; uninstall the old build.

**Analysis tool** (user): **R** (user is learning it), SPSS as fallback. Hence:
- Logs are "long": one event per line (`events.jsonl`), one scene visit per row (`summary.csv`).
- The R script also writes a **wide** file, one row per participant (`active_s_<Scene>`, `paused_s_<Scene>`, `time_to_correct_s_<Scene>`, `errors_<Scene>`, …; scenes without a confirm or quiz get no outcome columns), for SPSS and for joining paper questionnaires by `participant_id`.
- CSV: comma separator, dot decimal, UTF-8, header row, UTC timestamps in ISO 8601. (German Excel may mis-split columns; R/SPSS read it correctly.)
- R script written for a beginner: one `.R` file run top to bottom, commented step by step, packages limited to `tidyverse` and `jsonlite`.

**Study setup** (spec §9, decided 2026-11-01): no between-group condition, one run per participant; facilitator types the ID; menu locked to build order during the study (#42); head pose only, no hand positions; facilitator deletes logs on the headset after copying. Paper questionnaires (user, 2026-11-01): **SSQ before putting on the headset and again at the end**, **NASA-TLX once at the end**. The wide file joins one set of paper columns per participant (`ssq_pre_*`, `ssq_post_*`, `tlx_*`). Question 4 uses the SSQ change (post − pre, per subscale and total) against whole-session log measures (session `fps_avg`/dropped frames, head movement, total active time). Question 5 compares NASA-TLX with total attempts and active time. Per-scene load or sickness can't be analysed. The paper forms and their entry template belong to #31; the app doesn't change.

**Session start before #27 exists:** the logger starts a session automatically at app start with a configured test ID. Default `P000` = practice mode, logged to `research/practice/` and excluded from analysis. #27 later replaces the automatic start with the keypad flow through a public `StartSession(participantId)`.

**Resume after a crash** (user, 2026-11-01): the app is reopened and the facilitator types the **same ID** again. The logger finds that ID's earlier session without `session_end` and the last scene it entered. #27 asks the facilitator whether to continue that ID's course and, on yes, loads that scene directly via `SceneMenuController.OpenScene` (decision 004 §5; WelcomeScene is never a resume point). The trainee restarts **that scene from its beginning**; progress inside the scene is not saved. #26 provides the query `ResearchLogger.TryFindResumePoint(participantId, out scene)`: true with the last scene entered when that ID's newest earlier session ended in a crash. It reads the existing files on the device and returns false when the ID has no files or its last session ended cleanly.

**Test IDs** *(designer's call)*: `P000` and the whole range `P900`–`P999` are test IDs. They go to `research/practice/`, and `analyse.R` also drops them if they turn up anywhere else. Real participants get `P001`–`P899` on the paper forms. #27 may show "test mode" for these IDs.

**Clean end vs crash** *(designer's call)*: `session_end` with reason `quit` is written when Unity's quit callback runs (also when Play mode stops in the Editor). Any events file that has no `session_end` is reported as `crash_recovered` on the next start. Quest often skips the quit callback (system-menu close, power-off), so those sessions count as crashes. That is the honest reading for question 6. To test crash recovery in the Editor, a developer-only option makes the logger skip `session_end` on quit. This replaces "stop Play midway" in the current #26 *Done when*.

**Source of truth** *(designer's call)*: `events.jsonl` is the source of truth. `summary.csv` is a convenience copy for SPSS/Excel. `analyse.R` rebuilds the per-visit table from events, so a crash that loses the open scene's summary row loses nothing for the analysis. `analyse.R` also compares the rebuilt table against `summary.csv` where both exist, which checks the C# summary code.

**Several sessions per participant** *(designer's call)*: a crash or restart gives a new `session_id` under the same `participant_id`. `scene_visit` restarts in each session, so the C# side carries nothing over. `analyse.R` merges a participant's sessions in time order and renumbers visits per scene across them. It also sets a `multi_session` flag in the data-quality table and the wide file, so the wide file still has exactly one row per participant. Fake data includes at least one such participant.

**Active time vs wall time** *(designer's call)*: `t` keeps running while paused or with the headset off. `summary.csv` therefore has `duration_s` (active time, with pauses subtracted) and `paused_s`. The same subtraction applies to `time_to_first_confirm_s` and `time_to_correct_s` when #28 fills them. `analyse.R` uses active time.

**Error rate per skill (question 1)** *(designer's call; analysis only, the log format doesn't depend on it)*: the primary measure is the share of confirm presses in a scene visit that carry at least one error code of that skill. It is computed from the per-confirm events (#28), not from the per-visit `error_codes_all`. A second chart uses first attempts only. `analysis/scene_skills.csv` (`scene, skill`) lists which scene tests which skill, so a tested but error-free skill shows 0 % instead of being missing. The study lead can change both CSVs without code changes. Note for #28: ConceptScene's quiz needs a `WRONG_ANSWER` code (today only `NO_ANSWER` exists) so the quiz can map to a skill.

**Skill map:** a small CSV `analysis/skill_map.csv` (`error_code, skill, description`) kept with the analysis, not in app code, so the study lead can change the mapping without a rebuild.

**Build identity:** the events header carries the git short hash of the build, so data from before/after a fix (e.g. #14) can be separated.

**Dry run with fake data, inside #26:** an R script generates fake sessions for several participants in exactly the logger's format (including fake #28 outcome fields), and the analysis script draws the charts for questions 1, 2 and 6 from them. At least one **real** session file from a Unity Play-mode run must also load through the same analysis script, as the format check between C# and R.

**Spec cleanup:** update `docs/research-logging-spec.md`: remove `condition`/`run` (or fix them as constants), hand positions in the pose stream, the paper knowledge test (goal line and §6 "pre/post knowledge test gain"), and the "two validators in Basic" note (fixed by #1); remove `crash-recovered` from the `session_end` reasons; "created in WelcomeScene" becomes "created at app start"; rename time/length fields with units (`time_since_scene_enter_s`, `duration_s`, `fall_from_height_m`, …); add the skill map, the contract in §6 and the summary columns below.

## 5. Data shape

### events file `<participant_id>_<session_id>_events.jsonl`
Line 1: header (spec §3: `schema_version`, `app_version`, `build`, `device_model`, `os_version`, `participant_id`, `session_id`, `session_start_mode` (`auto` / `manual`), `language`, `input_mode_at_start` (may be `null`), `target_refresh_rate`; `condition`/`run` dropped; `build` may be `null` (resource missing) or `"unknown"` (git failed)). Every following line has the common fields from spec §3 (`t`, `utc`, `participant_id`, `session_id`, `scene`, `scene_visit`, `event`, `data`).

Events emitted by #26: `session_start`, `session_end` (reason), `scene_enter`, `scene_exit` (`data`: `duration_s`, `paused_s`, `pauses`, `hmd_removed_count`, `unity_errors`, so every summary column #26 fills can be rebuilt from events), `app_pause`, `app_resume` (`paused_s`, `causes`), `unity_error`, `crash_recovered` (spec §4.1, §4.6).

### summary file `<participant_id>_<session_id>_summary.csv`
One row per scene visit, appended on `scene_exit` and on `session_end` for the current scene. Columns (spec §5 minus `condition`/`run`): `participant_id, session_id, build, schema_version, scene, scene_visit, enter_utc, duration_s, paused_s, confirm_count, first_attempt_correct, attempts_to_correct, time_to_first_confirm_s, time_to_correct_s, completed, error_codes_all, grabs, drops_floor, container_enters, container_exits, dwell_wms_s, dwell_instructions_s, pauses, hmd_removed_count, fps_avg, dropped_frames, unity_errors`. #26 fills the identity, timing (`duration_s` active, `paused_s`), `pauses`, `hmd_removed_count` and `unity_errors` columns; the rest stay empty (NA in R) until #28–#30 fill them. Moving the summary skeleton into #26 means #28 only adds values, not the file.

### analysis folder (repo root, not under `Assets/`)
`analysis/skill_map.csv`, `analysis/scene_skills.csv`, `analysis/scenes.csv`, `analysis/make_fake_data.R`, `analysis/analyse.R`, `analysis/README.md` (how to install R + RStudio, run the scripts, where outputs go). Real logs are never committed (git-ignore `analysis/data/`). Chart outputs go to `analysis/output/` (git-ignored).

## 6. Contract for later events (fixed now, emitted by #27/#28)

The fake data in #26 already uses these shapes, so they are fixed here *(designer's call, after the data-analyst review)*. The in-code validator result type (pending decision with #14, `docs/coding-conventions.md` §5) can be shaped freely, as long as it maps 1:1 onto this log format.

**Session end reasons.** #26 provides `ResearchLogger.EndSession(reason)` and writes `quit` itself. #28 calls `EndSession("completed")` on the first correct confirm in ComplexScene. #27 adds a hidden facilitator control that calls `EndSession("facilitator")`. `crash` is never a `session_end` reason: a crash is the absence of `session_end`. `analyse.R` derives `end_status` (completed / facilitator / quit / crash) itself and computes `course_finished` from a `task_completed` in ComplexScene, so a Quest session that skips the quit callback after finishing still counts as finished.

**`confirm_pressed`** (one per press):
```json
{"event":"confirm_pressed","data":{
  "attempt_no":2,
  "container":{"gg":4,"vpe":0,"gga":null,"ggb":null,"loose":null,"other":0,"total_pieces":200},
  "required":{"gg":6,"vpe":0,"gga":null,"ggb":null,"loose":null,"other":0,"total_pieces":300},
  "correct":false,
  "error_codes":[{"code":"TOO_FEW","item":"gg"}],
  "time_since_scene_enter_s":41.2}}
```
`container`/`required` always carry the same fixed keys. A key that doesn't apply in a scene is `null`, not 0. `item` is `null` for codes without an item (`EMPTY`, `WRONG_TYPE`).

**`quiz_answer`**: `{"attempt_no":1,"selected":"vpe","correct":false,"error_codes":[{"code":"WRONG_ANSWER","item":null}]}`.

**`task_completed`** (first correct confirm per visit): `{"attempts_to_correct":3,"time_to_first_confirm_s":38.0,"time_to_correct_s":95.5}`. Times are active time.

**Invariant:** `correct == (error_codes is empty)`. The shared error-code helper decides both. `analyse.R` reports any row that breaks this. (Today `OrderConfirm` would call 6 GG + 1 VPE correct; #28 must make the rule and the codes agree.)

**Error codes:** spec §4.3 plus `WRONG_ANSWER`. `EMPTY` is logged alone, without an extra `TOO_FEW`. Every code has a row in `skill_map.csv`, including `EMPTY`, `WRONG_TYPE`, `NO_ANSWER` and `WRONG_ANSWER`.

**`crash_recovered`** is written into the old file with the old `session_id`, `t: null`, `scene`/`scene_visit` `null`, `utc` = time of detection, `data: {last_event_t, recovered_by_session_id}`. R doesn't depend on it, because it derives crashes from the missing `session_end`; a copy taken before the next start lacks the line anyway.

## 7. Logger rules found in review

- **Pauses:** the logger keeps one paused/active state. It writes `app_pause` (`causes`: list of focus_lost / hmd_unmounted / pause) on the change from active to paused, and `app_resume` when every cause has ended. Overlapping OVR/Unity signals therefore never double-count. `pauses` counts these transitions, and `hmd_removed_count` counts the ones whose causes include `hmd_unmounted`. The logger tracks four sources separately (app focus, OVR input focus, `OnApplicationPause`, HMD) and maps them to those three labels; `app_resume` carries `paused_s` and `causes` = the labelled union seen during the pause. A pause that spans a scene change counts in the visit where it started (`paused_s` is split by visit); one still open at a crash ends at the file's last event; on quit during a pause no `app_resume` is written and the open part is in `scene_exit.paused_s`. `analyse.R` builds pause intervals per session and clips them to each visit.
- **Error spam:** repeated `unity_error` with the same message within 5 s is folded into one line with `folded_count` (the repeats before that line) and `n_errors` (errors the line stands for: 1, `folded_count` + 1 after the window, `folded_count` on the visit-close line; R sums `n_errors`), and errors are capped per session (e.g. 200 lines, then one `unity_error_cap_reached`). File paths in messages are cut to the file name, so no `C:\Users\<name>` reaches the log.
- **Time:** `t` uses `Time.realtimeSinceStartupAsDouble`. Every time or length field carries its unit (`_s`, `_m`) and is active time unless named `_wall_s`.
- **Header line** carries `"event":"header"` so R can recognise it.
- **Language:** in research mode (non-test ID) the language is locked to DE and logged in the header. A `language_change` event exists for test mode.
- **Build identity** (`build`, `schema_version`) is also a column in `summary.csv`.

## 8. Analysis rules (`analyse.R`)

- **Rates:** per visit first (NA when `confirm_count == 0`, never 0 %), then mean per participant, then per scene. Pooling all presses would let trial-and-error trainees dominate.
- **Which visit counts for questions 1–2:** the first visit of a scene with at least one confirm. Later visits are flagged `repeat_visit`, and `repeat_after_crash` when a crash came before them. They are kept in the long data but not in the wide file's per-scene columns.
- **Survivor bias:** time-to-correct plots show the count of visits that never reached correct next to each box.
- **Data-quality table and wide file:** `end_status`, `course_finished`, `multi_session`, `mixed_build`, `build`, `schema_version`, `gap_h` between sessions (catches two people typed under one ID), `duration_censored` (last visit of a crashed session, ended at its last event). `course_finished` is NA when none of the person's sessions logged outcome events. The data-quality table also counts `exit_disagreements` (visits whose `scene_exit` differs from their events; part of the flagged list), `duplicate_files` (one `session_id` in several files: the longest file is kept, all paths are warned) and `error_cap_reached` (no `unity_errors` cross-check for that session); `paused_s` is in seconds.
- **Format checks:** warnings for `session_start_mode` `auto` with a real ID, `build` null vs a missing header vs `"unknown"`, and a gap over 1 s between `utc − header utc` and `t` (clock jump or realtime stopped during sleep).
- **Scene order:** `analysis/scenes.csv` (`scene, build_order`) fixes order and survives scene renames (#18).
- **Unknown codes:** a code missing from `skill_map.csv` is a warning, not silently dropped.
- **Validity limits printed with every report:** fixed scene order confounds difficulty with improvement; Concept quiz feedback reveals the answer and has a 33 % guess rate; data before #14 measures trial-and-error.

## 9. Done when (to merge into backlog #26 when it starts)

Existing #26 *Done when* stays, plus:
- `ProjectSettings` shows Company `XRISE`, Product `xpick`, Android package id `com.xrise.xpick`; the Play-mode log lands under `…/XRISE/xpick/research/`.
- After a Play-mode run with `P000` or any `P9xx` ID, the files are under `research/practice/`. With a participant-range ID (e.g. `P101`), they are under `research/`.
- With the developer "skip session_end" option on, stop Play and then run again: the old file gets a `crash_recovered` event. With the option off, stopping Play writes `session_end` with reason `quit`.
- Pause the app for a few seconds inside a scene (Editor: lose focus; Quest: system menu). `paused_s` grows by about that time and `duration_s` doesn't.
- Leaving a scene appends one row to `_summary.csv`, with the #26 columns filled and the later columns empty.
- (code) No logger call allocates or writes to disk per frame.
- Run a session with `P101`, enter MixedPickingScene, crash (dev option), restart: `TryFindResumePoint("P101", out scene)` returns true with MixedPickingScene. After a clean quit, and for an unknown ID, it returns false.
- (code) A `unity_error` raised every frame produces one folded line per 5 s, never one line per frame.
- `analysis/make_fake_data.R` produces fake sessions for at least 10 participants. The fake set includes these edge cases: two sessions (crash then resume), a crash with no later session, overlapping pause signals, a visit with 0 confirms, a `P9xx` file placed in `research/`, an error code missing from the skill map, and two builds. `analysis/analyse.R` reads them without errors and writes:
  - three charts: error rate per skill across scenes, error codes per scene, and the data-quality table;
  - a wide CSV with exactly one row per real participant (the test ID is dropped, and the two-session participant is flagged `multi_session`);
  - warnings for the unknown error code and for any row that breaks `correct == (error_codes is empty)`.
- The same `analyse.R` reads one real session from a Unity Play-mode run without errors (manual case: needs R installed by the user).
- `docs/research-logging-spec.md` no longer mentions `condition`/`run`, hand positions, a paper knowledge test, or two Basic validators.

## 10. Review

The `data-analyst` agent reviews this design before approval, every logging diff before commit, and one real Play-mode log per change (`unity-safe-change` step 3). It also owns `analysis/`. Its **chặn** findings block the item. A format change updates `make_fake_data.R` and `analyse.R` in the same change.

## 11. Left to the builder

- JSON writing: hand-written writer or `com.unity.nuget.newtonsoft-json` (present only as a transitive dependency; add it to `manifest.json` explicitly if used).
- Class split inside `Assets/Scripts/Research/`, how the test ID is configured (e.g. a settings asset), and how the git hash reaches the build (e.g. a build preprocessor writing a text asset; `editor` in Play mode).
- Crash detection details, the `unity_error` noise filter list, OVR pause/HMD event wiring.
- Fake-data distributions in R (plausible, not realistic: they only have to exercise every chart).
