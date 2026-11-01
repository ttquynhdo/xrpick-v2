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

  # 2. One row per real participant: P101-P110; the test ID P950 is dropped.
  stopifnot(nrow(wide) == 10)
  stopifnot(!"P950" %in% wide$participant_id)
  stopifnot(!"P950" %in% data_quality$participant_id)

  # 3. P103 crashed and came back in a second session.
  stopifnot(isTRUE(wide$multi_session[wide$participant_id == "P103"]))
  stopifnot(sum(wide$multi_session) == 1)

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
