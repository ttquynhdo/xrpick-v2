# Research data logging – specification (draft)

Status: draft for review, 2026-11-01. Nothing is implemented yet.

Goal: record, per learner and per session, how trainees perform and experience the VR order-picking course on Meta Quest 3, so the logs can later be analysed and joined with the paper questionnaires (SSQ, SUS/UEQ, NASA-TLX, IPQ, knowledge test) via a pseudonymous participant ID.

## 1. Principles

- **Pseudonymous.** The headset never stores names or other personal data. The only person key is `participant_id` (e.g. `P017`), printed on the paper questionnaire set. The name ↔ ID list stays on paper with the study lead (DSGVO/GDPR; signed consent before the session).
- **Offline, append-only.** Every event is written to disk immediately (flush per line), so a crash keeps everything up to the crash. No network.
- **One logger, many emitters.** A single `ResearchLogger` (DontDestroyOnLoad, created in WelcomeScene) owns the files; scene scripts only call `ResearchLogger.Log(eventType, payload)`. Scripts must work unchanged if the logger is absent (null-safe static API), so scenes stay testable on their own.
- **Logging never changes training behaviour.** It observes; it doesn't alter feedback, timing or scene flow.
- **Versioned schema.** Every file header carries `schema_version`; adding fields is fine, renaming/removing bumps the version.

## 2. Participant ID entry and session lifecycle

1. Facilitator puts the headset on the table, starts the app → WelcomeScene shows a **numeric keypad** panel (digits, delete, confirm). ID format `P` + 3 digits; the `P` is fixed, only digits are typed.
2. On confirm: a second panel shows `P017 – correct?` with *Yes* / *Back*. Only *Yes* starts the session. Before that, the rest of WelcomeScene is hidden/disabled.
3. Optional fields on the same flow (only if the study needs them): `condition` (A/B), `run` (1 = first attempt, 2 = repeat). Defaults: `condition = none`, `run` derived from existing files for that ID.
4. Session starts → logger creates files, writes the header, emits `session_start`.
5. Session ends when the trainee reaches the end (ComplexScene correct + congratulations), the facilitator uses a hidden *End session* control, or the app quits/pauses for good. Emits `session_end` with the reason. The end screen shows the participant ID again so the facilitator can check it against the paper form and write down the end time.
6. A **practice/demo mode** (ID `P000`) is logged to a separate folder and excluded from analysis.

## 3. Files

Location: `Application.persistentDataPath/research/` → on Quest `/sdcard/Android/data/<package>/files/research/`. Retrieve with Meta Quest Developer Hub or `adb pull`.

| File | Content |
|---|---|
| `<participant_id>_<session_id>_events.jsonl` | One JSON object per line, every event in time order |
| `<participant_id>_<session_id>_pose.csv` | Head (+ hands/controllers) samples at 10 Hz |
| `<participant_id>_<session_id>_summary.csv` | One row per scene visit, written when the scene is left; ready for SPSS/R/Excel |

`session_id` is a UUID generated at session start. File names never contain anything but the IDs.

### Common fields on every event

```json
{"t": 123.456, "utc": "2026-11-01T09:14:03.120Z", "participant_id": "P017", "session_id": "…",
 "scene": "ComplexScene", "scene_visit": 2, "event": "confirm_pressed", "data": { … }}
```

- `t` – seconds since session start (`Time.realtimeSinceStartup` offset; monotonic, use for durations).
- `utc` – wall-clock time, only for matching with paper forms.
- `scene_visit` – counter per scene, so revisiting a scene is distinguishable.

### Header (first line of the events file)

`schema_version`, `app_version`, `build` (git short hash baked at build), `device_model`, `os_version`, `participant_id`, `session_id`, `condition`, `run`, `language` (DE/EN), `input_mode_at_start` (hands/controllers), `target_refresh_rate`.

## 4. Event catalogue

### 4.1 Session and navigation

| Event | Data | Source |
|---|---|---|
| `session_start` / `session_end` | `reason`: completed / facilitator / quit / crash-recovered | logger |
| `scene_enter` / `scene_exit` | `from_scene`, `via`: menu / auto / start | `SceneManager.sceneLoaded`, `SceneMenuController` |
| `menu_select` | `target_scene`, `item_label` | `SceneMenuController.Open*` |
| `app_pause` / `app_resume` | `cause`: focus_lost (system menu) / hmd_unmounted / pause | `OVRManager.InputFocusLost/Acquired`, `HMDUnmounted/Mounted`, `OnApplicationPause` |

### 4.2 Learning outcomes (one event per confirm press)

