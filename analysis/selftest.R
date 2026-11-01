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

  # 2. One row per real participant: P101-P111; the test ID P950 is dropped.
  stopifnot(nrow(wide) == 11)
  stopifnot(!"P950" %in% wide$participant_id)
  stopifnot(!"P950" %in% data_quality$participant_id)

  # 3. P103 crashed and came back in a second session (and so did P111, the
  #    declined resume in 8i).
  stopifnot(isTRUE(wide$multi_session[wide$participant_id == "P103"]))
  stopifnot(identical(sort(wide$participant_id[wide$multi_session]), c("P103", "P111")))

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
