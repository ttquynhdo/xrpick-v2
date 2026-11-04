# analyse.R
#
# Turns research logs from the xpick app into charts and tables for
# research questions 1 (improvement per skill), 2 (hard scenes, common errors)
# and 6 (data quality). Rules: docs/superpowers/specs/2026-11-01-research-logger-design.md
# section 8. Log format: docs/research-logging-spec.md, schema_version 2
# (decision 021). Logs with schema_version 1 are read for sessions, visits and
# pauses, but their outcome events (v1 catalogue) are not: NA, not measured.
#
# events.jsonl is the source of truth: every per-visit number is rebuilt from
# the events. summary.csv is only used as a cross-check of the C# code.
#
# Run from the repo root:   Rscript analysis/analyse.R
# Or in RStudio: open the repo folder as the working directory, then Source.
#
# Input:  every *_events.jsonl / *_summary.csv under data_dir (default
#         analysis/data, sub folders included). Set data_dir before sourcing
#         to analyse one wave, e.g.  data_dir <- "analysis/data/wave1"
# Output: analysis/output/ (or output_dir if set before sourcing)
#
# Packages: tidyverse and jsonlite only.

suppressPackageStartupMessages({
  library(tidyverse)
  library(jsonlite)
})

# ---- 1. Settings and paths -------------------------------------------------

# Works from the repo root and from inside analysis/ (same rule as make_fake_data.R).
analysis_folder <- if (file.exists("skill_map.csv")) "." else "analysis"

# A caller (e.g. selftest.R) may set data_dir / output_dir before sourcing.
if (!exists("data_dir")) data_dir <- file.path(analysis_folder, "data")
if (!exists("output_dir")) output_dir <- file.path(analysis_folder, "output")
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
read_events_file <- function(path) {
  lines <- readLines(path, encoding = "UTF-8", warn = FALSE)
  keep <- nzchar(trimws(lines))
  line_no <- seq_along(lines)[keep]
  records <- map2(lines[keep], line_no, function(txt, i) {
    tryCatch(fromJSON(txt, simplifyVector = FALSE), error = function(e) {
      warn("unreadable line ", i, " in ", basename(path), " (skipped)")
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
  filter(folder == "practice", is_valid_id(participant_id), !is_test_id(participant_id)) %>%
  distinct(participant_id)
if (nrow(real_in_practice) > 0) {
  warn("real ID in practice/ (kept, check it): ",
       paste(real_in_practice$participant_id, collapse = ", "))
}
events <- events_raw %>% filter(is_valid_id(participant_id), !is_test_id(participant_id))
if (nrow(events) == 0) stop("No events left after dropping test IDs.")

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
                  "object_dropped_floor", "input_mode_change", "tracking_lost",
                  "tracking_regained", "perf_sample")
unknown_events <- setdiff(unique(events$event), known_events)
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
# Schema 2 adds layout_version ("019" = three-lane station, decision 019) and
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
non_de <- sessions %>% filter(has_header, !is.na(language), language != "DE")
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
         counted = event == "confirm_pressed" | !quiz %in% "complex_remainder")

# Invariant (spec section 6): correct == (no error codes). Report every break.
invariant_breaks <- presses %>% filter(is.na(correct) | correct != (n_codes == 0))
for (i in seq_len(nrow(invariant_breaks))) {
  r <- invariant_breaks[i, ]
  warn("invariant correct == (error_codes empty) broken: ", r$participant_id, " ", r$scene,
       " visit ", r$scene_visit, " ", r$event, " attempt ", r$attempt_no, " (correct = ", r$correct,
       ", codes = ", paste(r$codes[[1]], collapse = ";"), ")")
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
         counted, codes) %>%
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
    first_press_id = press_id[1],
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

# Station rides (decision 019): time the trainee waits while a storage box or
# the order box rides. Each ride = station_move_start -> station_move_arrived
# of the same (what, position). Rides overlap (old box leaves while the new
# one arrives), so the wait is the union of the ride intervals, not their
# sum, minus paused time. A ride with no arrival (crash) ends at the visit end.
first_or_na <- function(x) if (length(x) == 0) NA_real_ else min(x)
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
      out_e[k] <- max(out_e[k], end_t[i])  # overlaps the previous ride: extend it
    } else {
      out_s <- c(out_s, start_t[i])
      out_e <- c(out_e, end_t[i])
    }
  }
  tibble(start_t = out_s, end_t = out_e)
}
wait_stats <- tibble(session_id = character(), scene = character(), scene_visit = integer(),
                     ev_wait_s = double())
