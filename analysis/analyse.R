# analyse.R
#
# Turns research logs from the xpick app into charts and tables for
# research questions 1 (improvement per skill), 2 (hard scenes, common errors)
# and 6 (data quality). Rules: docs/superpowers/specs/2026-11-01-research-logger-design.md
# section 8.
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
mixed_files <- events %>% group_by(file) %>%
  summarise(n_pid = n_distinct(participant_id), n_sid = n_distinct(session_id), .groups = "drop") %>%
  filter(n_pid > 1 | n_sid > 1)
if (nrow(mixed_files) > 0) {
  warn("file(s) mixing participants or sessions: ", paste(mixed_files$file, collapse = ", "))
}

# Unknown event names are kept but reported (maybe a newer logger version).
known_events <- c("header", "session_start", "session_end", "scene_enter", "scene_exit",
                  "app_pause", "app_resume", "unity_error", "unity_error_cap_reached",
                  "crash_recovered", "confirm_pressed", "quiz_answer", "task_completed",
                  "language_change")
unknown_events <- setdiff(unique(events$event), known_events)
if (length(unknown_events) > 0) {
  message("Note: event names not used by this script: ", paste(unknown_events, collapse = ", "))
}

# ---- 5. Sessions: build, start, end, how they ended -------------------------------

# Header line: build and schema_version of the app that wrote the file.
headers <- events %>% filter(event == "header") %>%
  transmute(session_id,
            build = map_chr(data, ~ as_chr(.x$build)),
            schema_version = map_int(data, ~ as_int(.x$schema_version)))
if (any(duplicated(headers$session_id))) warn("a session has more than one header line")

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
  left_join(session_ends, by = "session_id") %>%
  mutate(end_status = if_else(is.na(end_reason), "crash", end_reason)) %>%
  arrange(participant_id, start_utc) %>%
  group_by(participant_id) %>%
  mutate(session_no = row_number(),
         # Hours between the end of the previous session and the start of this one.
         gap_h = as.numeric(difftime(start_utc, lag(last_utc), units = "hours")),
         crash_before = lag(cumsum(end_status == "crash"), default = 0) > 0) %>%
  ungroup()

