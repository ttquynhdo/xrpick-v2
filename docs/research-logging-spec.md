# Research data logging – specification (draft)

Status: v2 per decision 021 (2026-11-04), `schema_version` 2. The session core (sections 1, 3, 4.1, `unity_error` and `crash_recovered` in 4.6, 5) is implemented by backlog #26; the participant keypad, resume offer and the `session_start` fields (section 2, 4.1) by #27; the outcome events of 4.2/4.3 and the outcome columns of section 5 by #28 step 2; the interaction events of 4.1 (`menu_select`, `end_control_*`, `scene_enter.via`) and 4.4 by #29 (header `event_groups` `session`, `outcome`, `interaction`); there are no pose or perf data in the app (decision 022, #30 dropped: nothing extra is installed on the headset, smoothness is checked by eye in the pilot session, and no hand, tracking or floor-drop data (decision 022, #74 dropped) (the v1 catalogue of 2026-11-01 is replaced; decision: `docs/decisions/021-research-log-catalogue-v2.md`). Design and decisions: `docs/superpowers/specs/2026-11-01-research-logger-design.md`.

Goal: record, per learner and per session, how trainees perform and experience the VR order-picking course on Meta Quest 3, so the logs can later be analysed and joined with the paper questionnaires (SSQ before and after, NASA-TLX at the end) via a pseudonymous participant ID. There is no paper test of the course content and no control group, so improvement is read from error rates per skill across scenes (an indication, not proof).

## 1. Principles

