# analyse_set.R
#
# The analysis of ONE participant set. Don't run this file directly: run
# analyse.R, which runs it once for the study set (P001-P799, output_dir) and
# once for the pilot set (P800-P899, output_dir/pilot/), decision 023.
# analysis_group ("study" / "pilot") says which set; every table below holds
# only that set's IDs, so pilot and study never mix.
#
# Turns research logs from the xpick app into charts and tables for
# research questions 1 (improvement per skill), 2 (hard scenes, common errors),
# 3 (where the time of a position goes, from the #29 interaction events),
# 4 and 5 (paper SSQ / NASA-TLX joined by participant_id, #31; only when a
# paper_forms.csv exists) and 6 (data quality). There is no pose, frame-rate
# or tracking data in the logs (decision 022): Q4 uses the SSQ change with
# headset removals, pauses, early end and active time only. Rules: docs/superpowers/specs/2026-11-01-research-logger-design.md
# The ConceptScene field tour (#82, section 13c) gives time per step and
# "Zurueck" presses per step (tour_steps.csv); Concept data of different
# layout_version are never pooled (section 8b, decision 024).
# section 8. Log format: docs/research-logging-spec.md, schema_version 2
# (decision 021). Logs with schema_version 1 are read for sessions, visits and
# pauses, but their outcome events (v1 catalogue) are not: NA, not measured.
#
# events.jsonl is the source of truth: every per-visit number is rebuilt from
# the events. summary.csv is only used as a cross-check of the C# code.
#
# Input:  every *_events.jsonl / *_summary.csv / paper_forms*.csv under
#         data_dir (sub folders included), rows of this set's IDs only.
# Output: output_dir (set by analyse.R).
#
# Packages: tidyverse and jsonlite only.

suppressPackageStartupMessages({
  library(tidyverse)
  library(jsonlite)
})

# ---- 1. Settings and paths -------------------------------------------------

# Works from the repo root and from inside analysis/ (same rule as make_fake_data.R).
analysis_folder <- if (file.exists("skill_map.csv")) "." else "analysis"

# analyse.R sets data_dir, output_dir and analysis_group before sourcing.
if (!exists("data_dir")) data_dir <- file.path(analysis_folder, "data")
if (!exists("output_dir")) output_dir <- file.path(analysis_folder, "output")
if (!exists("analysis_group")) analysis_group <- "study"
stopifnot(analysis_group %in% c("study", "pilot"))
is_pilot_set <- analysis_group == "pilot"
message("set        = ", analysis_group)
message("data_dir   = ", data_dir)
message("output_dir = ", output_dir)

# Fake data must never end up in a real report by accident (e.g. data_dir
# left over from selftest.R). Only a caller that sets allow_fake <- TRUE
# (selftest.R, in its own environment) may analyse a "fake" folder.
if (grepl("fake", data_dir, ignore.case = TRUE) && !isTRUE(get0("allow_fake"))) {
  stop("data_dir '", data_dir, "' is FAKE data. Set data_dir to a real wave, e.g. ",
       "data_dir <- \"analysis/data/wave1\" (or allow_fake <- TRUE for tests only).")
}
dir.create(output_dir, recursive = TRUE, showWarnings = FALSE)

# Tolerance for comparing times: events and summary.csv round differently
# (up to 0.001 s), so a difference up to 0.002 s is not a mismatch.
time_tolerance_s <- 0.002
# A gap this long between two sessions of one ID is suspicious (two people
# typed under one ID?). Only a warning; the data are kept.
gap_warn_h <- 2

# Colour-blind-safe palette (Okabe-Ito) for skills and other small groups.
okabe_ito <- c("#E69F00", "#56B4E9", "#009E73", "#0072B2",
               "#D55E00", "#CC79A7", "#F0E442", "#999999", "#000000")

# All warnings go to the console now and into warnings.txt at the end.
warning_lines <- character(0)
warn <- function(...) {
  msg <- paste0(...)
  warning_lines <<- c(warning_lines, msg)
  message("WARNING: ", msg)
}

# Small helpers for JSON values: a JSON null arrives as NULL in R.
as_num <- function(x) if (is.null(x)) NA_real_ else as.numeric(x)
as_int <- function(x) if (is.null(x)) NA_integer_ else as.integer(x)
as_chr <- function(x) if (is.null(x)) NA_character_ else as.character(x)
as_lgl <- function(x) if (is.null(x)) NA else as.logical(x)

# ISO 8601 UTC text ("2026-11-03T08:00:00.006Z") -> R date-time in UTC.
parse_utc <- function(x) {
  as.POSIXct(sub("Z$", "", x), format = "%Y-%m-%dT%H:%M:%OS", tz = "UTC")
}

# Test IDs (P000, P900-P999) never enter the analysis (design section 4).
is_test_id <- function(pid) grepl("^P(000|9[0-9]{2})$", pid)
is_valid_id <- function(pid) grepl("^P[0-9]{3}$", pid)
# Pilot IDs (P800-P899, decision 023) run like participants but form their own
# set. in_set() keeps the rows of this run's set: the pilot run keeps pilot
# IDs only; the study run keeps everything else (test and invalid IDs are
# dropped below, with their warnings, only once: in the study run).
is_pilot_id <- function(pid) grepl("^P8[0-9]{2}$", pid)
in_set <- function(pid) if (is_pilot_set) coalesce(is_pilot_id(pid), FALSE) else !coalesce(is_pilot_id(pid), FALSE)

# ---- 2. Config: scene order, skill map, skills per scene --------------------

# Scene order comes from scenes.csv, so charts keep build order after renames.
scenes <- read_csv(file.path(analysis_folder, "scenes.csv"), show_col_types = FALSE) %>%
  arrange(build_order)
scene_levels <- scenes$scene

# Which skill each error code tests, and which skills each scene tests.
skill_map <- read_csv(file.path(analysis_folder, "skill_map.csv"), show_col_types = FALSE)
scene_skills <- read_csv(file.path(analysis_folder, "scene_skills.csv"), show_col_types = FALSE)
# Scenes with a confirm button or quiz (the only ones with outcome data).
outcome_scenes <- unique(scene_skills$scene)

# Skill and "scaffolded" flag per item (a quiz question_id or "position_<n>").
# WRONG_ANSWER has no fixed skill in skill_map.csv: it gets the skill of its
# question here. scaffolded = TRUE only where the CSV says true (Complex
# position 1: the remainder quiz hands the trainee the numbers); an empty cell
# or an item not in the CSV counts as not scaffolded.
item_skills <- read_csv(file.path(analysis_folder, "item_skills.csv"), show_col_types = FALSE,
                        col_types = cols(.default = col_character())) %>%
  mutate(skill = na_if(trimws(skill), ""),
         scaffolded = coalesce(tolower(trimws(scaffolded)) == "true", FALSE))

# A position item without a skill in item_skills.csv (all of them until the
# study lead decides D28) is shown as "picking": the whole position right at
# the first press, any error code.
position_skill_default <- "picking"

# Scenes with the red button (rated presses, confirm_ignored, station rides).
# ConceptScene has only the quiz (no red button since #73), WelcomeScene neither.
red_button_scenes <- c("BasicPickingScene", "MixedPickingScene", "ComplexScene")

# #29 interaction events. grab_start/_end exist only for packages with
# PackageData: ConceptScene (label board) and the picking scenes. The
# WelcomeScene practice box has none, so its grabs are NA (not measured),
# never 0. container_enter/_exit exist only where there is an order
# container: the picking scenes (the red-button scenes).
grab_scenes <- c("ConceptScene", red_button_scenes)
container_scenes <- red_button_scenes

# ---- 3. Read every events file -----------------------------------------------

# Find the files. Fake data are skipped unless data_dir points into fake/,
# so fake P101 can never mix with a real P101.
events_files <- list.files(data_dir, pattern = "_events\\.jsonl$", recursive = TRUE,
                           full.names = TRUE)
if (!grepl("fake", data_dir)) events_files <- events_files[!grepl("/fake/", events_files)]
if (length(events_files) == 0) stop("No *_events.jsonl files found under ", data_dir)
message("Reading ", length(events_files), " events file(s) from ", data_dir)

# One line = one event. A line that is not valid JSON (e.g. cut off by a
# crash) is skipped with a warning instead of stopping the whole analysis.
# The warning waits until we know whose file it is (below), so it lands in
# the warnings of that file's set only.
unreadable_lines <- tibble(path = character(), line = integer())
read_events_file <- function(path) {
  lines <- readLines(path, encoding = "UTF-8", warn = FALSE)
  keep <- nzchar(trimws(lines))
  line_no <- seq_along(lines)[keep]
  records <- map2(lines[keep], line_no, function(txt, i) {
    tryCatch(fromJSON(txt, simplifyVector = FALSE), error = function(e) {
      unreadable_lines <<- bind_rows(unreadable_lines, tibble(path = path, line = i))
      NULL
    })
  })
  ok <- !map_lgl(records, is.null)
  records <- records[ok]
  tibble(
    path = path,
    file = basename(path),
    folder = basename(dirname(path)),
    line = line_no[ok],
    t = map_dbl(records, ~ as_num(.x$t)),
    utc = map_chr(records, ~ as_chr(.x$utc)),
    participant_id = map_chr(records, ~ as_chr(.x$participant_id)),
    session_id = map_chr(records, ~ as_chr(.x$session_id)),
    scene = map_chr(records, ~ as_chr(.x$scene)),
    scene_visit = map_int(records, ~ as_int(.x$scene_visit)),
    event = map_chr(records, ~ as_chr(.x$event)),
    data = map(records, ~ if (is.null(.x$data)) list() else .x$data)
  )
}
events_raw <- map(events_files, read_events_file) %>% list_rbind()

# Keep only this set's rows (decision 023). The study run sees the pilot IDs
# only to count them for its report; the pilot run never sees study, test or
# invalid IDs, so their warnings are written once, in the study run.
pilot_ids_seen <- sort(unique(events_raw$participant_id[is_pilot_id(events_raw$participant_id)]))
readable_paths <- unique(events_raw$path)
events_raw <- events_raw %>% filter(in_set(participant_id))
# Unreadable lines: warned in the set that owns the file (a file with no
# readable line at all belongs to the study run).
for (i in seq_len(nrow(unreadable_lines))) {
  r <- unreadable_lines[i, ]
  if (r$path %in% events_raw$path || (!is_pilot_set && !r$path %in% readable_paths)) {
    warn("unreadable line ", r$line, " in ", basename(r$path), " (skipped)")
  }
}

# The same session in two files (e.g. a copy in a backup folder) would count
# every visit and error twice. Keep the longest file per session_id, drop the
# others, and name all paths. The participant is flagged in data_quality.
# Returns the paths to drop; `kind` is "events" or "summary" for the message.
pick_longest_files <- function(rows_per_file, kind) {
  dup <- rows_per_file %>% group_by(session_id) %>% filter(n_distinct(path) > 1) %>%
    arrange(desc(n_rows), path, .by_group = TRUE) %>%
    mutate(keep = row_number() == 1) %>% ungroup()
  unit <- if (kind == "events") " lines" else " rows"
  for (sid in unique(dup$session_id)) {
    d <- dup %>% filter(session_id == sid)
    warn("duplicate ", kind, " files for session ", sid, " (", d$participant_id[1], "): kept ",
         d$path[d$keep], " (", d$n_rows[d$keep], unit, "), dropped ",
         paste0(d$path[!d$keep], " (", d$n_rows[!d$keep], unit, ")", collapse = ", "))
  }
  dup %>% filter(!keep) %>% select(participant_id, session_id, path)
}
dropped_event_files <- events_raw %>%
  filter(!is.na(session_id)) %>%
  count(participant_id, session_id, path, name = "n_rows") %>%
  pick_longest_files("events")
events_raw <- events_raw %>% filter(!path %in% dropped_event_files$path)

# ---- 4. Drop test IDs and invalid IDs ------------------------------------------

# Test IDs are dropped wherever they turn up (practice/ or by mistake in research/).
test_rows <- events_raw %>% filter(is_test_id(participant_id))
if (nrow(test_rows) > 0) {
  dropped <- test_rows %>% distinct(participant_id, file)
  message("Dropped test ID file(s): ", paste(dropped$file, collapse = ", "))
  outside_practice <- test_rows %>% filter(folder != "practice") %>% distinct(participant_id)
  if (nrow(outside_practice) > 0) {
    warn("test ID ", paste(outside_practice$participant_id, collapse = ", "),
         " found outside research/practice/ (dropped)")
  }
}
bad_id_rows <- events_raw %>% filter(!is_valid_id(participant_id))
if (nrow(bad_id_rows) > 0) {
  warn("participant_id not in format P + 3 digits, dropped: ",
       paste(unique(bad_id_rows$participant_id), collapse = ", "))
}
real_in_practice <- events_raw %>%
  filter(folder == "practice", is_valid_id(participant_id), !is_test_id(participant_id),
         !is_pilot_id(participant_id)) %>%
  distinct(participant_id)
if (nrow(real_in_practice) > 0) {
  warn("real ID in practice/ (kept, check it): ",
       paste(real_in_practice$participant_id, collapse = ", "))
}
# Pilot IDs belong in research/pilot/, study IDs never there (decision 023).
# A log in the wrong folder stays in the set of its ID (the ID decides, so no
# data is lost), but is named: a copy mistake or a mistyped code.
pilot_outside <- events_raw %>% filter(is_pilot_id(participant_id), folder != "pilot") %>%
  distinct(participant_id)
if (nrow(pilot_outside) > 0) {
  warn("pilot ID outside research/pilot/ (kept in the pilot set, check it): ",
       paste(pilot_outside$participant_id, collapse = ", "))
}
study_in_pilot <- events_raw %>%
  filter(folder == "pilot", is_valid_id(participant_id), !is_test_id(participant_id),
         !is_pilot_id(participant_id)) %>%
  distinct(participant_id)
if (nrow(study_in_pilot) > 0) {
  warn("study ID in research/pilot/ (kept in the study set, check it): ",
       paste(study_in_pilot$participant_id, collapse = ", "))
}
events <- events_raw %>% filter(is_valid_id(participant_id), !is_test_id(participant_id))
if (nrow(events) == 0) stop("No ", analysis_group, " events left after dropping test IDs.")

# One file must hold one participant and one session.
mixed_files <- events %>% group_by(path, file) %>%
  summarise(n_pid = n_distinct(participant_id), n_sid = n_distinct(session_id), .groups = "drop") %>%
  filter(n_pid > 1 | n_sid > 1)
if (nrow(mixed_files) > 0) {
  warn("file(s) mixing participants or sessions: ", paste(mixed_files$file, collapse = ", "))
}

# Unknown event names are kept but reported (maybe a newer logger version).
# The list is the v2 catalogue (spec section 4); not all of them are used yet.
known_events <- c("header", "session_start", "session_end", "scene_enter", "scene_exit",
                  "menu_select", "end_control_opened", "end_control_cancelled",
                  "app_pause", "app_resume", "unity_error", "unity_error_cap_reached",
                  "crash_recovered", "confirm_pressed", "confirm_ignored", "position_completed",
                  "task_completed", "quiz_answer", "quiz_unlocked", "tour_step",
                  "station_move_start", "station_move_arrived", "welcome_practice_done",
                  "grab_start", "grab_end", "container_enter", "container_exit",
                  "concept_hint_shown", "concept_hint_cleared")
# Decision 022 dropped #30 (pose, perf) and #74 (floor drops, input mode,
# tracking): the app must never write these. If one turns up, the build is not
# the study build (or the decision was reopened): warn, don't read them.
dropped_events_022 <- c("perf_sample", "object_dropped_floor", "input_mode_change",
                        "tracking_lost", "tracking_regained")
seen_022 <- events %>% filter(event %in% dropped_events_022) %>% distinct(participant_id, event)
if (nrow(seen_022) > 0) {
  warn("event(s) the app should not write since decision 022 (not read): ",
       paste0(seen_022$participant_id, " ", seen_022$event, collapse = ", "))
}
unknown_events <- setdiff(unique(events$event), c(known_events, dropped_events_022))
if (length(unknown_events) > 0) {
  message("Note: event names not in the v2 catalogue: ", paste(unknown_events, collapse = ", "))
}

# ---- 5. Sessions: build, start, end, how they ended -------------------------------

# Header line: build and schema_version of the app that wrote the file.
# session_start_mode (added later, NA in older logs): "auto" = started by
# ResearchSettings.autoStart, "manual" = started by the keypad (#27).
# language (#11): "DE" / "EN", fixed for the whole session (chosen on the
# keypad before the code; there is no language_change event). NA = a log from
# a build before #11, which wrote null.
# Schema 2 adds layout_version ("019" = three-lane station, decision 019; "025" = same plus the Concept tour with Zurück, decisions 024/025;
# "029" = plus the Concept containers that count and give hint tags, decision 030 / #100) and
# event_groups (the groups this build emits). A group missing from
# event_groups means "not measured" (NA), never 0. Logs without the key
# (schema 1) keep event_groups NA.
headers <- events %>% filter(event == "header") %>%
  transmute(session_id,
            participant_id, file,
            has_header = TRUE,
            header_utc = parse_utc(utc),
            build = map_chr(data, ~ as_chr(.x$build)),
            schema_version = map_int(data, ~ as_int(.x$schema_version)),
            layout_version = map_chr(data, ~ as_chr(.x$layout_version)),
            groups_logged = map_lgl(data, ~ "event_groups" %in% names(.x)),
            event_groups = map_chr(data, ~ if (is.null(.x$event_groups)) NA_character_ else
              paste(unlist(.x$event_groups), collapse = ";")),
            session_start_mode = map_chr(data, ~ as_chr(.x$session_start_mode)),
            language = map_chr(data, ~ as_chr(.x$language)))
if (any(duplicated(headers$session_id))) warn("a session has more than one header line")

# Schema 1 logs (before decision 021) use the old outcome catalogue (gg/vpe
# container keys, NO_ANSWER, quiz without question_id). They are not read as
# outcomes: the session counts for data quality, its outcome columns are NA.
old_schema <- headers %>% filter(schema_version %in% 1L)
for (i in seq_len(nrow(old_schema))) {
  warn("schema_version 1 log ", old_schema$file[i], " (", old_schema$participant_id[i],
       "): v1 outcome events are not read, ",
       "its outcome columns are NA (not measured)")
}
headers <- headers %>% select(-file, -participant_id)

# session_start: the resume offer after a crash (#27). resume_offered_scene =
# the scene the keypad offered (null = nothing offered), resume_accepted =
# the answer (null when nothing was offered). Logs written before #27 lack
# both keys: resume_logged FALSE, and the values stay NA (not measured).
# ended_session_warning (added in the #27 fix wave): TRUE = the "already
# finished" warning was shown and the facilitator pressed "Trotzdem starten".
# Older logs lack the key: warning_logged FALSE, value NA.
session_starts <- events %>% filter(event == "session_start") %>%
  transmute(session_id,
            resume_logged = map_lgl(data, ~ "resume_offered_scene" %in% names(.x)),
            resume_offered_scene = map_chr(data, ~ as_chr(.x$resume_offered_scene)),
            resume_accepted = map_lgl(data, ~ as_lgl(.x$resume_accepted)),
            warning_logged = map_lgl(data, ~ "ended_session_warning" %in% names(.x)),
            ended_session_warning = map_lgl(data, ~ as_lgl(.x$ended_session_warning))) %>%
  distinct(session_id, .keep_all = TRUE)

# A session ended cleanly only if it has session_end. No session_end = crash
# (Quest often skips the quit callback, design section 4).
session_ends <- events %>% filter(event == "session_end") %>%
  transmute(session_id, end_reason = map_chr(data, ~ as_chr(.x$reason)))

sessions <- events %>%
  filter(event != "crash_recovered") %>%  # written later, by the next session
  group_by(participant_id, session_id) %>%
  summarise(start_utc = min(parse_utc(utc)), last_utc = max(parse_utc(utc)),
            last_t = max(t, na.rm = TRUE), .groups = "drop") %>%
  left_join(headers, by = "session_id") %>%
  left_join(session_starts, by = "session_id") %>%
  left_join(session_ends, by = "session_id") %>%
  mutate(has_header = coalesce(has_header, FALSE),
         resume_logged = coalesce(resume_logged, FALSE),
         warning_logged = coalesce(warning_logged, FALSE),
         end_status = if_else(is.na(end_reason), "crash", end_reason)) %>%
  arrange(participant_id, start_utc) %>%
  group_by(participant_id) %>%
  mutate(session_no = row_number(),
         # Hours between the end of the previous session and the start of this one.
         gap_h = as.numeric(difftime(start_utc, lag(last_utc), units = "hours")),
         # Any earlier session crashed (for repeat_after_crash).
         crash_before = lag(cumsum(end_status == "crash"), default = 0) > 0,
         # The immediately previous session crashed (the only case the keypad offers a resume).
         prev_crash = lag(end_status, default = "none") == "crash") %>%
  ungroup()

missing_header <- sessions %>% filter(!has_header)
if (nrow(missing_header) > 0) {
  warn("session(s) without header line: ", paste(missing_header$session_id, collapse = ", "))
}
# build null = the ResearchBuildInfo resource was missing in the app build;
# "unknown" = git failed when the app was built. Both: builds can't be told apart.
null_build <- sessions %>% filter(has_header, is.na(build))
if (nrow(null_build) > 0) {
  warn("header with build null (ResearchBuildInfo missing in the app build): ",
       paste(null_build$session_id, collapse = ", "))
}
unknown_build <- sessions %>% filter(build %in% "unknown")
if (nrow(unknown_build) > 0) {
  warn("build 'unknown' (git failed when the app was built): ",
       paste(unknown_build$session_id, collapse = ", "))
}
# A real participant's session should be started from the keypad (#27). "auto"
# means the study build still had autoStart on (backlog #27).
auto_real <- sessions %>% filter(session_start_mode %in% "auto")
for (i in seq_len(nrow(auto_real))) {
  warn("session_start_mode 'auto' for real ID ", auto_real$participant_id[i], " (session ",
       auto_real$session_id[i], "): started by autoStart, not by the keypad")
}

# Language (#11). Research IDs must run in German: the keypad forces "DE"
# for them, so "EN" means the session bypassed the keypad (e.g. autoStart) or
# a bug; the trainee saw English texts, so their data are not comparable.
# Null is allowed (logs from before #11) but counted, so the report says how
# many sessions have no recorded language.
# Pilot IDs (decision 023) keep the language chosen on the keypad (EN pilot,
# a German expert in DE): no warning in the pilot set.
non_de <- sessions %>% filter(!is_pilot_set, has_header, !is.na(language), language != "DE")
for (i in seq_len(nrow(non_de))) {
  warn("language '", non_de$language[i], "' for real ID ", non_de$participant_id[i], " (session ",
       non_de$session_id[i], "): research sessions must run in DE")
}
null_lang <- sessions %>% filter(has_header, is.na(language))
if (nrow(null_lang) > 0) {
  message("Note: ", nrow(null_lang), " session(s) with header language null (logs from before #11): ",
          paste(unique(null_lang$participant_id), collapse = ", "))
}

