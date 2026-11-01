# Research data logging – specification (draft)

Status: 2026-11-01. The session core (sections 1, 3, 4.1, `unity_error` and `crash_recovered` in 4.6, 5) is implemented by backlog #26; the participant keypad, resume offer and the `session_start` fields (section 2, 4.1) by #27; the other events are still planned (#28-#30). Design and decisions: `docs/superpowers/specs/2026-11-01-research-logger-design.md`.

Goal: record, per learner and per session, how trainees perform and experience the VR order-picking course on Meta Quest 3, so the logs can later be analysed and joined with the paper questionnaires (SSQ before and after, NASA-TLX at the end) via a pseudonymous participant ID. There is no paper test of the course content and no control group, so improvement is read from error rates per skill across scenes (an indication, not proof).

## 1. Principles

- **Pseudonymous.** The headset never stores names or other personal data. The only person key is `participant_id` (e.g. `P017`), printed on the paper questionnaire set. The name ↔ ID list stays on paper with the study lead (DSGVO/GDPR; signed consent before the session).
- **Offline, append-only.** Every event is written to disk immediately (flush per line), so a crash keeps everything up to the crash. No network.
- **One logger, many emitters.** A single `ResearchLogger` (DontDestroyOnLoad, created at app start by `RuntimeInitializeOnLoadMethod`) owns the files; scene scripts only call `ResearchLogger.Log(eventType, payload)` (one way: the logger never calls scene code). Scripts must work unchanged if the logger is absent (null-safe static API), so scenes stay testable on their own.
- **Logging never changes training behaviour.** It observes; it doesn't alter feedback, timing or scene flow.
- **Versioned schema.** Every file header carries `schema_version` (now 1); adding fields is fine, renaming/removing bumps the version.

## 2. Participant ID entry and session lifecycle

1. Facilitator puts the headset on the table, starts the app → WelcomeScene shows a **numeric keypad** panel (digits, delete, confirm). ID format `P` + 3 digits; the `P` is fixed, only digits are typed.
2. On confirm: a second panel shows `P017 – correct?` with *Yes* / *Back*. Only *Yes* starts the session. Before that, the rest of WelcomeScene is hidden/disabled.
3. There is no between-group variable and each participant runs the course once, so the header has no group or run field. A restart after a crash gets a new `session_id` under the same `participant_id`; `ResearchLogger.TryFindResumePoint(participantId, out scene)` tells whether that ID's newest earlier session ended in a crash and which scene it was in.
4. Session starts → logger creates files, writes the header, emits `session_start`. Since #27 the keypad starts it; the logger starts a session itself (ID from the `ResearchSettings` asset, default `P000`) only when the app starts in a scene other than WelcomeScene (build index 0), i.e. in the Editor.
5. Session ends when the trainee reaches the end (ComplexScene correct + congratulations), the facilitator uses a hidden *End session* control, or the app quits/pauses for good. Emits `session_end` with the reason (`completed`, `facilitator`, `quit`). A crash is never a reason: it is a file without `session_end`. The end screen shows the participant ID again so the facilitator can check it against the paper form and write down the end time.
6. **Test IDs** `P000` and `P900`-`P999` are logged to `research/practice/` and excluded from analysis (`analyse.R` also drops them if they appear elsewhere). Real participants use `P001`-`P899`.

## 3. Files

Location: `Application.persistentDataPath/research/` → on Quest `/sdcard/Android/data/<package>/files/research/`. Retrieve with Meta Quest Developer Hub or `adb pull`.

