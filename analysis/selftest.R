# selftest.R
#
# Checks that analyse.R handles every edge case in the fake data
# (make_fake_data.R). Run it after any change to analyse.R, make_fake_data.R
# or the logger format.
#
# Run from the repo root:   Rscript analysis/selftest.R
# Expected last line:       selftest OK
#
# Everything runs inside local({ ... }): data_dir, output_dir, allow_fake and
# all tables live in a private environment that disappears afterwards, also
# when an assertion fails. So sourcing this in RStudio never leaves settings
# behind that would send a later real analysis to fake data or the wrong folder.

local({
  # What the global environment holds before the test (checked at the end).
  globals_before <- ls(globalenv())

  # Fake data in, selftest outputs in their own folder, so a real run's
  # outputs in analysis/output/ are never overwritten by the test.
  data_dir <- "analysis/data/fake"
  output_dir <- "analysis/output/selftest"
  allow_fake <- TRUE  # analyse.R refuses fake folders without this

  # 1. Make fresh fake data (same seed every time), then run the analysis,
  #    both in this private environment (local = TRUE).
  source("analysis/make_fake_data.R", local = TRUE)
  source("analysis/analyse.R", local = TRUE)

  # 2. One row per real participant: P101-P115; the test ID P950 is dropped.
  stopifnot(nrow(wide) == 15)
  stopifnot(!"P950" %in% wide$participant_id)
  stopifnot(!"P950" %in% data_quality$participant_id)

  # 3. P103 crashed and came back in a second session (and so did P111, the
  #    declined resume in 8i; P112 quit and was restarted despite the warning, 8j).
  stopifnot(isTRUE(wide$multi_session[wide$participant_id == "P103"]))
  stopifnot(identical(sort(wide$participant_id[wide$multi_session]), c("P103", "P111", "P112")))

  # 4. P105 crashed and never came back.
  stopifnot(wide$end_status[wide$participant_id == "P105"] == "crash")
  stopifnot(isFALSE(wide$course_finished[wide$participant_id == "P105"]))

  # 5. P107 entered ComplexScene but never confirmed: a measured 0 confirms,
  #    and the rate is NA (not NaN, not 0).
  p107 <- visits[visits$participant_id == "P107" & visits$scene == "ComplexScene", ]
  stopifnot(nrow(p107) == 1)
  stopifnot(identical(p107$confirm_count, 0L))
  stopifnot(is.na(p107$error_rate), !is.nan(p107$error_rate))
  p107_wide <- wide$error_rate_ComplexScene[wide$participant_id == "P107"]
  stopifnot(is.na(p107_wide), !is.nan(p107_wide))
  # P107 was ended by the facilitator (#44).
  stopifnot(wide$end_status[wide$participant_id == "P107"] == "facilitator")

  # 5b. A tested skill without errors in a visit gives 0, not NA.
  stopifnot(!anyNA(skill_rates$error_rate))
  error_free <- skill_rates[skill_rates$presses_with_skill_error == 0, ]
  stopifnot(nrow(error_free) > 0, all(error_free$error_rate == 0))

  # 6. Two builds: only P103 mixes them, and the data-quality table shows both.
  stopifnot(identical(wide$participant_id[wide$mixed_build], "P103"))
  stopifnot(grepl("aaa1111", data_quality$builds[data_quality$participant_id == "P103"]))
  stopifnot(grepl("bbb2222", data_quality$builds[data_quality$participant_id == "P103"]))
  stopifnot(all(c("aaa1111", "bbb2222") %in% unlist(strsplit(data_quality$builds, ";"))))

  # 7. Warnings name the unknown code and the invariant break.
  warn_lines <- readLines(file.path(output_dir, "warnings.txt"), encoding = "UTF-8")
  stopifnot(any(grepl("FOO_BAR", warn_lines)))
  stopifnot(any(grepl("P110", warn_lines) & grepl("invariant", warn_lines)))

  # 8. Every output file exists.
  expected_files <- c("error_rate_by_skill.png", "error_rate_by_skill_first_attempt.png",
                      "error_codes_by_scene.png", "time_to_correct_by_scene.png",
                      "data_quality.csv", "data_quality.png", "wide.csv", "warnings.txt")
  missing_files <- expected_files[!file.exists(file.path(output_dir, expected_files))]
  if (length(missing_files) > 0) stop("missing outputs: ", paste(missing_files, collapse = ", "))

  # 8b. Seam checks: the fake data follow the C# rules exactly, so any
  #     "scene_exit disagrees" or "summary.csv differs" warning is a bug in
  #     analyse.R or make_fake_data.R (they stand in for the C# logger).
  seam <- warn_lines[grepl("disagrees|differs", warn_lines)]
  if (length(seam) > 0) stop("seam warnings on fake data:\n", paste(seam, collapse = "\n"))
  stopifnot(all(data_quality$exit_disagreements == 0), all(data_quality$summary_mismatches == 0))

  # 8c. n_errors: P102's 9 s burst in WelcomeScene gives a line after the 5 s
  #     window (n_errors = folded_count + 1) and a line at the visit close
  #     without a stack (n_errors = folded_count).
  p102_err <- error_lines[error_lines$participant_id == "P102" & error_lines$scene == "WelcomeScene", ]
  folded <- map_dbl(p102_err$data, ~ .x$folded_count)
  has_stack <- map_lgl(p102_err$data, ~ !is.null(.x$stack))
  stopifnot(any(has_stack & folded > 0 & p102_err$n_err == folded + 1))
  stopifnot(any(!has_stack & folded > 0 & p102_err$n_err == folded))
  stopifnot(!any(error_lines$legacy))

  # 8m. Log contract v2 (decision 021): first attempt per item, rated vs
  #     ignored presses, completion only at the last position, schema 1.
  warn_lines <- readLines(file.path(output_dir, "warnings.txt"), encoding = "UTF-8")
  items <- read_csv(file.path(output_dir, "items.csv"), show_col_types = FALSE)
  stopifnot(file.exists(file.path(output_dir, "first_attempt_by_item.png")))
  stopifnot(all(c("participant_id", "scene", "item", "skill", "scaffolded",
                  "first_attempt_correct", "attempts") %in% names(items)))
  # Review Focus 4: right only on a later touch counts as first-attempt wrong.
  stopifnot(all(!items$first_attempt_correct[items$attempts > 1]))
  # Review Focus 2: ignored presses never count as attempts.
  stopifnot(all(visits$confirm_count == visits$rated_presses, na.rm = TRUE))
  stopifnot(any(visits$confirm_ignored > 0, na.rm = TRUE))
  # Review Focus 3: Mixed/Complex completed only after the last position.
  # Measured FALSE, not NA: isFALSE fails on a missing value.
  stopifnot(isFALSE(wide$completed_ComplexScene[wide$participant_id == "P114"]))
  # Review Focus 5: no check_material rate for Mixed/Complex; tested skills present.
  stopifnot(!any(skill_rates$scene %in% c("MixedPickingScene", "ComplexScene") & skill_rates$skill == "check_material"))
  stopifnot(all(c("calculate_quantity", "calculate_loose", "ekz_placement") %in% skill_rates$skill))
  # Review Focus 1: the schema-1 file is warned, not fatal; its outcome is NA.
  stopifnot(any(grepl("schema_version 1", warn_lines)))
  stopifnot(any(grepl("schema_version 1", warn_lines) & grepl("P115", warn_lines)))
  stopifnot(is.na(wide$confirms_BasicPickingScene[wide$participant_id == "P115"]),
            is.na(wide$course_finished[wide$participant_id == "P115"]))
  stopifnot(!"P115" %in% items$participant_id, !any(grepl("NO_ANSWER", warn_lines)))
  # WRONG_ANSWER gets its skill from item_skills.csv: never an unknown code.
  stopifnot(!any(grepl("unknown error code WRONG_ANSWER", warn_lines)))
  # P113: Mixed position 2 first press EMPTY, then correct -> wrong first, 2 attempts.
  it <- function(pid, sc, item) items[items$participant_id == pid & items$scene == sc & items$item == item, ]
  p113 <- it("P113", "MixedPickingScene", "position_2")
  stopifnot(nrow(p113) == 1, isFALSE(p113$first_attempt_correct), p113$attempts == 2)
  # P114: Complex position 1 right at once (scaffolded), then a crash: no
  # position_2 item, the visit is not completed but the item is measured.
  p114 <- it("P114", "ComplexScene", "position_1")
  stopifnot(nrow(p114) == 1, isTRUE(p114$first_attempt_correct), isTRUE(p114$scaffolded))
  stopifnot(nrow(it("P114", "ComplexScene", "position_2")) == 0)
  stopifnot(isFALSE(it("P114", "ComplexScene", "cartons")$scaffolded))
  # Quiz items carry the skill of item_skills.csv.
  stopifnot(all(items$skill[items$item == "loose"] == "calculate_loose"),
            all(items$skill[items$item == "material_by_matnr"] == "check_material"))
  # wide.csv: confirm_ignored and station wait per picking scene, not for Concept.
  stopifnot(all(c("confirm_ignored_BasicPickingScene", "confirm_ignored_ComplexScene",
                  "wait_s_MixedPickingScene", "wait_s_ComplexScene") %in% names(wide)))
  stopifnot(!any(c("confirm_ignored_ConceptScene", "wait_s_ConceptScene") %in% names(wide)))
  stopifnot(all(wide$wait_s_MixedPickingScene[!is.na(wide$wait_s_MixedPickingScene)] >= 0))
  # Known value: P114 Mixed (no pauses). The trainee is blocked from
  # position_completed 1 (326.367) until storage_in 2 arrives (330.149):
  # 3.782 s. The 0.6 s startDelay counts; storage_out (326.967 -> 330.148)
  # lies inside it and adds nothing. The end-of-order rides (351.199 ->
  # 355.573, after task_completed at 350.599) are not in wait_s but in
  # wait_after_done_s (union 4.374).
  m114 <- visits[visits$participant_id == "P114" & visits$scene == "MixedPickingScene", ]
  stopifnot(nrow(m114) == 1, abs(m114$wait_s - 3.782) < 0.002,
            abs(m114$wait_after_done_s - 4.374) < 0.002)
  # Known value: P101 Complex (no pauses), two blocked intervals: box 1 from
  # the last remainder-quiz touch (488.838; earlier touches are not the
  # trigger) to storage_in 1 arrived (492.015) = 3.177 s, and
  # position_completed 1 (517.467) to storage_in 2 arrived (521.300) =
  # 3.833 s. Sum 7.010. Complex ends the session at task_completed: after part 0.
  c101 <- visits[visits$participant_id == "P101" & visits$scene == "ComplexScene", ]
  stopifnot(nrow(c101) == 1, abs(c101$wait_s - 7.010) < 0.002,
            abs(c101$wait_after_done_s - 0) < 0.002)
  # Basic has one position and no storage_in ride, so wait_s 0.
  stopifnot(all(visits$wait_s[visits$scene == "BasicPickingScene"] == 0, na.rm = TRUE))
  # Scaffolded Complex position 1 is out of the skill rates: the denominator
  # of every Complex rate = the visit's position-2 presses; P114 (position 1
  # only, then crash) has no Complex skill rate.
  pos2 <- presses %>% filter(scene == "ComplexScene", item == "position_2") %>%
    count(session_id, scene_visit, name = "n_pos2")
  cx <- skill_rates %>% filter(scene == "ComplexScene") %>%
    left_join(pos2, by = c("session_id", "scene_visit"))
  stopifnot(nrow(cx) > 0, all(cx$skill_presses == cx$n_pos2))
  stopifnot(!any(skill_rates$participant_id == "P114" & skill_rates$scene == "ComplexScene"))
  stopifnot(any(cx$skill_presses < visits$confirm_count[match(paste(cx$session_id, "ComplexScene", cx$scene_visit),
                                                                paste(visits$session_id, visits$scene, visits$scene_visit))]))
  stopifnot(all(c("wait_after_done_s_MixedPickingScene") %in% names(wide)),
            !"wait_after_done_s_ConceptScene" %in% names(wide))

  # Per-item skill denominators (final fix wave): a Concept skill's rate counts
  # only touches on its own questions, and its first attempt is the first
  # touch on one of them, not the quiz's first touch (always cartons_from_soll).
  # So Concept check_material equals material_by_matnr in items.csv.
  cq <- presses %>% filter(scene == "ConceptScene", counted) %>%
    count(session_id, scene_visit, item, name = "n_item")
  cm <- skill_rates %>% filter(scene == "ConceptScene", skill == "check_material") %>%
    left_join(cq %>% filter(item == "material_by_matnr"), by = c("session_id", "scene_visit"))
  stopifnot(nrow(cm) > 0, all(cm$skill_presses == cm$n_item))
  mat <- items %>% filter(scene == "ConceptScene", item == "material_by_matnr") %>%
    select(participant_id, session_id, scene_visit, first_attempt_correct)
  cm_mat <- inner_join(cm, mat, by = c("participant_id", "session_id", "scene_visit"))
  stopifnot(nrow(cm_mat) == nrow(cm),
            all(cm_mat$first_attempt_skill_error == !cm_mat$first_attempt_correct))
  # Not structurally 0: someone is wrong at the first material question.
  stopifnot(any(cm$first_attempt_skill_error))
  # ekz_placement in Concept = touches on ekz_003 + ekz_000 only.
  ce <- skill_rates %>% filter(scene == "ConceptScene", skill == "ekz_placement") %>%
    left_join(cq %>% filter(item %in% c("ekz_003", "ekz_000")) %>%
                group_by(session_id, scene_visit) %>% summarise(n_ekz = sum(n_item), .groups = "drop"),
              by = c("session_id", "scene_visit"))
  stopifnot(nrow(ce) > 0, all(ce$skill_presses == ce$n_ekz))
  # Picking scenes: a position without a skill tests every scene skill, so the
  # denominator is every counted press (Basic: both skills, same count).
  bs <- skill_rates %>% filter(scene == "BasicPickingScene")
  stopifnot(nrow(bs) > 0, all(bs$skill_presses == visits$confirm_count[match(
    paste(bs$session_id, "BasicPickingScene", bs$scene_visit),
    paste(visits$session_id, visits$scene, visits$scene_visit))]))

  # Complex attempts_to_correct = red-button presses (confirm_pressed) of the
  # visit; remainder-quiz touches are not counted. If the C# logger counts
  # them, this fails and the rule must be decided, not drifted into.
  cx_tc <- v2_events %>% filter(scene == "ComplexScene", event == "task_completed") %>%
    transmute(session_id, scene_visit, atc = map_int(data, ~ as.integer(.x$attempts_to_correct)))
  cx_cp <- v2_events %>% filter(scene == "ComplexScene", event == "confirm_pressed") %>%
    count(session_id, scene_visit, name = "n_confirm")
  cx_atc <- left_join(cx_tc, cx_cp, by = c("session_id", "scene_visit"))
  stopifnot(nrow(cx_atc) > 0, all(cx_atc$atc == cx_atc$n_confirm))

  # 8n. Interaction events (#29). P104 ran a build before #29 (header
  #     event_groups without "interaction"): every interaction number is NA,
  #     never 0, and its empty summary cells match (seam check in 8b).
  rw <- function(col, pid) wide[[col]][wide$participant_id == pid]
  dq <- function(col, pid) data_quality[[col]][data_quality$participant_id == pid]
  v104 <- visits[visits$participant_id == "P104", ]
  stopifnot(nrow(v104) == 5, !any(v104$interaction_logged))
  stopifnot(all(is.na(v104$grabs)), all(is.na(v104$container_enters)), all(is.na(v104$container_exits)),
            all(is.na(v104$menu_blocked)), all(is.na(v104$end_control_opened)))
  stopifnot(isFALSE(dq("interaction_logged", "P104")), is.na(dq("end_control_opens", "P104")),
            is.na(rw("grabs_BasicPickingScene", "P104")), is.na(rw("menu_blocked", "P104")))
  # Where a scene can't produce the event it is NA, also in measuring sessions:
  # WelcomeScene grabs (its box has no PackageData), container counts in
  # Welcome and Concept (no order container). wide.csv has no such columns.
  vi <- visits[visits$interaction_logged, ]
  stopifnot(all(is.na(vi$grabs[vi$scene == "WelcomeScene"])),
            all(is.na(vi$container_enters[vi$scene %in% c("WelcomeScene", "ConceptScene")])))
  stopifnot(!anyNA(vi$grabs[vi$scene %in% grab_scenes]),
            !anyNA(vi$container_enters[vi$scene %in% container_scenes]))
  stopifnot(!any(c("grabs_WelcomeScene", "container_enters_ConceptScene", "container_exits_WelcomeScene")
                 %in% names(wide)),
            all(c("grabs_ConceptScene", "grabs_ComplexScene", "container_exits_MixedPickingScene",
                  "read_s_MixedPickingScene_p2", "check_s_ComplexScene_p2") %in% names(wide)))
  # Measured 0, not NA: P107 never confirmed in ComplexScene, so nothing went
  # into the order container.
  stopifnot(identical(visits$container_enters[visits$participant_id == "P107" &
                                                         visits$scene == "ComplexScene"], 0L))
  # No contract warnings on the fake data (count_after, via, grab durations).
  stopifnot(!any(grepl("count_after|scene_enter.via|duration_s differ|without a menu_select|lacks \"interaction\"",
                       warn_lines)))
  # Blocked menu touches (#42 lock) are counted.
  stopifnot(sum(data_quality$menu_blocked, na.rm = TRUE) > 0)
  # count_after counts the whole container, finished positions included: just
  # before P113's EMPTY press at Mixed position 2, the 3 cartons of position 1
  # are still in it.
  first_p2 <- presses %>% filter(participant_id == "P113", scene == "MixedPickingScene", position == 2) %>%
    slice_min(t, n = 1)
  last_count <- container_rows %>% filter(participant_id == "P113", scene == "MixedPickingScene",
                                          t < first_p2$t) %>% slice_max(t, n = 1)
  stopifnot(nrow(last_count) == 1, last_count$count_after == 3, isTRUE("EMPTY" %in% first_p2$codes[[1]]))
  # P114 held a package when the app crashed: one open grab, no duration.
  stopifnot(visits$open_grabs[visits$participant_id == "P114" & visits$scene == "ComplexScene"] == 1,
            dq("open_grabs", "P114") == 1)
  # Re-grabs: grabs of an object already grabbed in the visit exist somewhere.
  stopifnot(any(visits$regrabs > 0, na.rm = TRUE))
  # P107: the end control was opened twice (one cancelled), and the 1.5 s
  # from the last open to the session end are the facilitator's.
  stopifnot(dq("end_control_opens", "P107") == 2, abs(dq("facilitator_tail_s", "P107") - 1.5) < 0.002)
  stopifnot(all(is.na(visits$facilitator_tail_s[visits$participant_id != "P107"])))
  # Q3 phases add up to the position's active time and never include the
  # box-change wait (a later position starts when its storage box arrived).
  proc <- read_csv(file.path(output_dir, "process_by_position.csv"), show_col_types = FALSE)
  done <- proc %>% filter(completed, !is.na(pick_s))
  stopifnot(nrow(done) > 0, all(abs(done$read_s + done$pick_s + done$check_s - done$position_s) < 0.003),
            all(done$read_s >= 0), all(done$pick_s >= 0), all(done$check_s >= 0))
  stopifnot(!any(proc$participant_id == "P104"), file.exists(file.path(output_dir, "time_by_phase.png")))
  # P114 Complex position 2 was never completed: no pick/check time, but read_s
  # (box arrived -> the grab that was open at the crash) is measured.
  p114_p2 <- proc %>% filter(participant_id == "P114", scene == "ComplexScene", position == 2)
  stopifnot(nrow(p114_p2) == 1, !p114_p2$completed, !is.na(p114_p2$read_s), is.na(p114_p2$pick_s))

  # 8d. Pauses: P102's pause spans Mixed visit 1 -> Basic visit 2: it counts in
  #     Mixed 1, its time is split (4.5 s + 7.25 s). P104 quits while paused:
  #     the open 12.5 s are in ComplexScene's paused_s without an app_resume.
  b2 <- visits[visits$participant_id == "P102" & visits$scene == "BasicPickingScene" & visits$scene_visit == 2, ]
  stopifnot(nrow(b2) == 1, b2$pauses == 0, abs(b2$paused_s - 7.25) < 0.002)
  m1 <- visits[visits$participant_id == "P102" & visits$scene == "MixedPickingScene" & visits$scene_visit == 1, ]
  stopifnot(nrow(m1) == 1, m1$pauses >= 1, m1$paused_s >= 4.5)
  c104 <- visits[visits$participant_id == "P104" & visits$scene == "ComplexScene", ]
  stopifnot(nrow(c104) == 1, c104$paused_s >= 12.5, c104$pauses >= 1)

  # 8e. Error cap: P107 is flagged and named in the warnings.
  stopifnot(isTRUE(data_quality$error_cap_reached[data_quality$participant_id == "P107"]))
  stopifnot(sum(data_quality$error_cap_reached) == 1, "P107" %in% flagged)
  stopifnot(any(grepl("cap reached", warn_lines) & grepl("P107", warn_lines)))

  # 8f. session_start_mode "auto" for a real ID (P108) is warned; the test ID
  #     P950 (also "auto") is dropped before, so it is not.
  stopifnot(any(grepl("session_start_mode 'auto'", warn_lines) & grepl("P108", warn_lines)))
  stopifnot(!any(grepl("session_start_mode 'auto'", warn_lines) & grepl("P950", warn_lines)))

  # 8g. wide.csv: units in the time columns, no WelcomeScene outcome columns.
  stopifnot(all(c("active_s_ComplexScene", "paused_s_ComplexScene", "time_to_correct_s_ComplexScene",
                  "active_s_WelcomeScene") %in% names(wide)))
  stopifnot(!any(c("time_ComplexScene", "confirms_WelcomeScene", "error_rate_WelcomeScene",
                   "completed_WelcomeScene") %in% names(wide)))

  # 8i. Resume (#27): P103 accepted the offer after the crash, P111 declined
  #     it and repeated the lessons from WelcomeScene (warned). Everyone else
  #     was never offered one: FALSE (measured), not NA.
  rw <- function(col, pid) wide[[col]][wide$participant_id == pid]
  stopifnot(isTRUE(rw("resume_offered", "P103")), isFALSE(rw("resume_declined", "P103")))
  stopifnot(isTRUE(rw("resume_offered", "P111")), isTRUE(rw("resume_declined", "P111")))
  stopifnot(identical(wide$participant_id[wide$resume_declined], "P111"))
  stopifnot(isFALSE(rw("resume_offered", "P101")), isFALSE(rw("resume_declined", "P101")))
  stopifnot(identical(data_quality$resume_declined, wide$resume_declined))
  stopifnot(any(grepl("resume", warn_lines) & grepl("declined", warn_lines) & grepl("P111", warn_lines)))
  stopifnot(!any(grepl("declined", warn_lines) & grepl("P103", warn_lines)))
  stopifnot("P111" %in% flagged)
  # An offer is checked against the session right before it (P103, P111: a
  # crash), so no "files missing?" warning on the fake data. P112's restart
  # follows a clean quit: no resume offered, prev_crash FALSE.
  stopifnot(!any(grepl("not a crash", warn_lines)))
  prev <- sessions %>% filter(participant_id %in% c("P111", "P112"), session_no == 2) %>%
    arrange(participant_id) %>% pull(prev_crash)
  stopifnot(identical(prev, c(TRUE, FALSE)))

  # 8j. Finished-session warning overridden (#27 fix wave): only P112's second
  #     session has ended_session_warning = true. Everyone else logged false
  #     (FALSE, measured, not NA). Warned by name and flagged.
  stopifnot(isTRUE(rw("ended_session_override", "P112")))
  stopifnot(identical(wide$participant_id[wide$ended_session_override], "P112"))
  stopifnot(isFALSE(rw("ended_session_override", "P101")), isFALSE(rw("ended_session_override", "P103")))
  stopifnot(isFALSE(rw("resume_offered", "P112")))
  stopifnot(identical(data_quality$ended_session_override, wide$ended_session_override))
  stopifnot(any(grepl("ended-session warning overridden", warn_lines) & grepl("P112", warn_lines)))
  stopifnot(sum(grepl("ended-session warning overridden", warn_lines)) == 1)
  stopifnot("P112" %in% flagged)

  # 8l. Language (#11): research sessions are DE; P108 ran in EN (autoStart
  #     bypassed the keypad's DE lock) and is warned and flagged; P105 is a
  #     pre-#11 log (language null): "NA", not flagged, not warned. The test
  #     ID P950 (EN) is dropped before the check.
  dq <- function(col, pid) data_quality[[col]][data_quality$participant_id == pid]
  stopifnot(dq("languages", "P108") == "EN", isTRUE(dq("language_not_de", "P108")))
  stopifnot(dq("languages", "P105") == "NA", isFALSE(dq("language_not_de", "P105")))
  stopifnot(dq("languages", "P101") == "DE", isFALSE(dq("language_not_de", "P101")))
  stopifnot(identical(data_quality$participant_id[data_quality$language_not_de], "P108"))
  stopifnot("P108" %in% flagged)
  stopifnot(sum(grepl("language '", warn_lines)) == 1,
            any(grepl("language 'EN'", warn_lines) & grepl("P108", warn_lines)))
  stopifnot(!any(grepl("P950", warn_lines) & grepl("language", warn_lines)))

  # 8h. Duplicate files: P101's partial copy in research/backup/ is dropped
  #     (events + summary), named with both paths, and P101 is flagged.
  stopifnot(data_quality$duplicate_files[data_quality$participant_id == "P101"] == 2)
  stopifnot("P101" %in% flagged)
  dup_warn <- warn_lines[grepl("duplicate events files", warn_lines)]
  stopifnot(length(dup_warn) == 1, grepl("backup/", dup_warn), grepl("research/P101_", dup_warn))
  # Results must equal a run without the copy.
  wide_with_copy <- wide
  visits_with_copy <- visits
  dq_with_copy <- data_quality
  flagged_with_copy <- flagged
  unlink(file.path(data_dir, "research", "backup"), recursive = TRUE)
  output_dir <- "analysis/output/selftest_nodup"
  source("analysis/analyse.R", local = TRUE)
  stopifnot(isTRUE(all.equal(wide_with_copy, wide)))
  stopifnot(isTRUE(all.equal(visits_with_copy, visits)))
  stopifnot(isTRUE(all.equal(dq_with_copy %>% select(-duplicate_files), data_quality %>% select(-duplicate_files))))
  stopifnot(all(data_quality$duplicate_files == 0), !"P101" %in% flagged)

  # 8k. NA paths for older logs, on a temp copy of the fake data:
  #     P106 = a pre-#27 log (session_start data {}: no resume fields, no
  #     ended_session_warning) -> resume_offered, resume_declined and
  #     ended_session_override all NA (not measured), not FALSE.
  #     P109 = a #27 log from before the fix wave (resume fields, but no
  #     ended_session_warning) -> resume_offered FALSE, ended_session_override NA.
  old_logs <- file.path(tempdir(), "selftest_old_logs")
  unlink(old_logs, recursive = TRUE)
  dir.create(file.path(old_logs, "research"), recursive = TRUE)
  file.copy(list.files(file.path(data_dir, "research"), full.names = TRUE),
            file.path(old_logs, "research"))
  rewrite_session_start <- function(pid, edit) {
    for (path in list.files(file.path(old_logs, "research"), pattern = paste0("^", pid, "_.*_events\\.jsonl$"),
                            full.names = TRUE)) {
      lines <- readLines(path, encoding = "UTF-8")
      hit <- grepl("\"event\":\"session_start\"", lines, fixed = TRUE)
      stopifnot(sum(hit) == 1)
      lines[hit] <- edit(lines[hit])
      con <- file(path, open = "wb")
      writeLines(enc2utf8(lines), con, sep = "\n", useBytes = TRUE)
      close(con)
    }
  }
  rewrite_session_start("P106", function(l) sub("\"data\":\\{[^}]*\\}", "\"data\":{}", l))
  rewrite_session_start("P109", function(l) sub(",\"ended_session_warning\":(true|false)", "", l))
  # #29: a summary.csv that writes 0 grabs for WelcomeScene (not measured
  # there) must be reported as a mismatch, not read as a measured 0.
  p101_sum <- list.files(file.path(old_logs, "research"), pattern = "^P101_.*_summary\\.csv$", full.names = TRUE)
  stopifnot(length(p101_sum) == 1)
  sl <- readLines(p101_sum, encoding = "UTF-8")
  wl <- grep(",WelcomeScene,", sl)
  cells <- strsplit(sl[wl], ",", fixed = TRUE)[[1]]
  cells <- c(cells, rep("", 26 - length(cells)))
  stopifnot(cells[18] == "")
  cells[18] <- "0"
  sl[wl] <- paste(cells, collapse = ",")
  con <- file(p101_sum, open = "wb")
  writeLines(sl, con, sep = "\n")
  close(con)
  data_dir <- old_logs
  output_dir <- file.path(tempdir(), "selftest_old_logs_out")
  source("analysis/analyse.R", local = TRUE)
  stopifnot(is.na(rw("resume_offered", "P106")), is.na(rw("resume_declined", "P106")),
            is.na(rw("ended_session_override", "P106")))
  stopifnot(isFALSE(rw("resume_offered", "P109")), isFALSE(rw("resume_declined", "P109")),
            is.na(rw("ended_session_override", "P109")))
  stopifnot(isTRUE(rw("ended_session_override", "P112")), isFALSE(rw("ended_session_override", "P101")))
  stopifnot(identical(data_quality$ended_session_override, wide$ended_session_override))
  old_warn <- readLines(file.path(output_dir, "warnings.txt"), encoding = "UTF-8")
  stopifnot(!any(grepl("inconsistent", old_warn)))
  stopifnot(any(grepl("P101 WelcomeScene visit 1 grabs events=NA summary=0", old_warn, fixed = TRUE)))
  unlink(c(old_logs, output_dir), recursive = TRUE)

  # 9. Without allow_fake, analyse.R must refuse the fake folder.
  # (parent = globalenv(), so it can't see allow_fake from this environment)
  guard <- new.env(parent = globalenv())
  guard$data_dir <- "analysis/data/fake"
  guard$output_dir <- file.path(tempdir(), "selftest_guard")
  refused <- tryCatch({ sys.source("analysis/analyse.R", envir = guard); FALSE },
                      error = function(e) grepl("FAKE", conditionMessage(e)))
  stopifnot(refused)

  # 10. The test added nothing to the global environment.
  added <- setdiff(ls(globalenv()), globals_before)
  if (length(added) > 0) stop("selftest leaked into the global environment: ",
                              paste(added, collapse = ", "))
})

cat("selftest OK\n")