# Resume offers (#27). A declined offer means the trainee started again at
# WelcomeScene and repeated lessons already seen: their later visits are
# second exposures (repeat_after_crash), not first attempts.
declined <- sessions %>% filter(resume_accepted %in% FALSE)
for (i in seq_len(nrow(declined))) {
  warn("resume declined by ", declined$participant_id[i], " (session ", declined$session_id[i],
       ", offered ", declined$resume_offered_scene[i], "): the trainee repeated lessons already seen")
}
# Contract: resume_accepted is null exactly when resume_offered_scene is null.
bad_resume <- sessions %>% filter(resume_logged, is.na(resume_offered_scene) != is.na(resume_accepted))
for (i in seq_len(nrow(bad_resume))) {
  warn("session_start resume fields inconsistent for ", bad_resume$participant_id[i], " (session ",
       bad_resume$session_id[i], "): resume_offered_scene = ", bad_resume$resume_offered_scene[i],
       ", resume_accepted = ", bad_resume$resume_accepted[i])
}
# An offer follows a crash of the same ID's newest earlier session; if the
# session right before is not a crash, the crashed session's files are
# probably missing from this copy.
orphan_offer <- sessions %>% filter(!is.na(resume_offered_scene), !prev_crash)
for (i in seq_len(nrow(orphan_offer))) {
  warn("resume offered to ", orphan_offer$participant_id[i], " (session ", orphan_offer$session_id[i],
       ") but the session right before it in the data is not a crash (files missing?)")
}
# The facilitator started a session although the ID already had a finished
# session ("Trotzdem starten"): a second run of the course, or a mistyped code.
override <- sessions %>% filter(ended_session_warning %in% TRUE)
for (i in seq_len(nrow(override))) {
  warn("ended-session warning overridden for ", override$participant_id[i], " (session ",
       override$session_id[i], "): the ID already had a finished session; check it is the same person")
}

# Clock check: utc - header utc should equal t. A gap over 1 s means the wall
# clock jumped or t stopped (e.g. during headset sleep; check on Quest, L19).
clock_drift <- events %>%
  filter(!is.na(t), event != "crash_recovered") %>%
  inner_join(headers %>% select(session_id, header_utc), by = "session_id") %>%
  mutate(drift_s = as.numeric(difftime(parse_utc(utc), header_utc, units = "secs")) - t) %>%
  group_by(participant_id, session_id) %>%
  summarise(max_drift_s = max(abs(drift_s), na.rm = TRUE), .groups = "drop") %>%
  filter(is.finite(max_drift_s), max_drift_s > 1)
for (i in seq_len(nrow(clock_drift))) {
  warn("clock drift in session ", clock_drift$session_id[i], " (", clock_drift$participant_id[i],
       "): utc and t differ by up to ", round(clock_drift$max_drift_s[i], 1), " s")
}
long_gaps <- sessions %>% filter(!is.na(gap_h), gap_h > gap_warn_h)
for (i in seq_len(nrow(long_gaps))) {
  warn(long_gaps$participant_id[i], ": ", round(long_gaps$gap_h[i], 1),
       " h between sessions (two people under one ID?)")
}
editor_builds <- sessions %>% filter(build == "editor") %>% distinct(participant_id)
if (nrow(editor_builds) > 0) {
  warn("build 'editor' (Unity Play mode, not a Quest build) for: ",
       paste(editor_builds$participant_id, collapse = ", "))
}

# ---- 6. Rebuild one row per scene visit from the events ------------------------

# Every event with a scene_visit belongs to the visit (session, scene, scene_visit).
visit_events <- events %>% filter(!is.na(scene_visit), !is.na(t))

enters <- visit_events %>% filter(event == "scene_enter") %>%
  transmute(session_id, scene, scene_visit, enter_t = t, enter_utc = utc)

exits <- visit_events %>% filter(event == "scene_exit") %>%
  transmute(session_id, scene, scene_visit, exit_t = t,
            exit_duration_s = map_dbl(data, ~ as_num(.x$duration_s)),
            exit_paused_s = map_dbl(data, ~ as_num(.x$paused_s)),
            exit_pauses = map_int(data, ~ as_int(.x$pauses)),
            exit_hmd_removed_count = map_int(data, ~ as_int(.x$hmd_removed_count)),
            exit_unity_errors = map_int(data, ~ as_int(.x$unity_errors)))

# Pauses as intervals per session: app_pause starts one, the next app_resume
# ends it. A pause never resumed (crash, or quit while paused: the logger then
# writes no app_resume) ends at the session's last t. The C# logger splits a
# pause that spans a scene change by visit, so a visit's paused time is the
# overlap of these intervals with the visit. The pause itself (and a headset
# removal in it) counts in the visit where it started.
has_hmd <- function(d) "hmd_unmounted" %in% unlist(d$causes)
session_last_t <- events %>% filter(!is.na(t), event != "crash_recovered") %>%
  group_by(session_id) %>% summarise(session_last_t = max(t), .groups = "drop")
pair_pauses <- function(rows) {
  out <- list()
  open <- NULL
  for (i in seq_len(nrow(rows))) {
    if (rows$event[i] == "app_pause" && is.null(open)) {
      open <- list(scene = rows$scene[i], scene_visit = rows$scene_visit[i], start_t = rows$t[i],
                   hmd = has_hmd(rows$data[[i]]))
    } else if (rows$event[i] == "app_resume" && !is.null(open)) {
      # The resume lists every cause seen during the pause.
      open$end_t <- rows$t[i]
      open$hmd <- open$hmd || has_hmd(rows$data[[i]])
      open$resumed <- TRUE
      out[[length(out) + 1]] <- open
      open <- NULL
    }
  }
  if (!is.null(open)) {
    open$end_t <- rows$session_last_t[1]
    open$resumed <- FALSE
    out[[length(out) + 1]] <- open
  }
  bind_rows(lapply(out, as_tibble))
}
pause_intervals <- visit_events %>%
  filter(event %in% c("app_pause", "app_resume")) %>%
  left_join(session_last_t, by = "session_id") %>%
  arrange(session_id, t, line) %>%
  group_by(session_id) %>%
  group_modify(~ pair_pauses(.x)) %>%
  ungroup()
if (nrow(pause_intervals) == 0) {
  pause_intervals <- tibble(session_id = character(), scene = character(), scene_visit = integer(),
                            start_t = double(), end_t = double(), hmd = logical(), resumed = logical())
}

# Paused seconds inside [from_t, to_t] of one session (vectorised over rows).
paused_between <- function(sid, from_t, to_t) {
  map_dbl(seq_along(sid), function(i) {
    p <- pause_intervals[pause_intervals$session_id == sid[i], ]
    if (nrow(p) == 0 || is.na(from_t[i]) || is.na(to_t[i])) return(0)
    sum(pmax(0, pmin(p$end_t, to_t[i]) - pmax(p$start_t, from_t[i])))
  })
}

pause_counts <- pause_intervals %>%
  group_by(session_id, scene, scene_visit) %>%
  summarise(ev_pauses = n(), ev_hmd = sum(hmd), .groups = "drop")

# Unity errors: n_errors = how many errors the line stands for. Logs written
# before n_errors existed: a line with a stack after the 5 s window stands for
# folded_count + 1, a line without a stack (written at visit close) for
# folded_count, a first occurrence (folded_count 0) for 1.
line_n_errors <- function(d) {
  n <- as_num(d$n_errors)
  if (!is.na(n)) return(n)
  f <- as_num(d$folded_count)
  if (is.na(f) || f == 0) return(1)
  if (is.null(d$stack)) f else f + 1
}
error_lines <- visit_events %>% filter(event == "unity_error") %>%
  mutate(n_err = map_dbl(data, line_n_errors),
         legacy = map_lgl(data, ~ is.null(.x$n_errors)))
if (any(error_lines$legacy)) {
  warn(sum(error_lines$legacy), " unity_error line(s) without n_errors (older logger): ",
       "counts inferred from folded_count and stack")
}
error_stats <- error_lines %>%
  group_by(session_id, scene, scene_visit) %>%
  summarise(ev_unity_errors = sum(n_err), .groups = "drop")

# After the error cap (unity_error_cap_reached) further errors are only counted
# in scene_exit.unity_errors, so the events can't rebuild that number.
cap_sessions <- events %>% filter(event == "unity_error_cap_reached") %>%
  distinct(participant_id, session_id)
for (i in seq_len(nrow(cap_sessions))) {
  warn("unity_error cap reached in session ", cap_sessions$session_id[i], " (",
       cap_sessions$participant_id[i], "): errors after the cap have no event lines; ",
       "unity_errors taken from scene_exit, not cross-checked")
}

last_events <- visit_events %>% group_by(session_id, scene, scene_visit) %>%
  summarise(last_t = max(t), .groups = "drop")