- **Pseudonymous.** The headset never stores names or other personal data. The only person key is `participant_id` (e.g. `P017`), printed on the paper questionnaire set. The name ↔ ID list stays on paper with the study lead (DSGVO/GDPR; signed consent before the session).
- **Offline, append-only.** Every event is written to disk immediately (flush per line), so a crash keeps everything up to the crash. No network.
- **One logger, many emitters.** A single `ResearchLogger` (DontDestroyOnLoad, created at app start by `RuntimeInitializeOnLoadMethod`) owns the files; scene scripts only call `ResearchLogger.Log(eventType, payload)` (one way: the logger never calls scene code). Scripts must work unchanged if the logger is absent (null-safe static API), so scenes stay testable on their own.
- **Logging never changes training behaviour.** It observes; it doesn't alter feedback, timing or scene flow.
- **Versioned schema.** Every file header carries `schema_version` (now 2: one bump for the whole v2 catalogue of #28–#30); adding fields is fine, renaming/removing bumps the version.

## 2. Participant ID entry and session lifecycle

1. Facilitator puts the headset on the table, starts the app → WelcomeScene shows a **numeric keypad** panel (digits, delete, confirm). ID format `P` + 3 digits; the `P` is fixed, only digits are typed.
2. On confirm: a second panel shows `P017 – correct?` with *Yes* / *Back*. Only *Yes* starts the session. Before that, the rest of WelcomeScene is hidden/disabled.
3. There is no between-group variable and each participant runs the course once, so the header has no group or run field. A restart after a crash gets a new `session_id` under the same `participant_id`; `ResearchLogger.TryFindResumePoint(participantId, out scene)` tells whether that ID's newest earlier session ended in a crash and which scene it was in.
4. Session starts → logger creates files, writes the header, emits `session_start`. Since #27 the keypad starts it; the logger starts a session itself (ID from the `ResearchSettings` asset, default `P000`) only when the app starts in a scene other than WelcomeScene (build index 0), i.e. in the Editor.
5. Session ends when the trainee reaches the end (ComplexScene correct + congratulations), the facilitator uses a hidden *End session* control, or the app quits/pauses for good. Emits `session_end` with the reason (`completed`, `facilitator`, `quit`). A crash is never a reason: it is a file without `session_end`. The end screen shows the participant ID again so the facilitator can check it against the paper form and write down the end time.
6. **Test IDs** `P000` and `P900`-`P999` are logged to `research/practice/` and excluded from analysis (`analyse.R` also drops them if they appear elsewhere). **Pilot IDs** `P800`-`P899` (decision 023, #75) run exactly like study participants (Menu lock, resume, "already finished" warning) but keep the language chosen on the keypad; they are logged to `research/pilot/` and analysed as a separate set (`output/pilot/`), never mixed with the study. Study participants use `P001`-`P799`. The log format is the same for all three groups; R tells them apart by ID range.

## 3. Files

Location: `Application.persistentDataPath/research/` → on Quest `/sdcard/Android/data/<package>/files/research/`. Retrieve with Meta Quest Developer Hub or `adb pull`.

| File | Content |
|---|---|
| `<participant_id>_<session_id>_events.jsonl` | One JSON object per line, every event in time order |
| `<participant_id>_<session_id>_pose.csv` | Head pose samples at 10 Hz (planned, #30). Head-direction zones are assigned from it in R |
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
- Every time or length field carries its unit (`_s`, `_m`) and is active time (pauses subtracted) unless named `_wall_s`. A key that doesn't apply is `null`, not 0.

### Header (first line of the events file)

The first line has `"event":"header"` (so R can recognise it), `t` 0, and `data`: `schema_version` (2), `layout_version` (string; `"019"` = the three-lane station of decision 019; `"025"` = the same station in Basic/Mixed/Complex plus the ConceptScene of decisions 024/025 (three lanes in Concept, tour with „Zurück“, #80); `"029"` = plus the ConceptScene containers of decision 030 (#91/#100: the sample LVS Ist counts the order container, Rest fixed 120, `concept_hint_*` events; „Zurück“ as in `"025"`); R assigns head-direction zones per layout; ConceptScene data of different layouts are never pooled (decision 024): when a participant set holds more than one `layout_version`, `analyse.R` warns, sets `mixed_layout` in `data_quality.csv` and splits every ConceptScene figure and table (tour, quiz, `items.csv`, time to correct) by `layout_version`; a log with `"019"` or no `layout_version` has no „Zurück“, so its Back counts are NA, not 0, #82), `event_groups` (array of `"session"`, `"outcome"`, `"interaction"`, `"pose"`, `"perf"`: the groups this build emits; R treats a missing group as NA, not 0), `app_version`, `build` (7-digit git short hash, plus `-dirty` with uncommitted changes, baked at build by `git describe --always --dirty --abbrev=7 --exclude=*`; `"editor"` in Play mode; `"unknown"` when git failed at build time; `null` when the `ResearchBuildInfo` resource is missing from the build), `device_model`, `os_version`, `participant_id`, `session_id`, `session_start_mode` (`auto` = started by `ResearchSettings.autoStart`, `manual` = `StartSession` called by other code, e.g. the #27 keypad; a real participant's session should be `manual`), `language` (`DE` / `EN`, #11; fixed for the whole session: chosen on the WelcomeScene keypad before the code is entered, research IDs always `DE`; `null` only in logs written before #11), `input_mode_at_start` (`hands` / `controllers` / `none`; `none` when no input is active yet, always in the Editor without a headset; `null` when the Meta SDK can't be read), `target_refresh_rate` (`null` without a headset).

Group `interaction` (#29) = `grab_start`, `grab_end`, `container_enter`, `container_exit`, `menu_select`, `end_control_opened`, `end_control_cancelled`, `concept_hint_shown`, `concept_hint_cleared` (layout `"029"` on, #100), and `scene_enter.via` after the first entry. A session without it has NA for all of these (summary columns empty).

## 4. Event catalogue

### 4.1 Session and navigation

Emitted by #26 unless noted.

| Event | Data | Source |
|---|---|---|
| `session_start` / `session_end` | `session_start` (#27): `resume_offered_scene` = build scene name the keypad offered to resume at (from `TryFindResumePoint`, i.e. the newest earlier session of this ID crashed in that scene), `null` when nothing was offered; `resume_accepted` = `true` / `false` for the answer, `null` exactly when `resume_offered_scene` is `null` (so "declined" and "never asked" stay apart). Logs from before #27 lack both keys (R: NA, not measured). A crash while WelcomeScene was the last scene offers no resume (WelcomeScene is never a resume point). `ended_session_warning` (#27 fix wave) = `true` when the keypad showed the "already finished" warning (a real ID with an earlier session that has `session_end`) and the facilitator pressed `Trotzdem starten`, otherwise `false` (also for `auto` starts); logs written before it lack the key (R: NA). Additive. `session_end`: `reason` completed / facilitator / quit. `scene_visit` is `null` on both | logger |
| `scene_enter` | `from_scene` (`null` on first entry), `via`: `start` (first entry of a session) \| `menu` (`SceneMenuController.Open*`) \| `resume` (keypad resume, `SceneMenuController.OpenScene`) \| `null` (any other load; none in the app). There is no `facilitator_end`: `Beenden` ends the session (`session_end.reason` `facilitator`) before WelcomeScene loads, so no `scene_enter` follows; the next session starts with `start`. Logs before #29: `null` after the first entry | `SceneManager.activeSceneChanged` |
| `scene_exit` | `duration_s` (active), `paused_s`, `pauses`, `hmd_removed_count`, `unity_errors` (all `unity_error` lines of the visit incl. folded and post-cap), so every summary column #26 fills can be rebuilt from events | logger |
| `menu_select` | `target_scene`, `blocked` (bool, #42 lock; locked touches are caught by `LockedMenuItemRelay`, added at runtime by `MenuCurrentLesson`; a blocked touch loads nothing) (#29). Written in the visit being left, before the load. Test IDs have no locked items, so `blocked: true` only occurs for research IDs. No `item_label` | `SceneMenuController.Open*` |
| `end_control_opened` / `end_control_cancelled` | `{}` (facilitator end control, #44: the 3 s hold opened the confirmation / `Fortsetzen` was pressed) | `FacilitatorEndControl` |
| `app_pause` | `causes`: list of `focus_lost` / `pause` / `hmd_unmounted` | logger |
| `app_resume` | `paused_s`, `causes`: union of the causes seen during this pause | logger |

Pause handling: four internal sources are tracked separately (app focus, OVR input focus, `OnApplicationPause`, HMD mounted) and mapped to the three labels. `app_pause` is written when the state goes from active to paused, `app_resume` when every source is active again, so overlapping signals never double-count. `pauses` counts these transitions, `hmd_removed_count` those whose causes include `hmd_unmounted`. A pause that spans a scene change counts in the visit where it started (its `paused_s` is split by visit); a pause still open at a crash ends at the file's last event. On quit during a pause no `app_resume` is written; the open part is included in `scene_exit.paused_s`.

### 4.2 Learning outcomes and process milestones (#28 step 2)

Positions: Basic 1 position, Mixed 2, Complex 2 (decisions 012, 016); Concept has none. Orders: Basic 1 position `training_b` 300/50; Mixed pos 1 `connector_a` 150/50, pos 2 `screw_b` 60/20; Complex pos 1 `ring_d` 55/25, pos 2 `training_b` 156/50. Only Complex counts loose pieces. Item keys are material keys: `connector_a`, `screw_b`, `ring_d`, `training_b`.

| Event | Data | Source |
|---|---|---|
| `confirm_pressed` | One per **rated** press only, written before the position advances. `attempt_no` (per scene visit; a counter separate from `quiz_answer.attempt_no`), `position`, `position_count`, `attempt_in_position`, `line`: `{item, soll, vpe, required_full, required_loose, placed_full, placed_loose}` (current position only; `required_loose` / `placed_loose` are `null` outside Complex), `foreign`: `[{item, count}]` (materials not in the order, `[]` if none; `count` = full cartons + loose pieces of that material, from `ForeignMaterial.Full + Loose`), `correct` (bool), `error_codes` (list of `{code, item}`, see 4.3), `time_since_scene_enter_s` | `OrderConfirm.OrderConfirmation`, `MixedPickingValidator.ConfirmOrder`, `PiecesOrderValidator.ConfirmOrder` |
| `confirm_ignored` | `reason`: `station_moving` \| `completed` \| `quiz_first` (the early returns, incl. the #58 reminder), `position` (the order's current position: for `station_moving` already the **new** position, because the logic advances before the boxes ride), `time_since_scene_enter_s`. Not an attempt, not in `confirm_count` | the same validators |
| `position_completed` | `position`, `attempts_in_position` (from `LineAdvanced`, and once for the last position at `OrderCompleted`) | validators |
| `task_completed` | `attempts_to_correct` (Basic/Mixed: number of `confirm_pressed`; Complex: number of `confirm_pressed` until completion over positions 1 and 2, remainder-quiz touches excluded; Concept: number of `quiz_answer`; always equals `confirm_count` of the visit), `time_to_first_confirm_s`, `time_to_correct_s`. Picking scenes: at `OrderCompleted`; Concept: when all 4 questions are right (`LessonCompleted`). Complex then calls `EndSession("completed")` | validators, `PackageQuizController` |
| `quiz_answer` | `attempt_no` (per visit, separate counter from `confirm_pressed.attempt_no`), `quiz`: `concept` \| `complex_remainder`, `question_id`, `selected` (answer key), `correct`, `attempt_in_question`, `error_codes`, `time_since_scene_enter_s`. Every rated touch (a touched toggle is rated at once, decision 020). In builds from #78 on (after commit 39f8c3c, which also logs layout `"025"` but never left the developer machine, so no study or pilot data exists from it), the Concept quiz (decision 024) and, from #85 on, the Complex remainder quiz (the Complex builds between #78 and #85 also log "025" without the hold, but none of them left the developer machine either, so all study and pilot "025" data has the hold in both quizzes) keep „Richtig!“ under a correctly answered question (not the last) for 1.5 s with the toggles not interactable: touches in that time are not rated and write no `quiz_answer` | `PackageQuizController.AnswerChosen`, `RemainderQuizController.AnswerChosen` |
| `quiz_unlocked` | `{}` (Concept quiz opens at tour step `material_flow`) | `PackageQuizController.Unlocked` |
| `tour_step` | `step_id`, `step_index` (1–11; „Von vorn“ = back to 1; from layout `"025"` „Zurück“ = `step_index` one lower than the previous `tour_step`, so R derives the direction: +1 Next, −1 Back, 11→1 „Von vorn“; „Zurück“ at step 1 does nothing and logs nothing; R takes the direction from consecutive `tour_step` lines of one `scene_visit` in file line order, not `t`, „Von vorn“ = Δ −(steps − 1), #82) | `WmsFieldTour.ShowStep` (`ResearchLogger.LogTourStep`) |
| `station_move_start` / `station_move_arrived` | `what`: `storage_out` \| `storage_in` \| `order_box`, `position` (the position just completed for `storage_out` / `order_box`, the new one for `storage_in`; Complex box 1 arrives after the remainder quiz) | `PositionStockSwitcher`, `OrderBoxConveyor` (new events, logging only) |
| `welcome_practice_done` | `action`: `grab` \| `button` (first time each, #39) | `WelcomePracticeFeedback` |

Order rule: `confirm_pressed` → `position_completed` → `station_move_start` → `station_move_arrived`. Complex has no last-position `storage_out` / `order_box` rides in the log: `EndSession("completed")` runs at `OrderCompleted`, and events after session end are dropped.

Question ids: Concept `cartons_from_soll` {`2`, `4` (right), `25`}, `material_by_matnr` {`connector_a`, `screw_b` (right), `ring_d`}, `ekz_003` {`ordered` (right), `poured`}, `ekz_000` {`ordered`, `poured` (right)}; Complex position-1 remainder quiz `cartons` {`"1"`, `"2"` (right), `"3"`}, `loose` {`"5"` (right), `"20"`, `"55"`}. The Concept tour has 11 steps and unlocks the quiz at step id `material_flow`.

Basic logs the result of `OrderConfirm` as one position (`position_count` 1); `EKZ_PLACEMENT` is added after the `OrderRule` codes. The exact shapes are fixed in section 6.

### 4.3 Error codes

Derived from container contents vs. order, independent of the text shown to the trainee. Eight codes are logged:

| Code | Meaning |
|---|---|
| `EMPTY` | confirm with nothing of the **current position** in the box (at position > 1, packages of finished positions don't count) |
| `WRONG_MATERIAL` | a material (`item` = material key) not in the order, or not of the current position, is inside (packages of finished positions don't count) |
| `TOO_FEW` / `TOO_MANY` | full cartons short/over for the ordered `item` |
| `MISSING_LOOSE` / `EXTRA_LOOSE` | loose pieces short/over (Complex only) |
| `WRONG_ANSWER` | quiz touch rated wrong (Concept, Complex remainder quiz); `item` is `null`, the question is in `question_id` of the event |
| `EKZ_PLACEMENT` | Basic: cartons not lying ordered in the 6 snap places (EKZ 3, #41); always after the `OrderRule` codes |

`WRONG_TYPE` and `NO_ANSWER` are reserved in the enum and never logged (no package types, decisions 006/007; a quiz touch is always an answer). `EMPTY` is logged alone, without an extra `TOO_FEW`.

**Skill mapping** is done in R, not in the app, so the study lead can edit it without a rebuild. Three files in `analysis/`:

- `skill_map.csv` (`error_code, skill, description`): skill of each error code (`WRONG_MATERIAL` → `check_material`, `TOO_FEW` / `TOO_MANY` → `calculate_quantity`, `MISSING_LOOSE` / `EXTRA_LOOSE` → `calculate_loose`, `EKZ_PLACEMENT` → `ekz_placement`). `WRONG_ANSWER` has no fixed skill: it comes from `item_skills.csv` by `question_id`.
- `scene_skills.csv` (`scene, skill`): which scene tests which skill (Concept `calculate_quantity`, `check_material`, `ekz_placement`; Basic `calculate_quantity`, `ekz_placement`; Mixed `calculate_quantity`; Complex `calculate_quantity`, `calculate_loose`). Mixed and Complex don't test `check_material`: the storage box holds only the current position's material (D12, #71).
- `item_skills.csv` (new, `analysis/item_skills.csv`; columns `scene,item,skill,scaffolded`): `item` is a `question_id` or `position_<n>`; `scaffolded` is `true` where the app hands the trainee the numbers (Complex `position_1`, via the remainder quiz) and `false` at `position_2`. Initial content is pending the study lead (D28, D29).

Main Q1 measure: share correct at the **first attempt per item** (question / position). The error rate per press is secondary (the A/B/C quiz has a 33 % guess rate).

### 4.4 Interaction process (#29)

| Event | Data | Source |
|---|---|---|
| `grab_start` | `object_id` (hierarchy path of the GameObject carrying `PackageData`, e.g. `Interactables/ring (3)`, Basic `GG_Package (25)/SM_box_1`; taken when the scene loads, unique per scene), `material_key`, `hand` (`L`/`R`), `input` (`hand`/`controller`); `hand` and `input` are always `null` (decision 022, #74 dropped) | `PackageGrabLogger`, created at runtime per loaded scene, subscribes to every `Grabbable` with `PackageData` (no component in the Inspector), `WhenPointerEventRaised`: one grab per package, first selecting pointer → `grab_start` |
| `grab_end` | `object_id`, `material_key`, `hand`, `input` (as above), `duration_s` (active seconds since this object's `grab_start` in the same visit, `null` when that start is not in this visit) | the same, last pointer released (Unselect or Cancel). A `grab_start` without `grab_end` (crash or scene change while holding) is an open grab. WelcomeScene logs no grabs (its box has no `PackageData`) |
| `container_enter` / `container_exit` | `container`: `order`, `object_id`, `material_key`, `count_after` (packages in the order container after the change: every package the counter holds, finished positions included, each loose piece 1, the opened display carton never; Basic: cartons in the trigger. Not the WMS Ist) | counters' `OnTriggerEnter/Exit` via `OrderContainerLog`, only when the set actually changes. From `OrderCompleted` on (container shipped, `OrderBoxConveyor.IsShipped`) no `container_*` is written |
| `concept_hint_shown` / `concept_hint_cleared` (#100, decision 030, layout `"029"` on) | `container`: `order` \| `storage`, `reason`: `WRONG_MATERIAL` \| `TOO_MANY`, `object_id` (as `grab_start`), `material_key`, `tour_step_id` (`WmsFieldTour.CurrentStepId`; `null` before the tour bar opens) | ConceptScene only: `ConceptBoxHints`. A yellow tag appeared beside a package that has lain in a container, not held, for 0.5 s (`shown`), or went away (`cleared`: taken out, picked up, or another package's hint replaced it — then `cleared` of the old one comes right before `shown` of the new one). One tag at a time, so shown/cleared alternate per visit; a `shown` without `cleared` = the scene was left while it was shown. Order container: another material (opened carton and loose pieces included) = `WRONG_MATERIAL`, a STECKER C carton that brings the sample Ist above Soll = `TOO_MANY`; storage container: another material = `WRONG_MATERIAL` (an exercise rule, assumption D12). Not an attempt: no `quiz_answer`, no `confirm_*`, no change to the summary outcome columns; Concept still writes no `container_*`. A tag can also start or end without the trainee touching anything: `TOO_MANY` appears on a STECKER C carton put in before step `cartons` once the demo's third carton shows (and again at `material_flow`, where all three show at once), and goes away when the demo cartons hide (another step); R tells these apart by the absence of a `grab_start` of that `object_id` while the tag is shown. At tour step `material_flow` the lane demo waits (from its 4 s start delay on) while a tag is shown or a package of another material lies in a container not yet settled (< 0.5 s), so a long `shown` → `cleared` there is waiting time; the wait itself is not logged (no `station_move_*` in Concept) |
| `object_dropped_floor` (#74, **not implemented, decision 022**) | `object_id`, `fall_from_height_m` | collision with floor layer |
| `input_mode_change` (#74, **not implemented, decision 022**) | `from`, `to` | `OVRInput.GetActiveController` change |
| `tracking_lost` / `tracking_regained` (#74, **not implemented, decision 022**) | `what`: `hand_L` / `hand_R` / `controller` | OVR hand/controller tracking state |

Removed from v1: `button_touch`, `info_shown`, `end_pos`; there is no `step_changed` / `first_grab` (R rebuilds the steps from `grab_start` + `position_completed`). `hand` stays `null`: #74 is dropped (decision 022).

### 4.5 Attention proxy (Quest 3 has no eye tracking)

**Not implemented (decision 022, 2026-11-04):** the app writes no `_pose.csv`, so there are no head-direction zones. The text below is kept as the design if the study lead revives #30.

There is no `gaze_target_*` event. Head-direction zones are assigned in R from `_pose.csv` per `layout_version` (an angle table per layout; the 200 ms minimum dwell is an R parameter). Picking scenes: `screen_column` (WMS + quiz + Beispiel), `shelf`, `desk_left`, `desk_right`, `confirm_area` (red button + result card), `menu`. Concept: `tour`, `ekz`, `flow`, `labelboard`. There is no `order_box` / `storage_box` zone. This is **head direction**, not eye gaze – state it as such in any publication, and treat it as "reading" only after the headset check of #30.

### 4.6 Comfort and technical

| Event / data | Data | Source |
|---|---|---|
| `perf_sample` (every 5 s, #30; **not implemented, decision 022**: there are no FPS or dropped-frame data per participant) | `fps_avg`, `frame_ms_p95`, `dropped_frames`, `battery_level`, `cpu_level`/`gpu_level` if available | `FrameTimingManager` / `OVRPlugin.GetAppPerfStats`, `SystemInfo.batteryLevel` |
| `unity_error` | `type` (lowercase `error` / `exception` / `assert`), `message` (rooted file paths cut to the file name), `stack` (first lines; `null` on a line that only reports folded repeats), `folded_count` (repeats of the same message folded in before this line), `n_errors` (how many errors the line stands for: 1 for a first occurrence, `folded_count` + 1 for a line written after the 5 s window, `folded_count` for a line written at visit close that only reports folded repeats; the sum over a visit's lines equals `scene_exit.unity_errors` unless the cap was reached) | `Application.logMessageReceived` (warnings and known OVRPlugin XR start-up noise filtered by a list; the logger's own errors are not logged) |
| `crash_recovered` | `last_event_t`, `recovered_by_session_id`; written into the old file with its old `session_id`, `t`, `scene` and `scene_visit` `null`, `utc` = time of detection | on next start, any earlier events file without `session_end` |

Error spam: the same message within 5 s is folded into one line carrying `folded_count`; folded counts are written before `scene_exit` and before `session_end`. After 200 `unity_error` lines in a session one `unity_error_cap_reached` event is written and further errors are only counted (in `scene_exit.unity_errors`), so for such a session the events can't rebuild `unity_errors`. `app_pause`, `scene_exit` and `session_end` lines are forced to the storage device (fsync), since sleep or power-off may follow them.

Clean end vs crash: `session_end` with reason `quit` is written by Unity's quit callback (also when Play mode stops). `session_end` is always the last line of a clean file. Quest often skips the quit callback (system-menu close, power-off), so those sessions count as crashes. A developer option in `ResearchSettings` skips `session_end` on quit, to test crash recovery.

### 4.7 Pose stream (`_pose.csv`)

**Not implemented (decision 022, 2026-11-04):** no per-frame sampler is added to the app, so the trainee's frame time carries no logging cost. Kept as the design if #30 is revived.

10 Hz, head pose only (no hand positions): `t, scene, head_px, head_py, head_pz, head_rx, head_ry, head_rz, head_rw`. Enables path length, head movement (comfort) and the zone assignment of 4.5. Written through an open stream, flushed and closed on pause, scene change and session end. The sampler (timer/coroutine) lives outside `Research/` (in `Assembly-CSharp`) and calls the logger one way; `Research/` has no per-frame method. Planned, #30. If 10 Hz costs FPS on Quest, drop to 5 Hz and note it here.

## 5. Summary CSV (one row per scene visit)

`participant_id, session_id, build, schema_version, scene, scene_visit, enter_utc, duration_s, paused_s, confirm_count, confirm_ignored, first_attempt_correct, attempts_to_correct, time_to_first_confirm_s, time_to_correct_s, completed, error_codes_all, grabs, drops_floor, container_enters, container_exits, pauses, hmd_removed_count, fps_avg, dropped_frames, unity_errors`

`duration_s` is active time (pauses subtracted), `paused_s` the paused time of the visit. #26 fills identity, `build`, `schema_version`, `enter_utc`, `duration_s`, `paused_s`, `pauses`, `hmd_removed_count`, `unity_errors`; #28 step 2 fills the outcome columns and #29 `grabs`, `container_enters`, `container_exits`; `drops_floor` is always empty (decision 022, #74 dropped); `fps_avg` and `dropped_frames` are always empty (decision 022: no perf data in the app). `error_codes_all` is semicolon-separated `CODE:n` (e.g. `TOO_FEW:2;EMPTY:1`). The events file is the source of truth; `analyse.R` rebuilds the table from events and compares it with `summary.csv` (tolerance 0.002 s for `_s` columns). The v1 dwell columns are gone: R computes dwell in the wide file.

**summary.csv column rules** (what the C# writer computes; step 2 is written from this table). A *counted attempt* is: in ConceptScene a `quiz_answer` (every rated quiz touch); in Basic, Mixed and Complex a `confirm_pressed`. Complex remainder-quiz touches (`quiz` = `complex_remainder`) and every `confirm_ignored` are never counted attempts.

| Column | Covers | Value per scene |
|---|---|---|
| `confirm_count` | counted attempts of the visit | Concept: quiz touches; Basic/Mixed/Complex: rated red-button presses; Welcome: empty |
| `confirm_ignored` | `confirm_ignored` events | Basic/Mixed/Complex: count (0 if none); Welcome and Concept: empty |
| `first_attempt_correct` | the first counted attempt only | `true`/`false`; Concept: the first quiz touch; empty when the visit has no counted attempt, and in Welcome |
| `time_to_first_confirm_s` | the first counted attempt | `time_since_scene_enter_s` of that attempt (Concept: the first rated quiz touch; Complex: the first red-button press, not a remainder-quiz touch); empty when none |
| `error_codes_all` | counted attempts only | `CODE:n` summed over their `error_codes`; Complex: red-button presses only (remainder-quiz `WRONG_ANSWER` not included); empty when none |
| `attempts_to_correct`, `time_to_correct_s` | the visit's `task_completed` | its values; empty when the visit has no `task_completed`; `attempts_to_correct` equals `confirm_count` |
| `completed` | `task_completed` in the visit | `true` if the visit has one, else `false`; Welcome: empty |
| `grabs` | `grab_start` events | Concept, Basic, Mixed, Complex: count (0 if none); Welcome: empty |
| `container_enters`, `container_exits` | `container_enter` / `container_exit` | Basic, Mixed, Complex: count (0 if none); Welcome and Concept: empty. All three #29 columns are empty when the header lacks `interaction` |
| `drops_floor` | `object_dropped_floor` | always empty (decision 022: #74 dropped) |

WelcomeScene leaves every outcome column empty (`confirm_count`, `confirm_ignored`, `first_attempt_correct`, `attempts_to_correct`, `time_to_first_confirm_s`, `time_to_correct_s`, `completed`, `error_codes_all`).

## 6. Contract for outcome events (#28) and analysis

These shapes are fixed now; the fake data in `analysis/` follows them (`analysis/make_fake_data.R`). The in-code validator result type can be shaped freely as long as it maps 1:1 onto this format.

**`confirm_pressed`** (one per rated press; Mixed position 1, 2 of 3 cartons placed):
```json
{"event":"confirm_pressed","data":{
  "attempt_no":1,"position":1,"position_count":2,"attempt_in_position":1,
  "line":{"item":"connector_a","soll":150,"vpe":50,"required_full":3,"required_loose":null,"placed_full":2,"placed_loose":null},
  "foreign":[],
  "correct":false,
  "error_codes":[{"code":"TOO_FEW","item":"connector_a"}],
  "time_since_scene_enter_s":41.2}}
```
A key that doesn't apply in a scene is `null`, not 0. `item` is `null` for codes without an item (`EMPTY`, `WRONG_ANSWER`, `EKZ_PLACEMENT`).

**`quiz_answer`** (Complex position-1 remainder quiz, full cartons answered `"1"` instead of `"2"`):
```json
{"event":"quiz_answer","data":{
  "attempt_no":1,"quiz":"complex_remainder","question_id":"cartons","selected":"1",
  "correct":false,"attempt_in_question":1,
  "error_codes":[{"code":"WRONG_ANSWER","item":null}],
  "time_since_scene_enter_s":22.8}}
```

**`task_completed`** (at order completion; Concept: all 4 questions right): `{"attempts_to_correct":3,"time_to_first_confirm_s":38.0,"time_to_correct_s":95.5}`. Times are active time.

**Invariant:** `correct == (error_codes is empty)`. One shared helper decides both; `analyse.R` reports any row that breaks it.

**Error codes and skills:** section 4.3 (`skill_map.csv` + `scene_skills.csv` + `item_skills.csv`).

**Session end:** #28 calls `ResearchLogger.EndSession("completed")` at `OrderCompleted` in ComplexScene; #44 adds the hidden facilitator control (decision 004 §9) for `EndSession("facilitator")`. `analyse.R` derives `end_status` (completed / facilitator / quit / crash) and `course_finished` (a `task_completed` in ComplexScene) itself.

**Derived measures (computed later, not logged):** task time, share correct at first attempt per item (main) and error rate per skill (secondary, per visit and skill: denominator = counted attempts that test that skill via `item_skills.csv` / `scene_skills.csv` (an order position with no skill row tests every skill its scene lists), numerator = those with at least one error code of that skill, first attempt = the earliest such attempt; scaffolded items (Complex `position_1`) and remainder-quiz touches are excluded; NA, not 0, when the visit has no such attempt), attempts to correct, error profile per scene, hesitation (`grab_start` to `grab_end` and `container_enter` to `container_exit` of the same object), re-grabs, time spent reading WMS before first grab (minus `wait_s`), `wait_s` (time the trainee is blocked by a box change, per `storage_in` ride: from its trigger to that ride's `station_move_arrived`; the trigger is the `position_completed` of the previous position, or for position 1 (Complex, box 1 after the remainder quiz) the last `quiz_answer` before the ride; intervals unioned and cut at `task_completed`. The red button is blocked from the trigger on (`startDelay` 0.6 s before `station_move_start`) until the new box stands at the work spot, while `storage_out` keeps riding after the station is free, so ride intervals alone would miss the first and overcount the second) and `wait_after_done_s` (union of the rides after `task_completed`; R-derived, not logged), Q3 phases per position (active time): start = scene enter (Complex position 1: storage box 1 `station_move_arrived`; later positions: their `storage_in` arrived, so `wait_s` is never inside) → `read_s` to the first `grab_start` → `pick_s` to the last `container_*` before `position_completed` → `check_s` to `position_completed`; the three add up to `position_s`. `facilitator_tail_s` = active time from the last `end_control_opened` to the end of a session ended by the facilitator. Re-grab = `grab_start` of an `object_id` already grabbed in the visit. SSQ change (paper, pre/post) vs active time, pauses and headset removals; NASA-TLX vs attempts and active time. There is no head movement, head-direction dwell or frame-drop measure (decision 022). Which visit counts: the first visit of a scene with at least one rated confirm; later visits are flagged `repeat_visit`.

## 7. Dependencies and validity notes

- **Complex position 1 is scaffolded by the remainder quiz**: the quiz hands the trainee the numbers (full cartons, loose pieces) before the first pick, so position 1 measures picking, not calculation; position 2 is unscaffolded (`item_skills.csv`, `scaffolded`). Don't pool the two positions as one `calculate_quantity` measure without this flag.
- Language (#11): the header's `language` is `DE` or `EN`. The Deutsch / English buttons sit on the WelcomeScene keypad and are hidden once a session runs, so the language cannot change inside a session and there is no `language_change` event. Study IDs (`P001`–`P799`) always run in `DE`: choosing English and then typing a study code switches back to German before the session starts. Pilot IDs (`P800`–`P899`) and test IDs keep the chosen language (decision 023).
- Improvement is an indication only: fixed scene order confounds difficulty with improvement, the Concept quiz has a 33 % guess rate, and there is no control group.
- A session that ended in a crash has its last visit cut at the last event (`duration_censored`).
- Keep `object_id` stable: package GameObject names in scenes must not change between study waves.
- `read_s` is time before the first grab, not proof of reading the WMS (head direction comes with #30); in Complex position 1 the WMS is mostly read during the remainder quiz, before the position starts.

## 8. Implementation outline (to become backlog items)

1. `ResearchLogger` core (#26, done): session, header, JSONL/CSV writers with per-line flush, error capture, crash recovery, test IDs, R analysis on fake and real data. No scene changes. Keypad, resume offer and end control (#27, #44) are done too.
2. #28 step 2 (done): outcome events (shared error-code helper + hooks in the validators, both quizzes, tour, `PositionStockSwitcher` / `OrderBoxConveyor`, `WelcomePracticeFeedback`, `EkzPlacementCheck`) and the summary CSV rules of section 5.
3. #29 (reduced): grab events registered at runtime on `Grabbable` + `PackageData` (no Inspector component), container enter/exit, `menu_select`, `scene_enter.via`, end-control events.
4. #30: pose stream (`_pose.csv`) and `perf_sample`; sampler outside `Research/`, head-direction zones assigned in R.
5. #74: hand and input mode, tracking lost/regained, floor drops. **Dropped (decision 022).**
6. Retrieval guide for facilitators (MQDH/adb) and the merge of the wide file with the paper data by `participant_id`.

## 9. Open decisions (need the study lead)

- ~~Is there a between-group variable, and are repeat runs planned?~~ Decided 2026-11-01: no between-group variable; each participant runs the course once.
- ~~Who types the ID: facilitator before handing over (default), or the trainee?~~ Decided 2026-11-01: the facilitator types it before handing over the headset.
- ~~Should trainees be able to skip scenes freely during the study, or should the menu be locked to the build order?~~ Decided 2026-11-01: the menu is locked to the build order during the study.
- ~~Pose stream at 10 Hz and hand positions: needed for the research questions, or is head pose enough?~~ Decided 2026-11-01: head pose only, no hand positions.
- ~~Retention: how long logs stay on the headset; who deletes them after transfer.~~ Decided 2026-11-01: the facilitator deletes the logs on the headset right after copying them and checking that all files are complete.
- D28: map of question / position to skill (does Complex position 1 count for `calculate_quantity`?); D29: is `ekz_placement` its own skill in Q1 with only 2 measuring points? (decision 021)
