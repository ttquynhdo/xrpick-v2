# selftest.R
#
# Checks that analyse.R handles every edge case in the fake data
# (make_fake_data.R). Run it after any change to analyse.R, make_fake_data.R
# or the logger format.
#
# Run from the repo root:   Rscript analysis/selftest.R
# Expected last line:       selftest OK

# Fake data in, selftest outputs in their own folder, so a real run's
# outputs in analysis/output/ are never overwritten by the test.
data_dir <- "analysis/data/fake"
output_dir <- "analysis/output/selftest"

# 1. Make fresh fake data (same seed every time), then run the analysis.
source("analysis/make_fake_data.R")
source("analysis/analyse.R")

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

# 5. P107 entered ComplexScene but never confirmed: the rate is NA, not 0.
p107_complex <- visits$error_rate[visits$participant_id == "P107" &
                                    visits$scene == "ComplexScene"]
stopifnot(length(p107_complex) == 1, is.na(p107_complex))
stopifnot(is.na(wide$error_rate_ComplexScene[wide$participant_id == "P107"]))

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

cat("selftest OK\n")
