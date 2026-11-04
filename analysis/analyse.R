# analyse.R
#
# Entry point of the analysis. Run this file; it runs analyse_set.R (the
# analysis itself) once per participant set (decision 023, backlog #75):
#
#   study  P001-P799   logs in research/        -> output_dir   (analysis/output/)
#   pilot  P800-P899   logs in research/pilot/  -> output_dir/pilot/
#   test   P000, P900-P999 in research/practice/: dropped, never analysed
#
# The two sets never mix: each run reads only its own IDs (by ID range; a log
# in the wrong folder is kept in its set and warned) and writes the same
# outputs (charts, items.csv, wide.csv, wide_with_paper.csv, warnings.txt)
# into its own folder. Paper-form rows join the set of their participant_id.
#
# Run from the repo root:   Rscript analysis/analyse.R
# Or in RStudio: open the repo folder as the working directory, then Source.
# Set data_dir / output_dir before sourcing to analyse one wave, e.g.
#   data_dir <- "analysis/data/wave1"
#
# After the run the study tables (wide, visits, items, ...) are in the
# environment that sourced this file, the pilot tables in the environment
# pilot_set (e.g. pilot_set$wide). Packages: tidyverse and jsonlite only.

# Works from the repo root and from inside analysis/ (same rule as analyse_set.R).
analysis_folder <- if (file.exists("skill_map.csv")) "." else "analysis"
if (!exists("data_dir")) data_dir <- file.path(analysis_folder, "data")
if (!exists("output_dir")) output_dir <- file.path(analysis_folder, "output")

# Same fake-data guard as analyse_set.R, before anything is written.
if (grepl("fake", data_dir, ignore.case = TRUE) && !isTRUE(get0("allow_fake"))) {
  stop("data_dir '", data_dir, "' is FAKE data. Set data_dir to a real wave, e.g. ",
       "data_dir <- \"analysis/data/wave1\" (or allow_fake <- TRUE for tests only).")
}

# Which sets have logs? The app names every file <participant_id>_<session_id>_...,
# so the file name tells the set without reading the file.
set_paths <- list.files(data_dir, pattern = "_events\\.jsonl$", recursive = TRUE, full.names = TRUE)
if (!grepl("fake", data_dir)) set_paths <- set_paths[!grepl("/fake/", set_paths)]
set_files <- basename(set_paths)
has_pilot <- any(grepl("^P8[0-9]{2}_", set_files))
has_study <- any(!grepl("^P(000|8[0-9]{2}|9[0-9]{2})_", set_files))
if (!has_pilot && !has_study) stop("No study or pilot *_events.jsonl files found under ", data_dir)
set_script <- file.path(analysis_folder, "analyse_set.R")

# 1. Pilot set: in its own environment (pilot_set), so its tables never
#    overwrite the study's. Its parent is this environment, so settings set
#    before sourcing (allow_fake, paper_time_zone) reach it too.
pilot_output_dir <- file.path(output_dir, "pilot")
if (has_pilot) {
  message("\n==== Pilot set (P800-P899) -> ", pilot_output_dir, " ====")
  pilot_set <- new.env(parent = environment())
  pilot_set$analysis_group <- "pilot"
  pilot_set$data_dir <- data_dir
  pilot_set$output_dir <- pilot_output_dir
  sys.source(set_script, envir = pilot_set)
} else {
  # No pilot logs: old pilot outputs must not look current.
  message("No pilot logs (P800-P899) under ", data_dir, ": ", pilot_output_dir, " removed if it existed.")
  unlink(pilot_output_dir, recursive = TRUE)
  pilot_set <- NULL
}

# 2. Study set: in this environment, so wide, visits, items ... are at hand
#    after the run (and selftest.R can check them).
if (has_study) {
  message("\n==== Study set (P001-P799) -> ", output_dir, " ====")
  analysis_group <- "study"
  sys.source(set_script, envir = environment())
} else {
  message("No study logs (P001-P799) under ", data_dir, ": the study set was not analysed.")
}