| File | Content |
|---|---|
| `<participant_id>_<session_id>_events.jsonl` | One JSON object per line, every event in time order |
| `<participant_id>_<session_id>_pose.csv` | Head pose samples at 10 Hz (planned, #30) |
| `<participant_id>_<session_id>_summary.csv` | One row per scene visit, appended when the scene is left (and for the open scene at `session_end`); ready for SPSS/R/Excel. A convenience copy: the events file is the source of truth |

`session_id` is a UUID generated at session start. File names never contain anything but the IDs. Format: UTF-8, LF line endings, comma separator, dot decimal, ISO 8601 UTC timestamps.

### Common fields on every event

```json
{"t": 123.456, "utc": "2026-11-01T09:14:03.120Z", "participant_id": "P017", "session_id": "…",
 "scene": "ComplexScene", "scene_visit": 2, "event": "confirm_pressed", "data": { … }}
```

- `t` – seconds since session start (`Time.realtimeSinceStartupAsDouble` offset; monotonic). It keeps running while the app is paused, so use the `_s` active-time fields for durations. `t` is `null` on `crash_recovered`.
- `utc` – wall-clock time, only for matching with paper forms.
- `scene_visit` – counter per scene within the session, starting at 1 in each session, so revisiting a scene is distinguishable. `null` on `header`, `session_start` and `session_end`.
- `scene` – the active scene; `null` on the header.
- Every time or length field carries its unit (`_s`, `_m`) and is active time (pauses subtracted) unless named `_wall_s`.

### Header (first line of the events file)

The first line has `"event":"header"` (so R can recognise it), `t` 0, and `data`: `schema_version`, `app_version`, `build` (7-digit git short hash, plus `-dirty` with uncommitted changes, baked at build by `git describe --always --dirty --abbrev=7 --exclude=*`; `"editor"` in Play mode; `"unknown"` when git failed at build time; `null` when the `ResearchBuildInfo` resource is missing from the build), `device_model`, `os_version`, `participant_id`, `session_id`, `session_start_mode` (`auto` = started by `ResearchSettings.autoStart`, `manual` = `StartSession` called by other code, e.g. the #27 keypad; a real participant's session should be `manual`), `language` (`null` until #11; #11 must make it non-null before real data collection), `input_mode_at_start` (`hands` / `controllers` / `none`; `none` when no input is active yet, always in the Editor without a headset; `null` when the Meta SDK can't be read), `target_refresh_rate` (`null` without a headset).

## 4. Event catalogue

### 4.1 Session and navigation

Emitted by #26 unless noted.

| Event | Data | Source |
|---|---|---|
| `session_start` / `session_end` | `session_start` (#27): `resume_offered_scene` = build scene name the keypad offered to resume at (from `TryFindResumePoint`, i.e. the newest earlier session of this ID crashed in that scene), `null` when nothing was offered; `resume_accepted` = `true` / `false` for the answer, `null` exactly when `resume_offered_scene` is `null` (so "declined" and "never asked" stay apart). Logs from before #27 lack both keys (R: NA, not measured). A crash while WelcomeScene was the last scene offers no resume (WelcomeScene is never a resume point). `ended_session_warning` (#27 fix wave) = `true` when the keypad showed the "already finished" warning (a real ID with an earlier session that has `session_end`) and the facilitator pressed `Trotzdem starten`, otherwise `false` (also for `auto` starts); logs written before it lack the key (R: NA). Additive, `schema_version` stays 1. `session_end`: `reason` completed / facilitator / quit. `scene_visit` is `null` on both | logger |
| `scene_enter` | `from_scene` (`null` on first entry), `via`: `"start"` on the first entry, otherwise `null` until #29 knows menu / auto | `SceneManager.activeSceneChanged` |
| `scene_exit` | `duration_s` (active), `paused_s`, `pauses`, `hmd_removed_count`, `unity_errors` (all `unity_error` lines of the visit incl. folded and post-cap), so every summary column #26 fills can be rebuilt from events | logger |
| `menu_select` | `target_scene`, `item_label` (#29) | `SceneMenuController.Open*` |
| `app_pause` | `causes`: list of `focus_lost` / `pause` / `hmd_unmounted` | logger |
| `app_resume` | `paused_s`, `causes`: union of the causes seen during this pause | logger |

Pause handling: four internal sources are tracked separately (app focus, OVR input focus, `OnApplicationPause`, HMD mounted) and mapped to the three labels. `app_pause` is written when the state goes from active to paused, `app_resume` when every source is active again, so overlapping signals never double-count. `pauses` counts these transitions, `hmd_removed_count` those whose causes include `hmd_unmounted`. A pause that spans a scene change counts in the visit where it started (its `paused_s` is split by visit); a pause still open at a crash ends at the file's last event. On quit during a pause no `app_resume` is written; the open part is included in `scene_exit.paused_s`.

### 4.2 Learning outcomes (one event per confirm press)

| Event | Data | Source |
|---|---|---|
| `confirm_pressed` | `attempt_no` (per scene visit), `container`: counts per type (`gg`, `vpe`, `gga`, `ggb`, `loose`, `other`, `total_pieces`), `required`: the same keys, `correct` (bool), `error_codes` (list of `{code, item}`, see 4.3), `time_since_scene_enter_s` | `OrderConfirm.OrderConfirmation`, `MixedPickingValidator.ConfirmOrder`, `PiecesOrderValidator.ConfirmOrder` |
| `quiz_answer` | `selected`: gg / vpe / both, or `null` when nothing was chosen (with `NO_ANSWER`); `correct`, `attempt_no`, `error_codes` | `PackageQuizController.CheckAnswer` |
| `task_completed` | `attempts_to_correct`, `time_to_first_confirm_s`, `time_to_correct_s` | validators (first correct confirm per visit) |

BasicPickingScene logs only the result of `OrderConfirm` (total-pieces rule).

The exact shapes are fixed in section 6.

### 4.3 Error codes

Derived from container contents vs. order, independent of the text shown to the trainee:

| Code | Meaning |
|---|---|
| `WRONG_TYPE` | a package type not in the order is inside (e.g. VPE where only GG is ordered) |
| `WRONG_MATERIAL` | right package form but wrong material (e.g. GGA in ComplexScene) |
| `TOO_FEW` / `TOO_MANY` | per ordered item, with `item` |
| `MISSING_LOOSE` / `EXTRA_LOOSE` | loose pieces short/over (ComplexScene) |
| `EMPTY` | confirm with an empty container |
| `NO_ANSWER` | quiz confirm without selection |
| `WRONG_ANSWER` | quiz answered, but wrongly (ConceptScene) |

Validators get one shared helper that computes these codes; the visible feedback text can then be built from the same codes (backlog #14).

### 4.4 Interaction process

| Event | Data | Source |
|---|---|---|
| `grab_start` / `grab_end` | `object_id` (stable name), `package_type`, `hand` (L/R), `input` (hand/controller), `duration_s` (on end), `end_pos` | Interaction SDK grabbable `WhenPointerEventRaised` (Select/Unselect), via a small `GrabLogger` component on each package prefab |
| `object_dropped_floor` | `object_id`, `fall_from_height_m` | collision with floor layer |
| `container_enter` / `container_exit` | `container`: order/storage, `object_id`, `package_type`, `count_after` | counters' `OnTriggerEnter/Exit` (only when the HashSet actually changes) |
| `info_shown` | `object_id`, `panel` | `PackageInfoOnGrab.ShowInfo` (ConceptScene) |
| `button_touch` | `button`: confirm / menu item / quiz toggle, `hand` | poke interactable events |
| `input_mode_change` | `from`, `to` | `OVRInput.GetActiveController` change |
| `tracking_lost` / `tracking_regained` | `what`: hand_L / hand_R / controller | OVR hand/controller tracking state |

### 4.5 Attention proxy (Quest 3 has no eye tracking)

`gaze_target_enter` / `gaze_target_exit` with `target`: wms_panel / instruction_cards / order_box / storage_box / menu, computed from a ray along the head forward vector against tagged colliders, minimum dwell 200 ms. This is **head direction**, not eye gaze – state it as such in any publication.

### 4.6 Comfort and technical

| Event / data | Data | Source |
|---|---|---|
| `perf_sample` (every 5 s) | `fps_avg`, `frame_ms_p95`, `dropped_frames`, `battery_level`, `cpu_level`/`gpu_level` if available | `FrameTimingManager` / `OVRPlugin.GetAppPerfStats`, `SystemInfo.batteryLevel` |
| `unity_error` | `type` (lowercase `error` / `exception` / `assert`), `message` (rooted file paths cut to the file name), `stack` (first lines; `null` on a line that only reports folded repeats), `folded_count` (repeats of the same message folded in before this line), `n_errors` (how many errors the line stands for: 1 for a first occurrence, `folded_count` + 1 for a line written after the 5 s window, `folded_count` for a line written at visit close that only reports folded repeats; the sum over a visit's lines equals `scene_exit.unity_errors` unless the cap was reached) | `Application.logMessageReceived` (warnings and known OVRPlugin XR start-up noise filtered by a list; the logger's own errors are not logged) |
| `crash_recovered` | `last_event_t`, `recovered_by_session_id`; written into the old file with its old `session_id`, `t`, `scene` and `scene_visit` `null`, `utc` = time of detection | on next start, any earlier events file without `session_end` |

Error spam: the same message within 5 s is folded into one line carrying `folded_count`; folded counts are written before `scene_exit` and before `session_end`. After 200 `unity_error` lines in a session one `unity_error_cap_reached` event is written and further errors are only counted (in `scene_exit.unity_errors`), so for such a session the events can't rebuild `unity_errors`. `app_pause`, `scene_exit` and `session_end` lines are forced to the storage device (fsync), since sleep or power-off may follow them.

Clean end vs crash: `session_end` with reason `quit` is written by Unity's quit callback (also when Play mode stops). `session_end` is always the last line of a clean file. Quest often skips the quit callback (system-menu close, power-off), so those sessions count as crashes. A developer option in `ResearchSettings` skips `session_end` on quit, to test crash recovery.

### 4.7 Pose stream (`_pose.csv`)

10 Hz, head pose only (no hand positions): `t, scene, head_px, head_py, head_pz, head_rx, head_ry, head_rz, head_rw`. Enables path length and head movement (comfort). Planned, #30.

## 5. Summary CSV (one row per scene visit)

`participant_id, session_id, build, schema_version, scene, scene_visit, enter_utc, duration_s, paused_s, confirm_count, first_attempt_correct, attempts_to_correct, time_to_first_confirm_s, time_to_correct_s, completed, error_codes_all, grabs, drops_floor, container_enters, container_exits, dwell_wms_s, dwell_instructions_s, pauses, hmd_removed_count, fps_avg, dropped_frames, unity_errors`

`duration_s` is active time (pauses subtracted), `paused_s` the paused time of the visit. #26 fills identity, `build`, `schema_version`, `enter_utc`, `duration_s`, `paused_s`, `pauses`, `hmd_removed_count`, `unity_errors`; the rest stay empty (NA in R) until #28-#30. `error_codes_all` is semicolon-separated `CODE:n` (e.g. `TOO_FEW:2;EMPTY:1`). The events file is the source of truth; `analyse.R` rebuilds the table from events and compares it with `summary.csv` (tolerance 0.002 s for `_s` columns).

## 6. Contract for outcome events (#27/#28) and analysis

These shapes are fixed now; the fake data in `analysis/` already uses them. The in-code validator result type can be shaped freely as long as it maps 1:1 onto this format.

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
`container`/`required` always carry the same fixed keys; a key that doesn't apply in a scene is `null`, not 0. `item` is `null` for codes without an item (`EMPTY`, `WRONG_TYPE`, `NO_ANSWER`, `WRONG_ANSWER`).

**`quiz_answer`**: `{"attempt_no":1,"selected":"vpe","correct":false,"error_codes":[{"code":"WRONG_ANSWER","item":null}]}`.

**`task_completed`** (first correct confirm per visit): `{"attempts_to_correct":3,"time_to_first_confirm_s":38.0,"time_to_correct_s":95.5}`. Times are active time.

**Invariant:** `correct == (error_codes is empty)`. One shared helper decides both; `analyse.R` reports any row that breaks it.

**Error codes:** section 4.3 including `WRONG_ANSWER`. `EMPTY` is logged alone, without an extra `TOO_FEW`. Every code has a row in `analysis/skill_map.csv` (`error_code, skill, description`), which maps it to the skill it tests (e.g. `WRONG_MATERIAL` to check material, `TOO_FEW`/`TOO_MANY` to calculate quantity); the study lead can edit it without a rebuild. `analysis/scene_skills.csv` lists which scene tests which skill.

**Session end:** #28 calls `ResearchLogger.EndSession("completed")` on the first correct confirm in ComplexScene; #27 adds a hidden facilitator control for `EndSession("facilitator")`. `analyse.R` derives `end_status` (completed / facilitator / quit / crash) and `course_finished` (a `task_completed` in ComplexScene) itself.

**Derived measures (computed later, not logged):** task time, error rate per skill across scenes (share of confirm presses per visit with at least one code of that skill; also first attempts only), attempts to correct, error profile per scene, hesitation (enter to exit of the same object), re-grabs, time spent reading WMS before first grab, head movement per minute, frame drops vs SSQ change, NASA-TLX vs attempts and active time. Which visit counts: the first visit of a scene with at least one confirm; later visits are flagged `repeat_visit`.

## 7. Dependencies and validity notes

- **Feedback reveals the answer** in MixedPickingScene and ComplexScene (failure text prints targets) and ComplexScene's counters show `/ 2`, `/ 55` (backlog #14, #15). `attempts_to_correct` then measures trial-and-error, not calculation. Fix before real data collection, or record which build each participant used (`build` in the header) and analyse separately.
- Language: `language` is `null` in the header until DE/EN switching lands (#11); it must be non-null (and a `language_change` event must exist) before real data collection. In research mode the language is locked to DE.
- Improvement is an indication only: fixed scene order confounds difficulty with improvement, the Concept quiz has a 33 % guess rate, and there is no control group.
- A session that ended in a crash has its last visit cut at the last event (`duration_censored`).
- Keep `object_id` stable: package GameObject names in scenes must not change between study waves.

## 8. Implementation outline (to become backlog items)

1. `ResearchLogger` core (#26, done): session, header, JSONL/CSV writers with per-line flush, error capture, crash recovery, test IDs, R analysis on fake and real data. No scene changes.
2. Participant ID keypad + confirmation + end screen in WelcomeScene.
3. Outcome events: shared error-code helper + hooks in the four validators and the quiz; summary CSV.
4. Interaction events: `GrabLogger` on package prefabs, counter enter/exit, button touches, input mode, tracking.
5. Attention proxy + pose stream + perf samples.
6. Retrieval guide for facilitators (MQDH/adb) and the merge of the wide file with the paper data by `participant_id`.

## 9. Open decisions (need the study lead)

- ~~Is there a between-group variable, and are repeat runs planned?~~ Decided 2026-11-01: no between-group variable; each participant runs the course once.
- ~~Who types the ID: facilitator before handing over (default), or the trainee?~~ Decided 2026-11-01: the facilitator types it before handing over the headset.
- ~~Should trainees be able to skip scenes freely during the study, or should the menu be locked to the build order?~~ Decided 2026-11-01: the menu is locked to the build order during the study.
- ~~Pose stream at 10 Hz and hand positions: needed for the research questions, or is head pose enough?~~ Decided 2026-11-01: head pose only, no hand positions.
- ~~Retention: how long logs stay on the headset; who deletes them after transfer.~~ Decided 2026-11-01: the facilitator deletes the logs on the headset right after copying them and checking that all files are complete.
