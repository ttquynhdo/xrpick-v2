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

  # 2. One row per study participant: P101-P115; the test ID P950 is dropped,
  #    the pilot IDs P801-P803, P811-P817 are in their own set (8q).
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
                      "data_quality.csv", "data_quality.png", "wide.csv", "warnings.txt",
                      "tour_steps.csv", "tour_time_by_step.png", "tour_back_by_step.png")
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
  # denominator is every counted press, except ekz_placement (#94, decision
  # 028): only presses with the right quantity (no code but EKZ_PLACEMENT) at
  # an Empf Kz "003" position, the only presses whose placement the app rates.
  bs <- skill_rates %>% filter(scene == "BasicPickingScene", skill == "calculate_quantity")
  stopifnot(nrow(bs) > 0, all(bs$skill_presses == visits$confirm_count[match(
    paste(bs$session_id, "BasicPickingScene", bs$scene_visit),
    paste(visits$session_id, visits$scene, visits$scene_visit))]))
  be <- skill_rates %>% filter(scene == "BasicPickingScene", skill == "ekz_placement") %>%
    left_join(presses %>% filter(scene == "BasicPickingScene", event == "confirm_pressed",
                                 map_lgl(codes, ~ all(.x %in% "EKZ_PLACEMENT"))) %>%
                count(session_id, scene_visit, name = "n_rule_ok"),
              by = c("session_id", "scene_visit"))
  stopifnot(nrow(be) > 0, all(be$skill_presses == be$n_rule_ok),
            # Fewer than all presses somewhere: wrong-quantity presses are left out.
            any(be$skill_presses < visits$confirm_count[match(paste(be$session_id, "BasicPickingScene", be$scene_visit),
                                                             paste(visits$session_id, visits$scene, visits$scene_visit))]))
  # Study set: all "025" builds, so Mixed position 1 was Empf Kz 000 and Mixed
  # has no ekz_placement rate (not measured = no row, never 0 %).
  stopifnot(!any(skill_rates$scene == "MixedPickingScene" & skill_rates$skill == "ekz_placement"),
            all(presses$empf_kz[presses$scene == "MixedPickingScene" & presses$event == "confirm_pressed"] == "000"),
            all(is.na(presses$empf_kz_logged)),
            all(presses$empf_kz[presses$scene == "BasicPickingScene" & presses$event == "confirm_pressed"] == "003"),
            !any(grepl("set mixes Mixed position_1 Empf Kz", warn_lines)),
            !any(grepl("EKZ_PLACEMENT where the app does not rate", warn_lines)))
  # Empf Kz rule and the EKZ-3 opportunity rule, called directly. The field
  # tells the build (has_empf_kz); a "029" log without it is a build before
  # #94 (Mixed position 1 = 000); a session without presses from "029" on
  # leaves Mixed position 1 unknown.
  stopifnot(identical(expected_empf_kz(c("BasicPickingScene", "MixedPickingScene", "MixedPickingScene",
                                         "MixedPickingScene", "MixedPickingScene", "MixedPickingScene",
                                         "MixedPickingScene", "ComplexScene", "ConceptScene"),
                                       c(1L, 1L, 1L, 1L, 1L, 1L, 2L, 1L, NA),
                                       c(FALSE, FALSE, TRUE, FALSE, NA, NA, TRUE, TRUE, NA),
                                       c("019", "025", "029", "029", "025", "029", "029", "029", "029")),
                      c("003", "000", "003", "000", "000", NA, "000", "000", NA)),
            identical(is_ekz_opportunity(rep("confirm_pressed", 5), c("003", "003", "003", "000", NA),
                                         list(character(0), "EKZ_PLACEMENT", c("TOO_FEW"), character(0),
                                              character(0))),
                      c(TRUE, TRUE, FALSE, FALSE, FALSE)))
  # items.csv and process_by_position.csv carry empf_kz (#94).
  stopifnot("empf_kz" %in% names(read_csv(file.path(output_dir, "items.csv"), show_col_types = FALSE)),
            "empf_kz" %in% names(read_csv(file.path(output_dir, "process_by_position.csv"), show_col_types = FALSE)))

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

  # 8p. Paper forms (#31): data/fake/paper_forms.csv (semicolons,
  #     Windows-1252, German dates, as German Excel saves it) is read,
  #     validated, scored and joined into wide_with_paper.csv; wide.csv
  #     stays log-only. Expected values below are worked out by hand.
  stopifnot(!any(grepl("^(ssq|tlx)_", names(wide))))
  wp_path <- file.path(output_dir, "wide_with_paper.csv")
  stopifnot(file.exists(wp_path), file.exists(file.path(output_dir, "ssq_pre_post.png")),
            file.exists(file.path(output_dir, "tlx_vs_attempts.png")))
  wp <- read_csv(wp_path, show_col_types = FALSE, guess_max = 1000)
  pw <- function(col, pid) wp[[col]][wp$participant_id == pid]
  # 15 log participants + P120 (paper only); P950 (test) and "P13" (typo) not.
  stopifnot(nrow(wp) == 16, anyDuplicated(wp$participant_id) == 0)
  stopifnot(identical(sort(wp$participant_id[!wp$has_log]), "P120"))
  stopifnot(!any(c("P950", "P13") %in% wp$participant_id))
  stopifnot(identical(sort(wp$participant_id[wp$paper_status == "no_paper"]), c("P113", "P114")),
            pw("paper_status", "P102") == "duplicate", is.na(pw("ssq_post_total", "P102")))
  # No free text in the joined file, only whether a note exists.
  stopifnot(!"note" %in% names(wp), isTRUE(pw("paper_note_present", "P107")),
            isFALSE(pw("paper_note_present", "P101")))
  # P101: pre all 0; post items 1, 3, 8 = 1 -> raw N = 2 (items 1, 8),
  # O = 2 (items 1, 3), D = 1 (item 8): N 19.08, O 15.16, D 13.92,
  # total (2 + 2 + 1) * 3.74 = 18.7. TLX (40+10+30+20+45+15)/6 = 26.7.
  stopifnot(pw("ssq_pre_total", "P101") == 0, pw("ssq_post_nausea", "P101") == 19.08,
            pw("ssq_post_oculomotor", "P101") == 15.16, pw("ssq_post_disorientation", "P101") == 13.92,
            pw("ssq_post_total", "P101") == 18.7, pw("ssq_change_total", "P101") == 18.7,
            pw("tlx_raw", "P101") == 26.7, pw("session_date", "P101") == "2026-11-03")
  stopifnot(abs(pw("end_time_diff_min", "P101")) <= 1, abs(pw("end_time_diff_min", "P103")) <= 1)
  # P108: item 3 (headache) empty -> oculomotor and total NA, nausea measured.
  stopifnot(is.na(pw("ssq_pre_03_headache", "P108")), is.na(pw("ssq_pre_oculomotor", "P108")),
            is.na(pw("ssq_pre_total", "P108")), !is.na(pw("ssq_pre_nausea", "P108")),
            is.na(pw("ssq_change_total", "P108")))
  # P106: post item 8 = 4 and TLX effort = 120 are set to empty: nausea and
  # disorientation NA (item 8 is in both), oculomotor measured; tlx_raw NA.
  stopifnot(is.na(pw("ssq_post_08_nausea", "P106")), is.na(pw("ssq_post_nausea", "P106")),
            is.na(pw("ssq_post_disorientation", "P106")), !is.na(pw("ssq_post_oculomotor", "P106")),
            is.na(pw("tlx_effort", "P106")), is.na(pw("tlx_raw", "P106")), pw("paper_problems", "P106") == 2)
  stopifnot(pw("abbruchgrund", "P107") == "mued_schwindlig", pw("abbruchgrund", "P104") == "zeit",
            is.na(pw("abbruchgrund", "P105")), is.na(pw("tlx_raw", "P112")))
  warn_lines <- readLines(file.path(output_dir, "warnings.txt"), encoding = "UTF-8")
  has_warn <- function(...) any(Reduce(`&`, lapply(c(...), function(p) grepl(p, warn_lines, fixed = TRUE))))
  stopifnot(has_warn("'P13'", "not P + 3 digits"), has_warn("paper form for P120 but no log"),
            has_warn("log for P113 but no paper form"), has_warn("log for P114 but no paper form"),
            has_warn("P102 appears 2 times"), has_warn("P106", "ssq_post_08_nausea = '4'"),
            has_warn("P106", "tlx_effort = '120'"), has_warn("P108", "SSQ pre item(s) empty (03_headache)"),
            has_warn("'absturz'"), has_warn("P109", "abbruchgrund given although"),
            has_warn("paper form P110: end time"), has_warn("P112: NASA-TLX not filled"))
  # P107 was ended by the facilitator and has a reason: no warning for it.
  stopifnot(!has_warn("P107", "no abbruchgrund"), !has_warn("P101: end time"), !has_warn("paper", "P950"))
  check_line <- warn_lines[grepl("^Paper forms to check", warn_lines)]
  stopifnot(length(check_line) == 1,
            all(sapply(c("P102", "P105", "P106", "P108", "P109", "P110", "P112", "P113", "P114", "P120"),
                       grepl, check_line)),
            !grepl("P101", check_line), !grepl("P107", check_line))
  # Windows-1252 text was read as UTF-8 (the note keeps its umlaut).
  stopifnot(paper_raw$note[paper_raw$participant_id == "P107"] == "Übelkeit in Komplex")
  # The template (commas, UTF-8, ISO date) reads the same way; its header is
  # the column contract with analyse.R.
  tpl <- readLines("analysis/paper_forms_template.csv", warn = FALSE)
  stopifnot(identical(strsplit(tpl[1], ",")[[1]], paper_cols))
  tpl_try <- file.path(tempdir(), "paper_forms_comma.csv")
  writeLines(c(tpl[1], paste(c("P101", "2026-11-03", "F1", "9:05", rep("0", 31), "2", rep("50", 6),
                               "zeit", ""), collapse = ",")), tpl_try)
  tpl_read <- read_paper_file(tpl_try)
  stopifnot(nrow(tpl_read) == 1, tpl_read$participant_id == "P101", tpl_read$ssq_post_16_burping == "2",
            tpl_read$abbruchgrund == "zeit", tpl_read$paper_row == 2)
  unlink(tpl_try)
  # Decision 022: no pose/perf promise left in the Q4 output.
  stopifnot(any(grepl("decision 022", warn_lines)))

  # 8q. Pilot set (decision 023, #75): P801 (EN), P802 (DE), P803 (EN, crash
  #     + accepted resume), P811 (EN, layout "019") and P812 (EN, fixed tour,
  #     8s) in research/pilot/ are analysed as their own set
  #     into output_dir/pilot/, with every main output, and never mix with
  #     the study set (logs and paper forms).
  pilot_dir <- file.path(output_dir, "pilot")
  pilot_files <- c(expected_files, "items.csv", "first_attempt_by_item.png", "process_by_position.csv",
                   "time_by_phase.png", "wide_with_paper.csv", "ssq_pre_post.png", "tlx_vs_attempts.png")
  missing_pilot <- pilot_files[!file.exists(file.path(pilot_dir, pilot_files))]
  if (length(missing_pilot) > 0) stop("missing pilot outputs: ", paste(missing_pilot, collapse = ", "))
  stopifnot(is.environment(pilot_set), identical(pilot_set$analysis_group, "pilot"))
  pilot_ids <- c("P801", "P802", "P803", "P811", "P812", "P813", "P814", "P815", "P816", "P817")
  stopifnot(identical(sort(pilot_set$wide$participant_id), pilot_ids),
            identical(sort(pilot_set$data_quality$participant_id), pilot_ids))
  is_p8 <- function(x) grepl("^P8[0-9]{2}$", x)
  # No pilot ID in any study table or file, no study ID in any pilot one.
  for (tab in list(wide, visits, items, data_quality, presses, sessions)) stopifnot(!any(is_p8(tab$participant_id)))
  for (tab in list(pilot_set$wide, pilot_set$visits, pilot_set$items, pilot_set$data_quality,
                   pilot_set$presses, pilot_set$sessions)) stopifnot(all(is_p8(tab$participant_id)))
  p_items <- read_csv(file.path(pilot_dir, "items.csv"), show_col_types = FALSE)
  stopifnot(nrow(p_items) > 0, all(is_p8(p_items$participant_id)))
  # Paper rows join the set of their ID: study wp keeps its 16 rows (8p), the
  # pilot wp has exactly the pilots, all joined.
  p_wp <- read_csv(file.path(pilot_dir, "wide_with_paper.csv"), show_col_types = FALSE, guess_max = 1000)
  stopifnot(identical(sort(p_wp$participant_id), pilot_ids), all(p_wp$paper_status == "joined"),
            all(p_wp$has_log), !any(is_p8(wp$participant_id)))
  # EN is expected for pilots: no language warning, language_not_de FALSE,
  # languages recorded as logged.
  p_warn <- readLines(file.path(pilot_dir, "warnings.txt"), encoding = "UTF-8")
  stopifnot(!any(grepl("language '", p_warn)), !any(pilot_set$data_quality$language_not_de))
  pq <- function(col, pid) pilot_set$data_quality[[col]][pilot_set$data_quality$participant_id == pid]
  stopifnot(pq("languages", "P801") == "EN", pq("languages", "P802") == "DE", pq("languages", "P803") == "EN")
  stopifnot(any(grepl("PILOT SET", p_warn)), !any(grepl("PILOT SET", warn_lines)))
  # P803: crash, accepted resume, finished; the others one clean session.
  pw8 <- function(col, pid) pilot_set$wide[[col]][pilot_set$wide$participant_id == pid]
  stopifnot(isTRUE(pw8("multi_session", "P803")), isTRUE(pw8("resume_offered", "P803")),
            isFALSE(pw8("resume_declined", "P803")), isTRUE(pw8("course_finished", "P803")),
            isFALSE(pw8("multi_session", "P801")), isTRUE(pw8("course_finished", "P801")))
  # Pilot seam checks as for the study (8b), no folder warnings on clean data.
  stopifnot(!any(grepl("disagrees|differs", p_warn)),
            !any(grepl("outside research/pilot|in research/pilot", c(p_warn, warn_lines))))
  # The study report names the pilot IDs it left out.
  stopifnot(any(grepl("^Pilot IDs .*P801, P802, P803", warn_lines)))

  # 8s. ConceptScene tour (#82). P812 ("025") runs a fixed tour
  #     (make_fake_data.R p812_tour): Zurück 4 -> 3 after 1.2 s (quick, booked
  #     on step 4), the chain 9 -> 8 -> 7 (8 -> 7 at the same t), a 10 s pause
  #     on step 6, "Von vorn" 11 -> 1. P811 ran a "019" build: no Zurück.
  ts <- read_csv(file.path(pilot_dir, "tour_steps.csv"), show_col_types = FALSE,
                 col_types = cols(layout_version = col_character()))
  stopifnot(pilot_set$n_steps == 11)
  t812 <- ts %>% filter(participant_id == "P812") %>% arrange(step_index)
  stopifnot(nrow(t812) == 11, all(t812$layout_version == "025"), all(t812$zurueck_available))
  # Back count per step for the known chain: steps 4, 8 and 9, once each.
  stopifnot(identical(as.integer(t812$back_presses), as.integer(seq_len(11) %in% c(4, 8, 9))))
  # Next then Back within < 2 s: only 3 -> 4 -> 3 (1.2 s); 8 -> 9 -> 8 took 3.5 s,
  # and 8 -> 7 follows a Back, not a Weiter.
  stopifnot(identical(as.integer(t812$quick_next_back), as.integer(seq_len(11) == 4)))
  # "Von vorn" is booked on the last step, as a restart (not a jump, not a Back).
  stopifnot(identical(as.integer(t812$restarts), as.integer(seq_len(11) == 11)))
  # Active time: step 4 = 1.2 + 7 s, step 6 = 2 + 4 s (the 10 s pause removed),
  # step 8 = 6 + 0 + 4 s (three showings), step 11 = 5.001 s (until "Von vorn").
  stopifnot(isTRUE(all.equal(t812$active_s[c(4, 6, 8, 11)], c(8.2, 6, 10, 5.001), tolerance = 1e-9)),
            identical(as.integer(t812$shows[c(3, 8)]), c(3L, 3L)))
  # Line order, not t: the two Back lines 9 -> 8 -> 7 share t and are both "back".
  tie <- pilot_set$tour_shows %>% filter(participant_id == "P812") %>% group_by(t) %>% filter(n() > 1) %>% ungroup()
  stopifnot(nrow(tie) == 2, identical(tie$step_index, c(8L, 7L)), all(tie$direction == "back"))
  # P811 ("019"): time measured, Back counts NA (not 0).
  t811 <- ts %>% filter(participant_id == "P811")
  stopifnot(nrow(t811) == 11, all(t811$layout_version == "019"), !any(t811$zurueck_available),
            all(is.na(t811$back_presses)), all(is.na(t811$quick_next_back)), !anyNA(t811$active_s))
  stopifnot(is.na(pw8("tour_back_presses_ConceptScene", "P811")),
            pw8("tour_back_presses_ConceptScene", "P812") == 3,
            pw8("tour_quick_next_back_ConceptScene", "P812") == 1)
  # A header without layout_version counts as "no Zurück", like "019".
  stopifnot(identical(pilot_set$tour_has_zurueck(c("019", NA, "025", "026")), c(FALSE, FALSE, TRUE, TRUE)))
  # No unexpected tour moves in the fake data (only Weiter / Zurück / Von vorn).
  stopifnot(!any(grepl("not Weiter/Zurück/Von vorn|no Zurück button", c(p_warn, warn_lines))))
  # Study set: every row of a visit with a tour, the random Back chains of
  # make_fake_data.R counted (a two-step chain gives 2 Backs in one visit).
  stopifnot(nrow(tour_steps) == 11 * nrow(distinct(tour_steps, session_id, scene_visit)),
            all(tour_steps$zurueck_available), !anyNA(tour_steps$back_presses),
            sum(tour_steps$back_presses) == sum(tour_shows$direction == "back"))
  # Mixed layouts (decision 024): the pilot set mixes "019" and "025", the
  # study set does not. Concept items and times are split by layout_version.
  stopifnot(all(pilot_set$data_quality$mixed_layout), !any(data_quality$mixed_layout),
            any(grepl("set mixes layout_version 019 / 025", p_warn)), !any(grepl("set mixes layout_version", warn_lines)))
  concept_items <- p_items %>% filter(scene == "ConceptScene")
  stopifnot(all(c("019", "025") %in% as.character(concept_items$layout_version)),
            identical(as.character(concept_items$layout_version[concept_items$participant_id == "P811"])[1], "019"))
  stopifnot(all(c("ConceptScene [019]", "ConceptScene [025]") %in% as.character(pilot_set$ttc_people$scene)),
            all(c("ConceptScene [019]", "ConceptScene [025]") %in% as.character(pilot_set$item_shares$chart_scene)),
            !any(grepl("\\[", as.character(ttc_people$scene))))

  # 8t. ConceptScene hint tags (#100, decision 030). P813 ran a "029" build
  #     with a fixed set of tags (make_fake_data.R p813_hints): one stray
  #     cleared (warned, ignored), 6 tags, one moved to another package in
  #     the same frame, TOO_MANY cleared by the step change, a 22.5 s wait at
  #     material_flow, one tag still shown when the scene was left.
  stopifnot(file.exists(file.path(pilot_dir, "concept_hints.csv")),
            file.exists(file.path(pilot_dir, "concept_hints_by_visit.csv")),
            file.exists(file.path(output_dir, "concept_hints_by_visit.csv")))
  h813 <- pilot_set$concept_hints %>% filter(participant_id == "P813") %>% arrange(hint_no)
  stopifnot(nrow(h813) == 6, all(h813$layout_version == "029"),
            identical(h813$container, c("order", "order", "storage", "order", "order", "storage")),
            identical(h813$reason, c("WRONG_MATERIAL", "WRONG_MATERIAL", "WRONG_MATERIAL", "TOO_MANY",
                                     "WRONG_MATERIAL", "WRONG_MATERIAL")),
            identical(h813$tour_step_id, c(NA, "material", "material", "cartons", "material_flow", "material_flow")),
            identical(h813$ended_by, c(rep("cleared", 5), "scene_exit")),
            identical(h813$removed_by_grab, c(TRUE, TRUE, TRUE, FALSE, TRUE, NA)),
            h813$cleared_step_id[4] == "ist")
  # Active seconds shown: 0.5 s settle is before shown, the grab ends it 0.011 s later.
  stopifnot(isTRUE(all.equal(h813$shown_s[c(1, 2, 3, 5)], c(6.011, 5.011, 4.811, 25.011), tolerance = 1e-9)))
  # The moved tag: the bag's shown has the ring's cleared t.
  stopifnot(h813$shown_t[3] == h813$end_t[2])
  v813 <- pilot_set$concept_hints_by_visit %>% filter(participant_id == "P813")
  stopifnot(nrow(v813) == 1, isTRUE(v813$hints_logged), v813$hints_shown == 6,
            v813$hints_order_wrong_material == 3, v813$hints_order_too_many == 1,
            v813$hints_storage_wrong_material == 2, v813$hints_removed_by_grab == 4,
            v813$hints_open_at_exit == 1, v813$hint_contract_breaks == 1,
            v813$material_flow_demo_held == 1, isFALSE(v813$material_flow_wait_censored),
            abs(v813$material_flow_wait_max_s - 22.512) < 0.002)
  stopifnot(any(grepl("concept_hint_cleared without a preceding", p_warn) & grepl("P813", p_warn)),
            sum(grepl("concept_hint_", p_warn, fixed = TRUE)) == 1, !any(grepl("concept_hint_", warn_lines, fixed = TRUE)))
  # Older layouts: no tags existed, so every hint column is NA (not 0).
  old_v <- pilot_set$concept_hints_by_visit %>% filter(!participant_id %in% c("P813", "P814", "P815", "P816", "P817"))
  stopifnot(nrow(old_v) > 0, !any(old_v$hints_logged), all(is.na(old_v$hints_shown)),
            all(is.na(old_v$hint_shown_s)), all(is.na(old_v$material_flow_wait_max_s)))
  stopifnot(nrow(concept_hints_by_visit) > 0, !any(concept_hints_by_visit$hints_logged),
            all(is.na(concept_hints_by_visit$hints_shown)), nrow(concept_hints) == 0)
  # Hints are not attempts: P813's Concept confirm_count = its quiz touches.
  c813 <- pilot_set$visits %>% filter(participant_id == "P813", scene == "ConceptScene")
  q813 <- pilot_set$presses %>% filter(participant_id == "P813", scene == "ConceptScene")
  stopifnot(nrow(c813) == 1, c813$confirm_count == nrow(q813), all(q813$event == "quiz_answer"))
  # "029" is its own Concept layout, never pooled with "025" (decision 024).
  stopifnot(any(grepl("set mixes layout_version 019 / 025 / 029", p_warn)),
            "ConceptScene [029]" %in% as.character(pilot_set$item_shares$chart_scene),
            identical(pilot_set$tour_has_zurueck("029"), TRUE),
            all(ts$zurueck_available[ts$participant_id == "P813"]))

  # 8u. ConceptScene containers (#92, decision 032). P814 ran a "032" build
  #     with fixed carton moves (make_fake_data.R p814_hints): carton 1 out of
  #     the storage container and back, cartons 1-4 into the order container
  #     (the 4th gives TOO_MANY, taken back by the trainee), the demo reset at
  #     "Zurueck" after material_flow, then carton 6 into the order container
  #     (counts restart at storage 6 / order 0, so its count_after is 1).
  stopifnot(file.exists(file.path(pilot_dir, "concept_containers_by_visit.csv")),
            file.exists(file.path(output_dir, "concept_containers_by_visit.csv")))
  c814 <- pilot_set$concept_containers_by_visit %>% filter(participant_id == "P814")
  stopifnot(nrow(c814) == 1, isTRUE(c814$containers_logged), c814$layout_version == "032",
            c814$storage_exits == 6, c814$storage_enters == 2, c814$order_enters == 5, c814$order_exits == 1,
            c814$order_max_cartons == 4, isTRUE(c814$soll_reached), isTRUE(c814$over_soll),
            c814$order_over_soll_enters == 1, c814$demo_resets >= 1,
            c814$container_count_breaks == 0, c814$container_unlinked_lines == 0)
  # time_to_soll_s = active seconds from scene enter to the 3rd carton's enter.
  cr814 <- pilot_set$cc_rows %>% filter(participant_id == "P814")
  soll_line <- cr814 %>% filter(container == "order", event == "container_enter", running == 3) %>% slice_min(t, n = 1)
  e814 <- pilot_set$enters %>% filter(session_id == soll_line$session_id, scene == "ConceptScene")
  stopifnot(nrow(soll_line) == 1, soll_line$object_id == "Interactables/STECKER C Karton 3",
            abs(c814$time_to_soll_s - (soll_line$t - e814$enter_t -
                                         pilot_set$paused_between(soll_line$session_id, e814$enter_t, soll_line$t))) < 0.002)
  # Carton 6 after the reset: count_after 1 (not 4), with storage back at 5.
  k6 <- cr814 %>% filter(object_id == "Interactables/STECKER C Karton 6")
  stopifnot(identical(k6$container, c("storage", "order")), identical(k6$count_after, c(5L, 1L)),
            all(k6$segment >= 1))
  # The one hint: TOO_MANY on carton 4, removed by the trainee.
  h814 <- pilot_set$concept_hints %>% filter(participant_id == "P814")
  stopifnot(nrow(h814) == 1, h814$reason == "TOO_MANY", h814$container == "order",
            h814$object_id == "Interactables/STECKER C Karton 4", isTRUE(h814$removed_by_grab))
  # Concept container_* stay out of the per-visit columns and summary.csv.
  stopifnot(is.na(pilot_set$visits$container_enters[pilot_set$visits$participant_id == "P814" &
                                                      pilot_set$visits$scene == "ConceptScene"]))
  # Older layouts: NA, not 0; no container warnings on the fake data.
  stopifnot(all(is.na(pilot_set$concept_containers_by_visit$order_enters[
              !pilot_set$concept_containers_by_visit$participant_id %in% c("P814", "P815", "P816", "P817")])),
            !any(concept_containers_by_visit$containers_logged),
            !any(grepl("container_\\* line|ConceptScene (order|storage) container", c(p_warn, warn_lines))))
  # label_by_matnr replaces material_by_matnr from "032", same skill.
  q814 <- p_items %>% filter(participant_id == "P814", scene == "ConceptScene")
  stopifnot("label_by_matnr" %in% q814$item, !"material_by_matnr" %in% q814$item,
            all(q814$skill[q814$item == "label_by_matnr"] == "check_material"),
            "material_by_matnr" %in% p_items$item[p_items$participant_id == "P813"])
  # Complex position_1 scaffold (#93): "019"/"025" = quiz_visible, "029" =
  # result_on_card. The pilot set holds both, so position_1 is split in the
  # item chart and the phase chart; position 2 and the study set are not.
  stopifnot(identical(pilot_set$complex_p1_scaffold(c("019", "025", "029", NA)),
                      c("quiz_visible", "quiz_visible", "result_on_card", "quiz_visible")))
  cp1 <- p_items %>% filter(scene == "ComplexScene", item == "position_1")
  stopifnot(all(cp1$scaffold_level[cp1$participant_id == "P813"] == "result_on_card"),
            all(cp1$scaffold_level[cp1$participant_id %in% c("P811", "P812")] == "quiz_visible"),
            all(is.na(p_items$scaffold_level[!(p_items$scene == "ComplexScene" & p_items$item == "position_1")])))
  cx <- as.character(pilot_set$item_shares$x_label[pilot_set$item_shares$scene == "ComplexScene"])
  # The result_on_card label lists every layout of that level: P813 "029",
  # P814 "032", P815 "033", P816 "034".
  stopifnot(all(c("Complex\nposition_1\n[019/025]", "Complex\nposition_1\n[029/032/033/034]", "Complex\nposition_2") %in% cx),
            !"Complex\nposition_1" %in% cx,
            any(grepl("set mixes Complex position_1 scaffold", p_warn)),
            !any(grepl("set mixes Complex position_1 scaffold", warn_lines)),
            !any(grepl("\\[", as.character(item_shares$x_label))))
  slots <- levels(pilot_set$proc_long$slot)
  stopifnot(all(c("Complex\nposition 1\n[019/025]", "Complex\nposition 1\n[029/032/033/034]", "Complex\nposition 2") %in% slots))
  pp <- read_csv(file.path(pilot_dir, "process_by_position.csv"), show_col_types = FALSE,
                 col_types = cols(layout_version = col_character()))
  stopifnot(all(c("layout_version", "scaffold_level") %in% names(pp)),
            identical(sort(unique(na.omit(pp$scaffold_level[pp$scene == "ComplexScene" & pp$position == 1]))),
                      c("quiz_visible", "result_on_card")),
            all(is.na(pp$scaffold_level[!(pp$scene == "ComplexScene" & pp$position == 1)])))
  # The hint event names are in the catalogue (no "not in the v2 catalogue" note for them).
  stopifnot(all(c("concept_hint_shown", "concept_hint_cleared") %in% pilot_set$known_events))

  # 8v. Station start delay (#99, decision 033). P815 ran a "033" build: the
  #     boxes start 4 s after a correct confirm (storage_out, storage_in p > 1),
  #     P814 ("032") and the study set 0.6 s. R takes the delay from the data
  #     (trigger -> station_move_start), never assumes it, and never pools the
  #     two groups.
  stopifnot(file.exists(file.path(pilot_dir, "station_waits.csv")),
            file.exists(file.path(output_dir, "station_waits.csv")))
  stopifnot(identical(pilot_set$station_wait_group(c("019", "032", "033", "034", NA)),
                      c("before_033", "before_033", "033_on", "033_on", NA)))
  sw <- pilot_set$station_waits
  sw815 <- sw %>% filter(participant_id == "P815", !is.na(expected_delay_s))
  sw814 <- sw %>% filter(participant_id == "P814", !is.na(expected_delay_s))
  stopifnot(nrow(sw815) == 6, all(sw815$station_wait_group == "033_on"), all(abs(sw815$start_delay_s - 4) < 0.05),
            nrow(sw814) == 6, all(sw814$station_wait_group == "before_033"), all(abs(sw814$start_delay_s - 0.6) < 0.05),
            !any(sw$delay_unexpected), !any(station_waits$delay_unexpected),
            all(station_waits$station_wait_group[!is.na(station_waits$expected_delay_s)] == "before_033"),
            # Only the per-ride warning (": station start delay <s> s"); the
            # pilot set's "set mixes station start delays" is expected (below).
            !any(grepl(": station start delay ", c(p_warn, warn_lines), fixed = TRUE)))
  # Complex box 1 (after the quiz) and the order box are not checked (NA).
  stopifnot(all(is.na(sw$expected_delay_s[sw$what == "order_box" |
                                            (sw$what == "storage_in" & sw$position == 1)])))
  # wait_s = the logged delay + the storage_in ride, whatever the delay was.
  for (pid in c("P814", "P815")) {
    v <- pilot_set$visits %>% filter(participant_id == pid, scene == "MixedPickingScene")
    r <- sw %>% filter(participant_id == pid, scene == "MixedPickingScene", what == "storage_in", position == 2)
    stopifnot(nrow(v) == 1, nrow(r) == 1, abs(v$wait_s - (r$start_delay_s + r$ride_s)) < 0.01,
              v$station_wait_group == if (pid == "P815") "033_on" else "before_033",
              abs(v$time_to_correct_net_s - (v$time_to_correct_s - v$wait_s)) < 0.002)
    # read_s of position 2 starts when its box has arrived: the wait is not in it.
    pr <- pilot_set$process %>% filter(participant_id == pid, scene == "MixedPickingScene", position == 2)
    stopifnot(nrow(pr) == 1, abs(pr$start_t - r$arrived_t) < 0.002)
  }
  w815 <- pilot_set$visits$wait_s[pilot_set$visits$participant_id == "P815" & pilot_set$visits$scene == "MixedPickingScene"]
  w814 <- pilot_set$visits$wait_s[pilot_set$visits$participant_id == "P814" & pilot_set$visits$scene == "MixedPickingScene"]
  stopifnot(w815 - w814 > 2.5)
  # Never pooled: the pilot set mixes both groups and says so, the study set does not.
  stopifnot(all(pilot_set$data_quality$mixed_station_wait), !any(data_quality$mixed_station_wait),
            any(grepl("set mixes station start delays", p_warn)),
            !any(grepl("set mixes station start delays", warn_lines)))

  # 8w. Course objectives (#86, decision 034): objectives_summary.csv has one
  #     row per objectives.csv row; evidence "none" is NA (never 0 %), rated
  #     objectives are the pooled first attempts, observed D2/P0 per person.
  for (os_set in list(list(o = objectives_summary, it = items, v = visits, dir = output_dir),
                      list(o = pilot_set$objectives_summary, it = pilot_set$items, v = pilot_set$visits,
                           dir = pilot_dir))) {
    os <- os_set$o
    stopifnot(file.exists(file.path(os_set$dir, "objectives_summary.csv")),
              identical(os$objective_id, c("D1", "D2", "D3", "P0", "P1", "P2", "P3", "P4a", "P4b")),
              all(os$knowledge_type == if_else(startsWith(os$objective_id, "D"), "declarative", "procedural")))
    none <- os %>% filter(evidence == "none")
    stopifnot(identical(none$objective_id, "D1"), is.na(none$n), is.na(none$n_persons), is.na(none$share),
              is.na(none$share_lo))
    # A measured objective without data has n 0 and share NA, never 0 %.
    stopifnot(all(is.na(os$share[os$n %in% 0L])), all(!is.na(os$share[which(os$n > 0)])))
    rated <- os %>% filter(evidence == "rated")
    stopifnot(all(rated$n > 0), all(rated$share >= 0 & rated$share <= 1),
              all(rated$share_lo <= rated$share & rated$share <= rated$share_hi))
    # D3 = the two EKZ quiz questions, P3 = the Mixed positions, whole first attempt (items.csv).
    d3_items <- os_set$it %>% filter(scene == "ConceptScene", item %in% c("ekz_003", "ekz_000"))
    mx_items <- os_set$it %>% filter(scene == "MixedPickingScene", kind == "position")
    stopifnot(os$n[os$objective_id == "D3"] == nrow(d3_items),
              os$k[os$objective_id == "D3"] == sum(d3_items$first_attempt_correct %in% TRUE),
              os$n[os$objective_id == "P3"] == nrow(mx_items),
              os$k[os$objective_id == "P3"] == sum(mx_items$first_attempt_correct %in% TRUE))
    # Scaffolded Complex position 1 never counts; EKZ placement only in Basic/Mixed.
    stopifnot(!any(grepl("Complex:position_1", os$sources, fixed = TRUE)),
              !grepl("Complex", os$sources[os$objective_id == "P2"]))
    # P0: persons with an outcome-logged visit of all 3 picking scenes; done = completed each.
    p0 <- os_set$v %>% filter(outcome_logged %in% TRUE,
                              scene %in% c("BasicPickingScene", "MixedPickingScene", "ComplexScene")) %>%
      group_by(participant_id, scene) %>% summarise(done = any(completed %in% TRUE), .groups = "drop") %>%
      group_by(participant_id) %>% filter(n() == 3) %>% summarise(done = all(done), .groups = "drop")
    stopifnot(os$n[os$objective_id == "P0"] == nrow(p0), os$k[os$objective_id == "P0"] == sum(p0$done))
  }
  # D2 (Concept order container, layout "032" on): the study set has no such
  # log (n 0, share NA); the pilot set has P814 and P815.
  stopifnot(objectives_summary$n[objectives_summary$objective_id == "D2"] == 0,
            is.na(objectives_summary$share[objectives_summary$objective_id == "D2"]),
            pilot_set$objectives_summary$n[pilot_set$objectives_summary$objective_id == "D2"] >= 1)

  # 8u. Mixed position 1 Empf Kz (#94, decision 028). P813 ("029") logs
  #     line.empf_kz; its fixed Mixed presses: TOO_FEW, TOO_FEW, EKZ_PLACEMENT,
  #     correct (position 1, "003"), correct (position 2, "000"). EKZ
  #     opportunities = presses 3 and 4 only; quantity tested by all 5.
  pm <- pilot_set$presses %>% filter(participant_id == "P813", scene == "MixedPickingScene") %>% arrange(t)
  stopifnot(identical(pm$empf_kz_logged, c("003", "003", "003", "003", "000")),
            identical(pm$ekz_opportunity, c(FALSE, FALSE, TRUE, TRUE, FALSE)),
            # Builds before layout "029" never log the field (P814/P815 are later builds).
            all(is.na(pilot_set$presses$empf_kz_logged[
              !coalesce(suppressWarnings(as.integer(pilot_set$presses$layout_version)) >= 29L, FALSE)])))
  m813 <- pilot_set$skill_rates %>% filter(participant_id == "P813", scene == "MixedPickingScene")
  stopifnot(identical(m813$skill_presses[m813$skill == "ekz_placement"], 2L),
            identical(m813$presses_with_skill_error[m813$skill == "ekz_placement"], 1L),
            isTRUE(m813$first_attempt_skill_error[m813$skill == "ekz_placement"]),
            identical(m813$skill_presses[m813$skill == "calculate_quantity"], 5L),
            identical(m813$presses_with_skill_error[m813$skill == "calculate_quantity"], 2L))
  # "025"/"019" Mixed visits never measured placement: no ekz_placement row.
  # Builds from layout "029" on (P813, and the later P814/P815) have Empf Kz 003 there.
  ekz_builds <- pilot_set$sessions$participant_id[
    coalesce(suppressWarnings(as.integer(pilot_set$sessions$layout_version)) >= 29L, FALSE)]
  stopifnot("P813" %in% ekz_builds,
            !any(pilot_set$skill_rates$scene == "MixedPickingScene" & pilot_set$skill_rates$skill == "ekz_placement" &
                   !pilot_set$skill_rates$participant_id %in% ekz_builds))
  # Mixed position_1 is split by Empf Kz in the item and phase charts, never pooled.
  pi <- read_csv(file.path(pilot_dir, "items.csv"), show_col_types = FALSE,
                 col_types = cols(.default = col_guess(), empf_kz = col_character(), layout_version = col_character()))
  mi <- pi %>% filter(scene == "MixedPickingScene")
  stopifnot(identical(mi$empf_kz[mi$participant_id == "P813" & mi$item == "position_1"], "003"),
            isFALSE(mi$first_attempt_correct[mi$participant_id == "P813" & mi$item == "position_1"]),
            identical(mi$attempts[mi$participant_id == "P813" & mi$item == "position_1"], 4),
            all(mi$empf_kz[!mi$participant_id %in% ekz_builds | mi$item == "position_2"] == "000"),
            all(is.na(pi$empf_kz[pi$kind == "quiz"])))
  mx <- as.character(pilot_set$item_shares$x_label[pilot_set$item_shares$scene == "MixedPickingScene"])
  # P816 ("034") adds the EKZ place help level (#108, 8x), so Empf Kz 003 is
  # split once more: places hidden ("029"-"033") / places after error ("034").
  stopifnot(all(c("Mixed\nposition_1\n[Empf Kz 000]", "Mixed\nposition_1\n[Empf Kz 003, places hidden]",
                  "Mixed\nposition_1\n[Empf Kz 003, places after error]", "Mixed\nposition_2") %in% mx),
            !"Mixed\nposition_1" %in% mx,
            any(grepl("set mixes Mixed position_1 Empf Kz", p_warn)),
            !any(grepl("EKZ_PLACEMENT where the app does not rate", p_warn)),
            !any(grepl("line.empf_kz on only|line.empf_kz in a log|log without line.empf_kz|line.empf_kz [0-9]", p_warn)))
  stopifnot(all(c("Mixed\nposition 1\n[Empf Kz 000]", "Mixed\nposition 1\n[Empf Kz 003, places hidden]",
                  "Mixed\nposition 1\n[Empf Kz 003, places after error]") %in% slots))
  ppe <- read_csv(file.path(pilot_dir, "process_by_position.csv"), show_col_types = FALSE,
                  col_types = cols(.default = col_guess(), empf_kz = col_character()))
  stopifnot(identical(ppe$empf_kz[ppe$participant_id == "P813" & ppe$scene == "MixedPickingScene"], c("003", "000")),
            all(ppe$empf_kz[ppe$scene == "BasicPickingScene"] == "003"),
            all(ppe$empf_kz[ppe$scene == "ComplexScene"] == "000"))

  # 8x. EKZ place help and Menu cue (#108/#109, decision 037, layout "034").
  #     P816 ran a "034" build with fixed presses (make_fake_data.R):
  #     Basic p1: EKZ_PLACEMENT, TOO_FEW, correct (outlines always on);
  #     Mixed p1: EKZ_PLACEMENT, EKZ_PLACEMENT, correct (outlines after the
  #     first EKZ_PLACEMENT); Mixed p2: correct.
  sc_rows <- pilot_set$skill_scaffolds
  stopifnot(nrow(sc_rows) == 2, all(sc_rows$skill == "ekz_placement"), all(sc_rows$from_layout == 34L),
            identical(sort(sc_rows$shown), c("after_error", "always")))
  # Press x skill pairs: Basic all 3 presses (ekz only), Mixed p1 presses 2 and 3.
  pr816 <- pilot_set$presses %>% filter(participant_id == "P816", event == "confirm_pressed") %>%
    arrange(t, line)
  b816 <- pr816 %>% filter(scene == "BasicPickingScene")
  m816 <- pr816 %>% filter(scene == "MixedPickingScene")
  stopifnot(nrow(b816) == 3, nrow(m816) == 4,
            identical(b816$ekz_opportunity, c(TRUE, FALSE, TRUE)),
            identical(m816$ekz_opportunity, c(TRUE, TRUE, TRUE, FALSE)))
  scf <- pilot_set$press_skill_scaffolded
  stopifnot(all(scf$skill == "ekz_placement"),
            setequal(scf$press_id[scf$press_id %in% pr816$press_id],
                     c(b816$press_id, m816$press_id[2:3])),
            # Older layouts never scaffold a skill.
            !any(scf$press_id %in% pilot_set$presses$press_id[pilot_set$presses$participant_id != "P816"]))
  sr816 <- pilot_set$skill_rates %>% filter(participant_id == "P816")
  # Basic: no ekz_placement test at all from "034"; quantity tested by all 3 presses, 1 error.
  stopifnot(!any(sr816$scene == "BasicPickingScene" & sr816$skill == "ekz_placement"),
            identical(sr816$skill_presses[sr816$scene == "BasicPickingScene" & sr816$skill == "calculate_quantity"], 3L),
            identical(sr816$presses_with_skill_error[sr816$scene == "BasicPickingScene" &
                                                        sr816$skill == "calculate_quantity"], 1L))
  # Mixed: ekz_placement = the first EKZ opportunity only (wrong); quantity = all 4 presses, 0 errors.
  me <- sr816 %>% filter(scene == "MixedPickingScene", skill == "ekz_placement")
  stopifnot(nrow(me) == 1, me$skill_presses == 1L, me$presses_with_skill_error == 1L,
            isTRUE(me$first_attempt_skill_error), me$ekz_places == "after_error",
            as.character(me$chart_scene) == "MixedPickingScene [places after error]",
            identical(sr816$skill_presses[sr816$scene == "MixedPickingScene" & sr816$skill == "calculate_quantity"], 4L))
  # Older "003" Mixed rates keep every EKZ opportunity and their own x level.
  m813r <- pilot_set$skill_rates %>% filter(participant_id == "P813", scene == "MixedPickingScene", skill == "ekz_placement")
  stopifnot(identical(m813r$skill_presses, 2L), m813r$ekz_places == "hidden",
            as.character(m813r$chart_scene) == "MixedPickingScene [places hidden]",
            all(c("MixedPickingScene", "MixedPickingScene [places hidden]", "MixedPickingScene [places after error]",
                  "BasicPickingScene [places hidden]", "BasicPickingScene [places shown]") %in%
                  levels(pilot_set$skill_rates$chart_scene)),
            # The study set has no "034": no extra level, no ekz_places split.
            !any(grepl("places", levels(skill_rates$chart_scene))))
  # items.csv / process_by_position.csv: ekz_places on Empf Kz 003 positions only.
  i816 <- pi %>% filter(participant_id == "P816")
  stopifnot(i816$ekz_places[i816$scene == "BasicPickingScene" & i816$item == "position_1"] == "always",
            i816$ekz_places[i816$scene == "MixedPickingScene" & i816$item == "position_1"] == "after_error",
            all(pi$ekz_places[pi$participant_id %in% c("P813", "P814", "P815") & pi$empf_kz %in% "003"] == "hidden"),
            all(is.na(pi$ekz_places[!pi$empf_kz %in% "003"])),
            "ekz_places" %in% names(ppe),
            all(ppe$ekz_places[ppe$participant_id == "P816" & ppe$scene == "BasicPickingScene"] == "always"))
  bx <- as.character(pilot_set$item_shares$x_label[pilot_set$item_shares$scene == "BasicPickingScene"])
  stopifnot(setequal(bx, c("Basic\nposition_1\n[places hidden]", "Basic\nposition_1\n[places shown]")),
            any(grepl("set mixes EKZ place help in BasicPickingScene", p_warn)),
            any(grepl("set mixes EKZ place help in MixedPickingScene", p_warn)),
            !any(grepl("set mixes EKZ place help", warn_lines)),
            all(pilot_set$data_quality$mixed_ekz_places), !any(data_quality$mixed_ekz_places))
  # P2: Basic of "034" never tests it; Mixed's first EKZ opportunity does (wrong).
  p2_tests <- pilot_set$obj_code_tests %>% filter(participant_id == "P816", objective_id == "P2")
  stopifnot(nrow(p2_tests) == 1, p2_tests$scene == "MixedPickingScene", isFALSE(p2_tests$correct))
  # Menu after a done order: one row per Basic/Mixed visit with task_completed,
  # menu_cue "highlight" only for "034", the times are active seconds.
  stopifnot(file.exists(file.path(pilot_dir, "menu_after_done.csv")),
            file.exists(file.path(output_dir, "menu_after_done.csv")))
  mad <- pilot_set$menu_after_done
  stopifnot(identical(sort(unique(mad$menu_cue[mad$participant_id == "P816"])), "highlight"),
            all(mad$menu_cue[mad$participant_id != "P816"] == "text"),
            all(menu_after_done$menu_cue == "text"),
            nrow(mad %>% filter(participant_id == "P816")) == 2,
            all(!is.na(mad$departed_t[mad$participant_id == "P816"])),
            all(mad$menu_target[mad$participant_id == "P816"] == c("MixedPickingScene", "ComplexScene")),
            all(mad$time_to_menu_after_done_s >= mad$time_to_menu_after_departed_s),
            all(pilot_set$data_quality$mixed_menu_cue), !any(data_quality$mixed_menu_cue),
            any(grepl("set mixes the Menu cue", p_warn)), !any(grepl("set mixes the Menu cue", warn_lines)))
  r816 <- mad %>% filter(participant_id == "P816", scene == "BasicPickingScene")
  stopifnot(abs(r816$time_to_menu_after_done_s -
                  (r816$menu_t - r816$tc_t - pilot_set$paused_between(r816$session_id, r816$tc_t, r816$menu_t))) < 0.002)
  # Station waits of "034" are grouped with "033" (4 s).
  stopifnot(all(pilot_set$station_waits$station_wait_group[pilot_set$station_waits$participant_id == "P816"] == "033_on"))

  # 8y. Snap-place grab restarts (#114). P817 ran a "033" build with the old
  #     grab logger: every carton let go into a Basic / Mixed position-1 snap
  #     place got a second grab_start 0.013 s after its grab_end (truth:
  #     make_fake_data.R snap_artifact_log). R drops each restart with the end
  #     paired to it and counts it; nobody else has any.
  sa <- snap_artifact_log %>% filter(pid == "P817")
  stopifnot(nrow(snap_artifact_log) == nrow(sa), nrow(sa) == 9,
            sum(sa$scene == "BasicPickingScene") == 6, sum(sa$scene == "MixedPickingScene") == 3)
  stopifnot(pq("snap_grab_artifacts", "P817") == 9,
            # Basic's 6 restarts were never ended in the log: not open grabs.
            pq("open_grabs", "P817") == 0,
            all(pilot_set$data_quality$snap_grab_artifacts[pilot_set$data_quality$participant_id != "P817"] == 0),
            all(data_quality$snap_grab_artifacts[data_quality$interaction_logged] == 0),
            is.na(dq("snap_grab_artifacts", "P104")))
  for (sc in c("BasicPickingScene", "MixedPickingScene")) {
    v <- pilot_set$visits %>% filter(participant_id == "P817", scene == sc)
    sum_row <- pilot_set$summary_rows %>% filter(participant_id == "P817", scene == sc)
    n_art <- sum(sa$scene == sc)
    objs <- pilot_set$grabs %>% filter(participant_id == "P817", scene == sc)
    stopifnot(nrow(v) == 1, nrow(sum_row) == 1, n_art > 0,
              # The app's summary grabs count the restarts, R's grabs do not
              # (and the summary seam stays clean, 8q: no "differs" warning).
              v$grabs == sum_row$grabs - n_art,
              v$snap_grab_artifacts == n_art, v$open_grabs == 0,
              # regrabs as with the fixed logger: the restarts add none.
              v$regrabs == sum_row$grabs - n_art - n_distinct(objs$object_id),
              !any(objs$open))
  }
  # The hand grabs of the restarted cartons stay (one per carton in the box).
  stopifnot(all(sa$obj %in% pilot_set$grabs$object_id[pilot_set$grabs$participant_id == "P817"]))
  # P817 is a "033" log and never took a snapped carton out again: neither
  # the "034"-or-newer warning nor the lost-grab warning fires; the column is in wide.csv.
  stopifnot(!any(grepl("grab restart(s) <", c(p_warn, warn_lines), fixed = TRUE)),
            !any(grepl("snap-place grab restart was open", c(p_warn, warn_lines), fixed = TRUE)),
            pw8("snap_grab_artifacts", "P817") == 9)

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
  # No paper file here (only research/ was copied): no error, and a joined
  # file left from an earlier run is removed, never shown as current.
  dir.create(output_dir, showWarnings = FALSE)
  writeLines("stale", file.path(output_dir, "wide_with_paper.csv"))
  source("analysis/analyse.R", local = TRUE)
  stopifnot(!file.exists(file.path(output_dir, "wide_with_paper.csv")))
  # No pilot logs in this copy: no pilot set, and no old pilot folder left.
  stopifnot(is.null(pilot_set), !dir.exists(file.path(output_dir, "pilot")))
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

  # 8r. Wrong folders (decision 023), on a temp copy: P801's files moved to
  #     research/, P101's into research/pilot/. The ID decides the set (no
  #     data lost), and each move is warned in the set that keeps the data.
  moved <- file.path(tempdir(), "selftest_moved")
  unlink(moved, recursive = TRUE)
  dir.create(moved)
  file.copy("analysis/data/fake/research", moved, recursive = TRUE)
  mv <- function(pattern, from, to) {
    f <- list.files(file.path(moved, "research", from), pattern = pattern, full.names = TRUE)
    stopifnot(length(f) == 2, all(file.rename(f, file.path(moved, "research", to, basename(f)))))
  }
  mv("^P801_", "pilot", ".")
  mv("^P101_", ".", "pilot")
  data_dir <- moved
  output_dir <- file.path(tempdir(), "selftest_moved_out")
  source("analysis/analyse.R", local = TRUE)
  s_warn <- readLines(file.path(output_dir, "warnings.txt"), encoding = "UTF-8")
  p_warn <- readLines(file.path(output_dir, "pilot", "warnings.txt"), encoding = "UTF-8")
  stopifnot(any(grepl("study ID in research/pilot/", s_warn) & grepl("P101", s_warn)),
            any(grepl("pilot ID outside research/pilot/", p_warn) & grepl("P801", p_warn)),
            !any(grepl("pilot ID outside", s_warn)), !any(grepl("study ID in research/pilot", p_warn)))
  stopifnot("P101" %in% wide$participant_id, !"P801" %in% wide$participant_id,
            "P801" %in% pilot_set$wide$participant_id, !"P101" %in% pilot_set$wide$participant_id)
  unlink(c(moved, output_dir), recursive = TRUE)

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