if (nrow(ride_rows) > 0) {
  wait_stats <- ride_rows %>%
    group_by(session_id, scene, scene_visit) %>%
    group_modify(~ union_intervals(.x$start_t, .x$end_t)) %>%
    ungroup() %>%
    mutate(active_wait_s = end_t - start_t - paused_between(session_id, start_t, end_t)) %>%
    group_by(session_id, scene, scene_visit) %>%
    summarise(ev_wait_s = round(sum(active_wait_s), 3), .groups = "drop")
}

# Sessions that measured outcomes (rule at the top of this section).
session_outcome <- sessions %>%
  transmute(session_id,
            outcome_measured = case_when(
              schema_version %in% 1L ~ FALSE,
              groups_logged %in% TRUE ~ str_detect(coalesce(event_groups, ""), "(^|;)outcome(;|$)"),
              TRUE ~ session_id %in% presses$session_id))
outcome_sessions <- session_outcome$session_id[session_outcome$outcome_measured]

# ---- 8. Merge sessions per participant and number visits across them ----------

visits <- visits_base %>%
  left_join(sessions %>% select(participant_id, session_id, session_no, build, schema_version,
                                end_status, crash_before, start_utc),
            by = "session_id") %>%
  left_join(press_visit, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(task_completed, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(rated_counts, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(ignored_counts, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(wait_stats, by = c("session_id", "scene", "scene_visit")) %>%
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
    # Completed = the visit has a task_completed (Mixed/Complex: after the last
    # position, so a correct position 1 followed by a crash is not completed).
    completed = if_else(outcome_logged, coalesce(tc_seen, FALSE), NA),
    attempts_to_correct = tc_attempts_to_correct,
    time_to_first_confirm_s = coalesce(tc_time_to_first_confirm_s, time_to_first_confirm_presses),
    time_to_correct_s = tc_time_to_correct_s,
    # Error rate per visit: share of presses with >= 1 code; NA when 0 presses.
    error_rate = if_else(!is.na(confirm_count) & confirm_count > 0,
                         presses_with_error / confirm_count, NA_real_),
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

unknown_scenes <- setdiff(levels(visits$scene), scene_levels)
if (length(unknown_scenes) > 0) {
  warn("scene(s) missing from scenes.csv (put last in charts): ", paste(unknown_scenes, collapse = ", "))
}

# ---- 9. Compare the rebuilt visits with summary.csv ---------------------------

summary_files <- list.files(data_dir, pattern = "_summary\\.csv$", recursive = TRUE, full.names = TRUE)
if (!grepl("fake", data_dir)) summary_files <- summary_files[!grepl("/fake/", summary_files)]
# Every cell is read as text first, then the known columns get their type.
# So a summary.csv of another schema (schema 1 with dwell_* columns and no
# confirm_ignored) reads without errors; a missing column becomes NA below.
summary_num_cols <- c("schema_version", "scene_visit", "duration_s", "paused_s", "confirm_count",
                      "confirm_ignored", "attempts_to_correct", "time_to_first_confirm_s",
                      "time_to_correct_s", "pauses", "hmd_removed_count", "unity_errors")
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
    filter(is_valid_id(participant_id), !is_test_id(participant_id))
  # Same rule as for events files: one file per session, the longest wins.
  dropped_summary_files <- summary_rows %>%
    count(participant_id, session_id, path, name = "n_rows") %>%
    pick_longest_files("summary")
  summary_rows <- summary_rows %>% filter(!path %in% dropped_summary_files$path) %>% select(-path)
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
compare_cols <- c("duration_s", "paused_s", "pauses", "hmd_removed_count", "unity_errors",
                  outcome_compare_cols)
if (nrow(summary_rows) > 0) {
  # A column this summary.csv schema doesn't have is "not measured".
  for (cl in setdiff(compare_cols, names(summary_rows))) {
    summary_rows[[cl]] <- if (cl %in% summary_lgl_cols) NA else
      if (cl == "error_codes_all") NA_character_ else NA_real_
  }
  # Schema 1 rows: their outcome columns follow the v1 catalogue and are not
  # read (same rule as for the events, section 5), so they are not compared.
  summary_rows <- summary_rows %>%
    mutate(across(all_of(outcome_compare_cols), ~ if_else(schema_version %in% 1, NA, .x)))
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
    language_not_de = any(!is.na(language) & language != "DE"),
    schema_versions = paste(unique(schema_version), collapse = ";"),
    # Station layout per session ("NA" = schema 1 log, no layout_version).
    layout_versions = paste(unique(coalesce(layout_version, "NA")), collapse = ";"),
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
         mixed_build, languages, language_not_de, schema_versions, old_schema_log, layout_versions, gap_h, n_crashes, resume_offered, resume_declined,
         ended_session_override, duration_censored, n_visits,
         n_repeat_visits, n_repeat_after_crash, active_s, paused_s, pauses, hmd_removed_count,
         unity_errors, error_cap_reached, outcome_logged, total_confirms, invariant_breaks,
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
           column == "outcome" ~ value == "FALSE",
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
            .groups = "drop") %>%
  mutate(first_attempt_codes = na_if(first_attempt_codes, "")) %>%
  left_join(item_skills, by = c("scene", "item")) %>%
  mutate(
    # Positions without a skill in item_skills.csv: "picking" (whole position).
    skill = if_else(is.na(skill) & kind == "position", position_skill_default, skill),
    scaffolded = coalesce(scaffolded, FALSE),
    scene = factor(scene, levels = levels(visits$scene))) %>%
  arrange(participant_id, scene, item)
unmapped_items <- items %>% filter(is.na(skill)) %>% distinct(scene, item)
for (i in seq_len(nrow(unmapped_items))) {
  warn("quiz item ", unmapped_items$item[i], " in ", unmapped_items$scene[i],
       " is not in item_skills.csv: left out of first_attempt_by_item.png")
}
write_csv(items %>% select(participant_id, scene, item, kind, quiz, skill, scaffolded,
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
  group_by(skill, scene, item, kind, scaffolded) %>%
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
           x_label = paste0(str_remove(as.character(scene), "(Picking)?Scene$"), "\n", item),
           skill = factor(skill, levels = skill_levels),
           # Hollow = scaffolded (the app handed the trainee the numbers).
           fill_colour = if_else(scaffolded, "white", skill_colours[as.character(skill)])) %>%
    arrange(as.integer(scene), item_rank) %>%
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
# confirm_count (red-button presses; quiz touches in ConceptScene) are in the
# numerator and the denominator; the Complex remainder quiz is in items above.
counting_visits <- visits %>% filter(counting, confirm_count > 0)

# Per counting visit and per skill the scene tests: share of presses with a
# code of that skill. A tested but error-free skill gives 0 %, not missing.
skill_rates <- counting_visits %>%
  mutate(scene = as.character(scene)) %>%
  select(participant_id, session_id, scene, scene_visit, confirm_count, first_press_id) %>%
  inner_join(skill_pairs, by = "scene", relationship = "many-to-many") %>%
  left_join(press_codes %>% filter(counted, !is.na(skill)) %>%
              distinct(session_id, scene, scene_visit, press_id, skill) %>%
              group_by(session_id, scene, scene_visit, skill) %>%
              summarise(presses_with_skill_error = n(),
                        skill_error_press_ids = list(press_id), .groups = "drop"),
            by = c("session_id", "scene", "scene_visit", "skill")) %>%
  mutate(presses_with_skill_error = coalesce(presses_with_skill_error, 0L),
         # The visit's first counted attempt carried a code of this skill.
         first_attempt_skill_error = map2_lgl(skill_error_press_ids, first_press_id,
                                              ~ !is.null(.x) && .y %in% .x),
         error_rate = presses_with_skill_error / confirm_count,
         scene = factor(scene, levels = levels(visits$scene))) %>%
  select(-skill_error_press_ids, -first_press_id)

# Scenes with skills, in build order, so every layer uses the same x order.
skill_scene_limits <- intersect(levels(visits$scene), unique(skill_pairs$scene))

title_q1 <- "Secondary: does the error rate per press fall across the course, per skill?"
if (nrow(skill_rates) == 0) {
  p_skill <- empty_chart(title_q1)
  warn("no outcome data: error_rate_by_skill.png and error_rate_by_skill_first_attempt.png are empty")
} else {
  # Mean per scene and skill (= mean of participants, each with one visit).
  skill_means <- skill_rates %>% group_by(scene, skill) %>%
    summarise(mean_rate = mean(error_rate), n = n(), .groups = "drop")
  p_skill <- ggplot(skill_rates, aes(scene, 100 * error_rate, colour = skill)) +
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
                           "Indication only: fixed scene order, no control group."),
         x = "Scene (build order)", y = "Confirm presses with an error of this skill (%)") +
    theme_minimal(base_size = 11) +
    theme(axis.text.x = element_text(angle = 25, hjust = 1))
}
ggsave(file.path(output_dir, "error_rate_by_skill.png"), p_skill, width = 10, height = 6.5, dpi = 150, bg = "white")

# Second chart: first attempts only (did the first press already avoid this error?).
title_q1b <- "First attempt: how many participants made an error of this skill?"
if (nrow(skill_rates) == 0) {
  p_first <- empty_chart(title_q1b)
} else {
  first_rates <- skill_rates %>% group_by(scene, skill) %>%
    summarise(k = sum(first_attempt_skill_error), n = n(), .groups = "drop") %>%
    mutate(share = k / n)
  p_first <- ggplot(first_rates, aes(scene, 100 * share, colour = skill, group = skill)) +
    geom_line(data = first_rates %>% group_by(skill) %>% filter(n() > 1), linewidth = 0.8) +
    geom_point(size = 3) +
    geom_text(aes(label = paste0(k, "/", n)), vjust = -1, size = 3, colour = "grey20") +
    facet_wrap(~ skill) +
    scale_colour_manual(values = skill_colours, guide = "none") +
    scale_x_discrete(limits = skill_scene_limits) +
    scale_y_continuous(limits = c(0, 110), breaks = seq(0, 100, 25)) +
    labs(title = title_q1b,
         subtitle = "Label = participants with this error on attempt 1 / participants with a counting visit.",
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
         scene = factor(scene, levels = levels(visits$scene)))
if (nrow(codes_counting) == 0) {
  p_codes <- empty_chart(title_q2)
  warn("no error codes in counting visits: error_codes_by_scene.png is empty")
} else {
  scene_n <- counting_visits %>% group_by(scene) %>%
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
ttc_people <- visits %>% filter(outcome_logged) %>%
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

# ---- 14. Wide file: one row per participant (for SPSS and paper forms) ---------

# Per-scene columns come from the counting visit only (repeat visits stay in
# the long `visits` table). Paper questionnaires (#31) join on participant_id.
per_scene <- visits %>% filter(counting) %>%
  mutate(scene = as.character(scene)) %>%
  transmute(participant_id, scene,
            active_s = duration_s, paused_s = paused_s, confirms = confirm_count,
            confirm_ignored = confirm_ignored, wait_s = wait_s,
            errors = presses_with_error, error_rate = error_rate,
            first_correct = first_attempt_correct, attempts_to_correct = attempts_to_correct,
            time_to_correct_s = time_to_correct_s, completed = completed,
            visit_censored = duration_censored)
visit_counts <- visits %>% mutate(scene = as.character(scene)) %>% count(participant_id, scene, name = "visits")
per_scene <- full_join(visit_counts, per_scene, by = c("participant_id", "scene"))

outcome_measures <- c("confirms", "errors", "error_rate", "first_correct", "attempts_to_correct",
                      "time_to_correct_s", "completed")
wide_scene <- per_scene %>%
  pivot_wider(id_cols = participant_id, names_from = scene, names_glue = "{.value}_{scene}",
              values_from = c(visits, active_s, paused_s, confirms, confirm_ignored, wait_s, errors,
                              error_rate, first_correct, attempts_to_correct, time_to_correct_s,
                              completed, visit_censored))
# Scenes without a confirm or quiz (WelcomeScene) never have outcome values:
# drop those always-empty columns.
no_outcome_cols <- as.vector(outer(outcome_measures, setdiff(unique(per_scene$scene), outcome_scenes),
                                   paste, sep = "_"))
# Ignored presses and station waits exist only in scenes with a red button.
no_button_cols <- as.vector(outer(c("confirm_ignored", "wait_s"),
                                  setdiff(unique(per_scene$scene), red_button_scenes), paste, sep = "_"))
wide_scene <- wide_scene %>% select(-any_of(c(no_outcome_cols, no_button_cols)))

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
                      "hmd_removed_count", "unity_errors", "total_confirms")
wide <- data_quality %>%
  select(all_of(participant_cols)) %>%
  left_join(wide_scene, by = "participant_id") %>%
  left_join(wide_skill, by = "participant_id")
# A scene nobody had a counting visit for still gets its error-rate column
# (added before sorting, so it lands in build order).
for (s in intersect(levels(visits$scene), outcome_scenes)) {
  if (!paste0("error_rate_", s) %in% names(wide)) wide[[paste0("error_rate_", s)]] <- NA_real_
}
scene_cols <- setdiff(names(wide), participant_cols)
wide <- wide %>% select(all_of(participant_cols), all_of(scene_col_order(scene_cols)))
write_csv(wide, file.path(output_dir, "wide.csv"), na = "")

# ---- 15. Validity limits and warnings file --------------------------------------

validity_limits <- c(
  "VALIDITY LIMITS (print with every report):",
  "- Fixed scene order: a lower error rate in a later scene mixes learning with scene difficulty. It is an indication of improvement, not proof (no control group, no pre/post test).",
  "- Quizzes (ConceptScene, Complex remainder quiz): a guess is right 33 % of the time (A/B/C; 50 % for the two-option EKZ questions) and a touch is rated at once, so later touches are trial-and-error. Use the first attempt per item (items.csv), not the number of touches.",
  "- Complex position 1 is scaffolded by the remainder quiz: the quiz hands the trainee the numbers (full cartons, loose pieces) before the first pick, so position 1 measures picking, not calculation; position 2 is unscaffolded (item_skills.csv, column scaffolded; hollow points in first_attempt_by_item.png). Don't pool the two positions as one calculate_quantity measure.",
  "- Schema 1 logs (before decision 021) are kept for data quality, but their outcome columns are NA (not measured).",
  "- Small N, one run per person: describe distributions; any test is exploratory.",
  "- Time-to-correct only covers visits that reached correct; the 'never' counts show who is missing.")

flagged <- data_quality %>%
  filter(end_status != "completed" | multi_session | mixed_build | duration_censored |
           invariant_breaks > 0 | unknown_codes > 0 | summary_mismatches > 0 |
           exit_disagreements > 0 | duplicate_files > 0 | error_cap_reached |
           resume_declined %in% TRUE | ended_session_override %in% TRUE | language_not_de |
           old_schema_log) %>%
  pull(participant_id)

report_lines <- c(
  paste0("analyse.R run ", format(Sys.time(), "%Y-%m-%dT%H:%M:%SZ", tz = "UTC"),
         " on ", data_dir, ": ", nrow(data_quality), " participant(s), ",
         nrow(sessions), " session(s), ", nrow(visits), " visit(s)."),
  paste0("Participants to check in data_quality.csv: ",
         if (length(flagged) == 0) "none" else paste(flagged, collapse = ", ")),
  "",
  "WARNINGS:",
  if (length(warning_lines) == 0) "(none)" else paste0("- ", warning_lines),
  "",
  validity_limits)
writeLines(report_lines, file.path(output_dir, "warnings.txt"), useBytes = TRUE)

cat("\n", paste(validity_limits, collapse = "\n"), "\n\n", sep = "")
cat("Outputs written to ", normalizePath(output_dir), "\n", sep = "")