visits_base <- enters %>%
  left_join(exits, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(pause_counts, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(error_stats, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(last_events, by = c("session_id", "scene", "scene_visit")) %>%
  replace_na(list(ev_pauses = 0L, ev_hmd = 0L, ev_unity_errors = 0)) %>%
  mutate(
    closed = !is.na(exit_t),
    # A visit without scene_exit is the open visit of a crashed session. Its
    # time ends at its last event; a pause still open then covers the rest.
    end_t = if_else(closed, exit_t, last_t),
    ev_paused_s = round(paused_between(session_id, enter_t, end_t), 3),
    rebuilt_duration_s = round(end_t - enter_t - ev_paused_s, 3),
    duration_censored = !closed,
    duration_s = if_else(closed, exit_duration_s, rebuilt_duration_s),
    paused_s = if_else(closed, exit_paused_s, ev_paused_s),
    pauses = if_else(closed, exit_pauses, as.integer(ev_pauses)),
    hmd_removed_count = if_else(closed, exit_hmd_removed_count, as.integer(ev_hmd)),
    error_cap = session_id %in% cap_sessions$session_id,
    unity_errors = if_else(closed, exit_unity_errors, as.integer(ev_unity_errors)))

# Cross-check: scene_exit numbers must match what the other events say. The
# warning names each field that differs (events value vs scene_exit value).
exit_checks <- visits_base %>% filter(closed) %>%
  mutate(
    bad_duration = abs(rebuilt_duration_s - exit_duration_s) > 0.005,
    bad_paused = abs(ev_paused_s - exit_paused_s) > time_tolerance_s,
    bad_pauses = ev_pauses != exit_pauses,
    bad_errors = !error_cap & ev_unity_errors != exit_unity_errors) %>%
  filter(bad_duration | bad_paused | bad_pauses | bad_errors) %>%
  left_join(sessions %>% select(session_id, participant_id), by = "session_id")
for (i in seq_len(nrow(exit_checks))) {
  r <- exit_checks[i, ]
  fields <- c(
    if (r$bad_duration) paste0("duration_s events=", r$rebuilt_duration_s, " exit=", r$exit_duration_s),
    if (r$bad_paused) paste0("paused_s events=", r$ev_paused_s, " exit=", r$exit_paused_s),
    if (r$bad_pauses) paste0("pauses events=", r$ev_pauses, " exit=", r$exit_pauses),
    if (r$bad_errors) paste0("unity_errors events=", r$ev_unity_errors, " exit=", r$exit_unity_errors))
  warn("scene_exit disagrees with the visit's events: ", r$participant_id, " session ", r$session_id,
       " ", r$scene, " visit ", r$scene_visit, ": ", paste(fields, collapse = "; "))
}

# ---- 7. Outcome events (#28): rated presses, quiz touches, station rides ------

# Which sessions measured outcomes at all (spec section 3, header):
# - schema 1: no. Its outcome events use the v1 catalogue (warned in section 5).
# - schema 2: the header's event_groups says whether "outcome" was emitted; a
#   missing group means not measured (NA), never 0.
# - no header / no event_groups key: measured when the session has any rated
#   press (the rule before schema 2).
# Only events of sessions that are not schema 1 are read as outcomes.
v2_events <- visit_events %>%
  semi_join(sessions %>% filter(!schema_version %in% 1L), by = "session_id")

# Empf Kz of a picking position (#94, decision 028) where line.empf_kz is not
# logged. The field came with the build that made Mixed position 1 Empf Kz
# "003" (3 snap places), and that build kept layout_version "029" (an earlier
# "029" build had "000" there). So the field itself, not the layout, tells the
# build: has_empf_kz = the session logged line.empf_kz on its presses.
#   Basic "003" always (6 snap places, #41).
#   has_empf_kz TRUE: Mixed position 1 "003" (normally logged anyway).
#   has_empf_kz FALSE (presses without the field = a build before #94):
#     Mixed position 1 "000".
#   has_empf_kz NA (no press in the session): "000" up to layout "025",
#     unknown (NA) for Mixed position 1 from "029" (either build possible).
#   Mixed position 2 and both Complex positions "000"; quiz touches NA.
expected_empf_kz <- function(scene, position, has_empf_kz, layout_version) {
  sc <- as.character(scene)
  from_029 <- coalesce(suppressWarnings(as.integer(layout_version)) >= 29L, FALSE)
  case_when(is.na(position) ~ NA_character_,
            sc == "BasicPickingScene" ~ "003",
            sc == "MixedPickingScene" & position == 1L & has_empf_kz %in% TRUE ~ "003",
            sc == "MixedPickingScene" & position == 1L & is.na(has_empf_kz) & from_029 ~ NA_character_,
            sc %in% c("MixedPickingScene", "ComplexScene") ~ "000",
            TRUE ~ NA_character_)
}
# An EKZ-3 opportunity (the denominator of ekz_placement at a position): a
# rated red-button press at an Empf Kz "003" position whose OrderRule result
# was right (no code but EKZ_PLACEMENT), because the validator checks the snap
# places only once the quantity is right. A press with TOO_FEW etc. never had
# its placement rated, so it is no EKZ attempt (neither right nor wrong).
is_ekz_opportunity <- function(event, empf_kz, codes) {
  rule_ok <- map_lgl(codes, ~ all(.x %in% "EKZ_PLACEMENT"))
  event == "confirm_pressed" & empf_kz %in% "003" & rule_ok
}

# One row per rated attempt: a red-button press (confirm_pressed) or a quiz
# touch (quiz_answer). The app counts attempt_no separately for the two, so a
# press is identified by press_id (its row here), never by attempt_no.
# error_codes is a list of {code, item}; foreign a list of {item, count}.
presses <- v2_events %>% filter(event %in% c("confirm_pressed", "quiz_answer")) %>%
  arrange(session_id, t, line) %>%
  transmute(participant_id, session_id, scene, scene_visit, t, line, event,
            attempt_no = map_int(data, ~ as_int(.x$attempt_no)),
            # Quiz touches (null on red-button presses).
            quiz = map_chr(data, ~ as_chr(.x$quiz)),
            question_id = map_chr(data, ~ as_chr(.x$question_id)),
            selected = map_chr(data, ~ as_chr(.x$selected)),
            # Red-button presses (null on quiz touches).
            position = map_int(data, ~ as_int(.x$position)),
            position_count = map_int(data, ~ as_int(.x$position_count)),
            # Attempt number at this item: per position or per question.
            attempt_in_item = map_int(data, ~ as_int(.x$attempt_in_position %||% .x$attempt_in_question)),
            # The current order line (required_loose / placed_loose are null outside Complex).
            line_item = map_chr(data, ~ as_chr(.x$line$item)),
            soll = map_int(data, ~ as_int(.x$line$soll)),
            vpe = map_int(data, ~ as_int(.x$line$vpe)),
            # Empf Kz of the position (#94, layout "029" on); NA in older logs.
            empf_kz_logged = map_chr(data, ~ as_chr(.x$line$empf_kz)),
            required_full = map_int(data, ~ as_int(.x$line$required_full)),
            required_loose = map_int(data, ~ as_int(.x$line$required_loose)),
            placed_full = map_int(data, ~ as_int(.x$line$placed_full)),
            placed_loose = map_int(data, ~ as_int(.x$line$placed_loose)),
            # Materials not in the order; "" = none (logged as []).
            n_foreign = map_int(data, ~ length(.x$foreign %||% list())),
            foreign_items = map_chr(data, ~ paste(map_chr(.x$foreign %||% list(),
                                                          function(f) as_chr(f$item)), collapse = ";")),
            correct = map_lgl(data, ~ as_lgl(.x$correct)),
            codes = map(data, ~ map_chr(.x$error_codes %||% list(), function(e) as_chr(e$code))),
            time_since_scene_enter_s = map_dbl(data, ~ as_num(.x$time_since_scene_enter_s))) %>%
  mutate(n_codes = lengths(codes),
         press_id = row_number(),
         # The item an attempt belongs to: the quiz question or the order position.
         item = if_else(event == "quiz_answer", question_id,
                        if_else(is.na(position), NA_character_, paste0("position_", position))),
         # Attempts that summary.csv counts in confirm_count: rated red-button
         # presses, and quiz touches in ConceptScene. The Complex remainder quiz
         # is only in the events (it shows up in items, not in visit totals).
         counted = event == "confirm_pressed" | !quiz %in% "complex_remainder",
         layout_version = sessions$layout_version[match(session_id, sessions$session_id)]) %>%
  # Empf Kz (#94, decision 028): the logged one, else what the build had
  # (expected_empf_kz above; the session's presses tell the build).
  group_by(session_id) %>%
  mutate(has_empf_kz = if (any(event == "confirm_pressed")) any(!is.na(empf_kz_logged)) else NA) %>%
  ungroup() %>%
  mutate(empf_kz_expected = expected_empf_kz(scene, position, has_empf_kz, layout_version),
         empf_kz = coalesce(empf_kz_logged, empf_kz_expected),
         ekz_opportunity = is_ekz_opportunity(event, empf_kz, codes)) %>%
  # scaffolded = the app handed the trainee the numbers for this item
  # (item_skills.csv: Complex position 1). Kept out of the skill rates.
  left_join(item_skills %>% select(scene, item, scaffolded), by = c("scene", "item")) %>%
  mutate(scaffolded = coalesce(scaffolded, FALSE))

# Invariant (spec section 6): correct == (no error codes). Report every break.
invariant_breaks <- presses %>% filter(is.na(correct) | correct != (n_codes == 0))
for (i in seq_len(nrow(invariant_breaks))) {
  r <- invariant_breaks[i, ]
  warn("invariant correct == (error_codes empty) broken: ", r$participant_id, " ", r$scene,
       " visit ", r$scene_visit, " ", r$event, " attempt ", r$attempt_no, " (correct = ", r$correct,
       ", codes = ", paste(r$codes[[1]], collapse = ";"), ")")
}

# Empf Kz checks (#94). A logged empf_kz that differs from the current orders
# (an order changed without telling R?) is warned; R uses the logged one. A
# session that logs the field on some presses but not on others, a "025"-or-
# older log with the field, and a "029" log without it (a build from before
# #94: Mixed position 1 read as Empf Kz 000) are warned too, and
# EKZ_PLACEMENT where the app can't write it (not "003", or next to an
# OrderRule code) is a contract break.
empf_kz_diff <- presses %>% filter(event == "confirm_pressed", !is.na(empf_kz_logged),
                                   !is.na(empf_kz_expected), empf_kz_logged != empf_kz_expected) %>%
  distinct(participant_id, scene, position, empf_kz_logged, empf_kz_expected)
for (i in seq_len(nrow(empf_kz_diff))) {
  r <- empf_kz_diff[i, ]
  warn("line.empf_kz ", r$empf_kz_logged, " at ", r$scene, " position ", r$position, " for ", r$participant_id,
       ", but the course has ", r$empf_kz_expected, " there: the logged value is used ",
       "(order changed? update expected_empf_kz in analyse_set.R)")
}
empf_kz_sessions <- presses %>% filter(event == "confirm_pressed") %>%
  group_by(participant_id, session_id, layout_version) %>%
  summarise(n_with = sum(!is.na(empf_kz_logged)), n_without = sum(is.na(empf_kz_logged)), .groups = "drop") %>%
  mutate(lay = suppressWarnings(as.integer(layout_version)))
for (r in split(empf_kz_sessions, seq_len(nrow(empf_kz_sessions)))) {
  if (r$n_with > 0 && r$n_without > 0) {
    warn("line.empf_kz on only ", r$n_with, " of ", r$n_with + r$n_without, " confirm_pressed lines: ",
         r$participant_id, " session ", r$session_id, " (missing ones read as the build without the field)")
  } else if (r$n_with > 0 && !coalesce(r$lay >= 29L, FALSE)) {
    warn("line.empf_kz in a log of layout ", coalesce(r$layout_version, "none"), " (only builds from #94, layout ",
         "\"029\", write it): ", r$participant_id, " session ", r$session_id)
  } else if (r$n_with == 0 && coalesce(r$lay >= 29L, FALSE)) {
    warn("layout ", r$layout_version, " log without line.empf_kz: a build from before #94, Mixed position 1 read ",
         "as Empf Kz 000 (not rated for placement): ", r$participant_id, " session ", r$session_id)
  }
}
ekz_misplaced <- presses %>%
  filter(map_lgl(codes, ~ "EKZ_PLACEMENT" %in% .x), !ekz_opportunity)
for (i in seq_len(nrow(ekz_misplaced))) {
  r <- ekz_misplaced[i, ]
  warn("EKZ_PLACEMENT where the app does not rate placement: ", r$participant_id, " ", r$scene,
       " visit ", r$scene_visit, " position ", r$position, " (Empf Kz ", coalesce(r$empf_kz, "NA"),
       ", codes ", paste(r$codes[[1]], collapse = ";"), "): not counted in the ekz_placement rate")
}

# Every attempt needs its item (position / question_id), and attempt_in_position
# / attempt_in_question must count 1, 2, 3 ... per item within a visit.
no_item <- presses %>% filter(is.na(item))
for (i in seq_len(nrow(no_item))) {
  warn(no_item$event[i], " without position / question_id: ", no_item$participant_id[i], " ",
       no_item$scene[i], " visit ", no_item$scene_visit[i], " (not in items.csv)")
}
bad_item_count <- presses %>% filter(!is.na(item)) %>%
  group_by(participant_id, session_id, scene, scene_visit, item) %>%
  filter(is.na(attempt_in_item) | attempt_in_item != row_number()) %>% slice(1) %>% ungroup()
for (i in seq_len(nrow(bad_item_count))) {
  r <- bad_item_count[i, ]
  warn("attempt_in_position / attempt_in_question out of order: ", r$participant_id, " ", r$scene,
       " visit ", r$scene_visit, " item ", r$item, " (logged ", r$attempt_in_item, ")")
}

# One row per error code on an attempt, with its skill. skill_map.csv gives
# the skill of each code; WRONG_ANSWER has none there and takes the skill of
# its question from item_skills.csv (resolved first, so it is never "unknown").
press_codes <- presses %>%
  select(participant_id, session_id, scene, scene_visit, press_id, attempt_no, event, item,
         counted, scaffolded, codes) %>%
  unnest_longer(codes, values_to = "code") %>%
  mutate(code = as.character(code)) %>%  # stays text even with no presses (pre-#28 logs)
  filter(!is.na(code)) %>%
  left_join(skill_map %>% transmute(error_code, code_skill = as.character(skill)),
            by = c("code" = "error_code")) %>%
  left_join(item_skills %>% transmute(scene, item, item_skill = skill), by = c("scene", "item")) %>%
  mutate(known_code = code %in% skill_map$error_code,
         skill = if_else(code == "WRONG_ANSWER", item_skill, code_skill))

# Unknown codes are warned, never silently dropped (design section 8).
unknown_codes <- press_codes %>% filter(!known_code)
for (i in seq_len(nrow(unknown_codes))) {
  r <- unknown_codes[i, ]
  warn("unknown error code ", r$code, " (not in skill_map.csv): ", r$participant_id, " ",
       r$scene, " visit ", r$scene_visit, " attempt ", r$attempt_no)
}
# A wrong quiz touch whose question has no skill in item_skills.csv.
unmapped_questions <- press_codes %>% filter(code == "WRONG_ANSWER", is.na(skill)) %>%
  distinct(scene, item)
for (i in seq_len(nrow(unmapped_questions))) {
  warn("question_id ", unmapped_questions$item[i], " in ", unmapped_questions$scene[i],
       " has no skill in item_skills.csv: its WRONG_ANSWER codes count for no skill")
}

# A known code whose skill scene_skills.csv doesn't list for that scene. The
# study lead's CSV decides which skills a scene tests, so it is not counted in
# the skill rates, but warned (it still shows in error_codes_by_scene.png).
observed_pairs <- press_codes %>% filter(!is.na(skill)) %>% distinct(participant_id, scene, skill, code)
extra_pairs <- anti_join(observed_pairs, scene_skills, by = c("scene", "skill"))
for (i in seq_len(nrow(extra_pairs))) {
  warn("code ", extra_pairs$code[i], " (skill ", extra_pairs$skill[i], ") in ", extra_pairs$scene[i],
       " for ", extra_pairs$participant_id[i], ", but scene_skills.csv doesn't list that skill there: ",
       "not counted in the skill rates (add the row if the scene tests it)")
}
skill_pairs <- scene_skills %>% distinct(scene, skill)

# task_completed = the whole task of the visit is done: in Mixed and Complex
# only after the last position (OrderCompleted), in Concept when all 4
# questions are right. Its presence is what "completed" means.
task_completed <- v2_events %>% filter(event == "task_completed") %>%
  transmute(session_id, scene, scene_visit,
            tc_seen = TRUE,
            tc_t = t,  # session time of completion (station rides are cut here)
            tc_attempts_to_correct = map_int(data, ~ as_int(.x$attempts_to_correct)),
            tc_time_to_first_confirm_s = map_dbl(data, ~ as_num(.x$time_to_first_confirm_s)),
            tc_time_to_correct_s = map_dbl(data, ~ as_num(.x$time_to_correct_s))) %>%
  distinct(session_id, scene, scene_visit, .keep_all = TRUE)

# Active time at an attempt, for logs whose attempts carry no time field: wall
# time since scene enter minus the paused time between enter and the attempt.
press_active <- presses %>%
  select(press_id, session_id, scene, scene_visit, t, time_since_scene_enter_s) %>%
  left_join(enters %>% select(session_id, scene, scene_visit, enter_t),
            by = c("session_id", "scene", "scene_visit")) %>%
  mutate(paused_before = paused_between(session_id, enter_t, t),
         active_at_s = coalesce(time_since_scene_enter_s, round(t - enter_t - paused_before, 3))) %>%
  select(press_id, active_at_s)

# Per visit outcome numbers, from the counted attempts only (the same set as
# summary.csv). A press "has an error" when it carries at least one code.
press_visit <- presses %>%
  filter(counted) %>%
  left_join(press_active, by = "press_id") %>%
  arrange(session_id, scene, scene_visit, t, line) %>%
  group_by(session_id, scene, scene_visit) %>%
  summarise(
    confirm_count = n(),
    presses_with_error = sum(n_codes > 0),
    # Rows are sorted by time, so row 1 is the first attempt.
    first_attempt_correct = correct[1],
    time_to_first_confirm_presses = active_at_s[1],
    error_codes_all = {
      cc <- unlist(codes)
      if (length(cc) == 0) NA_character_ else {
        tab <- table(cc)
        paste0(names(tab), ":", as.integer(tab), collapse = ";")
      }
    },
    .groups = "drop")

# Rated attempts counted a second way, straight from the event names (red
# button scenes: confirm_pressed; other scenes: quiz_answer). selftest.R checks
# that it equals confirm_count, so confirm_ignored can never slip in.
rated_counts <- v2_events %>%
  filter((scene %in% red_button_scenes & event == "confirm_pressed") |
           (!scene %in% red_button_scenes & event == "quiz_answer")) %>%
  count(session_id, scene, scene_visit, name = "ev_rated_presses")

# Red-button presses the validator ignored (station moving, order already
# completed, Complex quiz not answered yet). Never an attempt, never in a
# denominator; only counted.
ignored_counts <- v2_events %>% filter(event == "confirm_ignored") %>%
  count(session_id, scene, scene_visit, name = "ev_confirm_ignored")

# Station rides (decision 019, spec §6 "Derived measures"). Two numbers:
#
# wait_s = time the trainee is blocked by a box change BEFORE task_completed.
#   The red button is blocked from the moment a position is done (the box
#   starts after a 0.6 s startDelay) until the new storage box stands at the
#   work spot (storage_in arrived). The old box (storage_out) keeps riding
#   after that, but the trainee can already work. So there is one blocked
#   interval per storage_in ride: from its trigger to its arrived line.
#   Trigger = the position_completed of the previous position; for
#   storage_in position 1 (Complex only: box 1 after the remainder quiz) the
#   last quiz_answer before the ride starts. A storage_in ride without an
#   arrived line (crash/quit) ends at the visit's last event. Intervals are
#   unioned (never summed twice), cut at task_completed, minus paused time.
#   Basic has no storage_in ride, so its wait_s is 0.
# wait_after_done_s = union of all rides (storage_out, order_box, any what)
#   after task_completed: the boxes leaving at the end. Basic/Mixed then
#   linger until the scene is left while Complex ends the session at once,
#   so this part is kept apart from wait_s.
first_or_na <- function(x) if (length(x) == 0) NA_real_ else min(x)
last_or_na <- function(x) if (length(x) == 0) NA_real_ else max(x)
ride_rows <- v2_events %>% filter(event %in% c("station_move_start", "station_move_arrived")) %>%
  mutate(what = map_chr(data, ~ as_chr(.x$what)),
         position = map_int(data, ~ as_int(.x$position))) %>%
  group_by(session_id, scene, scene_visit, what, position) %>%
  summarise(start_t = first_or_na(t[event == "station_move_start"]),
            arrived_t = first_or_na(t[event == "station_move_arrived"]), .groups = "drop") %>%
  left_join(visits_base %>% select(session_id, scene, scene_visit, end_t),
            by = c("session_id", "scene", "scene_visit"))
no_start <- ride_rows %>% filter(is.na(start_t))
if (nrow(no_start) > 0) {
  warn(nrow(no_start), " station_move_arrived without a station_move_start (ignored)")
}
ride_rows <- ride_rows %>% filter(!is.na(start_t)) %>%
  mutate(end_t = coalesce(arrived_t, end_t))

# Triggers of the storage_in rides: position_completed times per position,
# and quiz_answer times (for box 1 in Complex).
pos_done_t <- v2_events %>% filter(event == "position_completed") %>%
  mutate(done_position = map_int(data, ~ as_int(.x$position))) %>%
  group_by(session_id, scene, scene_visit, done_position) %>%
  summarise(done_t = first_or_na(t), .groups = "drop")
quiz_t <- v2_events %>% filter(event == "quiz_answer") %>%
  select(session_id, scene, scene_visit, quiz_t = t)
storage_in <- ride_rows %>% filter(what == "storage_in") %>%
  mutate(done_position = position - 1L) %>%
  left_join(pos_done_t, by = c("session_id", "scene", "scene_visit", "done_position"))
# Last quiz touch before each ride's start (only looked up for position 1).
quiz_trigger <- storage_in %>% filter(position == 1L) %>%
  select(session_id, scene, scene_visit, position, start_t) %>%
  inner_join(quiz_t, by = c("session_id", "scene", "scene_visit"),
             relationship = "many-to-many") %>%
  filter(quiz_t <= start_t) %>%
  group_by(session_id, scene, scene_visit, position) %>%
  summarise(quiz_trigger_t = last_or_na(quiz_t), .groups = "drop")
storage_in <- storage_in %>%
  left_join(quiz_trigger, by = c("session_id", "scene", "scene_visit", "position")) %>%
  mutate(trigger_t = if_else(position == 1L, quiz_trigger_t, done_t))
no_trigger <- storage_in %>% filter(is.na(trigger_t))
if (nrow(no_trigger) > 0) {
  # Should not happen with the real app; the ride start is the best guess.
  warn(nrow(no_trigger), " storage_in ride(s) without a trigger (position_completed / quiz_answer); wait starts at the ride start")
}
storage_in <- storage_in %>% mutate(trigger_t = coalesce(trigger_t, start_t))

# Merge overlapping [start, end] intervals into one (union).
union_intervals <- function(start_t, end_t) {
  o <- order(start_t)
  start_t <- start_t[o]
  end_t <- end_t[o]
  out_s <- numeric(0)
  out_e <- numeric(0)
  for (i in seq_along(start_t)) {
    k <- length(out_e)
    if (k > 0 && start_t[i] <= out_e[k]) {
      out_e[k] <- max(out_e[k], end_t[i])  # overlaps the previous interval: extend it
    } else {
      out_s <- c(out_s, start_t[i])
      out_e <- c(out_e, end_t[i])
    }
  }
  tibble(start_t = out_s, end_t = out_e)
}
tc_cut <- task_completed %>% select(session_id, scene, scene_visit, tc_t)

# wait_s: blocked intervals (trigger -> storage_in arrived), part before task_completed.
wait_before <- tibble(session_id = character(), scene = character(), scene_visit = integer(),
                      ev_wait_s = double())
if (nrow(storage_in) > 0) {
  wait_before <- storage_in %>%
    group_by(session_id, scene, scene_visit) %>%
    group_modify(~ union_intervals(.x$trigger_t, .x$end_t)) %>%
    ungroup() %>%
    left_join(tc_cut, by = c("session_id", "scene", "scene_visit")) %>%
    mutate(before_end = pmin(end_t, coalesce(tc_t, Inf)),
           before_s = if_else(before_end > start_t,
                              before_end - start_t - paused_between(session_id, start_t, before_end), 0)) %>%
    group_by(session_id, scene, scene_visit) %>%
    summarise(ev_wait_s = round(sum(before_s), 3), .groups = "drop")
}
# wait_after_done_s: all rides, part after task_completed.
wait_after <- tibble(session_id = character(), scene = character(), scene_visit = integer(),
                     ev_wait_after_done_s = double())
if (nrow(ride_rows) > 0) {
  wait_after <- ride_rows %>%
    group_by(session_id, scene, scene_visit) %>%
    group_modify(~ union_intervals(.x$start_t, .x$end_t)) %>%
    ungroup() %>%
    inner_join(tc_cut, by = c("session_id", "scene", "scene_visit")) %>%
    mutate(after_start = pmax(start_t, tc_t),
           after_s = if_else(end_t > after_start,
                             end_t - after_start - paused_between(session_id, after_start, end_t), 0)) %>%
    group_by(session_id, scene, scene_visit) %>%
    summarise(ev_wait_after_done_s = round(sum(after_s), 3), .groups = "drop")
}
wait_stats <- full_join(wait_before, wait_after, by = c("session_id", "scene", "scene_visit"))

# Sessions that measured outcomes (rule at the top of this section).
session_outcome <- sessions %>%
  transmute(session_id,
            outcome_measured = case_when(
              schema_version %in% 1L ~ FALSE,
              groups_logged %in% TRUE ~ str_detect(coalesce(event_groups, ""), "(^|;)outcome(;|$)"),
              TRUE ~ session_id %in% presses$session_id))
outcome_sessions <- session_outcome$session_id[session_outcome$outcome_measured]

# ---- 7b. Interaction events (#29): grabs, order container, menu, end control --

# Which sessions logged the interaction group, same rule as for outcomes:
# schema 1 no; with event_groups in the header: "interaction" listed; no
# event_groups key (no header line): yes when the session has any of its events.
# A session without the group has NA interaction numbers, never 0.
interaction_event_names <- c("grab_start", "grab_end", "container_enter", "container_exit",
                             "menu_select", "end_control_opened", "end_control_cancelled",
                             "concept_hint_shown", "concept_hint_cleared")
session_interaction <- sessions %>%
  transmute(session_id, participant_id,
            interaction_measured = case_when(
              schema_version %in% 1L ~ FALSE,
              groups_logged %in% TRUE ~ str_detect(coalesce(event_groups, ""), "(^|;)interaction(;|$)"),
              TRUE ~ session_id %in% events$session_id[events$event %in% interaction_event_names]))
interaction_sessions <- session_interaction$session_id[session_interaction$interaction_measured]
# Interaction events in a session whose header doesn't list the group: the
# header is wrong (contract break). They are not read.
stray_interaction <- events %>%
  filter(event %in% interaction_event_names, !session_id %in% interaction_sessions) %>%
  distinct(participant_id, session_id)
for (i in seq_len(nrow(stray_interaction))) {
  warn("interaction events in session ", stray_interaction$session_id[i], " (",
       stray_interaction$participant_id[i], ") whose header event_groups lacks \"interaction\" (not read)")
}
ix_events <- visit_events %>% filter(session_id %in% interaction_sessions)

# Grabs: one grab_start per package grab (two hands on one package = one grab).
# Pair each grab_end with the open grab_start of the same object_id in the
# same visit. grab_no counts the object's starts, so an end belongs to the
# latest start; grab_no 0 = an end without a start in this visit (logged
# duration_s null). A start without an end is an open grab (crash, scene
# change or session end while holding): censored, no duration.
grab_rows <- ix_events %>% filter(event %in% c("grab_start", "grab_end")) %>%
  transmute(participant_id, session_id, scene, scene_visit, t, line, event,
            object_id = map_chr(data, ~ as_chr(.x$object_id)),
            material_key = map_chr(data, ~ as_chr(.x$material_key)),
            logged_duration_s = map_dbl(data, ~ as_num(.x$duration_s))) %>%
  arrange(session_id, scene, scene_visit, t, line) %>%
  group_by(session_id, scene, scene_visit, object_id) %>%
  mutate(grab_no = cumsum(event == "grab_start")) %>%
  ungroup()
no_object <- grab_rows %>% filter(is.na(object_id))
if (nrow(no_object) > 0) warn(nrow(no_object), " grab event(s) without object_id (cannot be paired)")
grabs <- grab_rows %>% filter(grab_no > 0, !is.na(object_id)) %>%
  group_by(participant_id, session_id, scene, scene_visit, object_id, grab_no) %>%
  summarise(material_key = first(material_key),
            start_t = first_or_na(t[event == "grab_start"]),
            end_t = first_or_na(t[event == "grab_end"]),
            logged_duration_s = first_or_na(logged_duration_s[event == "grab_end"]),
            .groups = "drop") %>%
  mutate(open = is.na(end_t),
         # Active hold time (pauses removed), the rule for every _s field.
         hold_s = if_else(open, NA_real_,
                          round(end_t - start_t - paused_between(session_id, start_t, end_t), 3)))
orphan_ends <- grab_rows %>% filter(event == "grab_end", grab_no == 0)
if (nrow(orphan_ends) > 0) {
  message("Note: ", nrow(orphan_ends), " grab_end without a grab_start in the same visit (duration_s null)")
}
# The logger's duration_s must equal the active time between start and end.
# A difference usually means the grab spanned a pause and the logger wrote
# wall time (spec section 3: _s fields are active time).
bad_hold <- grabs %>% filter(!open, !is.na(logged_duration_s),
                             abs(logged_duration_s - hold_s) > time_tolerance_s)
if (nrow(bad_hold) > 0) {
  warn(nrow(bad_hold), " grab_end.duration_s differ from the active time between grab_start and grab_end ",
       "(e.g. ", bad_hold$participant_id[1], " ", bad_hold$scene[1], " ", bad_hold$object_id[1],
       ": logged ", bad_hold$logged_duration_s[1], ", active ", bad_hold$hold_s[1], "); hold_s uses the active time")
}

# Order container: count_after must equal the running count of enters minus
# exits in the visit (the container is empty at scene start). It counts every
# package in the container, finished positions included.
container_rows <- ix_events %>% filter(event %in% c("container_enter", "container_exit")) %>%
  transmute(participant_id, session_id, scene, scene_visit, t, line, event,
            container = map_chr(data, ~ as_chr(.x$container)),
            object_id = map_chr(data, ~ as_chr(.x$object_id)),
            material_key = map_chr(data, ~ as_chr(.x$material_key)),
            count_after = map_int(data, ~ as_int(.x$count_after))) %>%
  arrange(session_id, scene, scene_visit, t, line) %>%
  group_by(session_id, scene, scene_visit) %>%
  mutate(running = cumsum(if_else(event == "container_enter", 1L, -1L))) %>%
  ungroup()
bad_count <- container_rows %>% filter(is.na(count_after) | count_after != running) %>%
  distinct(participant_id, session_id, scene, scene_visit, .keep_all = TRUE)
for (i in seq_len(nrow(bad_count))) {
  r <- bad_count[i, ]
  warn("container count_after disagrees with enters - exits: ", r$participant_id, " ", r$scene,
       " visit ", r$scene_visit, " at t = ", r$t, " (logged ", r$count_after, ", events ", r$running, ")")
}
other_container <- container_rows %>% filter(!container %in% "order")
if (nrow(other_container) > 0) warn(nrow(other_container), " container event(s) with container other than \"order\"")

# Menu touches (menu_select, written in the visit being left) and the
# facilitator end control (#44).
menu_rows <- ix_events %>% filter(event == "menu_select") %>%
  transmute(session_id, scene, scene_visit, t,
            target_scene = map_chr(data, ~ as_chr(.x$target_scene)),
            blocked = map_lgl(data, ~ as_lgl(.x$blocked)))
end_control_rows <- ix_events %>% filter(event %in% c("end_control_opened", "end_control_cancelled")) %>%
  select(session_id, scene, scene_visit, t, event)

interaction_visit <- enters %>% select(session_id, scene, scene_visit, enter_t) %>%
  filter(session_id %in% interaction_sessions) %>%
  left_join(grabs %>% group_by(session_id, scene, scene_visit) %>%
              summarise(ev_grabs = n(),
                        # A re-grab = a grab of a package already grabbed in this visit.
                        ev_regrabs = n() - n_distinct(object_id),
                        ev_open_grabs = sum(open),
                        ev_hold_s = round(sum(hold_s, na.rm = TRUE), 3),
                        first_grab_t = min(start_t), .groups = "drop"),
            by = c("session_id", "scene", "scene_visit")) %>%
  left_join(container_rows %>% group_by(session_id, scene, scene_visit) %>%
              summarise(ev_container_enters = sum(event == "container_enter"),
                        ev_container_exits = sum(event == "container_exit"), .groups = "drop"),
            by = c("session_id", "scene", "scene_visit")) %>%
  left_join(menu_rows %>% group_by(session_id, scene, scene_visit) %>%
              summarise(ev_menu_blocked = sum(blocked %in% TRUE), .groups = "drop"),
            by = c("session_id", "scene", "scene_visit")) %>%
  left_join(end_control_rows %>% group_by(session_id, scene, scene_visit) %>%
              summarise(ev_end_control_opened = sum(event == "end_control_opened"),
                        last_opened_t = last_or_na(t[event == "end_control_opened"]), .groups = "drop"),
            by = c("session_id", "scene", "scene_visit")) %>%
  mutate(across(c(ev_grabs, ev_regrabs, ev_open_grabs, ev_container_enters, ev_container_exits,
                  ev_menu_blocked, ev_end_control_opened), ~ coalesce(as.integer(.x), 0L)),
         ev_hold_s = coalesce(ev_hold_s, 0),
         # Active seconds from scene enter to the first grab (NA = no grab).
         ev_time_to_first_grab_s = round(first_grab_t - enter_t -
                                           paused_between(session_id, enter_t, first_grab_t), 3)) %>%
  select(-enter_t)

# scene_enter.via (#29): "start" on the first entry of a session, then "menu"
# (Menu item) or "resume" (keypad resume after a crash). "facilitator_end" is
# never written (the facilitator's Beenden ends the session before WelcomeScene
# loads), null = the load came from somewhere else (a bug, or test code).
via_rows <- ix_events %>% filter(event == "scene_enter") %>%
  arrange(session_id, t, line) %>%
  transmute(participant_id, session_id, scene, scene_visit, t,
            via = map_chr(data, ~ as_chr(.x$via))) %>%
  group_by(session_id) %>% mutate(entry_no = row_number()) %>% ungroup() %>%
  left_join(sessions %>% select(session_id, resume_accepted), by = "session_id")
bad_via <- via_rows %>%
  filter((entry_no == 1 & !via %in% "start") |
           (entry_no > 1 & !via %in% c("menu", "resume")) |
           (via %in% "resume" & !resume_accepted %in% TRUE))
for (i in seq_len(nrow(bad_via))) {
  r <- bad_via[i, ]
  warn("scene_enter.via '", coalesce(r$via, "null"), "' unexpected: ", r$participant_id, " ", r$scene,
       " visit ", r$scene_visit, " (entry ", r$entry_no, " of the session",
       if (r$via %in% "resume") ", but session_start has no accepted resume" else "", ")")
}
# A menu entry should follow a menu_select (blocked false) for that scene in
# the visit just left.
menu_entries <- via_rows %>% filter(via %in% "menu") %>%
  left_join(menu_rows %>% filter(blocked %in% FALSE) %>%
              select(session_id, target_scene, menu_t = t),
            by = c("session_id", "scene" = "target_scene"), relationship = "many-to-many") %>%
  group_by(session_id, scene, scene_visit, t) %>%
  summarise(has_select = any(menu_t <= t & menu_t >= t - 30, na.rm = TRUE), .groups = "drop") %>%
  filter(!has_select)
if (nrow(menu_entries) > 0) {
  warn(nrow(menu_entries), " scene_enter with via 'menu' without a menu_select for that scene just before it")
}

# ---- 8. Merge sessions per participant and number visits across them ----------

visits <- visits_base %>%
  left_join(sessions %>% select(participant_id, session_id, session_no, build, schema_version,
                                layout_version, end_status, crash_before, start_utc),
            by = "session_id") %>%
  left_join(press_visit, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(task_completed, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(rated_counts, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(ignored_counts, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(wait_stats, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(interaction_visit, by = c("session_id", "scene", "scene_visit")) %>%
  mutate(
    # Outcome numbers exist only for scenes with a confirm or quiz in sessions
    # that measured outcomes; everywhere else they stay NA (not measured).
    # Inside a measuring session a visit without a press is a real 0.
    outcome_logged = session_id %in% outcome_sessions & scene %in% outcome_scenes,
    red_button = outcome_logged & scene %in% red_button_scenes,
    confirm_count = if_else(outcome_logged, coalesce(confirm_count, 0L), NA_integer_),
    rated_presses = if_else(outcome_logged, coalesce(ev_rated_presses, 0L), NA_integer_),
    presses_with_error = if_else(outcome_logged, coalesce(presses_with_error, 0L), NA_integer_),
    # Ignored presses and station rides exist only where there is a red button.
    confirm_ignored = if_else(red_button, coalesce(ev_confirm_ignored, 0L), NA_integer_),
    wait_s = if_else(red_button, coalesce(ev_wait_s, 0), NA_real_),
    # Rides after task_completed (end of order); NA when not completed.
    wait_after_done_s = if_else(red_button & !is.na(tc_seen), coalesce(ev_wait_after_done_s, 0), NA_real_),
    # Completed = the visit has a task_completed (Mixed/Complex: after the last
    # position, so a correct position 1 followed by a crash is not completed).
    completed = if_else(outcome_logged, coalesce(tc_seen, FALSE), NA),
    attempts_to_correct = tc_attempts_to_correct,
    time_to_first_confirm_s = coalesce(tc_time_to_first_confirm_s, time_to_first_confirm_presses),
    time_to_correct_s = tc_time_to_correct_s,
    # Error rate per visit: share of presses with >= 1 code; NA when 0 presses.
    error_rate = if_else(!is.na(confirm_count) & confirm_count > 0,
                         presses_with_error / confirm_count, NA_real_),
    # #29 interaction numbers: NA unless the session logged the group AND the
    # scene can produce the event (grabs: packages with PackageData;
    # container_*: an order container). Inside that, a visit without events is 0.
    interaction_logged = session_id %in% interaction_sessions,
    grab_logged = interaction_logged & scene %in% grab_scenes,
    container_logged = interaction_logged & scene %in% container_scenes,
    grabs = if_else(grab_logged, ev_grabs, NA_integer_),
    regrabs = if_else(grab_logged, ev_regrabs, NA_integer_),
    open_grabs = if_else(grab_logged, ev_open_grabs, NA_integer_),
    hold_s = if_else(grab_logged, ev_hold_s, NA_real_),
    time_to_first_grab_s = if_else(grab_logged, ev_time_to_first_grab_s, NA_real_),
    container_enters = if_else(container_logged, ev_container_enters, NA_integer_),
    container_exits = if_else(container_logged, ev_container_exits, NA_integer_),
    menu_blocked = if_else(interaction_logged, ev_menu_blocked, NA_integer_),
    end_control_opened = if_else(interaction_logged, ev_end_control_opened, NA_integer_),
    scene = factor(scene, levels = union(scene_levels, unique(scene)))) %>%
  arrange(participant_id, session_no, enter_t) %>%
  group_by(participant_id, scene) %>%
  mutate(
    visit_no = row_number(),                # visit number across all sessions
    repeat_visit = visit_no > 1,
    repeat_after_crash = repeat_visit & crash_before,
    # The visit that counts for questions 1-2: the first with >= 1 confirm.
    # Scenes without outcome data (WelcomeScene, pre-#28 logs): the first visit.
    counting = if (any(confirm_count > 0, na.rm = TRUE)) {
      row_number() == which(confirm_count > 0)[1]
    } else if (all(is.na(confirm_count))) {
      visit_no == 1
    } else {
      FALSE  # visited, outcome logged, never confirmed: no counting visit
    }) %>%
  ungroup()

# Facilitator tail (#44): in the last visit of a session the facilitator
# ended, the time from the last end_control_opened to the end is the
# facilitator's, not the trainee's (active seconds). NA everywhere else and
# when the group was not logged.
visits <- visits %>%
  group_by(session_id) %>%
  mutate(last_of_session = enter_t == max(enter_t)) %>%
  ungroup() %>%
  mutate(facilitator_tail_s = if_else(
    interaction_logged & end_status %in% "facilitator" & last_of_session & !is.na(last_opened_t),
    round(end_t - last_opened_t - paused_between(session_id, last_opened_t, end_t), 3), NA_real_),
    trainee_active_s = if_else(is.na(facilitator_tail_s), duration_s, round(duration_s - facilitator_tail_s, 3)))
no_open <- visits %>% filter(interaction_logged, end_status %in% "facilitator", last_of_session,
                             is.na(last_opened_t))
for (i in seq_len(nrow(no_open))) {
  warn("session ", no_open$session_id[i], " (", no_open$participant_id[i], ") ended by the facilitator ",
       "without end_control_opened in its last visit: facilitator time can't be cut")
}

unknown_scenes <- setdiff(levels(visits$scene), scene_levels)
if (length(unknown_scenes) > 0) {
  warn("scene(s) missing from scenes.csv (put last in charts): ", paste(unknown_scenes, collapse = ", "))
}

# ---- 8b. Layout versions: ConceptScene before / after "Zurück" (#82) -----------

# Decision 024: ConceptScene changed with layout "025" (captions, tour with
# "Zurück", quiz hidden until step 11, 1.5 s hold), so Concept numbers of a
# "019" build (or a header without layout_version) and of a "025" build are
# never pooled. When a set holds more than one layout, every ConceptScene
# figure (tour, quiz, items.csv, time to correct) is split by layout_version
# and data_quality.csv says mixed_layout. Schema 1 logs are left out of this
# check: none of their Concept data enters a figure (outcomes not read).
# Label for a layout in charts and keys: NA (header without the field) = "none".
layout_label <- function(x) coalesce(as.character(x), "none")
# The tour has "Zurück" from layout "025" on (#80, "029" too); "019" and none have not.
# Layout "029" (decision 030) changes the Concept station again (Ist follows the
# order container, hint tags, the lane demo waits for a clear station), so
# "025" and "029" Concept data are split like any two layouts.
tour_has_zurueck <- function(layout_version) {
  coalesce(suppressWarnings(as.integer(layout_version)) >= 25L, FALSE)
}
session_layout <- sessions %>% filter(!schema_version %in% 1L) %>%
  transmute(session_id, layout_version)
layouts_in_set <- sort(unique(layout_label(session_layout$layout_version)))
mixed_layout_set <- length(layouts_in_set) > 1
if (mixed_layout_set) {
  warn("set mixes layout_version ", paste(layouts_in_set, collapse = " / "),
       " (", paste(map_chr(layouts_in_set, function(l) paste0(l, ": ", paste(sort(unique(
         sessions$participant_id[!sessions$schema_version %in% 1L & layout_label(sessions$layout_version) == l])),
         collapse = ", "))), collapse = "; "),
       "): ConceptScene figures, items.csv and tour_steps.csv are split by layout_version, never pooled (decision 024)")
}
# Scene names for charts: in a mixed set ConceptScene becomes one level per
# layout ("ConceptScene [019]", "ConceptScene [025]"), in build order.
chart_scene_levels <- levels(visits$scene)
if (mixed_layout_set && "ConceptScene" %in% chart_scene_levels) {
  i_concept <- match("ConceptScene", chart_scene_levels)
  chart_scene_levels <- append(chart_scene_levels[-i_concept],
                               paste0("ConceptScene [", layouts_in_set, "]"), after = i_concept - 1)
}
chart_scene <- function(scene, session_id) {
  sc <- as.character(scene)
  if (mixed_layout_set) {
    lay <- layout_label(sessions$layout_version[match(session_id, sessions$session_id)])
    sc <- if_else(sc == "ConceptScene", paste0("ConceptScene [", lay, "]"), sc)
  }
  factor(sc, levels = chart_scene_levels)
}

# Complex position 1 (#93, decision 026 items 6-7): up to layout "025" the
# remainder quiz and the Worked Example board stay below the screen during
# the whole of position 1 ("quiz_visible"); from "029" they hide once both
# answers are right and shelf card 2 shows the trainee's own result
# ("result_on_card"). Same items, less scaffold, so position_1 results of the
# two levels are never pooled. Position 2 and the other scenes did not change.
# Split by scaffold level, not by every layout: "019" and "025" share it.
complex_p1_scaffold <- function(layout_version) {
  if_else(coalesce(suppressWarnings(as.integer(layout_version)) >= 29L, FALSE),
          "result_on_card", "quiz_visible")
}
p1_levels <- complex_p1_scaffold(session_layout$layout_version)
mixed_p1_scaffold <- n_distinct(p1_levels) > 1
# Chart label per level = the layouts of the set with that level, e.g. "019/025".
p1_level_label <- map_chr(set_names(c("quiz_visible", "result_on_card")), function(l)
  paste(sort(unique(layout_label(session_layout$layout_version[p1_levels == l]))), collapse = "/"))
if (mixed_p1_scaffold) {
  warn("set mixes Complex position_1 scaffold (layout ", p1_level_label[["quiz_visible"]], " = quiz_visible, ",
       p1_level_label[["result_on_card"]], " = result_on_card, #93): ComplexScene position_1 in ",
       "first_attempt_by_item.png and time_by_phase.png is split, never pooled; items.csv and ",
       "process_by_position.csv carry scaffold_level")
}
# Mixed position 1 (#94, decision 028): builds before #94 had Empf Kz 000
# there (cartons put in any way, never rated), from #94 Empf Kz 003 (3 snap
# places, EKZ_PLACEMENT possible). Same order line, one more thing to get
# right, so position_1 results of the two levels are never pooled (like
# Complex above). The level of a session = the Empf Kz of its first Mixed
# position-1 press (logged, else expected_empf_kz). Both builds can say
# layout "029", so the split is by Empf Kz, not by layout.
mixed_p1_session <- presses %>%
  filter(scene == "MixedPickingScene", event == "confirm_pressed", position %in% 1L, !is.na(empf_kz)) %>%
  arrange(session_id, t, line) %>%
  distinct(session_id, .keep_all = TRUE) %>%
  select(session_id, participant_id, layout_version, empf_kz)
mixed_m1_ekz <- n_distinct(mixed_p1_session$empf_kz) > 1
if (mixed_m1_ekz) {
  warn("set mixes Mixed position_1 Empf Kz (",
       paste(map_chr(sort(unique(mixed_p1_session$empf_kz)), function(l) paste0(l, ": ", paste(sort(unique(
         mixed_p1_session$participant_id[mixed_p1_session$empf_kz == l])), collapse = ", "))), collapse = "; "),
       "; #94): MixedPickingScene position_1 in first_attempt_by_item.png and time_by_phase.png ",
       "is split, never pooled; items.csv and process_by_position.csv carry empf_kz")
}
# empf_kz column of process_by_position.csv for a position without a press:
# expected_empf_kz with what the session's presses tell about the build.
item_empf_kz <- function(scene, item, session_id) {
  pos <- suppressWarnings(as.integer(str_remove(item, "^position_")))
  has <- presses %>% filter(event == "confirm_pressed") %>% distinct(session_id, has_empf_kz)
  expected_empf_kz(scene, if_else(str_detect(coalesce(item, ""), "^position_"), pos, NA_integer_),
                   has$has_empf_kz[match(session_id, has$session_id)],
                   sessions$layout_version[match(session_id, sessions$session_id)])
}

# scaffold_level column: set for Complex position_1 only, NA elsewhere.
p1_scaffold_level <- function(scene, item, session_id) {
  if_else(as.character(scene) == "ComplexScene" & item == "position_1",
          complex_p1_scaffold(sessions$layout_version[match(session_id, sessions$session_id)]),
          NA_character_)
}
# Item name for charts: "position_1 [019/025]" / "position_1 [029]" for
# Complex in a set that mixes the two levels, "position_1 [Empf Kz 000]" /
# "[Empf Kz 003]" for Mixed in a set that mixes them (#94), else the item.
chart_item <- function(scene, item, session_id) {
  lev <- p1_scaffold_level(scene, item, session_id)
  m1 <- if_else(as.character(scene) == "MixedPickingScene" & item == "position_1",
                mixed_p1_session$empf_kz[match(session_id, mixed_p1_session$session_id)],
                NA_character_)
  case_when(mixed_p1_scaffold & !is.na(lev) ~ paste0(item, " [", p1_level_label[lev], "]"),
            mixed_m1_ekz & !is.na(m1) ~ paste0(item, " [Empf Kz ", m1, "]"),
            TRUE ~ item)
}

# ---- 9. Compare the rebuilt visits with summary.csv ---------------------------

summary_files <- list.files(data_dir, pattern = "_summary\\.csv$", recursive = TRUE, full.names = TRUE)
if (!grepl("fake", data_dir)) summary_files <- summary_files[!grepl("/fake/", summary_files)]
# Every cell is read as text first, then the known columns get their type.
# So a summary.csv of another schema (schema 1 with dwell_* columns and no
# confirm_ignored) reads without errors; a missing column becomes NA below.
summary_num_cols <- c("schema_version", "scene_visit", "duration_s", "paused_s", "confirm_count",
                      "confirm_ignored", "attempts_to_correct", "time_to_first_confirm_s",
                      "time_to_correct_s", "pauses", "hmd_removed_count", "unity_errors",
                      "grabs", "container_enters", "container_exits")
summary_lgl_cols <- c("first_attempt_correct", "completed")
read_summary <- function(path) {
  read_csv(path, show_col_types = FALSE, na = "", col_types = cols(.default = col_character())) %>%
    mutate(across(any_of(summary_num_cols), as.numeric),
           across(any_of(summary_lgl_cols), as.logical))
}
summary_rows <- tibble()
dropped_summary_files <- dropped_event_files[0, ]
if (length(summary_files) > 0) {
  summary_rows <- map(summary_files, ~ read_summary(.x) %>% mutate(path = .x)) %>% list_rbind() %>%
    filter(is_valid_id(participant_id), !is_test_id(participant_id), in_set(participant_id))
  # Same rule as for events files: one file per session, the longest wins.
  dropped_summary_files <- summary_rows %>%
    count(participant_id, session_id, path, name = "n_rows") %>%
    pick_longest_files("summary")
  summary_rows <- summary_rows %>% filter(!path %in% dropped_summary_files$path) %>% select(-path)
  # Decision 022: drops_floor, fps_avg and dropped_frames are always empty
  # (no floor, perf or frame data in the app). A value there is not a
  # measurement we can trust: warn and never use it.
  for (cl in intersect(c("drops_floor", "fps_avg", "dropped_frames"), names(summary_rows))) {
    filled <- summary_rows %>% filter(!is.na(.data[[cl]]), .data[[cl]] != "")
    if (nrow(filled) > 0) {
      warn("summary.csv column ", cl, " has values although decision 022 says it is always empty (ignored): ",
           paste(unique(filled$participant_id), collapse = ", "))
    }
  }
}

# Sort "B:1;A:2" -> "A:2;B:1" so the code order never counts as a difference.
norm_codes <- function(x) map_chr(x, ~ if (is.na(.x)) NA_character_ else
  paste(sort(strsplit(.x, ";", fixed = TRUE)[[1]]), collapse = ";"))

# Outcome columns: confirm_count = rated attempts (red-button presses; quiz
# touches in ConceptScene; the Complex remainder quiz is not in it),
# confirm_ignored = ignored red-button presses (empty outside picking scenes).
outcome_compare_cols <- c("confirm_count", "confirm_ignored", "first_attempt_correct",
                          "attempts_to_correct", "time_to_first_confirm_s", "time_to_correct_s",
                          "completed", "error_codes_all")
# Interaction columns (#29): grabs = grab_start lines; container_* = enter /
# exit lines. Empty (NA) where the scene can't produce them and in builds
# before #29.
interaction_compare_cols <- c("grabs", "container_enters", "container_exits")
compare_cols <- c("duration_s", "paused_s", "pauses", "hmd_removed_count", "unity_errors",
                  outcome_compare_cols, interaction_compare_cols)
if (nrow(summary_rows) > 0) {
  # A column this summary.csv schema doesn't have is "not measured".
  for (cl in setdiff(compare_cols, names(summary_rows))) {
    summary_rows[[cl]] <- if (cl %in% summary_lgl_cols) NA else
      if (cl == "error_codes_all") NA_character_ else NA_real_
  }
  # Schema 1 rows: their outcome columns follow the v1 catalogue and are not
  # read (same rule as for the events, section 5), so they are not compared.
  summary_rows <- summary_rows %>%
    mutate(across(all_of(c(outcome_compare_cols, interaction_compare_cols)),
                  ~ if_else(schema_version %in% 1, NA, .x)))
}
summary_mismatches <- tibble(participant_id = character(), session_id = character(),
                             scene = character(), scene_visit = integer(), column = character(),
                             events_value = character(), summary_value = character())
if (nrow(summary_rows) > 0) {
  both <- visits %>%
    mutate(scene = as.character(scene)) %>%
    select(participant_id, session_id, scene, scene_visit, closed, all_of(compare_cols)) %>%
    full_join(summary_rows %>% mutate(scene_visit = as.integer(scene_visit)) %>%
                select(participant_id, session_id, scene, scene_visit, all_of(compare_cols)),
              by = c("participant_id", "session_id", "scene", "scene_visit"),
              suffix = c("_ev", "_sum"), keep = FALSE) %>%
    mutate(in_events = !is.na(closed),
           in_summary = !is.na(duration_s_sum) | !is.na(pauses_sum))

  # A summary row with no visit in the events would be a C# bug.
  orphan <- both %>% filter(!in_events)
  for (i in seq_len(nrow(orphan))) {
    warn("summary.csv row with no visit in events: ", orphan$participant_id[i], " ",
         orphan$scene[i], " visit ", orphan$scene_visit[i])
  }
  # A closed visit without a summary row (the open visit of a crash is expected to have none).
  no_row <- both %>% filter(in_events, closed, !in_summary,
                            session_id %in% unique(summary_rows$session_id))
  for (i in seq_len(nrow(no_row))) {
    warn("closed visit missing from summary.csv: ", no_row$participant_id[i], " ",
         no_row$scene[i], " visit ", no_row$scene_visit[i])
  }

  matched <- both %>% filter(in_events, in_summary)
  for (col in compare_cols) {
    a <- matched[[paste0(col, "_ev")]]
    b <- matched[[paste0(col, "_sum")]]
    if (col == "error_codes_all") { a <- norm_codes(a); b <- norm_codes(b) }
    differs <- if (is.numeric(a) && is.numeric(b)) {
      tol <- if (str_ends(col, "_s")) time_tolerance_s else 0
      (is.na(a) != is.na(b)) | (!is.na(a) & !is.na(b) & abs(a - b) > tol + 1e-9)
    } else {
      (is.na(a) != is.na(b)) | (!is.na(a) & !is.na(b) & as.character(a) != as.character(b))
    }
    if (any(differs)) {
      summary_mismatches <- bind_rows(summary_mismatches, tibble(
        participant_id = matched$participant_id[differs], session_id = matched$session_id[differs],
        scene = matched$scene[differs], scene_visit = matched$scene_visit[differs], column = col,
        events_value = as.character(a[differs]), summary_value = as.character(b[differs])))
    }
  }
  for (i in seq_len(nrow(summary_mismatches))) {
    r <- summary_mismatches[i, ]
    warn("summary.csv differs from events: ", r$participant_id, " ", r$scene, " visit ",
         r$scene_visit, " ", r$column, " events=", r$events_value, " summary=", r$summary_value)
  }
  message("Compared ", nrow(matched), " visits with summary.csv: ",
          nrow(summary_mismatches), " mismatch(es)")
} else {
  message("No summary.csv files found; nothing to compare.")
}

# ---- 10. Per participant: data-quality table -----------------------------------

# Course finished = a correct confirm (task_completed) in ComplexScene in any
# session, even when the session then ended as a crash (design section 6).
finished_ids <- visits %>% filter(scene == "ComplexScene", completed %in% TRUE) %>%
  distinct(participant_id) %>% pull(participant_id)

data_quality <- sessions %>%
  group_by(participant_id) %>%
  summarise(
    n_sessions = n(),
    first_start_utc = format(min(start_utc), "%Y-%m-%dT%H:%M:%SZ", tz = "UTC"),
    builds = paste(unique(build), collapse = ";"),
    # Header language per session ("NA" = log from before #11, not recorded).
    languages = paste(unique(coalesce(language, "NA")), collapse = ";"),
    # TRUE = a session ran in a language other than DE (warned above).
    # Always FALSE in the pilot set: pilots may run in EN (decision 023).
    language_not_de = !is_pilot_set & any(!is.na(language) & language != "DE"),
    schema_versions = paste(unique(schema_version), collapse = ";"),
    # Station layout per session ("NA" = schema 1 log, no layout_version).
    layout_versions = paste(unique(coalesce(layout_version, "NA")), collapse = ";"),
    # TRUE on every row when the set holds more than one layout_version
    # (section 8b): Concept figures are split by layout, never pooled.
    mixed_layout = mixed_layout_set,
    n_crashes = sum(end_status == "crash"),
    # end_status of the participant = how their last session ended.
    end_status = last(end_status),
    gap_h = if (n() > 1) round(max(gap_h, na.rm = TRUE), 2) else NA_real_,
    # Resume (#27): NA when no session of the person logged the resume fields
    # (pre-#27 logs); otherwise FALSE = measured, never offered / never declined.
    resume_offered = if (any(resume_logged)) any(!is.na(resume_offered_scene[resume_logged])) else NA,
    resume_declined = if (any(resume_logged)) any(resume_accepted[resume_logged] %in% FALSE) else NA,
    # TRUE = a session was started despite the "already finished" warning. NA when
    # no session of the person logged ended_session_warning (logs before the #27 fix).
    ended_session_override = if (any(warning_logged)) any(ended_session_warning[warning_logged] %in% TRUE) else NA,
    .groups = "drop") %>%
  mutate(multi_session = n_sessions > 1,
         mixed_build = str_detect(builds, ";"),
         # TRUE = a session of the person is a schema 1 log (outcomes not read).
         old_schema_log = str_detect(schema_versions, "(^|;)1(;|$)"),
         # NA = not measured: none of the person's sessions measured outcomes.
         course_finished = if_else(participant_id %in% sessions$participant_id[sessions$session_id %in% outcome_sessions],
                                   participant_id %in% finished_ids, NA)) %>%
  left_join(visits %>% group_by(participant_id) %>%
              summarise(n_visits = n(),
                        n_repeat_visits = sum(repeat_visit),
                        n_repeat_after_crash = sum(repeat_after_crash),
                        duration_censored = any(duration_censored),
                        active_s = round(sum(duration_s, na.rm = TRUE), 1),
                        paused_s = round(sum(paused_s, na.rm = TRUE), 1),
                        pauses = sum(pauses, na.rm = TRUE),
                        hmd_removed_count = sum(hmd_removed_count, na.rm = TRUE),
                        unity_errors = sum(unity_errors, na.rm = TRUE),
                        outcome_logged = any(outcome_logged),
                        total_confirms = if (any(outcome_logged)) sum(confirm_count, na.rm = TRUE) else NA_integer_,
                        # #29: NA when no session of the person logged the interaction group.
                        interaction_logged = any(interaction_logged),
                        end_control_opens = if (any(interaction_logged)) sum(end_control_opened, na.rm = TRUE) else NA_integer_,
                        menu_blocked = if (any(interaction_logged)) sum(menu_blocked, na.rm = TRUE) else NA_integer_,
                        open_grabs = if (any(grab_logged)) sum(open_grabs, na.rm = TRUE) else NA_integer_,
                        facilitator_tail_s = if (any(!is.na(facilitator_tail_s))) sum(facilitator_tail_s, na.rm = TRUE) else NA_real_,
                        .groups = "drop"),
            by = "participant_id") %>%
  left_join(invariant_breaks %>% count(participant_id, name = "invariant_breaks"), by = "participant_id") %>%
  left_join(unknown_codes %>% count(participant_id, name = "unknown_codes"), by = "participant_id") %>%
  left_join(summary_mismatches %>% count(participant_id, name = "summary_mismatches"), by = "participant_id") %>%
  left_join(exit_checks %>% count(participant_id, name = "exit_disagreements"), by = "participant_id") %>%
  left_join(bind_rows(dropped_event_files, dropped_summary_files) %>%
              count(participant_id, name = "duplicate_files"), by = "participant_id") %>%
  mutate(across(c(invariant_breaks, unknown_codes, summary_mismatches, exit_disagreements,
                  duplicate_files), ~ coalesce(.x, 0L)),
         error_cap_reached = participant_id %in% cap_sessions$participant_id) %>%
  select(participant_id, end_status, course_finished, n_sessions, multi_session, builds,
         mixed_build, languages, language_not_de, schema_versions, old_schema_log, layout_versions, mixed_layout, gap_h, n_crashes, resume_offered, resume_declined,
         ended_session_override, duration_censored, n_visits,
         n_repeat_visits, n_repeat_after_crash, active_s, paused_s, pauses, hmd_removed_count,
         unity_errors, error_cap_reached, outcome_logged, total_confirms, interaction_logged,
         end_control_opens, menu_blocked, open_grabs, facilitator_tail_s, invariant_breaks,
         unknown_codes, summary_mismatches, exit_disagreements, duplicate_files,
         first_start_utc) %>%
  arrange(participant_id)

write_csv(data_quality, file.path(output_dir, "data_quality.csv"), na = "")

# Data-quality picture: one row per participant, cells that need a look in orange.
dq_long <- data_quality %>%
  transmute(participant_id,
            end_status,
            # Empty = not measured (no outcome events logged).
            course_finished = coalesce(as.character(course_finished), ""),
            sessions = as.character(n_sessions), builds = str_replace_all(builds, ";", "\n"),
            # Orange = not schema 2 (a schema 1 log: its outcomes are NA, not read).
            schema = str_replace_all(schema_versions, ";", "\n"),
            # "NA" = not recorded (log from before #11); orange = not DE.
            language = if_else(language_not_de, paste0("!", languages), languages),
            gap_h = if_else(is.na(gap_h), "", as.character(gap_h)),
            # Empty = not measured (log from before #27).
            resume_declined = coalesce(as.character(resume_declined), ""),
            # Empty = not measured (log from before the #27 fix wave).
            ended_override = coalesce(as.character(ended_session_override), ""),
            censored = as.character(duration_censored),
            repeats = as.character(n_repeat_visits),
            paused_s = as.character(paused_s),
            pauses = as.character(pauses), hmd_off = as.character(hmd_removed_count),
            unity_err = as.character(unity_errors),
            error_cap = as.character(error_cap_reached),
            outcome = as.character(outcome_logged),
            # Empty = not measured (build before #29).
            interaction = as.character(interaction_logged),
            end_ctrl_opens = coalesce(as.character(end_control_opens), ""),
            invariant = as.character(invariant_breaks),
            unknown_code = as.character(unknown_codes),
            summary_diff = as.character(summary_mismatches),
            exit_diff = as.character(exit_disagreements),
            duplicates = as.character(duplicate_files)) %>%
  pivot_longer(-participant_id, names_to = "column", values_to = "value") %>%
  mutate(value = coalesce(value, ""),
         attention = case_when(
           column == "end_status" ~ value %in% c("crash", "quit", "facilitator"),
           column == "course_finished" ~ value == "FALSE",
           column == "sessions" ~ value != "1",
           column == "builds" ~ str_detect(value, "\n") | value %in% c("editor", "unknown", "NA"),
           column == "schema" ~ value != "2",
           column == "gap_h" ~ value != "",
           column == "language" ~ str_starts(value, "!"),
           column %in% c("censored", "error_cap", "resume_declined", "ended_override") ~ value == "TRUE",
           column %in% c("outcome", "interaction") ~ value == "FALSE",
           # One open is the facilitator's own Beenden; more = accidental opens.
           column == "end_ctrl_opens" ~ value != "" & value != "0" & value != "1",
           TRUE ~ !value %in% c("0", "")),
         column = factor(column, levels = unique(column)),
         participant_id = factor(participant_id, levels = rev(sort(unique(participant_id)))))

p_dq <- ggplot(dq_long, aes(column, participant_id, fill = attention)) +
  geom_tile(colour = "white") +
  geom_text(aes(label = value), size = 3) +
  scale_fill_manual(values = c(`FALSE` = "#F2F2F2", `TRUE` = "#E69F00"),
                    labels = c(`FALSE` = "ok", `TRUE` = "check"), name = NULL) +
  labs(title = "Whose data is affected by crashes, pauses, headset removal or errors?",
       subtitle = paste0("n = ", nrow(data_quality), " participants (test IDs dropped). ",
                         "Orange = check before using this person's data."),
       x = NULL, y = "Participant") +
  theme_minimal(base_size = 10) +
  theme(axis.text.x = element_text(angle = 30, hjust = 1), panel.grid = element_blank(),
        legend.position = "right")
ggsave(file.path(output_dir, "data_quality.png"), p_dq,
       width = 12, height = 2.4 + 0.32 * nrow(data_quality), dpi = 150, bg = "white")

# ---- 11. Question 1: first attempt per item (main), error rate per skill ------

# Main measure (spec 4.3, decision 021): was the FIRST attempt at an item
# right? An item is a quiz question (question_id) or an order position
# ("position_<n>"). Right only on a later touch counts as wrong at the first
# attempt, so guessing through the A/B/C options doesn't look like knowledge.
# Per participant, scene and item the first visit with an attempt at that
# item counts (repeat visits after a crash are later exposures).
visit_order <- visits %>%
  transmute(session_id, scene = as.character(scene), scene_visit, visit_no, repeat_visit)
item_attempts <- presses %>%
  filter(!is.na(item)) %>%
  inner_join(visit_order, by = c("session_id", "scene", "scene_visit")) %>%
  arrange(participant_id, scene, item, visit_no, t, line) %>%
  group_by(participant_id, scene, item) %>%
  filter(visit_no == min(visit_no)) %>%
  ungroup()
items <- item_attempts %>%
  group_by(participant_id, scene, item) %>%
  summarise(session_id = first(session_id),
            scene_visit = first(scene_visit),
            repeat_visit = first(repeat_visit),
            kind = if_else(first(event) == "quiz_answer", "quiz", "position"),
            quiz = first(quiz),
            first_attempt_correct = first(correct),
            attempts = n(),
            reached_correct = any(correct %in% TRUE),
            first_attempt_codes = paste(first(codes), collapse = ";"),
            empf_kz = first(empf_kz),
            .groups = "drop") %>%
  mutate(first_attempt_codes = na_if(first_attempt_codes, "")) %>%
  left_join(item_skills, by = c("scene", "item")) %>%
  mutate(
    # Positions without a skill in item_skills.csv: "picking" (whole position).
    skill = if_else(is.na(skill) & kind == "position", position_skill_default, skill),
    scaffolded = coalesce(scaffolded, FALSE),
    # Header layout of the item's session (section 8b): ConceptScene items of
    # different layouts are never pooled.
    layout_version = sessions$layout_version[match(session_id, sessions$session_id)],
    # Complex position_1 scaffold level (#93); split in the chart when mixed.
    scaffold_level = p1_scaffold_level(scene, item, session_id),
    chart_item = chart_item(scene, item, session_id),
    chart_scene = chart_scene(scene, session_id),
    scene = factor(scene, levels = levels(visits$scene))) %>%
  arrange(participant_id, scene, item)
unmapped_items <- items %>% filter(is.na(skill)) %>% distinct(scene, item)
for (i in seq_len(nrow(unmapped_items))) {
  warn("quiz item ", unmapped_items$item[i], " in ", unmapped_items$scene[i],
       " is not in item_skills.csv: left out of first_attempt_by_item.png")
}
write_csv(items %>% select(participant_id, scene, layout_version, item, kind, quiz, skill, scaffolded, scaffold_level, empf_kz,
                           first_attempt_correct, attempts, reached_correct, first_attempt_codes,
                           session_id, scene_visit, repeat_visit),
          file.path(output_dir, "items.csv"), na = "")

# Empty chart with a note, used when there are no outcome events (pre-#28 logs).
empty_chart <- function(title) {
  ggplot() +
    annotate("text", x = 0, y = 0, label = "keine/no outcome data yet", size = 6, colour = "grey40") +
    labs(title = title, subtitle = "keine/no outcome data yet (confirm_pressed / quiz_answer come with #28)") +
    theme_void()
}

# 95 % Wilson interval for k of n (better than +-1.96 SE for small n and
# shares near 0 or 100 %).
wilson <- function(k, n, z = 1.96) {
  p <- k / n
  centre <- (p + z^2 / (2 * n)) / (1 + z^2 / n)
  half <- z * sqrt(p * (1 - p) / n + z^2 / (4 * n^2)) / (1 + z^2 / n)
  tibble(lo = pmax(0, centre - half), hi = pmin(1, centre + half))
}

# Skills in a fixed order (scene_skills.csv, then "picking") and one
# colour-blind-safe colour each, shared by all Q1 charts.
skill_levels <- unique(c(unique(scene_skills$skill), sort(unique(na.omit(item_skills$skill))),
                         position_skill_default))
skill_colours <- setNames(okabe_ito[seq_along(skill_levels)], skill_levels)

title_items <- "Is the first attempt per item right more often later in the course?"
item_shares <- items %>% filter(!is.na(skill)) %>%
  group_by(skill, scene, chart_scene, item, chart_item, kind, scaffolded) %>%
  summarise(k = sum(first_attempt_correct %in% TRUE), n = n(), .groups = "drop")
if (nrow(item_shares) == 0) {
  p_items <- empty_chart(title_items)
  warn("no outcome data: first_attempt_by_item.png is empty")
} else {
  # x axis = scene + item in build order; within a scene the order of
  # item_skills.csv (quiz before the positions in Complex), then positions.
  item_shares <- item_shares %>%
    bind_cols(wilson(item_shares$k, item_shares$n)) %>%
    mutate(share = k / n,
           csv_order = match(paste(scene, item), paste(item_skills$scene, item_skills$item)),
           pos_no = suppressWarnings(as.integer(str_remove(item, "^position_"))),
           item_rank = coalesce(csv_order, 1000L + coalesce(pos_no, 0L)),
           # chart_scene: "ConceptScene [019]" etc. in a mixed-layout set (8b).
           # chart_item: "position_1 [019/025]" etc. for Complex when mixed (#93).
           x_label = paste0(str_remove(as.character(chart_scene), "(Picking)?Scene"), "\n",
                            str_replace(chart_item, " \\[", "\n[")),
           skill = factor(skill, levels = skill_levels),
           # Hollow = scaffolded (the app handed the trainee the numbers).
           fill_colour = if_else(scaffolded, "white", skill_colours[as.character(skill)])) %>%
    arrange(as.integer(chart_scene), item_rank, chart_item) %>%
    mutate(x_label = factor(x_label, levels = unique(x_label)))
  p_items <- ggplot(item_shares, aes(x_label, 100 * share, colour = skill)) +
    geom_linerange(aes(ymin = 100 * lo, ymax = 100 * hi), linewidth = 0.7, alpha = 0.6) +
    geom_point(aes(shape = kind, fill = fill_colour), size = 3.5, stroke = 1.2) +
    geom_text(aes(y = -9, label = paste0(k, "/", n)), colour = "grey30", size = 3) +
    facet_wrap(~ skill, scales = "free_x", drop = TRUE) +
    scale_colour_manual(values = skill_colours, guide = "none") +
    scale_fill_identity() +
    scale_shape_manual(values = c(quiz = 21, position = 22), name = NULL,
                       labels = c(quiz = "quiz question", position = "order position")) +
    scale_y_continuous(limits = c(-12, 105), breaks = seq(0, 100, 25)) +
    labs(title = title_items,
         subtitle = paste0("Point = share of participants right at the first attempt; line = 95 % Wilson interval; ",
                           "label = right / participants.\nHollow = scaffolded (Complex position 1: the remainder ",
                           "quiz gave the numbers).\nQuiz guess rate 33 % (A/B/C), 50 % (EKZ questions). ",
                           "Indication only: fixed scene order, no control group."),
         x = "Scene and item (build order)", y = "Right at the first attempt (% of participants)") +
    theme_minimal(base_size = 11) +
    theme(legend.position = "bottom")
}
ggsave(file.path(output_dir, "first_attempt_by_item.png"), p_items, width = 11, height = 7, dpi = 150, bg = "white")

# Secondary measure: error rate per skill. Only the attempts counted in
# confirm_count (red-button presses; quiz touches in ConceptScene) can enter;
# the Complex remainder quiz is in items above.
counting_visits <- visits %>% filter(counting, confirm_count > 0)

# Which skills does an attempt test? The skill of its item, per item_skills.csv
# (a Concept quiz question tests exactly one skill). An order position without
# a skill there tests every skill its scene tests (scene_skills.csv): a press
# can fail on any of them. Why: the Concept quiz asks its questions in a fixed
# order, so "all presses of the visit" as denominator would dilute each skill
# with touches on the other questions, and "the visit's first press" would
# always be the first question (other skills 0 % by construction).
# Presses of scaffolded items (Complex position 1: the remainder quiz handed
# the trainee the numbers) measure picking, not calculation (decision 021),
# so they are left out: numerator AND denominator.
press_skill_tests <- presses %>%
  filter(counted, !scaffolded, !is.na(item)) %>%
  select(participant_id, session_id, scene, scene_visit, t, line, press_id, event, item, ekz_opportunity) %>%
  left_join(item_skills %>% filter(!is.na(skill)) %>% select(scene, item, item_skill = skill),
            by = c("scene", "item")) %>%
  left_join(skill_pairs %>% rename(scene_skill = skill), by = "scene", relationship = "many-to-many") %>%
  # Item with a skill: only that skill. Position without one: every scene skill,
  # except ekz_placement, which a press tests only at an EKZ-3 opportunity
  # (Empf Kz "003", quantity right; #94, decision 028). So Mixed position 2,
  # Mixed position 1 before layout "029" and wrong-quantity presses never
  # enter its denominator.
  filter(if_else(is.na(item_skill), event == "confirm_pressed", scene_skill == item_skill),
         is.na(item_skill) & scene_skill == "ekz_placement" & ekz_opportunity |
           !(is.na(item_skill) & scene_skill == "ekz_placement")) %>%
  transmute(participant_id, session_id, scene, scene_visit, t, line, press_id, skill = scene_skill)

# Per visit and skill: skill_presses = attempts that tested the skill (the
# denominator); first_skill_press_id = the first of them (the first attempt
# at an item of that skill).
skill_press_base <- press_skill_tests %>%
  arrange(session_id, scene, scene_visit, t, line) %>%
  group_by(session_id, scene, scene_visit, skill) %>%
  summarise(skill_presses = n(), first_skill_press_id = press_id[1], .groups = "drop")

# Per counting visit and per skill it tested: share of those attempts with a
# code of that skill. A tested but error-free skill gives 0 %, not missing; a
# skill with no attempt in the visit (question never reached, only scaffolded
# presses) has no rate.
skill_rates <- counting_visits %>%
  mutate(scene = as.character(scene)) %>%
  select(participant_id, session_id, scene, scene_visit) %>%
  inner_join(skill_press_base, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(press_codes %>% filter(!is.na(skill)) %>%
              distinct(session_id, scene, scene_visit, press_id, skill) %>%
              # Only codes on attempts that tested the skill (same set as the denominator).
              semi_join(press_skill_tests, by = c("press_id", "skill")) %>%
              group_by(session_id, scene, scene_visit, skill) %>%
              summarise(presses_with_skill_error = n(),
                        skill_error_press_ids = list(press_id), .groups = "drop"),
            by = c("session_id", "scene", "scene_visit", "skill")) %>%
  mutate(presses_with_skill_error = coalesce(presses_with_skill_error, 0L),
         # The first attempt at an item of this skill carried a code of this skill.
         first_attempt_skill_error = map2_lgl(skill_error_press_ids, first_skill_press_id,
                                              ~ !is.null(.x) && .y %in% .x),
         error_rate = presses_with_skill_error / skill_presses,
         # One x position per layout for ConceptScene in a mixed set (8b).
         chart_scene = chart_scene(scene, session_id),
         scene = factor(scene, levels = levels(visits$scene))) %>%
  select(-skill_error_press_ids, -first_skill_press_id)

# Scenes with skills, in build order, so every layer uses the same x order
# (ConceptScene split by layout in a mixed set).
skill_scene_limits <- chart_scene_levels[str_remove(chart_scene_levels, " \\[.*\\]$") %in% skill_pairs$scene]

title_q1 <- "Secondary: does the error rate per press fall across the course, per skill?"
if (nrow(skill_rates) == 0) {
  p_skill <- empty_chart(title_q1)
  warn("no outcome data: error_rate_by_skill.png and error_rate_by_skill_first_attempt.png are empty")
} else {
  # Mean per scene and skill (= mean of participants, each with one visit).
  skill_means <- skill_rates %>% group_by(chart_scene, skill) %>%
    summarise(mean_rate = mean(error_rate), n = n(), .groups = "drop")
  p_skill <- ggplot(skill_rates, aes(chart_scene, 100 * error_rate, colour = skill)) +
    geom_jitter(width = 0.12, height = 0, alpha = 0.55, size = 2) +
    # Lines only join scenes where a skill is tested more than once.
    geom_line(data = skill_means %>% group_by(skill) %>% filter(n() > 1),
              aes(y = 100 * mean_rate, group = skill), linewidth = 0.8) +
    geom_point(data = skill_means, aes(y = 100 * mean_rate), shape = 18, size = 4) +
    geom_text(data = skill_means, aes(y = -9, label = paste0("n=", n)), colour = "grey30", size = 3) +
    facet_wrap(~ skill) +
    scale_colour_manual(values = skill_colours, guide = "none") +
    scale_x_discrete(limits = skill_scene_limits) +
    scale_y_continuous(limits = c(-12, 105), breaks = seq(0, 100, 25)) +
    labs(title = title_q1,
         subtitle = paste0("Dots = one participant's first visit with >= 1 confirm; diamond = mean. ",
                           "Denominator = attempts at items of this skill (Concept: touches on its questions;
",
                           "picking: every press, a position tests all skills of its scene;\n",
                           "ekz_placement only presses with the right quantity at an Empf Kz 003 position). ",
                           "Scaffolded Complex position 1 left out. ",
                           "Indication only: fixed scene order, no control group."),
         x = "Scene (build order)", y = "Attempts at this skill with its error (%)") +
    theme_minimal(base_size = 11) +
    theme(axis.text.x = element_text(angle = 25, hjust = 1))
}
ggsave(file.path(output_dir, "error_rate_by_skill.png"), p_skill, width = 10, height = 6.5, dpi = 150, bg = "white")

# Second chart: first attempts only (did the first press already avoid this error?).
title_q1b <- "First attempt: how many participants made an error of this skill?"
if (nrow(skill_rates) == 0) {
  p_first <- empty_chart(title_q1b)
} else {
  first_rates <- skill_rates %>% group_by(chart_scene, skill) %>%
    summarise(k = sum(first_attempt_skill_error), n = n(), .groups = "drop") %>%
    mutate(share = k / n)
  p_first <- ggplot(first_rates, aes(chart_scene, 100 * share, colour = skill, group = skill)) +
    geom_line(data = first_rates %>% group_by(skill) %>% filter(n() > 1), linewidth = 0.8) +
    geom_point(size = 3) +
    geom_text(aes(label = paste0(k, "/", n)), vjust = -1, size = 3, colour = "grey20") +
    facet_wrap(~ skill) +
    scale_colour_manual(values = skill_colours, guide = "none") +
    scale_x_discrete(limits = skill_scene_limits) +
    scale_y_continuous(limits = c(0, 110), breaks = seq(0, 100, 25)) +
    labs(title = title_q1b,
         subtitle = paste0("Attempt 1 = the first attempt at an item of this skill (Concept: its first question, ",
                           "not the quiz's first touch).
Label = participants with this error on that attempt / ",
                           "participants who attempted the skill. Scaffolded Complex position 1 left out."),
         x = "Scene (build order)", y = "Participants with the error on attempt 1 (%)") +
    theme_minimal(base_size = 11) +
    theme(axis.text.x = element_text(angle = 25, hjust = 1))
}
ggsave(file.path(output_dir, "error_rate_by_skill_first_attempt.png"), p_first, width = 10, height = 6.5, dpi = 150, bg = "white")

# ---- 12. Which errors are common in which scene (question 2) -------------------

title_q2 <- "Which error codes occur in which scene?"
codes_counting <- press_codes %>%
  filter(counted) %>%
  semi_join(counting_visits %>% mutate(scene = as.character(scene)),
            by = c("session_id", "scene", "scene_visit")) %>%
  mutate(code = if_else(is.na(skill), paste0(code, " (unknown)"), code),
         scene = chart_scene(scene, session_id))
if (nrow(codes_counting) == 0) {
  p_codes <- empty_chart(title_q2)
  warn("no error codes in counting visits: error_codes_by_scene.png is empty")
} else {
  scene_n <- counting_visits %>% mutate(scene = chart_scene(scene, session_id)) %>% group_by(scene) %>%
    summarise(n_people = n_distinct(participant_id), n_presses = sum(confirm_count), .groups = "drop")
  code_totals <- codes_counting %>% count(scene, name = "n_codes")
  p_codes <- ggplot(codes_counting, aes(scene, fill = code)) +
    geom_bar(colour = "white", linewidth = 0.2) +
    geom_text(data = scene_n %>% left_join(code_totals, by = "scene") %>% mutate(n_codes = coalesce(n_codes, 0L)),
              aes(x = scene, y = n_codes, label = paste0("n=", n_people, " people\n", n_presses, " presses")),
              inherit.aes = FALSE, vjust = -0.3, size = 3) +
    scale_fill_viridis_d(option = "D", end = 0.95, name = "Error code") +
    scale_x_discrete(drop = FALSE) +
    scale_y_continuous(expand = expansion(mult = c(0, 0.25))) +
    labs(title = title_q2,
         subtitle = paste0("Counting visits only (first visit with >= 1 confirm). One press can carry several codes. ",
                           "Complex remainder quiz not included (see items.csv)."),
         x = "Scene (build order)", y = "Error codes logged (count)") +
    theme_minimal(base_size = 11) +
    theme(axis.text.x = element_text(angle = 25, hjust = 1))
}
ggsave(file.path(output_dir, "error_codes_by_scene.png"), p_codes, width = 10, height = 6, dpi = 150, bg = "white")

# ---- 13. Time to correct per scene, with survivors counted (question 2) -------

title_ttc <- "How long does it take to reach a correct order in each scene?"
# Everyone who entered a scene with outcome logging: reached correct on the
# counting visit, or not (no correct, or no confirm at all).
# In a mixed-layout set ConceptScene is one group per layout (section 8b).
ttc_people <- visits %>% filter(outcome_logged) %>%
  mutate(scene = chart_scene(scene, session_id)) %>%
  group_by(participant_id, scene) %>%
  summarise(has_counting = any(counting & confirm_count > 0, na.rm = TRUE),
            time_to_correct_s = if (has_counting) time_to_correct_s[counting & confirm_count > 0][1] else NA_real_,
            reached = has_counting & !is.na(time_to_correct_s), .groups = "drop")
if (!any(ttc_people$reached)) {
  p_ttc <- empty_chart(title_ttc)
  warn("no task_completed events: time_to_correct_by_scene.png is empty")
} else {
  ttc_n <- ttc_people %>% group_by(scene) %>%
    summarise(n_reached = sum(reached), n_never = sum(!reached),
              y = max(time_to_correct_s, na.rm = TRUE), .groups = "drop") %>%
    mutate(y = if_else(is.finite(y), y, 0))
  p_ttc <- ggplot(ttc_people %>% filter(reached), aes(scene, time_to_correct_s)) +
    geom_boxplot(outlier.shape = NA, fill = "#56B4E9", alpha = 0.35, width = 0.5) +
    geom_jitter(width = 0.1, height = 0, size = 2, alpha = 0.7) +
    geom_text(data = ttc_n, aes(x = scene, y = y,
                                label = paste0("n=", n_reached, " reached\n", n_never, " never")),
              vjust = -0.4, size = 3, colour = "grey20", inherit.aes = FALSE) +
    scale_x_discrete(drop = FALSE) +
    scale_y_continuous(expand = expansion(mult = c(0.05, 0.25))) +
    labs(title = title_ttc,
         subtitle = paste0("Active time (pauses removed) from scene start to the first correct confirm, ",
                           "counting visit. 'never' = no correct confirm in it (survivor bias)."),
         x = "Scene (build order)", y = "Time to correct (s, active)") +
    theme_minimal(base_size = 11) +
    theme(axis.text.x = element_text(angle = 25, hjust = 1))
}
ggsave(file.path(output_dir, "time_to_correct_by_scene.png"), p_ttc, width = 10, height = 6, dpi = 150, bg = "white")

# ---- 13b. Where the time of a position goes (question 3, #29) ------------------

# One row per order position of a picking-scene visit that logged the
# interaction group. The position's active time (pauses removed) is split at
# two moments taken from the interaction events:
#   start        position 1: scene enter (Complex: storage box 1 arrived, after
#                the remainder quiz); position p > 1: storage box p arrived
#                (station_move_arrived storage_in), so the box-change wait
#                (wait_s) is never part of it
#   first grab   the first grab_start after the start
#   last change  the last container_enter / _exit before position_completed
#   done         position_completed (the correct press)
# read_s = start -> first grab (reading the WMS line and the cards),
# pick_s = first grab -> last change (picking, incl. wrong presses and fixing
# the box), check_s = last change -> done (looking at the box and the WMS,
# then the correct press). They add up to position_s. A position that was
# never completed keeps read_s (if a grab happened) and NA for the rest.
title_q3 <- "Where does the time of an order position go: reading, picking or checking?"
first_after <- function(x, from, to) {
  x <- x[!is.na(x) & x > from & x <= to]
  if (length(x) == 0) NA_real_ else min(x)
}
last_before <- function(x, from, to) {
  x <- x[!is.na(x) & x > from & x <= to]
  if (length(x) == 0) NA_real_ else max(x)
}
active_between <- function(sid, from_t, to_t) {
  if_else(is.na(from_t) | is.na(to_t), NA_real_,
          round(to_t - from_t - paused_between(sid, from_t, to_t), 3))
}
proc_visits <- visits %>% filter(container_logged) %>%
  transmute(participant_id, session_id, scene = as.character(scene), scene_visit, enter_t,
            visit_end_t = end_t, counting, repeat_visit)
# Positions of each visit: position 1 always, plus every position that was
# pressed at or whose storage box arrived.
proc_positions <- bind_rows(
  proc_visits %>% transmute(session_id, scene, scene_visit, position = 1L),
  presses %>% filter(event == "confirm_pressed", !is.na(position)) %>%
    transmute(session_id, scene, scene_visit, position),
  storage_in %>% filter(!is.na(arrived_t)) %>% transmute(session_id, scene, scene_visit, position)) %>%
  distinct() %>%
  inner_join(proc_visits, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(storage_in %>% select(session_id, scene, scene_visit, position, box_arrived_t = arrived_t),
            by = c("session_id", "scene", "scene_visit", "position")) %>%
  left_join(pos_done_t %>% rename(position = done_position),
            by = c("session_id", "scene", "scene_visit", "position")) %>%
  mutate(start_t = case_when(position == 1L & scene != "ComplexScene" ~ enter_t,
                             TRUE ~ box_arrived_t),
         window_end_t = coalesce(done_t, visit_end_t),
         completed = !is.na(done_t))
grab_t <- grabs %>% select(session_id, scene, scene_visit, g_t = start_t)
change_t <- container_rows %>% select(session_id, scene, scene_visit, c_t = t)
press_t <- presses %>% filter(event == "confirm_pressed") %>%
  select(session_id, scene, scene_visit, position, p_t = t)
first_press_empf_kz <- presses %>% filter(event == "confirm_pressed", !is.na(position)) %>%
  arrange(t, line) %>%
  distinct(session_id, scene, scene_visit, position, .keep_all = TRUE) %>%
  transmute(key = paste(session_id, scene, scene_visit, position), empf_kz)
process <- proc_positions %>%
  rowwise() %>%
  mutate(
    first_grab_t = first_after(grab_t$g_t[grab_t$session_id == session_id & grab_t$scene == scene &
                                             grab_t$scene_visit == scene_visit], start_t, window_end_t),
    last_change_t = last_before(change_t$c_t[change_t$session_id == session_id & change_t$scene == scene &
                                               change_t$scene_visit == scene_visit], start_t, window_end_t),
    presses = sum(press_t$session_id == session_id & press_t$scene == scene &
                    press_t$scene_visit == scene_visit & press_t$position == position)) %>%
  ungroup() %>%
  mutate(
    read_s = active_between(session_id, start_t, first_grab_t),
    pick_s = if_else(completed, active_between(session_id, first_grab_t, last_change_t), NA_real_),
    check_s = if_else(completed, active_between(session_id, last_change_t, done_t), NA_real_),
    position_s = if_else(completed, active_between(session_id, start_t, done_t), NA_real_),
    item = paste0("position_", position)) %>%
  left_join(item_skills %>% select(scene, item, scaffolded), by = c("scene", "item")) %>%
  mutate(scaffolded = coalesce(scaffolded, FALSE),
         # Complex position_1 scaffold level (#93, section 8b): split when mixed.
         layout_version = sessions$layout_version[match(session_id, sessions$session_id)],
         scaffold_level = p1_scaffold_level(scene, item, session_id),
         # Empf Kz of the position (#94): logged at its first press, else from the layout.
         empf_kz = coalesce(first_press_empf_kz$empf_kz[match(paste(session_id, scene, scene_visit, position),
                                                              first_press_empf_kz$key)],
                            item_empf_kz(scene, item, session_id)),
         chart_item = chart_item(scene, item, session_id),
         scene = factor(scene, levels = levels(visits$scene))) %>%
  arrange(participant_id, scene, scene_visit, position)
write_csv(process %>% select(participant_id, session_id, scene, scene_visit, counting, repeat_visit,
                             layout_version, position, scaffolded, scaffold_level, empf_kz, completed, presses,
                             read_s, pick_s, check_s, position_s),
          file.path(output_dir, "process_by_position.csv"), na = "")

proc_long <- process %>% filter(counting, completed) %>%
  select(participant_id, scene, position, chart_item, read_s, pick_s, check_s) %>%
  pivot_longer(c(read_s, pick_s, check_s), names_to = "phase", values_to = "seconds") %>%
  filter(!is.na(seconds)) %>%
  mutate(phase = factor(phase, levels = c("read_s", "pick_s", "check_s"),
                        labels = c("read (start -> first grab)", "pick (first grab -> last box change)",
                                   "check (last box change -> correct press)")),
         # chart_item keeps Complex position 1 split by scaffold level (#93)
         # and Mixed position 1 by Empf Kz (#94).
         slot = paste0(str_remove(as.character(scene), "PickingScene|Scene"), "\n",
                       str_replace(str_replace(chart_item, "^position_", "position "), " \\[", "\n[")))
if (nrow(proc_long) == 0) {
  p_q3 <- empty_chart(title_q3)
  warn("no completed positions with interaction events: time_by_phase.png is empty")
} else {
  slot_levels <- proc_long %>% distinct(scene, position, slot) %>% arrange(scene, position) %>% pull(slot)
  proc_long <- proc_long %>% mutate(slot = factor(slot, levels = slot_levels))
  proc_n <- proc_long %>% group_by(slot) %>%
    summarise(n = n_distinct(participant_id), y = max(seconds), .groups = "drop")
  p_q3 <- ggplot(proc_long, aes(slot, seconds, colour = phase)) +
    geom_point(position = position_jitterdodge(jitter.width = 0.15, dodge.width = 0.7, seed = 29),
               size = 2, alpha = 0.75) +
    stat_summary(fun = median, geom = "crossbar", width = 0.2, linewidth = 0.4,
                 position = position_dodge(width = 0.7), show.legend = FALSE) +
    geom_text(data = proc_n, aes(x = slot, y = y, label = paste0("n=", n)), inherit.aes = FALSE,
              vjust = -0.6, size = 3, colour = "grey20") +
    scale_colour_manual(values = unname(okabe_ito[c(2, 1, 3)]), name = "Phase") +
    scale_y_continuous(expand = expansion(mult = c(0.02, 0.15))) +
    labs(title = title_q3,
         subtitle = paste0("Completed positions of counting visits; active seconds (pauses and box-change ",
                           "waits removed). Bar = median, point = one participant."),
         x = "Scene and order position (build order)", y = "Active time (s)") +
    theme_minimal(base_size = 11) +
    theme(legend.position = "bottom", legend.direction = "vertical")
}
ggsave(file.path(output_dir, "time_by_phase.png"), p_q3, width = 10, height = 6.5, dpi = 150, bg = "white")

# ---- 13c. ConceptScene field tour (#82): time per step, Back presses --------

# Every shown tour step logs one tour_step {step_id, step_index} (decision 024
# item 9: no other tour event). R derives what the trainee pressed from the
# change of step_index against the previous tour_step of the same visit:
#   +1 = "Weiter" (next), -1 = "Zurück" (back), last step -> 1 = "Von vorn"
#   (restart, a jump of -(tour_n_steps - 1)); the first line of a visit =
#   start. Anything else ("jump", "repeat") should not happen and is warned.
# Order = line order in the file, not t: two presses in one frame share t.
# Direction restarts at every scene_visit (a new visit starts the tour anew).
#
# Per shown step: active time from its line to the next tour_step of the
# visit, cut at the visit's first quiz_answer (the quiz then has the
# trainee's attention; step 11 ends there) and at the visit end (scene_exit;
# the last event of a crashed visit, then time_censored), minus paused time.
# A step shown after the first quiz_answer (e.g. "Von vorn" during the quiz)
# is counted in shows_after_quiz but adds no time.
# A press is booked on the step the trainee left (the step on screen when
# "Zurück" was pressed). quick_next_back = "Weiter" to this step, then
# "Zurück" within < tour_quick_back_s active seconds: a likely mis-press.
# Back counts exist only for layouts with "Zurück" ("025" on, section 8b);
# for "019" or a header without layout_version they are NA, never 0.

# Number of tour steps: taken from the logs (the highest step_index of the
# set) unless set here; "Von vorn" is a jump from this step to 1.
if (!exists("tour_n_steps")) tour_n_steps <- NULL
tour_quick_back_s <- 2

tour_rows <- v2_events %>%
  filter(scene == "ConceptScene", event == "tour_step", session_id %in% outcome_sessions) %>%
  transmute(participant_id, session_id, scene_visit, t, line,
            step_id = map_chr(data, ~ as_chr(.x$step_id)),
            step_index = map_int(data, ~ as_int(.x$step_index))) %>%
  left_join(sessions %>% select(session_id, layout_version), by = "session_id")
bad_step <- tour_rows %>% filter(is.na(step_index) | step_index < 1)
if (nrow(bad_step) > 0) {
  warn(nrow(bad_step), " tour_step line(s) without a valid step_index (ignored): ",
       paste(unique(bad_step$participant_id), collapse = ", "))
}
tour_rows <- tour_rows %>% filter(!is.na(step_index), step_index >= 1)
n_steps <- if (!is.null(tour_n_steps)) tour_n_steps else if (nrow(tour_rows) > 0) max(tour_rows$step_index) else NA_integer_
# Step names per index (the tour's step_id), for the table and the charts.
step_names <- tour_rows %>% filter(!is.na(step_id)) %>% count(step_index, step_id) %>%
  group_by(step_index) %>% slice_max(n, n = 1, with_ties = FALSE) %>% ungroup() %>% select(-n)
odd_names <- tour_rows %>% distinct(step_index, step_id) %>% count(step_index) %>% filter(n > 1)
if (nrow(odd_names) > 0) {
  warn("tour step_index with more than one step_id (renamed step? the name of the most frequent is used): ",
       paste(odd_names$step_index, collapse = ", "))
}

# Visit facts: end of the visit and the first quiz touch.
tour_visit_facts <- visits_base %>%
  filter(scene == "ConceptScene") %>%
  select(session_id, scene_visit, end_t, closed) %>%
  left_join(presses %>% filter(scene == "ConceptScene", event == "quiz_answer") %>%
              group_by(session_id, scene_visit) %>% summarise(first_quiz_t = min(t), .groups = "drop"),
            by = c("session_id", "scene_visit"))

tour_shows <- tour_rows %>%
  arrange(session_id, scene_visit, line) %>%
  group_by(session_id, scene_visit) %>%
  mutate(prev_index = lag(step_index),
         prev_t = lag(t),
         next_t = lead(t)) %>%
  ungroup() %>%
  left_join(tour_visit_facts, by = c("session_id", "scene_visit")) %>%
  mutate(
    delta = step_index - prev_index,
    direction = case_when(
      is.na(prev_index) ~ "start",
      delta == 1 ~ "next",
      delta == -1 ~ "back",
      prev_index == n_steps & step_index == 1 ~ "restart",
      delta == 0 ~ "repeat",
      TRUE ~ "jump"),
    after_quiz = !is.na(first_quiz_t) & t >= first_quiz_t,
    show_end_t = pmin(coalesce(next_t, Inf), coalesce(first_quiz_t, Inf), end_t),
    # The visit end of a crashed visit is only its last event: censored.
    time_censored = !after_quiz & is.na(next_t) & is.na(first_quiz_t) & !closed,
    show_s = if_else(after_quiz, NA_real_,
                     round(pmax(0, show_end_t - t - paused_between(session_id, t, show_end_t)), 3))) %>%
  group_by(session_id, scene_visit) %>%
  mutate(prev_direction = lag(direction),
         # Active seconds the previous step stayed on screen before this press.
         gap_s = round(t - prev_t - paused_between(session_id, prev_t, t), 3),
         quick_back = direction == "back" & prev_direction %in% "next" & gap_s < tour_quick_back_s) %>%
  ungroup()

odd_moves <- tour_shows %>% filter(direction %in% c("jump", "repeat"))
for (i in seq_len(nrow(odd_moves))) {
  r <- odd_moves[i, ]
  warn("tour_step ", r$prev_index, " -> ", r$step_index, " (", r$direction, ", not Weiter/Zurück/Von vorn): ",
       r$participant_id, " session ", r$session_id, " visit ", r$scene_visit, " line ", r$line)
}
back_without_button <- tour_shows %>% filter(direction == "back", !tour_has_zurueck(layout_version)) %>%
  distinct(participant_id, layout_version)
for (i in seq_len(nrow(back_without_button))) {
  warn("tour step back by 1 in a log of layout ", layout_label(back_without_button$layout_version[i]),
       " (no Zurück button there): ", back_without_button$participant_id[i], " (not counted as Back)")
}

# One row per Concept visit with a tour and per step 1..n_steps, so a step
# never reached is a row with shows 0 and active_s empty (not reached, not 0 s).
visit_order_concept <- visits %>% filter(scene == "ConceptScene") %>%
  select(session_id, scene_visit, visit_no, repeat_visit)
tour_visits <- tour_shows %>% distinct(participant_id, session_id, scene_visit, layout_version) %>%
  left_join(visit_order_concept, by = c("session_id", "scene_visit"))
tour_steps <- tibble(participant_id = character(), layout_version = character(), session_id = character(),
                     scene_visit = integer(), visit_no = integer(), repeat_visit = logical(),
                     zurueck_available = logical(), step_index = integer(), step_id = character(),
                     shows = integer(), shows_after_quiz = integer(), active_s = double(),
                     time_censored = logical(), back_presses = integer(), quick_next_back = integer(),
                     restarts = integer())
if (nrow(tour_visits) > 0) {
  per_step_shown <- tour_shows %>%
    group_by(session_id, scene_visit, step_index) %>%
    summarise(shows = n(),
              shows_after_quiz = sum(after_quiz),
              active_s = if (any(!after_quiz)) round(sum(show_s, na.rm = TRUE), 3) else NA_real_,
              time_censored = any(time_censored), .groups = "drop")
  # Presses are booked on the step that was left (prev_index).
  per_step_left <- tour_shows %>% filter(!is.na(prev_index)) %>%
    group_by(session_id, scene_visit, step_index = prev_index) %>%
    summarise(ev_back = sum(direction == "back"), ev_quick = sum(quick_back),
              ev_restart = sum(direction == "restart"), .groups = "drop")
  tour_steps <- tour_visits %>%
    crossing(step_index = seq_len(n_steps)) %>%
    left_join(step_names, by = "step_index") %>%
    left_join(per_step_shown, by = c("session_id", "scene_visit", "step_index")) %>%
    left_join(per_step_left, by = c("session_id", "scene_visit", "step_index")) %>%
    mutate(zurueck_available = tour_has_zurueck(layout_version),
           shows = coalesce(shows, 0L),
           shows_after_quiz = coalesce(shows_after_quiz, 0L),
           time_censored = coalesce(time_censored, FALSE),
           back_presses = if_else(zurueck_available, coalesce(ev_back, 0L), NA_integer_),
           quick_next_back = if_else(zurueck_available, coalesce(ev_quick, 0L), NA_integer_),
           # "Von vorn" exists in every layout.
           restarts = coalesce(ev_restart, 0L)) %>%
    select(participant_id, layout_version, session_id, scene_visit, visit_no, repeat_visit,
           zurueck_available, step_index, step_id, shows, shows_after_quiz, active_s, time_censored,
           back_presses, quick_next_back, restarts) %>%
    arrange(participant_id, visit_no, step_index)
}
write_csv(tour_steps, file.path(output_dir, "tour_steps.csv"), na = "")

# Step label on the x axis, e.g. "5 cartons" (step_index and step_id), in step order.
step_label_of <- function(i) paste(i, coalesce(step_names$step_id[match(i, step_names$step_index)], ""))

# Charts from the first Concept visit with a tour per participant (repeat
# visits after a crash are second exposures). One panel per layout_version,
# so "019" and "025" are never pooled (decision 024).
tour_first <- tour_steps %>%
  group_by(participant_id) %>% filter(visit_no == min(visit_no)) %>% ungroup() %>%
  mutate(step_label = factor(step_label_of(step_index), levels = step_label_of(seq_len(max(0, n_steps, na.rm = TRUE)))),
         layout_panel = paste0("layout ", layout_label(layout_version)))

title_tour <- "Which ConceptScene tour step holds the trainee longest?"
if (nrow(tour_first) == 0 || all(is.na(tour_first$active_s))) {
  p_tour <- empty_chart(title_tour)
  warn("no tour_step events: tour_time_by_step.png and tour_back_by_step.png are empty")
} else {
  tour_n <- tour_first %>% filter(!is.na(active_s)) %>%
    group_by(layout_panel, step_label) %>%
    summarise(n = n(), median_s = median(active_s), y = max(active_s), .groups = "drop")
  p_tour <- ggplot(tour_first %>% filter(!is.na(active_s)), aes(step_label, active_s)) +
    geom_jitter(width = 0.12, height = 0, size = 2, alpha = 0.6, colour = "#0072B2") +
    geom_point(data = tour_n, aes(y = median_s), shape = 18, size = 4, colour = "#D55E00") +
    geom_text(data = tour_n, aes(y = y, label = paste0("n=", n)), vjust = -0.8, size = 3, colour = "grey30") +
    facet_wrap(~ layout_panel, ncol = 1) +
    scale_y_continuous(expand = expansion(mult = c(0.05, 0.2))) +
    labs(title = title_tour,
         subtitle = paste0("Dot = one participant's active seconds on the step (all showings, pauses removed; ",
                           "the last step ends at the first quiz answer); diamond = median.\n",
                           "First Concept visit per participant. One panel per layout_version (never pooled, decision 024)."),
         x = "Tour step (step_index and step_id)", y = "Active time on the step (s)") +
    theme_minimal(base_size = 11) +
    theme(axis.text.x = element_text(angle = 35, hjust = 1))
}
ggsave(file.path(output_dir, "tour_time_by_step.png"), p_tour, width = 11,
       height = 3 + 3 * max(1, n_distinct(tour_first$layout_panel)), dpi = 150, bg = "white")

title_back <- "On which tour step do trainees press \"Zurück\"?"
tour_back <- tour_first %>% filter(zurueck_available)
if (nrow(tour_back) == 0) {
  p_back <- empty_chart(title_back)
} else {
  back_long <- tour_back %>%
    group_by(layout_panel, step_label) %>%
    summarise(quick = sum(quick_next_back), other = sum(back_presses) - sum(quick_next_back),
              people = sum(back_presses > 0), n = n(), .groups = "drop") %>%
    pivot_longer(c(quick, other), names_to = "kind", values_to = "presses") %>%
    mutate(kind = factor(kind, levels = c("other", "quick"),
                         labels = c("Zurück later", paste0("Zurück < ", tour_quick_back_s, " s after Weiter"))))
  back_n <- back_long %>% group_by(layout_panel, step_label) %>%
    summarise(total = sum(presses), people = first(people), n = first(n), .groups = "drop")
  p_back <- ggplot(back_long, aes(step_label, presses, fill = kind)) +
    geom_col(colour = "white", linewidth = 0.2) +
    geom_text(data = back_n, aes(y = total, label = paste0(people, "/", n), fill = NULL),
              vjust = -0.4, size = 3, colour = "grey30") +
    facet_wrap(~ layout_panel, ncol = 1) +
    scale_fill_manual(values = c("#56B4E9", "#D55E00"), name = NULL) +
    scale_y_continuous(expand = expansion(mult = c(0, 0.25))) +
    labs(title = title_back,
         subtitle = paste0("Bar = Zurück presses on the step (the step left), first Concept visit; label = participants ",
                           "with >= 1 press / participants.\nOnly layouts with Zurück (\"025\" on); ",
                           "\"019\" logs have no Back count (NA, not 0)."),
         x = "Tour step (step_index and step_id)", y = "Zurück presses (count)") +
    theme_minimal(base_size = 11) +
    theme(axis.text.x = element_text(angle = 35, hjust = 1), legend.position = "bottom")
}
ggsave(file.path(output_dir, "tour_back_by_step.png"), p_back, width = 11,
       height = 3.5 + 3 * max(1, n_distinct(tour_back$layout_panel)), dpi = 150, bg = "white")

# Per participant totals of the first Concept visit with a tour, for wide.csv.
tour_wide <- tour_first %>%
  group_by(participant_id) %>%
  summarise(tour_active_s_ConceptScene = if (all(is.na(active_s))) NA_real_ else round(sum(active_s, na.rm = TRUE), 3),
            tour_back_presses_ConceptScene = if (any(zurueck_available)) sum(back_presses) else NA_integer_,
            tour_quick_next_back_ConceptScene = if (any(zurueck_available)) sum(quick_next_back) else NA_integer_,
            tour_restarts_ConceptScene = sum(restarts), .groups = "drop")

# ---- 13d. ConceptScene hint tags (#100, decision 030) -------------------------

# From layout "029" on, a yellow tag appears beside a package the trainee laid
# into the Concept order container (another material, or a STECKER C carton
# that brings Ist above Soll) or storage container (another material):
# concept_hint_shown when it appears, concept_hint_cleared when it goes. One
# tag at a time, so in file line order shown / cleared alternate per visit;
# when the tag moves to another package, the old cleared and the new shown
# share a frame. A shown without cleared = the scene was left (or the app
# crashed) while the tag was shown. Hints are NOT attempts: nothing here
# feeds confirm_count, items.csv or an error rate. Putting packages into the
# Concept containers is voluntary and Concept writes no container_*, so
# there is no denominator: hint counts are counts, never a rate.
# Before layout "029" the tags did not exist: hint columns are NA (not
# measured), never 0.
# material_flow wait: at tour step material_flow the lane demo starts
# lane_demo_delay_s after the step is shown, but only once no tag is shown
# (LaneFlowDemo waits for ConceptBoxHints.IsClear). Wait per showing =
# active seconds from that start until the chain of tags (a tag moving on in
# the same frame continues the chain) ends, cut at the next tour_step or the
# visit end.

if (!exists("lane_demo_delay_s")) lane_demo_delay_s <- 4
hint_chain_gap_s <- 0.05
hints_logged <- function(layout_version) {
  coalesce(suppressWarnings(as.integer(layout_version)) >= 29L, FALSE)
}
hint_event_names <- c("concept_hint_shown", "concept_hint_cleared")
hint_reasons <- c("WRONG_MATERIAL", "TOO_MANY")
hint_containers <- c("order", "storage")

hint_lines <- ix_events %>% filter(event %in% hint_event_names) %>%
  transmute(participant_id, session_id, scene = as.character(scene), scene_visit, t, line, event,
            container = map_chr(data, ~ as_chr(.x$container)),
            reason = map_chr(data, ~ as_chr(.x$reason)),
            object_id = map_chr(data, ~ as_chr(.x$object_id)),
            material_key = map_chr(data, ~ as_chr(.x$material_key)),
            tour_step_id = map_chr(data, ~ as_chr(.x$tour_step_id))) %>%
  left_join(sessions %>% select(session_id, layout_version), by = "session_id")

# Contract checks: hints only in ConceptScene, only from layout "029" on,
# only the fixed codes (storage has no TOO_MANY). Such lines are not read.
bad_hint_scene <- hint_lines %>% filter(scene != "ConceptScene")
if (nrow(bad_hint_scene) > 0) {
  warn(nrow(bad_hint_scene), " concept_hint_* line(s) outside ConceptScene (not read): ",
       paste(unique(bad_hint_scene$participant_id), collapse = ", "))
}
bad_hint_layout <- hint_lines %>% filter(scene == "ConceptScene", !hints_logged(layout_version)) %>%
  distinct(participant_id, layout_version)
for (i in seq_len(nrow(bad_hint_layout))) {
  warn("concept_hint_* in a log of layout ", layout_label(bad_hint_layout$layout_version[i]),
       " (hints exist from \"029\" on): ", bad_hint_layout$participant_id[i], " (not read)")
}
bad_hint_code <- hint_lines %>%
  filter(!container %in% hint_containers | !reason %in% hint_reasons |
           (container %in% "storage" & reason %in% "TOO_MANY") | is.na(object_id))
if (nrow(bad_hint_code) > 0) {
  warn(nrow(bad_hint_code), " concept_hint_* line(s) with an unknown container / reason, storage TOO_MANY ",
       "or no object_id (not read): ", paste(unique(bad_hint_code$participant_id), collapse = ", "))
}
hint_lines <- hint_lines %>%
  filter(scene == "ConceptScene", hints_logged(layout_version)) %>%
  anti_join(bad_hint_code, by = c("session_id", "line")) %>%
  arrange(session_id, scene_visit, line)

# Visit ends: scene_exit, or the last event of a crashed visit.
concept_visit_ends <- visits %>% filter(scene == "ConceptScene") %>%
  select(session_id, scene_visit, visit_end_t = end_t, closed)

# Pair the lines of one visit in file line order. A cleared with no shown
# before it, or a second shown before the cleared, breaks the contract: warned
# below; an orphan cleared is ignored, a double shown ends the earlier tag.
pair_hint_lines <- function(d) {
  out <- list()
  open <- NULL
  for (i in seq_len(nrow(d))) {
    r <- d[i, ]
    if (r$event == "concept_hint_shown") {
      if (!is.null(open)) {
        out[[length(out) + 1]] <- tibble(open, end_t = r$t, ended_by = "next_shown_without_cleared",
                                         cleared_step_id = NA_character_, problem = "shown_twice")
      }
      open <- r
    } else if (is.null(open)) {
      out[[length(out) + 1]] <- tibble(r, end_t = r$t, ended_by = NA_character_,
                                       cleared_step_id = r$tour_step_id, problem = "cleared_without_shown")
    } else {
      same <- identical(r$object_id, open$object_id) && identical(r$container, open$container) &&
        identical(r$reason, open$reason)
      out[[length(out) + 1]] <- tibble(open, end_t = r$t, ended_by = "cleared",
                                       cleared_step_id = r$tour_step_id,
                                       problem = if (same) NA_character_ else "cleared_other_tag")
      open <- NULL
    }
  }
  if (!is.null(open)) {
    out[[length(out) + 1]] <- tibble(open, end_t = NA_real_, ended_by = "visit_end",
                                     cleared_step_id = NA_character_, problem = NA_character_)
  }
  bind_rows(out)
}

hint_pairs <- hint_lines %>%
  group_by(session_id, scene_visit) %>%
  group_modify(~ pair_hint_lines(.x)) %>%
  ungroup()
if (nrow(hint_pairs) == 0) {
  hint_pairs <- tibble(session_id = character(), scene_visit = integer(), participant_id = character(),
                       scene = character(), t = double(), line = integer(), event = character(),
                       container = character(), reason = character(), object_id = character(),
                       material_key = character(), tour_step_id = character(), layout_version = character(),
                       end_t = double(), ended_by = character(), cleared_step_id = character(),
                       problem = character())
}
hint_problems <- hint_pairs %>% filter(!is.na(problem))
for (i in seq_len(nrow(hint_problems))) {
  r <- hint_problems[i, ]
  warn(switch(r$problem,
              cleared_without_shown = "concept_hint_cleared without a preceding concept_hint_shown (ignored): ",
              shown_twice = "concept_hint_shown while another tag was still shown (the first ends there): ",
              cleared_other_tag = "concept_hint_cleared names another tag than the shown one (paired anyway): "),
       r$participant_id, " ConceptScene visit ", r$scene_visit, " line ", r$line)
}

# Grab lines of the Concept visits, to tell a tag the trainee removed (a
# grab_start of the same package while the tag was shown) from one the
# station cleared (e.g. TOO_MANY when the demo cartons leave with the step).
concept_grab_starts <- grab_rows %>% filter(scene == "ConceptScene", event == "grab_start") %>%
  select(session_id, scene_visit, object_id, grab_t = t)

concept_hints <- hint_pairs %>%
  filter(!problem %in% "cleared_without_shown") %>%
  rename(shown_t = t, shown_line = line) %>%
  left_join(concept_visit_ends, by = c("session_id", "scene_visit")) %>%
  mutate(ended_by = case_when(ended_by != "visit_end" ~ ended_by,
                              closed %in% TRUE ~ "scene_exit",
                              TRUE ~ "crash"),
         # A tag still shown at the visit end lasts until the scene exit
         # (or, after a crash, the last event: a lower bound).
         hint_end_t = coalesce(end_t, visit_end_t)) %>%
  select(-end_t, -visit_end_t, -closed)
concept_hints <- concept_hints %>%
  mutate(row_id = row_number()) %>%
  left_join(concept_grab_starts, by = c("session_id", "scene_visit", "object_id"),
            relationship = "many-to-many") %>%
  group_by(row_id) %>%
  summarise(across(-grab_t, first),
            removed_by_grab = any(!is.na(grab_t) & grab_t > shown_t & grab_t <= hint_end_t),
            .groups = "drop") %>%
  mutate(cleared = ended_by == "cleared",
         # Only a cleared tag can say who removed it.
         removed_by_grab = if_else(cleared, removed_by_grab, NA),
         shown_s = round(hint_end_t - shown_t - paused_between(session_id, shown_t, hint_end_t), 3)) %>%
  group_by(session_id, scene_visit) %>%
  mutate(hint_no = row_number()) %>%
  ungroup() %>%
  left_join(visits %>% filter(scene == "ConceptScene") %>% select(session_id, scene_visit, visit_no, repeat_visit),
            by = c("session_id", "scene_visit")) %>%
  select(participant_id, layout_version, session_id, scene_visit, visit_no, repeat_visit, hint_no,
         container, reason, object_id, material_key, tour_step_id, cleared_step_id,
         shown_t, end_t = hint_end_t, shown_s, ended_by, cleared, removed_by_grab) %>%
  arrange(participant_id, visit_no, hint_no)
write_csv(concept_hints, file.path(output_dir, "concept_hints.csv"), na = "")

# material_flow wait per showing of that step: does a tag chain cover the
# moment the demo would start? Chains = tags that follow each other within
# hint_chain_gap_s (the tag moved on in the same frame).
hint_chains <- concept_hints %>%
  arrange(session_id, scene_visit, shown_t) %>%
  group_by(session_id, scene_visit) %>%
  mutate(new_chain = is.na(lag(end_t)) | shown_t > lag(end_t) + hint_chain_gap_s,
         chain = cumsum(new_chain)) %>%
  group_by(session_id, scene_visit, chain) %>%
  summarise(chain_start = first_or_na(shown_t), chain_end = last_or_na(end_t),
            chain_open = any(ended_by == "crash"), .groups = "drop")
flow_shows <- tour_shows %>%
  filter(step_id %in% "material_flow", hints_logged(layout_version)) %>%
  transmute(session_id, scene_visit, demo_t = t + lane_demo_delay_s,
            step_end_t = coalesce(next_t, end_t), step_crashed = is.na(next_t) & !closed) %>%
  filter(step_end_t > demo_t) %>%
  mutate(show_id = row_number())
flow_waits <- flow_shows %>%
  left_join(hint_chains, by = c("session_id", "scene_visit"), relationship = "many-to-many") %>%
  mutate(covers = !is.na(chain_start) & chain_start <= demo_t & chain_end > demo_t) %>%
  group_by(show_id, session_id, scene_visit, demo_t, step_end_t, step_crashed) %>%
  summarise(wait_end_t = if (any(covers)) min(max(chain_end[covers]), first(step_end_t)) else NA_real_,
            wait_open = any(covers & chain_open), .groups = "drop") %>%
  mutate(held = !is.na(wait_end_t),
         wait_s = if_else(held, round(wait_end_t - demo_t - paused_between(session_id, demo_t, wait_end_t), 3), 0),
         # Still waiting when the app died, or when the step was left: a lower bound.
         wait_censored = held & (wait_open | (wait_end_t >= step_end_t & step_crashed)))

# One row per Concept visit of a session that logged the interaction group:
# counts per container x reason, seconds shown, the material_flow wait.
hint_visit_counts <- concept_hints %>%
  group_by(session_id, scene_visit) %>%
  summarise(hints_shown = n(),
            hints_order_wrong_material = sum(container == "order" & reason == "WRONG_MATERIAL"),
            hints_order_too_many = sum(container == "order" & reason == "TOO_MANY"),
            hints_storage_wrong_material = sum(container == "storage" & reason == "WRONG_MATERIAL"),
            hints_removed_by_grab = sum(removed_by_grab %in% TRUE),
            hints_open_at_exit = sum(!cleared),
            hint_shown_s = round(sum(shown_s), 3),
            hint_max_shown_s = last_or_na(shown_s), .groups = "drop")
hint_visit_waits <- flow_waits %>%
  group_by(session_id, scene_visit) %>%
  summarise(material_flow_demo_due = n(),
            material_flow_demo_held = sum(held),
            material_flow_wait_max_s = last_or_na(wait_s),
            material_flow_wait_s = round(sum(wait_s), 3),
            material_flow_wait_censored = any(wait_censored), .groups = "drop")
hint_visit_breaks <- hint_problems %>% count(session_id, scene_visit, name = "hint_contract_breaks")
hint_count_cols <- c("hints_shown", "hints_order_wrong_material", "hints_order_too_many",
                     "hints_storage_wrong_material", "hints_removed_by_grab", "hints_open_at_exit",
                     "hint_contract_breaks")
concept_hints_by_visit <- visits %>%
  filter(scene == "ConceptScene", interaction_logged %in% TRUE) %>%
  select(participant_id, layout_version, session_id, scene_visit, visit_no, repeat_visit) %>%
  mutate(hints_logged = hints_logged(layout_version)) %>%
  left_join(hint_visit_counts, by = c("session_id", "scene_visit")) %>%
  left_join(hint_visit_waits, by = c("session_id", "scene_visit")) %>%
  left_join(hint_visit_breaks, by = c("session_id", "scene_visit")) %>%
  mutate(across(all_of(hint_count_cols), ~ if_else(hints_logged, coalesce(as.integer(.x), 0L), NA_integer_)),
         hint_shown_s = if_else(hints_logged, coalesce(hint_shown_s, 0), NA_real_),
         # No tag: no longest tag (NA, not 0).
         hint_max_shown_s = if_else(hints_logged, hint_max_shown_s, NA_real_),
         # Demo columns: NA when not measured or the demo was never due in
         # the visit (material_flow not reached or left within the delay).
         material_flow_demo_due = if_else(hints_logged, coalesce(material_flow_demo_due, 0L), NA_integer_),
         material_flow_demo_held = if_else(hints_logged, coalesce(material_flow_demo_held, 0L), NA_integer_),
         across(c(material_flow_wait_max_s, material_flow_wait_s, material_flow_wait_censored),
                ~ if_else(hints_logged & material_flow_demo_due > 0, .x, NA))) %>%
  select(participant_id, layout_version, session_id, scene_visit, visit_no, repeat_visit, hints_logged,
         hints_shown, hints_order_wrong_material, hints_order_too_many, hints_storage_wrong_material,
         hints_removed_by_grab, hints_open_at_exit, hint_shown_s, hint_max_shown_s,
         material_flow_demo_due, material_flow_demo_held, material_flow_wait_max_s, material_flow_wait_s,
         material_flow_wait_censored, hint_contract_breaks) %>%
  arrange(participant_id, visit_no)
write_csv(concept_hints_by_visit, file.path(output_dir, "concept_hints_by_visit.csv"), na = "")
if (any(concept_hints_by_visit$hints_logged)) {
  message("Concept hint tags (layout 029 on): ", sum(concept_hints_by_visit$hints_shown, na.rm = TRUE),
          " shown in ", sum(concept_hints_by_visit$hints_logged), " visit(s); longest material_flow wait ",
          suppressWarnings(max(concept_hints_by_visit$material_flow_wait_max_s, na.rm = TRUE)), " s")
}

# ---- 14. Wide file: one row per participant (for SPSS and paper forms) ---------

# Per-scene columns come from the counting visit only (repeat visits stay in
# the long `visits` table). Paper questionnaires (#31) join on participant_id.
per_scene <- visits %>% filter(counting) %>%
  mutate(scene = as.character(scene)) %>%
  transmute(participant_id, scene,
            active_s = duration_s, paused_s = paused_s, confirms = confirm_count,
            confirm_ignored = confirm_ignored, wait_s = wait_s, wait_after_done_s = wait_after_done_s,
            errors = presses_with_error, error_rate = error_rate,
            first_correct = first_attempt_correct, attempts_to_correct = attempts_to_correct,
            time_to_correct_s = time_to_correct_s, completed = completed,
            visit_censored = duration_censored,
            grabs = grabs, regrabs = regrabs, container_enters = container_enters,
            container_exits = container_exits, time_to_first_grab_s = time_to_first_grab_s)
visit_counts <- visits %>% mutate(scene = as.character(scene)) %>% count(participant_id, scene, name = "visits")
per_scene <- full_join(visit_counts, per_scene, by = c("participant_id", "scene"))

outcome_measures <- c("confirms", "errors", "error_rate", "first_correct", "attempts_to_correct",
                      "time_to_correct_s", "completed")
wide_scene <- per_scene %>%
  pivot_wider(id_cols = participant_id, names_from = scene, names_glue = "{.value}_{scene}",
              values_from = c(visits, active_s, paused_s, confirms, confirm_ignored, wait_s, wait_after_done_s, errors,
                              error_rate, first_correct, attempts_to_correct, time_to_correct_s,
                              completed, visit_censored, grabs, regrabs, container_enters,
                              container_exits, time_to_first_grab_s))
# Scenes without a confirm or quiz (WelcomeScene) never have outcome values:
# drop those always-empty columns.
no_outcome_cols <- as.vector(outer(outcome_measures, setdiff(unique(per_scene$scene), outcome_scenes),
                                   paste, sep = "_"))
# Ignored presses and station waits exist only in scenes with a red button.
no_button_cols <- as.vector(outer(c("confirm_ignored", "wait_s", "wait_after_done_s"),
                                  setdiff(unique(per_scene$scene), red_button_scenes), paste, sep = "_"))
# Grabs exist only where packages have PackageData, container counts only
# where there is an order container (both NA elsewhere, always).
no_grab_cols <- as.vector(outer(c("grabs", "regrabs", "time_to_first_grab_s"),
                                setdiff(unique(per_scene$scene), grab_scenes), paste, sep = "_"))
no_container_cols <- as.vector(outer(c("container_enters", "container_exits"),
                                     setdiff(unique(per_scene$scene), container_scenes), paste, sep = "_"))
wide_scene <- wide_scene %>% select(-any_of(c(no_outcome_cols, no_button_cols, no_grab_cols, no_container_cols)))

# Q3 phases of the counting visit, one column per scene and position,
# e.g. read_s_MixedPickingScene_p2.
wide_process <- process %>% filter(counting) %>%
  transmute(participant_id, key = paste0(scene, "_p", position), read_s, pick_s, check_s) %>%
  pivot_wider(id_cols = participant_id, names_from = key, names_glue = "{.value}_{key}",
              values_from = c(read_s, pick_s, check_s))

wide_skill <- skill_rates %>%
  transmute(participant_id, name = paste0("error_rate_", scene, "_", skill), error_rate) %>%
  pivot_wider(names_from = name, values_from = error_rate)

# Keep scene columns in build order for every measure.
scene_col_order <- function(cols) {
  scene_of <- map_chr(cols, function(cl) {
    hit <- levels(visits$scene)[map_lgl(levels(visits$scene), ~ str_detect(cl, fixed(paste0("_", .x))))]
    if (length(hit) == 0) NA_character_ else hit[1]
  })
  measure <- str_remove(cols, paste0("_(", paste(levels(visits$scene), collapse = "|"), ").*$"))
  cols[order(factor(measure, levels = unique(measure)), match(scene_of, levels(visits$scene)))]
}

participant_cols <- c("participant_id", "end_status", "course_finished", "multi_session",
                      "mixed_build", "builds", "schema_versions", "layout_versions", "gap_h", "n_sessions",
                      "resume_offered", "resume_declined", "ended_session_override",
                      "duration_censored", "active_s", "paused_s", "pauses",
                      "hmd_removed_count", "unity_errors", "total_confirms", "interaction_logged",
                      "end_control_opens", "menu_blocked", "open_grabs", "facilitator_tail_s")
wide <- data_quality %>%
  select(all_of(participant_cols)) %>%
  left_join(wide_scene, by = "participant_id") %>%
  left_join(wide_process, by = "participant_id") %>%
  left_join(tour_wide, by = "participant_id") %>%
  left_join(wide_skill, by = "participant_id")
# A scene nobody had a counting visit for still gets its error-rate column
# (added before sorting, so it lands in build order).
for (s in intersect(levels(visits$scene), outcome_scenes)) {
  if (!paste0("error_rate_", s) %in% names(wide)) wide[[paste0("error_rate_", s)]] <- NA_real_
}
scene_cols <- setdiff(names(wide), participant_cols)
wide <- wide %>% select(all_of(participant_cols), all_of(scene_col_order(scene_cols)))
write_csv(wide, file.path(output_dir, "wide.csv"), na = "")

# ---- 14b. Paper forms (#31): SSQ before/after, NASA-TLX, Abbruchgrund ------------

# The facilitator types the paper forms into a CSV built from
# analysis/paper_forms_template.csv and saves it as
# analysis/data/<wave>/paper_forms.csv (columns: README, "Paper forms").
# The file is optional: without it only wide.csv is written and nothing fails.
# The joined table goes to its own file, wide_with_paper.csv, on purpose:
# wide.csv keeps the same log-only columns with or without paper data, and the
# SSQ answers are health data, so the one file holding them is easy to name,
# guard and delete.

# The paper end time is local wall-clock time; the log is UTC.
if (!exists("paper_time_zone")) paper_time_zone <- "Europe/Berlin"
# End time on paper vs the participant's last logged event: a bigger gap
# usually means a swapped form or a typo in the ID.
end_time_warn_min <- 15

# The 16 SSQ items in the standard order (Kennedy et al. 1993). The German
# paper form must use the same order: column 01 = item 1, and so on.
ssq_items <- c("01_general_discomfort", "02_fatigue", "03_headache", "04_eye_strain",
               "05_difficulty_focusing", "06_salivation_increased", "07_sweating", "08_nausea",
               "09_difficulty_concentrating", "10_fullness_of_head", "11_blurred_vision",
               "12_dizzy_eyes_open", "13_dizzy_eyes_closed", "14_vertigo",
               "15_stomach_awareness", "16_burping")
# Which items load on which subscale (Kennedy et al. 1993; some items count twice).
ssq_subscales <- list(nausea = c(1, 6, 7, 8, 9, 15, 16),
                      oculomotor = c(1, 2, 3, 4, 5, 9, 11),
                      disorientation = c(5, 8, 10, 11, 12, 13, 14))
ssq_weights <- c(nausea = 9.54, oculomotor = 7.58, disorientation = 13.92)
ssq_total_weight <- 3.74
# NASA-TLX raw (unweighted): 6 scales 0-100. Performance: 0 = good, 100 = poor
# (as on the original form), so for every scale higher = more load.
tlx_scales <- c("mental", "physical", "temporal", "performance", "effort", "frustration")
abbruch_levels <- c("mued_schwindlig", "zeit", "fehlbedienung", "andere")

ssq_cols <- function(form) paste0("ssq_", form, "_", ssq_items)
paper_cols <- c("participant_id", "session_date", "facilitator", "end_time_local",
                ssq_cols("pre"), ssq_cols("post"), paste0("tlx_", tlx_scales),
                "abbruchgrund", "note")

# The template and this script must name the same columns (a contract, like
# the log format): stop early if someone edited one of them alone.
template_path <- file.path(analysis_folder, "paper_forms_template.csv")
if (file.exists(template_path)) {
  template_cols <- strsplit(sub("﻿", "", readLines(template_path, n = 1, warn = FALSE)), ",")[[1]]
  if (!identical(template_cols, paper_cols)) {
    stop("paper_forms_template.csv and analyse.R list different paper columns; change both together.")
  }
}

# Reads one paper file as text cells. Excel on a German PC saves "CSV" with
# semicolons and often in Windows-1252, the template has commas and UTF-8:
# both are accepted, so whoever types the forms never has to fix the format.
read_paper_file <- function(path) {
  raw <- readLines(path, warn = FALSE)
  if (length(raw) == 0) return(NULL)
  if (!all(validUTF8(raw))) raw <- iconv(raw, from = "windows-1252", to = "UTF-8")
  raw[1] <- sub("^﻿", "", raw[1])
  delim <- if (str_count(raw[1], fixed(";")) > str_count(raw[1], fixed(","))) ";" else ","
  tab <- read_delim(I(paste(c(raw, ""), collapse = "\n")), delim = delim, na = character(),
                    col_types = cols(.default = col_character()), trim_ws = TRUE,
                    skip_empty_rows = FALSE, show_col_types = FALSE, progress = FALSE)
  names(tab) <- tolower(trimws(names(tab)))
  # Row number as Excel shows it (header = row 1), for the warnings.
  tab <- tab %>% mutate(paper_file = path, paper_row = row_number() + 1L, .before = 1)
  for (cl in setdiff(paper_cols, names(tab))) {
    warn("paper file ", path, " has no column ", cl, " (read as empty)")
    tab[[cl]] <- ""
  }
  extra <- setdiff(names(tab), c(paper_cols, "paper_file", "paper_row"))
  if (length(extra) > 0) warn("paper file ", path, ": unknown column(s) ignored: ", paste(extra, collapse = ", "))
  tab <- tab %>% select(paper_file, paper_row, all_of(paper_cols)) %>%
    mutate(across(all_of(paper_cols), ~ coalesce(trimws(.x), "")))
  # Rows Excel leaves behind with nothing in them are not data.
  tab %>% filter(if_any(all_of(paper_cols), ~ .x != ""))
}

# Paper files are named paper_forms*.csv; fake/ is skipped for real data_dirs
# (same rule as for the logs).
paper_files <- list.files(data_dir, pattern = "^paper_forms.*\\.csv$", recursive = TRUE, full.names = TRUE)
if (!grepl("fake", data_dir)) paper_files <- paper_files[!grepl("/fake/", paper_files)]
xlsx_only <- list.files(data_dir, pattern = "^paper_forms.*\\.xlsx$", recursive = TRUE, full.names = TRUE)
if (length(paper_files) == 0 && length(xlsx_only) > 0) {
  warn("paper forms found only as Excel file(s) (", paste(xlsx_only, collapse = ", "),
       "): save them as CSV (paper_forms.csv) to join them")
}
# An old joined file from an earlier run must not survive a run without paper.
unlink(file.path(output_dir, c("wide_with_paper.csv", "ssq_pre_post.png", "tlx_vs_attempts.png")))
paper_raw <- map(paper_files, read_paper_file) %>% compact() %>% list_rbind()
# Paper rows join the set of their participant_id (decision 023): the pilot
# run keeps only pilot IDs; the study run keeps the rest (and warns about rows
# without a valid ID, once).
if (!is.null(paper_raw) && nrow(paper_raw) > 0) paper_raw <- paper_raw %>% filter(in_set(participant_id))
paper_check_ids <- character(0)

if (length(paper_files) == 0 || nrow(paper_raw) == 0) {
  message("No paper forms (paper_forms.csv) under ", data_dir, ": wide_with_paper.csv not written.")
} else {
  message("Reading ", nrow(paper_raw), " paper form row(s) from ", paste(paper_files, collapse = ", "))
  where <- function(r) paste0(basename(r$paper_file), " row ", r$paper_row)

  # 1. IDs: a row without a valid P + 3 digits ID can't be joined; it is
  #    named in the warnings (never silently dropped). Test IDs are dropped
  #    like their logs.
  no_id <- paper_raw %>% filter(participant_id == "")
  for (i in seq_len(nrow(no_id))) warn("paper form ", where(no_id[i, ]), " has data but no participant_id (not joined)")
  bad_id <- paper_raw %>% filter(participant_id != "", !is_valid_id(participant_id))
  for (i in seq_len(nrow(bad_id))) {
    warn("paper form ", where(bad_id[i, ]), ": participant_id '", bad_id$participant_id[i],
         "' is not P + 3 digits (typo?), not joined")
  }
  paper_test <- paper_raw %>% filter(is_valid_id(participant_id), is_test_id(participant_id))
  if (nrow(paper_test) > 0) message("Dropped paper form(s) of test IDs: ", paste(paper_test$participant_id, collapse = ", "))
  paper <- paper_raw %>% filter(is_valid_id(participant_id), !is_test_id(participant_id))

  # 2. Duplicates: two rows for one ID can't be told apart (which is right?).
  #    Neither is joined; the ID gets paper_status "duplicate".
  dup_ids <- paper %>% count(participant_id) %>% filter(n > 1) %>% pull(participant_id)
  for (pid in dup_ids) {
    d <- paper %>% filter(participant_id == pid)
    warn("paper forms: ", pid, " appears ", nrow(d), " times (", paste(where(d), collapse = ", "),
         "): none joined, keep one row")
  }

  # 3. Numbers: SSQ items are whole numbers 0-3, TLX 0-100. An empty cell is
  #    "not filled" (NA, never 0); a value out of range is a typing error: the
  #    cell becomes NA and is named in the warnings.
  problems <- tibble(participant_id = character(), column = character())
  check_num <- function(df, cols, lo, hi, whole) {
    for (cl in cols) {
      txt <- df[[cl]]
      val <- suppressWarnings(as.numeric(sub(",", ".", txt, fixed = TRUE)))
      bad <- txt != "" & (is.na(val) | val < lo | val > hi | (whole & val != round(val)))
      for (i in which(bad)) {
        warn("paper form ", where(df[i, ]), " (", df$participant_id[i], "): ", cl, " = '", txt[i],
             "' is not ", if (whole) "a whole number " else "a number ", lo, "-", hi, " (set to empty)")
        problems <<- bind_rows(problems, tibble(participant_id = df$participant_id[i], column = cl))
      }
      val[bad | txt == ""] <- NA_real_
      df[[cl]] <- val
    }
    df
  }
  paper <- check_num(paper, c(ssq_cols("pre"), ssq_cols("post")), 0, 3, TRUE)
  paper <- check_num(paper, paste0("tlx_", tlx_scales), 0, 100, FALSE)
  # The TLX scale has 21 marks (steps of 5): another value is a reading slip.
  for (cl in paste0("tlx_", tlx_scales)) {
    off <- which(!is.na(paper[[cl]]) & paper[[cl]] %% 5 != 0)
    for (i in off) warn("paper form ", where(paper[i, ]), " (", paper$participant_id[i], "): ", cl, " = ",
                        paper[[cl]][i], " is between two marks of the scale (check the paper)")
  }

  # 4. Not filled: a whole form missing is common after an early stop and
  #    matters for Q4/Q5; single empty items make their subscales NA.
  for (i in seq_len(nrow(paper))) {
    for (form in c("pre", "post")) {
      miss <- ssq_items[is.na(unlist(paper[i, ssq_cols(form)]))]
      if (length(miss) > 0) paper_check_ids <- c(paper_check_ids, paper$participant_id[i])
      if (length(miss) == 16) warn("paper form ", paper$participant_id[i], ": SSQ ", form, " not filled")
      else if (length(miss) > 0) warn("paper form ", paper$participant_id[i], ": SSQ ", form, " item(s) empty (",
                                      paste(miss, collapse = ", "), "): their subscales and the total are NA")
    }
    miss <- tlx_scales[is.na(unlist(paper[i, paste0("tlx_", tlx_scales)]))]
    if (length(miss) > 0) paper_check_ids <- c(paper_check_ids, paper$participant_id[i])
    if (length(miss) == 6) warn("paper form ", paper$participant_id[i], ": NASA-TLX not filled")
    else if (length(miss) > 0) warn("paper form ", paper$participant_id[i], ": NASA-TLX scale(s) empty (",
                                    paste(miss, collapse = ", "), "): tlx_raw is NA")
  }

  # 5. Abbruchgrund: one of the fixed values or empty (case and spaces are
  #    ignored). Anything else is named and left empty.
  paper <- paper %>% mutate(abbruch_txt = abbruchgrund, abbruchgrund = tolower(trimws(abbruchgrund)))
  bad_ab <- paper %>% filter(abbruchgrund != "", !abbruchgrund %in% abbruch_levels)
  for (i in seq_len(nrow(bad_ab))) {
    warn("paper form ", where(bad_ab[i, ]), " (", bad_ab$participant_id[i], "): abbruchgrund '",
         bad_ab$abbruch_txt[i], "' is not one of ", paste(abbruch_levels, collapse = " / "), " (set to empty)")
    problems <- bind_rows(problems, tibble(participant_id = bad_ab$participant_id[i], column = "abbruchgrund"))
  }
  paper <- paper %>% mutate(abbruchgrund = if_else(abbruchgrund %in% abbruch_levels, abbruchgrund, NA_character_))

  # 6. Date (YYYY-MM-DD as in the template, or DD.MM.YYYY as German Excel
  #    saves it) and end time (HH:MM, Excel may add :SS) -> one local time.
  parse_paper_date <- function(x) {
    iso <- as.Date(x, format = "%Y-%m-%d")
    de <- as.Date(x, format = "%d.%m.%Y")
    out <- if_else(grepl("^\\d{4}-\\d{1,2}-\\d{1,2}$", x), iso, de)
    out[!grepl("^(\\d{4}-\\d{1,2}-\\d{1,2}|\\d{1,2}\\.\\d{1,2}\\.\\d{4})$", x)] <- NA
    out
  }
  paper <- paper %>%
    mutate(date_parsed = parse_paper_date(session_date),
           time_ok = grepl("^\\d{1,2}:\\d{2}(:\\d{2})?$", end_time_local),
           # "9:05" and "09:05:00" both become "09:05:00" (one format for all rows).
           time_hms = if_else(grepl("^\\d{1,2}:\\d{2}$", end_time_local), paste0(end_time_local, ":00"), end_time_local),
           end_local = as.POSIXct(if_else(!is.na(date_parsed) & time_ok,
                                          paste(format(date_parsed), time_hms), NA_character_),
                                  format = "%Y-%m-%d %H:%M:%S", tz = paper_time_zone))
  for (i in which(paper$session_date != "" & is.na(paper$date_parsed))) {
    warn("paper form ", where(paper[i, ]), " (", paper$participant_id[i], "): session_date '",
         paper$session_date[i], "' is not YYYY-MM-DD or DD.MM.YYYY")
  }
  for (i in which(paper$end_time_local != "" & !paper$time_ok)) {
    warn("paper form ", where(paper[i, ]), " (", paper$participant_id[i], "): end_time_local '",
         paper$end_time_local[i], "' is not HH:MM")
  }

  # 7. Scores. Raw subscale = sum of its items (NA if any item is NA, no
  #    guessing); weighted as Kennedy et al. 1993. Change = post - pre, so a
  #    trainee who came in with symptoms is not counted as made sick by VR.
  ssq_scores <- function(df, form) {
    m <- as.matrix(df[ssq_cols(form)])
    raw <- map(ssq_subscales, ~ rowSums(m[, .x, drop = FALSE]))
    out <- tibble(!!paste0("ssq_", form, "_nausea") := round(raw$nausea * ssq_weights[["nausea"]], 2),
                  !!paste0("ssq_", form, "_oculomotor") := round(raw$oculomotor * ssq_weights[["oculomotor"]], 2),
                  !!paste0("ssq_", form, "_disorientation") := round(raw$disorientation * ssq_weights[["disorientation"]], 2),
                  !!paste0("ssq_", form, "_total") := round((raw$nausea + raw$oculomotor + raw$disorientation) *
                                                              ssq_total_weight, 2))
    out
  }
  paper <- bind_cols(paper, ssq_scores(paper, "pre"), ssq_scores(paper, "post")) %>%
    mutate(ssq_change_nausea = ssq_post_nausea - ssq_pre_nausea,
           ssq_change_oculomotor = ssq_post_oculomotor - ssq_pre_oculomotor,
           ssq_change_disorientation = ssq_post_disorientation - ssq_pre_disorientation,
           ssq_change_total = ssq_post_total - ssq_pre_total,
           # Raw (unweighted) TLX: the mean of the 6 scales, NA if one is missing.
           tlx_raw = round(rowMeans(across(all_of(paste0("tlx_", tlx_scales)))), 1))

  # 8. Cross-checks with the logs. Every ID on one side only is named: a
  #    typo on the form, logs not copied, or a form not typed in yet.
  log_ids <- data_quality$participant_id
  paper_ids <- unique(paper$participant_id)
  for (pid in setdiff(paper_ids, log_ids)) warn("paper form for ", pid, " but no log (logs not copied? typo in the ID?)")
  for (pid in setdiff(log_ids, paper_ids)) warn("log for ", pid, " but no paper form (not typed in? typo in the ID?)")

  last_event <- sessions %>% group_by(participant_id) %>% summarise(last_utc = max(last_utc), .groups = "drop")
  paper <- paper %>% left_join(last_event, by = "participant_id") %>%
    mutate(end_time_diff_min = round(as.numeric(difftime(end_local, last_utc, units = "mins")), 1))
  far <- paper %>% filter(!participant_id %in% dup_ids, abs(end_time_diff_min) > end_time_warn_min)
  for (i in seq_len(nrow(far))) {
    warn("paper form ", far$participant_id[i], ": end time ", far$session_date[i], " ", far$end_time_local[i],
         " (", paper_time_zone, ") is ", far$end_time_diff_min[i], " min from the last logged event ",
         format(far$last_utc[i], "%Y-%m-%d %H:%M", tz = paper_time_zone), " (swapped form or wrong ID?)")
  }

  # Abbruchgrund vs how the log ended: a facilitator stop needs a reason on
  # paper; a reason for someone who finished the course is contradictory.
  ab_check <- paper %>% filter(!participant_id %in% dup_ids) %>%
    inner_join(data_quality %>% select(participant_id, end_status, course_finished), by = "participant_id")
  for (pid in ab_check$participant_id[ab_check$end_status == "facilitator" & is.na(ab_check$abbruchgrund)]) {
    warn("paper form ", pid, ": session ended by the facilitator but no abbruchgrund")
    paper_check_ids <- c(paper_check_ids, pid)
  }
  for (pid in ab_check$participant_id[!is.na(ab_check$abbruchgrund) & ab_check$course_finished %in% TRUE]) {
    warn("paper form ", pid, ": abbruchgrund given although the log shows the course finished")
    paper_check_ids <- c(paper_check_ids, pid)
  }

  # 9. The joined file: all log participants plus paper-only IDs (so nothing
  #    disappears), one row each. The free-text note never leaves the paper
  #    file (privacy, and no free text as data): only whether there is one.
  problem_counts <- problems %>% count(participant_id, name = "paper_problems")
  paper_out <- paper %>%
    filter(!participant_id %in% dup_ids) %>%
    transmute(participant_id, paper_status = "joined",
              session_date = format(date_parsed), facilitator = na_if(facilitator, ""),
              end_time_local = na_if(end_time_local, ""), end_time_diff_min,
              abbruchgrund, paper_note_present = note != "",
              across(starts_with("ssq_pre_")), across(starts_with("ssq_post_")),
              across(starts_with("ssq_change_")), across(all_of(paste0("tlx_", tlx_scales))), tlx_raw) %>%
    bind_rows(tibble(participant_id = dup_ids, paper_status = rep("duplicate", length(dup_ids)))) %>%
    left_join(problem_counts, by = "participant_id") %>%
    mutate(paper_problems = coalesce(paper_problems, 0L))
  wide_with_paper <- wide %>% mutate(has_log = TRUE) %>%
    full_join(paper_out, by = "participant_id") %>%
    mutate(has_log = coalesce(has_log, FALSE), paper_status = coalesce(paper_status, "no_paper")) %>%
    select(participant_id, has_log, paper_status, all_of(setdiff(names(paper_out), c("participant_id", "paper_status"))),
           everything()) %>%
    arrange(participant_id)
  write_csv(wide_with_paper, file.path(output_dir, "wide_with_paper.csv"), na = "")
  paper_check_ids <- sort(unique(c(paper_check_ids, dup_ids, problems$participant_id, setdiff(paper_ids, log_ids),
                                   setdiff(log_ids, paper_ids), far$participant_id)))

  # 10. Q4 chart: SSQ before vs after, per participant. There is no frame-rate
  #     or head-movement data (decision 022), so the only log signals shown
  #     are headset removal, pauses and an early end.
  joined <- wide_with_paper %>% filter(has_log, paper_status == "joined")
  q4 <- joined %>%
    mutate(log_signal = if_else(hmd_removed_count > 0 | pauses > 0 | end_status != "completed",
                                "headset off, paused or ended early", "none of these")) %>%
    select(participant_id, log_signal, matches("^ssq_(pre|post)_(nausea|oculomotor|disorientation|total)$")) %>%
    pivot_longer(-c(participant_id, log_signal), names_to = c("form", "score"),
                 names_pattern = "ssq_(pre|post)_(.*)") %>%
    group_by(participant_id, score) %>% filter(!anyNA(value), n() == 2) %>% ungroup()
  if (nrow(q4) > 0) {
    score_labels <- c(nausea = "Nausea", oculomotor = "Oculomotor", disorientation = "Disorientation",
                      total = "Total score")
    n_q4 <- q4 %>% distinct(participant_id, score) %>% count(score) %>%
      mutate(label = paste0(score_labels[score], " (n = ", n, ")"))
    score_levels <- intersect(names(score_labels), n_q4$score)
    q4 <- q4 %>% mutate(form = factor(form, levels = c("pre", "post"), labels = c("before", "after")),
                        score = factor(score, levels = score_levels,
                                       labels = n_q4$label[match(score_levels, n_q4$score)]))
    p_q4 <- ggplot(q4, aes(form, value, group = participant_id, colour = log_signal)) +
      geom_line(alpha = 0.6) + geom_point(size = 2) +
      facet_wrap(~ score, nrow = 1, scales = "free_y") +
      scale_colour_manual(values = c("headset off, paused or ended early" = okabe_ito[5],
                                     "none of these" = okabe_ito[4]), name = "In the log") +
      labs(title = "Q4: Does VR discomfort (SSQ) rise from before to after the course?",
           subtitle = paste0("Line = one participant with both forms. Weighted SSQ scores (Kennedy 1993). ",
                             "No frame-rate or head-movement data exist (decision 022)."),
           x = "SSQ form", y = "SSQ score (weighted)") +
      theme_minimal(base_size = 11) + theme(legend.position = "bottom")
    ggsave(file.path(output_dir, "ssq_pre_post.png"), p_q4, width = 10, height = 4.5, dpi = 150, bg = "white")
  }

  # 11. Q5 chart: NASA-TLX vs the attempts and the trainee's active time.
  #     Who stopped early did fewer scenes: hollow points.
  q5 <- joined %>% filter(!is.na(tlx_raw)) %>%
    transmute(participant_id, tlx_raw,
              finished = if_else(course_finished %in% TRUE, "finished the course", "did not finish / unknown"),
              `Rated attempts, whole course (quiz touches + red-button presses)` = total_confirms,
              `Trainee active time (min)` = round((active_s - coalesce(facilitator_tail_s, 0)) / 60, 1)) %>%
    pivot_longer(-c(participant_id, tlx_raw, finished), names_to = "measure") %>%
    filter(!is.na(value)) %>%
    # n per panel: schema-1 logs have no attempt count, so the panels can differ.
    group_by(measure) %>% mutate(measure = paste0(measure, " (n = ", n(), ")")) %>% ungroup()
  if (nrow(q5) > 0) {
    p_q5 <- ggplot(q5, aes(value, tlx_raw, shape = finished)) +
      geom_point(size = 2.5, colour = okabe_ito[4]) +
      scale_shape_manual(values = c("finished the course" = 16, "did not finish / unknown" = 1), name = NULL) +
      facet_wrap(~ measure, scales = "free_x") +
      labs(title = "Q5: Do trainees who needed more attempts or time report more load?",
           subtitle = paste0("Point = one participant. Raw NASA-TLX = mean of the 6 scales, once at the end. ",
                             "Hollow = stopped early: fewer scenes, so fewer attempts and less time."),
           x = NULL, y = "Raw NASA-TLX (0-100)") +
      theme_minimal(base_size = 11) + theme(legend.position = "bottom")
    ggsave(file.path(output_dir, "tlx_vs_attempts.png"), p_q5, width = 10, height = 4.5, dpi = 150, bg = "white")
  }
}

# ---- 15. Validity limits and warnings file --------------------------------------

validity_limits <- c(
  "VALIDITY LIMITS (print with every report):",
  "- Fixed scene order: a lower error rate in a later scene mixes learning with scene difficulty. It is an indication of improvement, not proof (no control group, no pre/post test).",
  "- Quizzes (ConceptScene, Complex remainder quiz): a guess is right 33 % of the time (A/B/C; 50 % for the two-option EKZ questions) and a touch is rated at once, so later touches are trial-and-error. Use the first attempt per item (items.csv), not the number of touches.",
  "- Complex position 1 is scaffolded by the remainder quiz: the quiz hands the trainee the numbers (full cartons, loose pieces) before the first pick, so position 1 measures picking, not calculation; position 2 is unscaffolded (item_skills.csv, column scaffolded; hollow points in first_attempt_by_item.png). Don't pool the two positions as one calculate_quantity measure.",
  "- Schema 1 logs (before decision 021) are kept for data quality, but their outcome columns are NA (not measured).",
  "- Small N, one run per person: describe distributions; any test is exploratory.",
  "- Time-to-correct only covers visits that reached correct; the 'never' counts show who is missing.",
  "- Q4 (paper SSQ): the logs hold no frame-rate, frame-drop, head-movement or tracking data (decision 022). SSQ change (post - pre) can only be read against headset removals, pauses, an early end (end_status, abbruchgrund) and active time, for the whole session (no per-scene sickness).",
  "- Q5 (paper NASA-TLX, once at the end, raw = unweighted mean): who stopped early did fewer scenes, so their attempts and time are not comparable with finishers.",
  "- ConceptScene tour (tour_steps.csv): Back presses are derived from step_index (-1 = Zurück) and exist only for layout_version \"025\" on; \"019\" logs and headers without layout_version have NA, not 0. A Zurück < 2 s after Weiter is read as a likely mis-press, not proven. Step time ends at the next step or the first quiz answer: time spent looking elsewhere (label board, packages) is in it.",
  "- Layout versions (decision 024): ConceptScene data of different layout_version (e.g. pilot P811 on \"019\") are never pooled; in a set with more than one layout the Concept figures and tables are split by layout_version (data_quality.csv mixed_layout).",
  "- Mixed position_1 (#94, decision 028): builds before #94 had Empf Kz 000 there (placement never rated), from #94 (line.empf_kz logged, layout \"029\") Empf Kz 003 (3 snap places, EKZ_PLACEMENT possible); an earlier \"029\" build without the field counts as 000. In a set with both, position_1 is split ([Empf Kz 000] / [Empf Kz 003]) in first_attempt_by_item.png and time_by_phase.png; the per-scene Mixed numbers (error_rate_MixedPickingScene, time_to_correct, error codes) still pool both and an Empf Kz 003 visit can have more presses (EKZ retries). The ekz_placement rate counts only presses with the right quantity at an Empf Kz 003 position (EKZ opportunities): its n is small and a wrong-quantity press is no EKZ attempt.",
  "- Complex position_1 (#93): up to layout \"025\" the remainder quiz and Worked Example stay visible for the whole position (scaffold_level quiz_visible); from \"029\" they hide after both answers are right and shelf card 2 shows the trainee's result (result_on_card). In a set with both levels position_1 is split in first_attempt_by_item.png and time_by_phase.png; the per-scene Complex numbers (time_to_correct_by_scene.png, error_codes_by_scene.png) still pool both levels.",
  "- ConceptScene hint tags (concept_hints*.csv, layout \"029\" on, decision 030): putting packages into the Concept containers is voluntary and not logged otherwise (no container_* in Concept), so hint counts have no denominator and are never an error rate or an attempt. One tag at a time: a second wrong package gets its tag only when the first is gone, so its shown time is not its placement time. The material_flow wait assumes the lane demo starts lane_demo_delay_s (4 s) after the step; older layouts have NA, not 0.")
# The pilot set (decision 023) is a try-out of the course and of this
# analysis, not study data: say so on every pilot report.
if (is_pilot_set) {
  validity_limits <- c(validity_limits,
    "- PILOT SET (P800-P899, decision 023): not study results. Pilots may run in EN or DE (see languages in data_quality.csv), are not the target group (developers, friends, an expert) and may know the course; never pool them with the study set.")
}

flagged <- data_quality %>%
  filter(end_status != "completed" | multi_session | mixed_build | duration_censored |
           invariant_breaks > 0 | unknown_codes > 0 | summary_mismatches > 0 |
           exit_disagreements > 0 | duplicate_files > 0 | error_cap_reached |
           resume_declined %in% TRUE | ended_session_override %in% TRUE | language_not_de |
           old_schema_log) %>%
  pull(participant_id)

report_lines <- c(
  paste0("analyse.R run ", format(Sys.time(), "%Y-%m-%dT%H:%M:%SZ", tz = "UTC"),
         " on ", data_dir, ", ", analysis_group, " set: ", nrow(data_quality), " participant(s), ",
         nrow(sessions), " session(s), ", nrow(visits), " visit(s)."),
  if (!is_pilot_set) paste0("Pilot IDs (P800-P899), analysed separately in output_dir/pilot/: ",
                            if (length(pilot_ids_seen) == 0) "none" else paste(pilot_ids_seen, collapse = ", ")),
  paste0("Participants to check in data_quality.csv: ",
         if (length(flagged) == 0) "none" else paste(flagged, collapse = ", ")),
  paste0("Paper forms to check (wide_with_paper.csv): ",
         if (length(paper_files) == 0) "no paper file"
         else if (length(paper_check_ids) == 0) "none" else paste(paper_check_ids, collapse = ", ")),
  "",
  "WARNINGS:",
  if (length(warning_lines) == 0) "(none)" else paste0("- ", warning_lines),
  "",
  validity_limits)
writeLines(report_lines, file.path(output_dir, "warnings.txt"), useBytes = TRUE)

cat("\n", paste(validity_limits, collapse = "\n"), "\n\n", sep = "")
cat("Outputs written to ", normalizePath(output_dir), "\n", sep = "")