missing_header <- sessions %>% filter(is.na(build))
if (nrow(missing_header) > 0) {
  warn("session(s) without header line: ", paste(missing_header$session_id, collapse = ", "))
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

# Pauses: app_pause starts one, app_resume (with paused_s) ends it. The causes
# list tells whether the headset was taken off.
has_hmd <- function(d) "hmd_unmounted" %in% unlist(d$causes)
pause_stats <- visit_events %>%
  filter(event %in% c("app_pause", "app_resume")) %>%
  group_by(session_id, scene, scene_visit) %>%
  summarise(
    ev_pauses = sum(event == "app_pause"),
    ev_paused_s = sum(map_dbl(data[event == "app_resume"], ~ as_num(.x$paused_s))),
    ev_hmd_resumed = sum(map_lgl(data[event == "app_resume"], has_hmd)),
    # A pause still open at the end (crash while paused): its start time.
    last_pause_t = suppressWarnings(max(t[event == "app_pause"])),
    last_resume_t = suppressWarnings(max(t[event == "app_resume"])),
    last_pause_hmd = if (any(event == "app_pause")) has_hmd(tail(data[event == "app_pause"], 1)[[1]]) else FALSE,
    .groups = "drop") %>%
  mutate(open_pause = is.finite(last_pause_t) & last_pause_t > last_resume_t)

# Unity errors: a line with folded_count > 0 stands for that many repeats,
# a line with folded_count 0 is one error.
error_stats <- visit_events %>% filter(event == "unity_error") %>%
  mutate(n_err = map_dbl(data, ~ { f <- as_num(.x$folded_count); if (is.na(f) || f == 0) 1 else f })) %>%
  group_by(session_id, scene, scene_visit) %>%
  summarise(ev_unity_errors = sum(n_err), .groups = "drop")

last_events <- visit_events %>% group_by(session_id, scene, scene_visit) %>%
  summarise(last_t = max(t), .groups = "drop")

visits_base <- enters %>%
  left_join(exits, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(pause_stats, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(error_stats, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(last_events, by = c("session_id", "scene", "scene_visit")) %>%
  replace_na(list(ev_pauses = 0L, ev_paused_s = 0, ev_hmd_resumed = 0L,
                  open_pause = FALSE, last_pause_hmd = FALSE, ev_unity_errors = 0)) %>%
  mutate(
    closed = !is.na(exit_t),
    # A visit without scene_exit is the open visit of a crashed session. Its
    # active time ends at its last event, or at the start of a pause still open.
    end_t = case_when(closed ~ exit_t, open_pause ~ last_pause_t, TRUE ~ last_t),
    rebuilt_duration_s = round(end_t - enter_t - ev_paused_s, 3),
    duration_censored = !closed,
    duration_s = if_else(closed, exit_duration_s, rebuilt_duration_s),
    paused_s = if_else(closed, exit_paused_s, ev_paused_s),
    pauses = if_else(closed, exit_pauses, as.integer(ev_pauses)),
    hmd_removed_count = if_else(closed, exit_hmd_removed_count,
                                as.integer(ev_hmd_resumed + (open_pause & last_pause_hmd))),
    unity_errors = if_else(closed, exit_unity_errors, as.integer(ev_unity_errors)))

# Cross-check: scene_exit numbers must match what the other events say.
exit_checks <- visits_base %>% filter(closed) %>%
  filter(abs(rebuilt_duration_s - exit_duration_s) > 0.005 |
           abs(ev_paused_s - exit_paused_s) > time_tolerance_s |
           ev_pauses != exit_pauses | ev_unity_errors != exit_unity_errors)
for (i in seq_len(nrow(exit_checks))) {
  r <- exit_checks[i, ]
  warn("scene_exit disagrees with the visit's events: session ", r$session_id, " ",
       r$scene, " visit ", r$scene_visit)
}

# ---- 7. Outcome events (#28): confirm presses and quiz answers ----------------

# One row per press. error_codes is a list of {code, item}; keep the codes.
press_events <- visit_events %>% filter(event %in% c("confirm_pressed", "quiz_answer"))
presses <- press_events %>%
  transmute(participant_id, session_id, scene, scene_visit, t, event,
            attempt_no = map_int(data, ~ as_int(.x$attempt_no)),
            correct = map_lgl(data, ~ as_lgl(.x$correct)),
            codes = map(data, ~ map_chr(.x$error_codes %||% list(), function(e) as_chr(e$code))),
            time_since_scene_enter_s = map_dbl(data, ~ as_num(.x$time_since_scene_enter_s))) %>%
  mutate(n_codes = lengths(codes))

# Invariant (design section 6): correct == (no error codes). Report every break.
invariant_breaks <- presses %>% filter(is.na(correct) | correct != (n_codes == 0))
for (i in seq_len(nrow(invariant_breaks))) {
  r <- invariant_breaks[i, ]
  warn("invariant correct == (error_codes empty) broken: ", r$participant_id, " ", r$scene,
       " visit ", r$scene_visit, " attempt ", r$attempt_no, " (correct = ", r$correct,
       ", codes = ", paste(r$codes[[1]], collapse = ";"), ")")
}

# One row per error code on a press, with its skill from skill_map.csv.
press_codes <- presses %>%
  select(participant_id, session_id, scene, scene_visit, attempt_no, codes) %>%
  unnest_longer(codes, values_to = "code") %>%
  mutate(code = as.character(code)) %>%  # stays text even with no presses (pre-#28 logs)
  filter(!is.na(code)) %>%
  left_join(skill_map %>% select(error_code, skill), by = c("code" = "error_code"))

# Unknown codes are warned, never silently dropped (design section 8).
unknown_codes <- press_codes %>% filter(is.na(skill))
for (i in seq_len(nrow(unknown_codes))) {
  r <- unknown_codes[i, ]
  warn("unknown error code ", r$code, " (not in skill_map.csv): ", r$participant_id, " ",
       r$scene, " visit ", r$scene_visit, " attempt ", r$attempt_no)
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

task_completed <- visit_events %>% filter(event == "task_completed") %>%
  transmute(session_id, scene, scene_visit,
            tc_attempts_to_correct = map_int(data, ~ as_int(.x$attempts_to_correct)),
            tc_time_to_first_confirm_s = map_dbl(data, ~ as_num(.x$time_to_first_confirm_s)),
            tc_time_to_correct_s = map_dbl(data, ~ as_num(.x$time_to_correct_s))) %>%
  distinct(session_id, scene, scene_visit, .keep_all = TRUE)

# Active time at a press, for quiz answers (they carry no time field): wall
# time since scene enter minus the pauses that ended before the press.
resumes <- visit_events %>% filter(event == "app_resume") %>%
  transmute(session_id, scene, scene_visit, resume_t = t,
            resume_paused_s = map_dbl(data, ~ as_num(.x$paused_s)))
press_active <- presses %>%
  select(session_id, scene, scene_visit, attempt_no, t, time_since_scene_enter_s) %>%
  left_join(enters %>% select(session_id, scene, scene_visit, enter_t),
            by = c("session_id", "scene", "scene_visit")) %>%
  left_join(resumes, by = c("session_id", "scene", "scene_visit"), relationship = "many-to-many") %>%
  group_by(session_id, scene, scene_visit, attempt_no, t, time_since_scene_enter_s, enter_t) %>%
  summarise(paused_before = sum(resume_paused_s[!is.na(resume_t) & resume_t <= t]), .groups = "drop") %>%
  mutate(active_at_s = coalesce(time_since_scene_enter_s, round(t - enter_t - paused_before, 3)))

# Per visit outcome numbers. A press "has an error" when it carries at least one code.
press_visit <- presses %>%
  left_join(press_active %>% select(session_id, scene, scene_visit, attempt_no, active_at_s),
            by = c("session_id", "scene", "scene_visit", "attempt_no")) %>%
  arrange(session_id, scene, scene_visit, attempt_no) %>%
  group_by(session_id, scene, scene_visit) %>%
  summarise(
    confirm_count = n(),
    presses_with_error = sum(n_codes > 0),
    # Rows are sorted by attempt_no, so row 1 is the first attempt.
    first_attempt_correct = correct[1],
    first_attempt_has_error = n_codes[1] > 0,
    attempts_to_correct_presses = if (any(correct, na.rm = TRUE)) min(attempt_no[correct %in% TRUE]) else NA_integer_,
    time_to_first_confirm_presses = active_at_s[1],
    error_codes_all = {
      cc <- unlist(codes)
      if (length(cc) == 0) NA_character_ else {
        tab <- table(cc)
        paste0(names(tab), ":", as.integer(tab), collapse = ";")
      }
    },
    .groups = "drop")

# Outcome data exist only once #28 logs them. A session with no press at all
# is treated as "not measured" (NA), not as "0 confirms": pre-#28 logs have
# no outcome events. A session with presses in some scenes measures them all,
# so a picking visit there with no press is a real 0.
outcome_sessions <- unique(presses$session_id)

# ---- 8. Merge sessions per participant and number visits across them ----------

visits <- visits_base %>%
  left_join(sessions %>% select(participant_id, session_id, session_no, build, schema_version,
                                end_status, crash_before, start_utc),
            by = "session_id") %>%
  left_join(press_visit, by = c("session_id", "scene", "scene_visit")) %>%
  left_join(task_completed, by = c("session_id", "scene", "scene_visit")) %>%
  mutate(
    outcome_logged = session_id %in% outcome_sessions & scene %in% outcome_scenes,
    confirm_count = if_else(outcome_logged, coalesce(confirm_count, 0L), NA_integer_),
    presses_with_error = if_else(outcome_logged, coalesce(presses_with_error, 0L), NA_integer_),
    completed = if_else(outcome_logged, !is.na(tc_time_to_correct_s) | !is.na(attempts_to_correct_presses), NA),
    attempts_to_correct = coalesce(tc_attempts_to_correct, attempts_to_correct_presses),
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
read_summary <- function(path) read_csv(path, show_col_types = FALSE, na = "",
  col_types = cols(.default = col_double(), participant_id = col_character(),
                   session_id = col_character(), build = col_character(), scene = col_character(),
                   enter_utc = col_character(), first_attempt_correct = col_logical(),
                   completed = col_logical(), error_codes_all = col_character()))
summary_rows <- tibble()
if (length(summary_files) > 0) {
  summary_rows <- map(summary_files, read_summary) %>% list_rbind() %>%
    filter(is_valid_id(participant_id), !is_test_id(participant_id))
}

# Sort "B:1;A:2" -> "A:2;B:1" so the code order never counts as a difference.
norm_codes <- function(x) map_chr(x, ~ if (is.na(.x)) NA_character_ else
  paste(sort(strsplit(.x, ";", fixed = TRUE)[[1]]), collapse = ";"))

compare_cols <- c("duration_s", "paused_s", "pauses", "hmd_removed_count", "unity_errors",
                  "confirm_count", "first_attempt_correct", "attempts_to_correct",
                  "time_to_first_confirm_s", "time_to_correct_s", "completed", "error_codes_all")
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
    schema_versions = paste(unique(schema_version), collapse = ";"),
    n_crashes = sum(end_status == "crash"),
    # end_status of the participant = how their last session ended.
    end_status = last(end_status),
    gap_h = if (n() > 1) round(max(gap_h, na.rm = TRUE), 2) else NA_real_,
    .groups = "drop") %>%
  mutate(multi_session = n_sessions > 1,
         mixed_build = str_detect(builds, ";"),
         course_finished = participant_id %in% finished_ids) %>%
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
  mutate(across(c(invariant_breaks, unknown_codes, summary_mismatches), ~ coalesce(.x, 0L))) %>%
  select(participant_id, end_status, course_finished, n_sessions, multi_session, builds,
         mixed_build, schema_versions, gap_h, n_crashes, duration_censored, n_visits,
         n_repeat_visits, n_repeat_after_crash, active_s, paused_s, pauses, hmd_removed_count,
         unity_errors, outcome_logged, total_confirms, invariant_breaks, unknown_codes,
         summary_mismatches, first_start_utc) %>%
  arrange(participant_id)

write_csv(data_quality, file.path(output_dir, "data_quality.csv"), na = "")

# Data-quality picture: one row per participant, cells that need a look in orange.
dq_long <- data_quality %>%
  transmute(participant_id,
            end_status, course_finished = as.character(course_finished),
            sessions = as.character(n_sessions), builds = str_replace_all(builds, ";", "\n"),
            gap_h = if_else(is.na(gap_h), "", as.character(gap_h)),
            censored = as.character(duration_censored),
            repeats = as.character(n_repeat_visits),
            paused_min = as.character(round(paused_s / 60, 1)),
            pauses = as.character(pauses), hmd_off = as.character(hmd_removed_count),
            unity_err = as.character(unity_errors),
            outcome = as.character(outcome_logged),
            invariant = as.character(invariant_breaks),
            unknown_code = as.character(unknown_codes),
            summary_diff = as.character(summary_mismatches)) %>%
  pivot_longer(-participant_id, names_to = "column", values_to = "value") %>%
  mutate(attention = case_when(
           column == "end_status" ~ value %in% c("crash", "quit", "facilitator"),
           column == "course_finished" ~ value == "FALSE",
           column == "sessions" ~ value != "1",
           column == "builds" ~ str_detect(value, "\n") | value == "editor",
           column == "gap_h" ~ value != "",
           column == "censored" ~ value == "TRUE",
           column == "outcome" ~ value == "FALSE",
           column == "paused_min" ~ value != "0",
           TRUE ~ value != "0"),
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

# ---- 11. Error rate per skill across scenes (question 1) -----------------------

counting_visits <- visits %>% filter(counting, confirm_count > 0)

# Per counting visit and per skill the scene tests: share of presses with a
# code of that skill. A tested but error-free skill gives 0 %, not missing.
skill_rates <- counting_visits %>%
  mutate(scene = as.character(scene)) %>%
  select(participant_id, session_id, scene, scene_visit, confirm_count) %>%
  inner_join(skill_pairs, by = "scene", relationship = "many-to-many") %>%
  left_join(press_codes %>% filter(!is.na(skill)) %>%
              distinct(session_id, scene, scene_visit, attempt_no, skill) %>%
              group_by(session_id, scene, scene_visit, skill) %>%
              summarise(presses_with_skill_error = n(),
                        first_attempt_skill_error = any(attempt_no == 1), .groups = "drop"),
            by = c("session_id", "scene", "scene_visit", "skill")) %>%
  mutate(presses_with_skill_error = coalesce(presses_with_skill_error, 0L),
         first_attempt_skill_error = coalesce(first_attempt_skill_error, FALSE),
         error_rate = presses_with_skill_error / confirm_count,
         scene = factor(scene, levels = levels(visits$scene)))

# Empty chart with a note, used when there are no outcome events (pre-#28 logs).
empty_chart <- function(title) {
  ggplot() +
    annotate("text", x = 0, y = 0, label = "keine/no outcome data yet", size = 6, colour = "grey40") +
    labs(title = title, subtitle = "keine/no outcome data yet (confirm_pressed / quiz_answer come with #28)") +
    theme_void()
}

# Scenes with skills, in build order, so every layer uses the same x order.
skill_scene_limits <- intersect(levels(visits$scene), unique(skill_pairs$scene))
skill_colours <- setNames(okabe_ito[seq_along(unique(skill_pairs$skill))], sort(unique(skill_pairs$skill)))

title_q1 <- "Does the error rate per skill fall across the course?"
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
         subtitle = "Counting visits only (first visit with >= 1 confirm). One press can carry several codes.",
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
            time = duration_s, paused = paused_s, confirms = confirm_count,
            errors = presses_with_error, error_rate = error_rate,
            first_correct = first_attempt_correct, attempts_to_correct = attempts_to_correct,
            time_to_correct = time_to_correct_s, completed = completed,
            visit_censored = duration_censored)
visit_counts <- visits %>% mutate(scene = as.character(scene)) %>% count(participant_id, scene, name = "visits")
per_scene <- full_join(visit_counts, per_scene, by = c("participant_id", "scene"))

wide_scene <- per_scene %>%
  pivot_wider(id_cols = participant_id, names_from = scene, names_glue = "{.value}_{scene}",
              values_from = c(visits, time, paused, confirms, errors, error_rate, first_correct,
                              attempts_to_correct, time_to_correct, completed, visit_censored))

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

wide <- data_quality %>%
  select(participant_id, end_status, course_finished, multi_session, mixed_build, builds,
         schema_versions, gap_h, n_sessions, duration_censored, active_s, paused_s, pauses,
         hmd_removed_count, unity_errors, total_confirms) %>%
  left_join(wide_scene, by = "participant_id") %>%
  left_join(wide_skill, by = "participant_id")
wide <- wide %>% select(1:16, all_of(scene_col_order(names(wide)[-(1:16)])))
for (s in unique(as.character(visits$scene))) {
  # A scene nobody had a counting visit for still gets its error-rate column.
  if (!paste0("error_rate_", s) %in% names(wide)) wide[[paste0("error_rate_", s)]] <- NA_real_
}
write_csv(wide, file.path(output_dir, "wide.csv"), na = "")

# ---- 15. Validity limits and warnings file --------------------------------------

validity_limits <- c(
  "VALIDITY LIMITS (print with every report):",
  "- Fixed scene order: a lower error rate in a later scene mixes learning with scene difficulty. It is an indication of improvement, not proof (no control group, no pre/post test).",
  "- ConceptScene quiz: the feedback reveals the right answer and a guess is right 33 % of the time, so quiz attempts say little about knowledge.",
  "- Data recorded before backlog #14 (feedback printing target numbers) measure trial-and-error, not skill. Separate builds before/after #14 (column builds).",
  "- Small N, one run per person: describe distributions; any test is exploratory.",
  "- Time-to-correct only covers visits that reached correct; the 'never' counts show who is missing.")

flagged <- data_quality %>%
  filter(end_status != "completed" | multi_session | mixed_build | duration_censored |
           invariant_breaks > 0 | unknown_codes > 0 | summary_mismatches > 0) %>%
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