| Event | Data | Source |
|---|---|---|
| `confirm_pressed` | `attempt_no` (per scene visit), `container`: counts per type (`gg`, `vpe`, `gga`, `ggb`, `loose`, `total_pieces`), `required`: the same keys, `correct` (bool), `error_codes` (list, see 4.3), `time_since_scene_enter` | `PackageCounter.CheckOrder`, `OrderConfirm.OrderConfirmation`, `MixedPickingValidator.ConfirmOrder`, `PiecesOrderValidator.ConfirmOrder` |
| `quiz_answer` | `selected`: gg / vpe / both / none, `correct`, `attempt_no` | `PackageQuizController.CheckAnswer` |
| `task_completed` | `attempts_to_correct`, `time_to_first_confirm`, `time_to_correct` | validators (first correct confirm per visit) |

BasicPickingScene currently runs two validators on one button (backlog #1); until that is fixed, log both results in the same `confirm_pressed` (`rule_package_counter`, `rule_calc_for_wms`).

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

Validators get one shared helper that computes these codes; the visible feedback text can then be built from the same codes (backlog #14).

### 4.4 Interaction process

| Event | Data | Source |
|---|---|---|
| `grab_start` / `grab_end` | `object_id` (stable name), `package_type`, `hand` (L/R), `input` (hand/controller), `duration` (on end), `end_pos` | Interaction SDK grabbable `WhenPointerEventRaised` (Select/Unselect), via a small `GrabLogger` component on each package prefab |
| `object_dropped_floor` | `object_id`, `fall_from_height` | collision with floor layer |
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
| `unity_error` | `type` (Error/Exception/Assert), `message`, first 5 stack lines | `Application.logMessageReceived` (Warnings and the known OVRPlugin XR noise filtered by a list) |
| `crash_recovered` | `last_event_t` | on next start, previous session file without `session_end` |

### 4.7 Pose stream (`_pose.csv`)

10 Hz: `t, scene, head_px, head_py, head_pz, head_rx, head_ry, head_rz, head_rw, lh_px…, rh_px…, lh_tracked, rh_tracked`. Enables path length, head movement (comfort), reach analysis. ~100 KB per minute; acceptable.

## 5. Summary CSV (one row per scene visit)

`participant_id, session_id, condition, run, scene, scene_visit, enter_utc, duration_s, confirm_count, first_attempt_correct, attempts_to_correct, time_to_first_confirm_s, time_to_correct_s, completed, error_codes_all (semicolon-separated counts), grabs, drops_floor, container_enters, container_exits, dwell_wms_s, dwell_instructions_s, pauses, hmd_removed_count, fps_avg, dropped_frames, unity_errors`

## 6. Derived measures for analysis (not logged, computed later)

Task time, first-attempt accuracy, attempts to correct, error profile per scene, hesitation (enter→exit of the same object), re-grabs, time spent reading WMS before first grab, head movement per minute, frame drops vs SSQ, learning curve across scenes, pre/post knowledge test gain.

## 7. Dependencies and validity notes

- **Feedback reveals the answer** in MixedPickingScene and ComplexScene (failure text prints targets) and ComplexScene's counters show `/ 2`, `/ 55` (backlog #14, #15). `attempts_to_correct` then measures trial-and-error, not calculation. Fix before real data collection, or record which build each participant used (`build` in the header) and analyse separately.
- BasicPickingScene's two disagreeing validators (backlog #1) make its `correct` flag ambiguous until fixed.
- Language: if DE/EN switching lands (#11), log `language` in the header and on change.
- Keep `object_id` stable: package GameObject names in scenes must not change between study waves.

## 8. Implementation outline (to become backlog items)

1. `ResearchLogger` core: session, header, JSONL/CSV writers with per-line flush, error capture, crash recovery, practice mode. No scene changes.
2. Participant ID keypad + confirmation + end screen in WelcomeScene.
3. Outcome events: shared error-code helper + hooks in the four validators and the quiz; summary CSV.
4. Interaction events: `GrabLogger` on package prefabs, counter enter/exit, button touches, input mode, tracking.
5. Attention proxy + pose stream + perf samples.
6. Retrieval guide for facilitators (MQDH/adb) and a small R/Python script that merges `summary.csv` files with the paper data by `participant_id`.

## 9. Open decisions (need the study lead)

- ~~Is there a between-group condition, and are repeat runs planned?~~ Decided 2026-11-01: no between-group condition; each participant runs the course once.
- ~~Who types the ID: facilitator before handing over (default), or the trainee?~~ Decided 2026-11-01: the facilitator types it before handing over the headset.
- ~~Should trainees be able to skip scenes freely during the study, or should the menu be locked to the build order?~~ Decided 2026-11-01: the menu is locked to the build order during the study.
- ~~Pose stream at 10 Hz and hand positions: needed for the research questions, or is head pose enough?~~ Decided 2026-11-01: head pose only, no hand positions.
- ~~Retention: how long logs stay on the headset; who deletes them after transfer.~~ Decided 2026-11-01: the facilitator deletes the logs on the headset right after copying them and checking that all files are complete.
