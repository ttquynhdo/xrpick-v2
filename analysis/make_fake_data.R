# make_fake_data.R
#
# Writes fake research logs in exactly the format of the C# ResearchLogger
# (Assets/Scripts/Research/), schema_version 2 (docs/research-logging-spec.md,
# decision 021): the session events of #26/#27 plus the outcome events that
# #28 step 2 will add (confirm_pressed per position, confirm_ignored,
# position_completed, task_completed, quiz_answer, quiz_unlocked, tour_step,
# station_move_start/_arrived, welcome_practice_done) and the interaction
# events of #29 (grab_start/_end, container_enter/_exit, menu_select,
# end_control_opened/_cancelled, scene_enter.via). Header event_groups =
# ["session","outcome","interaction"]; P104 ran a build before #29 (no
# "interaction" group: its interaction numbers must be NA, not 0). No pose,
# perf, floor-drop or tracking events: decision 022 dropped #30 and #74, the
# app never writes them.
# Also writes the typed-in paper forms (SSQ pre/post, NASA-TLX, Abbruchgrund,
# #31) as data/fake/paper_forms.csv, with the entry mistakes analyse.R must
# catch (section 9).
# One participant (P115) is written in the old schema-1 shape on purpose, to
# test that analyse.R warns about mixed schema versions.
# Why: analyse.R must be tested before real participants exist, and the
# fake files are the contract between the C# logger and R.
#
# Run from the repo root:   Rscript analysis/make_fake_data.R
# Output (git-ignored):     analysis/data/fake/research/ (+ research/pilot/ for P801-P803, research/practice/,
#                           research/backup/ with a partial duplicate of P101)
#
# Base R only (no packages), so it runs right after installing R.
# If the logger format changes, change this script in the same commit.

# The #29 interaction events (grabs, container changes, menu touches) draw
# from their own random stream, taken here before set.seed(26). So adding
# them changes no other fake value: presses, pauses and rides stay the same
# draws (selftest.R checks some of them by value). They only add time.
side <- new.env()
side$u <- local({ set.seed(29); runif(200000) })
side$i <- 0
side_runif <- function(a, b) {
  side$i <- side$i + 1
  a + (b - a) * side$u[side$i]
}
side_chance <- function(p) side_runif(0, 1) < p
side_pick <- function(x) x[min(length(x), floor(side_runif(0, 1) * length(x)) + 1)]

set.seed(26)  # same fake data on every run

# ---- 1. Paths --------------------------------------------------------------

# Works from the repo root (Rscript, source) and from inside analysis/.
analysis_folder <- if (file.exists("skill_map.csv")) "." else "analysis"
fake_root <- file.path(analysis_folder, "data", "fake")
fake_research <- file.path(fake_root, "research")
fake_practice <- file.path(fake_research, "practice")
# Pilot IDs P800-P899 log to research/pilot/ (decision 023, ResearchIds.FolderFor).
fake_pilot <- file.path(fake_research, "pilot")

# Start clean so a rerun never mixes old and new files.
unlink(fake_root, recursive = TRUE)
dir.create(fake_practice, recursive = TRUE, showWarnings = FALSE)
dir.create(fake_pilot, recursive = TRUE, showWarnings = FALSE)

# ---- 2. JSON writing (same text the C# logger writes) -----------------------

# A JSON object keeps its keys in the order given; NULL values become null.
json_obj <- function(...) structure(list(...), class = "json_obj")
# A JSON array, e.g. json_arr(c("focus_lost")) -> ["focus_lost"].
json_arr <- function(x) structure(as.list(x), class = "json_arr")

# Numbers: at most 3 decimals, no trailing zeros, never scientific notation
# (the logger rounds t and durations to 3 decimals and prints 8.86, 0, 72).
fmt_num <- function(x) {
  x <- round(x, 3)
  if (x == 0) return("0")
  format(x, scientific = FALSE, digits = 15, trim = TRUE)
}

# Escape the characters JSON needs escaped (same set as JsonFields.Escape).
json_escape <- function(s) {
  s <- gsub("\\", "\\\\", s, fixed = TRUE)
  s <- gsub("\"", "\\\"", s, fixed = TRUE)
  s <- gsub("\n", "\\n", s, fixed = TRUE)
  s <- gsub("\r", "\\r", s, fixed = TRUE)
  gsub("\t", "\\t", s, fixed = TRUE)
}

to_json <- function(x) {
  if (is.null(x)) return("null")
  if (inherits(x, "json_obj")) {
    if (length(x) == 0) return("{}")
    keys <- names(x)
    parts <- vapply(seq_along(x), function(i) {
      paste0("\"", keys[i], "\":", to_json(x[[i]]))
    }, character(1))
    return(paste0("{", paste(parts, collapse = ","), "}"))
  }
  if (inherits(x, "json_arr")) {
    parts <- vapply(unclass(x), to_json, character(1))
    return(paste0("[", paste(parts, collapse = ","), "]"))
  }
  if (length(x) != 1) stop("to_json: expected one value, got ", length(x))
  if (is.na(x)) return("null")
  if (is.logical(x)) return(if (x) "true" else "false")
  if (is.numeric(x)) return(fmt_num(x))
  paste0("\"", json_escape(x), "\"")
}

# UTC time as the logger writes it: ISO 8601, milliseconds, "Z".
fmt_utc <- function(start_epoch, t) {
  ms_total <- round((start_epoch + t) * 1000)
  secs <- ms_total %/% 1000
  stamp <- format(as.POSIXct(secs, origin = "1970-01-01", tz = "UTC"),
                  "%Y-%m-%dT%H:%M:%S", tz = "UTC")
  paste0(stamp, sprintf(".%03dZ", as.integer(ms_total %% 1000)))
}

to_epoch <- function(utc_text) {
  as.numeric(as.POSIXct(utc_text, format = "%Y-%m-%dT%H:%M:%SZ", tz = "UTC"))
}

# Random version-4 UUID in "D" format, like Guid.NewGuid().ToString("D").
make_uuid <- function() {
  h <- sample(c(0:9, letters[1:6]), 32, replace = TRUE)
  h[13] <- "4"
  h[17] <- sample(c("8", "9", "a", "b"), 1)
  paste0(paste(h[1:8], collapse = ""), "-", paste(h[9:12], collapse = ""), "-",
         paste(h[13:16], collapse = ""), "-", paste(h[17:20], collapse = ""), "-",
         paste(h[21:32], collapse = ""))
}

# One event line with the common fields in the logger's order.
event_line <- function(t, utc, pid, sid, scene, visit, event, data) {
  to_json(json_obj(t = t, utc = utc, participant_id = pid, session_id = sid,
                   scene = scene, scene_visit = visit, event = event,
                   data = if (is.null(data)) json_obj() else data))
}

# ---- 3. Summary CSV (same columns and formats as SummaryCsv.cs) -------------

# Schema 2 (spec section 5): confirm_ignored after confirm_count, no dwell columns.
summary_header <- paste(
  "participant_id,session_id,build,schema_version,scene,scene_visit,enter_utc,duration_s,paused_s",
  "confirm_count,confirm_ignored,first_attempt_correct,attempts_to_correct,time_to_first_confirm_s",
  "time_to_correct_s,completed,error_codes_all,grabs,drops_floor,container_enters,container_exits",
  "pauses,hmd_removed_count,fps_avg,dropped_frames,unity_errors",
  sep = ",")

# Legacy schema 1 columns (only for P115, the mixed-schema test).
summary_header_v1 <- paste(
  "participant_id,session_id,build,schema_version,scene,scene_visit,enter_utc,duration_s,paused_s",
  "confirm_count,first_attempt_correct,attempts_to_correct,time_to_first_confirm_s,time_to_correct_s",
  "completed,error_codes_all,grabs,drops_floor,container_enters,container_exits,dwell_wms_s",
  "dwell_instructions_s,pauses,hmd_removed_count,fps_avg,dropped_frames,unity_errors",
  sep = ",")

# Empty cell = not measured (NA in R). Never write 0 for "not measured".
cell <- function(x) {
  if (is.null(x) || length(x) == 0 || is.na(x)) return("")
  if (is.logical(x)) return(if (x) "true" else "false")
  if (is.numeric(x)) return(fmt_num(x))
  as.character(x)
}

# ---- 4. Course content: orders, skills, learning ---------------------------

scene_order <- c("WelcomeScene", "ConceptScene", "BasicPickingScene",
                 "MixedPickingScene", "ComplexScene")
picking_scenes <- c("BasicPickingScene", "MixedPickingScene", "ComplexScene")

# One order position: material key, Soll (pieces), VPE (pieces per carton).
# required_full = full cartons, required_loose = loose pieces. Only ComplexScene
# counts loose pieces; elsewhere required_loose stays NULL (null in the log,
# "doesn't apply", never 0).
make_position <- function(item, soll, vpe, counts_loose = FALSE) {
  list(item = item, soll = soll, vpe = vpe, required_full = soll %/% vpe,
       required_loose = if (counts_loose) soll %% vpe else NULL)
}

# The orders of the current scenes (spec 4.2; decisions 012, 016, #67).
order_positions <- list(
  BasicPickingScene = list(make_position("training_b", 300, 50)),
  MixedPickingScene = list(make_position("connector_a", 150, 50),
                           make_position("screw_b", 60, 20)),
  ComplexScene      = list(make_position("ring_d", 55, 25, counts_loose = TRUE),
                           make_position("training_b", 156, 50, counts_loose = TRUE)))

# The "line" object of confirm_pressed: the current position only, with what
# lies in the box. placed_loose is NULL wherever required_loose is NULL.
line_obj <- function(pos, placed_full, placed_loose) {
  json_obj(item = pos$item, soll = pos$soll, vpe = pos$vpe,
           required_full = pos$required_full, required_loose = pos$required_loose,
           placed_full = placed_full,
           placed_loose = if (is.null(pos$required_loose)) NULL else placed_loose)
}

# Skills a red-button press can fail on, per picking scene. Same skills as
# scene_skills.csv, minus check_material: no scene has foreign material in its
# storage box (D12, #71), so WRONG_MATERIAL is never drawn.
picking_skill_list <- list(
  BasicPickingScene = c("calculate_quantity", "ekz_placement"),
  MixedPickingScene = "calculate_quantity",
  ComplexScene      = c("calculate_quantity", "calculate_loose"))

# Quiz questions: answer keys, the right key, and the skill (same content as
# item_skills.csv). Answer keys are always strings, also the numbers.
concept_questions <- list(
  cartons_from_soll = list(keys = c("2", "4", "25"), right = "4", skill = "calculate_quantity"),
  material_by_matnr = list(keys = c("connector_a", "screw_b", "ring_d"), right = "screw_b",
                           skill = "check_material"),
  ekz_003           = list(keys = c("ordered", "poured"), right = "ordered", skill = "ekz_placement"),
  ekz_000           = list(keys = c("ordered", "poured"), right = "poured", skill = "ekz_placement"))
remainder_questions <- list(
  cartons = list(keys = c("1", "2", "3"), right = "2", skill = "calculate_quantity"),
  loose   = list(keys = c("5", "20", "55"), right = "5", skill = "calculate_loose"))

# The 11 steps of the Concept field tour (WmsFieldTour; keys concept.tour.<id>).
# The quiz opens at the last one, material_flow.
tour_steps <- c("intro", "material", "verp_einh", "soll", "cartons", "ist", "rest",
                "empf_kz_003", "empf_kz_000", "not_used", "material_flow")

# Chance of an error on attempt k: lower for able trainees, for later attempts
# at the same item, for skills practised before, and for a scaffolded item
# (Complex position 1: the remainder quiz hands over the numbers). This gives
# the fake data a visible "improvement" signal to test the charts with.
skill_base_logit <- c(calculate_quantity = 0.2, check_material = 0.0,
                      calculate_loose = 0.4, ekz_placement = -0.3)

p_error <- function(person, skill, k, scaffolded = FALSE) {
  practised <- person$seen[[skill]]
  if (is.null(practised)) practised <- 0
  help <- if (scaffolded) 1.0 else 0
  plogis(skill_base_logit[[skill]] - person$ability - 0.9 * (k - 1) - 0.6 * practised - help)
}

# One more practised round of a skill (used after a question / position).
practise <- function(person, skill) {
  person$seen[[skill]] <- c(person$seen[[skill]], 0)[1] + 1
}

err <- function(code, item = NULL) list(code = code, item = item)

# One rated press at position pos_no: what lies in the box and which error
# codes it gets. k = attempt at this position. force_wrong makes sure the
# press is wrong (for "gave up"/crash cases); force_right is the safety stop.
draw_attempt <- function(scene, pos_no, person, k, force_wrong = FALSE, force_right = FALSE) {
  pos <- order_positions[[scene]][[pos_no]]
  full <- pos$required_full
  loose <- pos$required_loose  # NULL outside ComplexScene
  codes <- list()
  scaffolded <- scene == "ComplexScene" && pos_no == 1

  if (!force_right && runif(1) < 0.03) {
    # EMPTY is logged alone, without an extra TOO_FEW (spec 4.3).
    full <- 0
    if (!is.null(loose)) loose <- 0
    codes <- list(err("EMPTY"))
  } else if (!force_right) {
    skills <- picking_skill_list[[scene]]
    wrong <- vapply(skills, function(sk) runif(1) < p_error(person, sk, k, scaffolded), logical(1))
    if (force_wrong && !any(wrong)) wrong[1] <- TRUE

    # Codes in OrderRule order: quantity, loose pieces, then (Basic) EKZ 3.
    if (wrong["calculate_quantity"]) {
      delta <- sample(c(-2, -1, 1, 2), 1)
      full <- full + delta
      codes <- c(codes, list(err(if (delta < 0) "TOO_FEW" else "TOO_MANY", pos$item)))
    }
    if (isTRUE(wrong["calculate_loose"])) {
      delta <- sample(c(-3, -2, -1, 1, 2), 1)
      loose <- loose + delta
      codes <- c(codes, list(err(if (delta < 0) "MISSING_LOOSE" else "EXTRA_LOOSE", pos$item)))
    }
    # EKZ_PLACEMENT only when the 6 cartons are there but one lies loose.
    if (isTRUE(wrong["ekz_placement"]) && !wrong["calculate_quantity"]) {
      codes <- c(codes, list(err("EKZ_PLACEMENT")))
    }
    # Nothing left in the box after the deltas: that is EMPTY, alone.
    if (full == 0 && (is.null(loose) || loose == 0)) codes <- list(err("EMPTY"))
  }

  list(pos = pos, placed_full = full, placed_loose = loose, codes = codes)
}

# Error texts as the logger would capture them (paths cut to the file name).
error_stack <- list(
  WelcomeScene      = "SceneMenuController:LoadConcept () (at SceneMenuController.cs:21)",
  ConceptScene      = "PackageQuizController:CheckAnswer () (at PackageQuizController.cs:41)",
  BasicPickingScene = "OrderConfirm:OrderConfirmation () (at OrderConfirm.cs:48)",
  MixedPickingScene = "MixedPickingValidator:ConfirmOrder () (at MixPickingValidator.cs:57)",
  ComplexScene      = "PiecesOrderValidator:ConfirmOrder () (at PiecesOrderValidator.cs:70)")
error_message <- "NullReferenceException: Object reference not set to an instance of an object"

# Same rules as ErrorFolder.cs: the same message within 5 s of its last
# written line is folded; at most 200 unity_error lines per session.
fold_window_s <- 5
error_cap <- 200

# ---- 5. Session state and event writers ------------------------------------

# The "device" holds every session in start order and the files that are
# still open (crashed), so the next session start can mark them.
device <- new.env()
device$sessions <- list()
device$open <- list()

new_person <- function(pid) {
  p <- new.env()
  p$pid <- pid
  p$ability <- rnorm(1, 0, 0.7)
  p$seen <- list()
  p
}

advance <- function(s, secs) {
  s$t <- round(s$t + secs, 3)
}

emit <- function(s, event, data = NULL) {
  visit <- if (s$visit_open) s$visit else NULL
  s$lines <- c(s$lines, event_line(s$t, fmt_utc(s$start, s$t), s$pid, s$sid,
                                   s$scene, visit, event, data))
  s$last_t <- s$t
}

# Part of a still-open pause that lies inside the current visit (a pause that
# spans a scene change is split by visit, like PauseTracker in the logger).
open_pause_part <- function(s) {
  if (!s$pause_open) return(0)
  round(s$t - max(s$pause_start, s$v$enter_t), 3)
}

# Active time in the current visit: wall time minus paused time.
active_s <- function(s) round(s$t - s$v$enter_t - s$v$paused_s - open_pause_part(s), 3)

# start_mode: "manual" (keypad, #27) or "auto" (ResearchSettings.autoStart).
# resume_scene / resume_accepted: the resume offer after a crash (#27).
# Nothing offered = both null; resume_accepted is null whenever resume_scene is.
# ended_warning: TRUE when the facilitator started despite the "already
# finished" warning ("Trotzdem starten"); the logger writes false otherwise.
# language: header language (#11), "DE" or "EN", fixed for the session. The
# keypad forces "DE" for study IDs; test and pilot IDs keep the chosen one. NULL =
# a log written before #11 (header language null).
# schema: 2 (current); 1 only for the legacy P115 file (section 7b).
# interaction: FALSE = a build before #29 (no "interaction" in event_groups,
# no interaction events, via null after the first scene, empty summary columns).
start_session <- function(person, build, start_utc, start_mode = "manual",
                          resume_scene = NULL, resume_accepted = NULL, ended_warning = FALSE,
                          language = "DE", schema = 2, interaction = TRUE) {
  if (is.null(resume_scene)) resume_accepted <- NULL
  s <- new.env()
  s$pid <- person$pid
  s$sid <- make_uuid()
  s$build <- build
  s$schema <- schema
  s$interaction <- schema == 2 && interaction
  s$language <- language
  # Pilot IDs log to research/pilot/; everything else here to research/ (P950,
  # a test ID, sits in research/ on purpose, see section 7).
  s$folder <- if (grepl("^P8[0-9]{2}$", s$pid)) fake_pilot else fake_research
  s$start <- to_epoch(start_utc)
  s$t <- 0
  s$last_t <- 0
  s$lines <- character(0)
  s$summary <- character(0)
  s$visits <- list()
  s$scene <- NULL
  s$visit <- NULL
  s$visit_open <- FALSE
  # Error folding state (session-wide, like ErrorFolder): last written t and
  # repeats not yet written, per message.
  s$err_last <- list()
  s$err_pending <- list()
  s$err_lines <- 0
  s$cap_reported <- FALSE
  # Pause state (session-wide; a pause may span a scene change).
  s$pause_open <- FALSE
  s$pause_start <- NA
  s$pause_causes <- character(0)

  # Like SessionFiles.MarkCrashedSessions: before the new header, every open
  # file on the device (any participant) gets a crash_recovered line.
  for (old in device$open) {
    old$lines <- c(old$lines, event_line(
      NULL, fmt_utc(s$start, 0), old$pid, old$sid, NULL, NULL, "crash_recovered",
      json_obj(last_event_t = old$last_t, recovered_by_session_id = s$sid)))
  }
  device$open <- list()
  device$sessions <- c(device$sessions, list(s))

  # Header and session_start: t = 0, scene_visit null. Schema 2 adds
  # layout_version (three-lane station, decision 019) and event_groups (the
  # groups this build emits; R reads a missing group as NA, not 0).
  header <- if (schema == 2) {
    json_obj(schema_version = 2, layout_version = "019",
             event_groups = json_arr(c("session", "outcome", if (s$interaction) "interaction")),
             app_version = "0.2.0", build = build,
             device_model = "Oculus Quest", os_version = "Android OS 14 / API-34",
             participant_id = s$pid, session_id = s$sid, session_start_mode = start_mode,
             language = s$language,
             input_mode_at_start = "controllers", target_refresh_rate = 72)
  } else {
    json_obj(schema_version = 1, app_version = "0.1.0", build = build,
             device_model = "Oculus Quest", os_version = "Android OS 14 / API-34",
             participant_id = s$pid, session_id = s$sid, session_start_mode = start_mode,
             language = s$language,
             input_mode_at_start = "controllers", target_refresh_rate = 72)
  }
  s$lines <- event_line(0, fmt_utc(s$start, 0), s$pid, s$sid, NULL, NULL, "header", header)
  s$scene <- "WelcomeScene"
  emit(s, "session_start", json_obj(resume_offered_scene = resume_scene,
                                    resume_accepted = resume_accepted,
                                    ended_session_warning = ended_warning))
  advance(s, 0.006)
  enter_scene(s, "WelcomeScene", NULL, "start")
  s
}

enter_scene <- function(s, scene, from, via) {
  n <- if (is.null(s$visits[[scene]])) 1 else s$visits[[scene]] + 1
  s$visits[[scene]] <- n
  s$scene <- scene
  s$visit <- n
  s$visit_open <- TRUE
  # attempts = rated attempts that summary.csv counts in confirm_count (red
  # button presses in picking scenes, quiz touches in ConceptScene); codes =
  # the error codes of exactly those attempts; ignored = confirm_ignored;
  # quiz_attempts = quiz touches (attempt_no of quiz_answer).
  s$v <- list(enter_t = s$t, enter_utc = fmt_utc(s$start, s$t), paused_s = 0,
              pauses = 0, hmd = 0, errors = 0, attempts = 0, ignored = 0,
              quiz_attempts = 0,
              codes = character(0), first_correct = NA, correct_at = NA,
              t_first = NA, t_correct = NA,
              # #29: grab_start count, container changes, open grabs (object_id
              # -> active time at grab_start), packages in the order container
              # (one row per package: object_id, material, position, form) and
              # how many packages of each kind were taken so far (for new ids).
              grabs = 0, c_enter = 0, c_exit = 0, grab_open = list(),
              box = data.frame(obj = character(0), item = character(0), pos = integer(0),
                               form = character(0), stringsAsFactors = FALSE),
              taken = list())
  emit(s, "scene_enter", json_obj(from_scene = from, via = via))
}

# Error code tally for summary.csv, e.g. "TOO_FEW:2;EMPTY:1".
codes_tally <- function(codes) {
  if (length(codes) == 0) return("")
  tab <- table(codes)
  paste0(names(tab), ":", as.integer(tab), collapse = ";")
}

exit_scene <- function(s) {
  if (!s$visit_open) return(invisible())
  # Folded repeats not yet written belong to this visit: one line per message,
  # no stack, n_errors = folded_count (no new error comes with the line).
  drained <- FALSE
  for (msg in names(s$err_pending)) {
    n <- s$err_pending[[msg]]
    if (n > 0) {
      emit(s, "unity_error", json_obj(type = "exception", message = msg, stack = NULL,
                                      folded_count = n, n_errors = n))
      s$err_pending[[msg]] <- 0
      drained <- TRUE
    }
  }
  if (drained) advance(s, 0.001)
  duration <- active_s(s)
  # A pause still open at the exit counts up to now (quit while paused, or a
  # scene change during a pause).
  paused <- round(s$v$paused_s + open_pause_part(s), 3)
  s$v$paused_s <- paused
  emit(s, "scene_exit", json_obj(duration_s = duration, paused_s = paused,
                                 pauses = s$v$pauses, hmd_removed_count = s$v$hmd,
                                 unity_errors = s$v$errors))

  # Outcome columns (#28) only where the scene has a confirm or quiz;
  # confirm_ignored only where there is a red button (picking scenes);
  # #29 grabs where the scene has packages with PackageData (not Welcome:
  # its practice box has none), container counts where there is an order
  # container (picking scenes); both empty in a build before #29.
  # drops_floor, fps_avg and dropped_frames stay empty (decision 022: never
  # measured, never 0).
  v <- s$v
  has_outcome <- s$scene != "WelcomeScene"
  outcome <- function(x) if (has_outcome) x else ""
  ignored <- if (s$scene %in% picking_scenes) cell(v$ignored) else ""
  grabs <- if (s$interaction && s$scene %in% grab_scenes) cell(v$grabs) else ""
  has_box <- s$interaction && s$scene %in% picking_scenes
  c_enter <- if (has_box) cell(v$c_enter) else ""
  c_exit <- if (has_box) cell(v$c_exit) else ""
  identity <- c(s$pid, s$sid, s$build, as.character(s$schema), s$scene, cell(s$visit),
                v$enter_utc, cell(duration), cell(v$paused_s))
  results <- c(outcome(cell(v$first_correct)), outcome(cell(v$correct_at)),
               outcome(cell(v$t_first)), outcome(cell(v$t_correct)),
               outcome(cell(!is.na(v$correct_at))), outcome(codes_tally(v$codes)))
  row <- if (s$schema == 2) {
    c(identity, outcome(cell(v$attempts)), ignored, results,
      grabs, "", c_enter, c_exit,           # grabs, drops_floor (#74), container_* (#29)
      cell(v$pauses), cell(v$hmd), "", "", cell(v$errors))
  } else {
    # Legacy schema 1: no confirm_ignored, two (empty) dwell columns.
    c(identity, outcome(cell(v$attempts)), results,
      "", "", "", "", "", "",
      cell(v$pauses), cell(v$hmd), "", "", cell(v$errors))
  }
  s$summary <- c(s$summary, paste(row, collapse = ","))
  s$visit_open <- FALSE
}

# via: "menu" (normal), "resume" (the scene the keypad offered after a crash).
# Schema 1 and builds before #29 wrote null for every entry after the first one.
# A menu change writes menu_select (blocked false) in the visit being left,
# then the scene loads (SceneMenuController.OpenFromMenu). Now and then a
# research trainee first touches a locked item (blocked true, no load).
go_to_scene <- function(s, scene, via = "menu") {
  from <- s$scene
  if (s$interaction && identical(via, "menu")) {
    if (side_chance(0.12)) {
      emit(s, "menu_select", json_obj(target_scene = side_pick(setdiff(scene_order, c(scene, from))),
                                      blocked = TRUE))
      advance(s, round(side_runif(1, 3), 3))
    }
    emit(s, "menu_select", json_obj(target_scene = scene, blocked = FALSE))
    advance(s, round(side_runif(0.3, 0.9), 3))  # the scene loads
  }
  exit_scene(s)
  advance(s, 0.001)
  if (s$schema == 1 || !s$interaction) via <- NULL
  enter_scene(s, scene, from, via)
}

end_session <- function(s, reason) {
  exit_scene(s)
  advance(s, 0.001)
  emit(s, "session_end", json_obj(reason = reason))
}

# A crash: nothing more is written; the file stays without session_end.
crash_session <- function(s) {
  device$open <- c(device$open, list(s))
}

# A pause starts: it counts in the current visit (also when it ends in a later
# one). all_causes = every cause seen until the resume.
begin_pause <- function(s, causes, all_causes = causes) {
  emit(s, "app_pause", json_obj(causes = json_arr(causes)))
  s$pause_open <- TRUE
  s$pause_start <- s$t
  s$pause_causes <- all_causes
  s$v$pauses <- s$v$pauses + 1
  if ("hmd_unmounted" %in% all_causes) s$v$hmd <- s$v$hmd + 1
}

# The pause ends: app_resume carries the whole pause; the current visit gets
# only its own part.
end_pause <- function(s) {
  emit(s, "app_resume", json_obj(paused_s = round(s$t - s$pause_start, 3),
                                 causes = json_arr(s$pause_causes)))
  s$v$paused_s <- round(s$v$paused_s + open_pause_part(s), 3)
  s$pause_open <- FALSE
}

# One pause from start to resume. resume_causes = every cause seen meanwhile.
pause_app <- function(s, causes, secs, resume_causes = causes) {
  begin_pause(s, causes, resume_causes)
  advance(s, round(secs, 3))
  end_pause(s)
}

maybe_pause <- function(s) {
  if (runif(1) < 0.15) {
    if (runif(1) < 0.5) {
      pause_app(s, "focus_lost", runif(1, 3, 20))
    } else {
      # Taking the headset off often also drops focus while paused.
      resume <- if (runif(1) < 0.5) c("hmd_unmounted", "focus_lost") else "hmd_unmounted"
      pause_app(s, "hmd_unmounted", runif(1, 10, 90), resume)
    }
    advance(s, runif(1, 2, 8))
  }
}

# One error raised now, decided like ErrorFolder.Decide: after the cap only
# counted; within 5 s of the message's last line folded; otherwise a line with
# folded_count = repeats swallowed since that line, n_errors = folded_count + 1.
raise_error <- function(s, message = error_message, stack = error_stack[[s$scene]]) {
  s$v$errors <- s$v$errors + 1
  if (s$err_lines >= error_cap) {
    if (!s$cap_reported) {
      s$cap_reported <- TRUE
      emit(s, "unity_error_cap_reached")
    }
    return(invisible())
  }
  last <- s$err_last[[message]]
  if (!is.null(last) && s$t - last < fold_window_s) {
    s$err_pending[[message]] <- s$err_pending[[message]] + 1
    return(invisible())
  }
  folded <- if (is.null(s$err_pending[[message]])) 0 else s$err_pending[[message]]
  emit(s, "unity_error", json_obj(type = "exception", message = message, stack = stack,
                                  folded_count = folded, n_errors = folded + 1))
  s$err_last[[message]] <- s$t
  s$err_pending[[message]] <- 0
  s$err_lines <- s$err_lines + 1
}

# n repeats of the same error, one every `spacing` seconds. A burst longer
# than 5 s writes a second line (folded_count + 1 errors) in the middle.
error_burst <- function(s, n, spacing, message = error_message) {
  for (i in seq_len(n)) {
    advance(s, spacing)
    raise_error(s, message)
  }
}

maybe_unity_error <- function(s) {
  if (runif(1) < 0.08) {
    raise_error(s)
    if (runif(1) < 0.5) error_burst(s, sample(3:40, 1), round(runif(1, 0.05, 0.3), 3))
    advance(s, runif(1, 1, 5))
  }
}

# ---- 5b. Outcome events (#28 step 2, spec 4.2 and section 6) ---------------

codes_json <- function(codes) {
  json_arr(lapply(codes, function(e) json_obj(code = e$code, item = e$item)))
}

# Bookkeeping for one rated attempt that summary.csv counts in confirm_count
# (picking scenes: a rated red-button press; ConceptScene: a quiz touch). The
# first one sets first_attempt_correct and time_to_first_confirm_s.
note_attempt <- function(s, correct, codes, at) {
  s$v$attempts <- s$v$attempts + 1
  if (s$v$attempts == 1) {
    s$v$first_correct <- correct
    s$v$t_first <- at
  }
  s$v$codes <- c(s$v$codes, vapply(codes, function(e) e$code, character(1)))
  s$v$last_at <- at
}

# task_completed: the whole task of the visit is done (picking scenes: the
# last position is correct, OrderCompleted; Concept: all 4 questions right).
# attempts_to_correct = every rated attempt of the visit so far.
complete_task <- function(s) {
  s$v$correct_at <- s$v$attempts
  s$v$t_correct <- s$v$last_at
  advance(s, 0.001)
  emit(s, "task_completed", json_obj(attempts_to_correct = s$v$attempts,
                                     time_to_first_confirm_s = s$v$t_first,
                                     time_to_correct_s = s$v$t_correct))
}

# One rated red-button press at position pos_no (k = attempt at this
# position). correct is normally "no error codes" (the invariant). Passing
# correct explicitly lets one fake press break the invariant on purpose (P110).
# No scene has foreign material, so "foreign" is always [].
press_confirm <- function(s, pos_no, k, attempt, correct = length(attempt$codes) == 0) {
  at <- active_s(s)
  note_attempt(s, correct, attempt$codes, at)
  emit(s, "confirm_pressed", json_obj(
    attempt_no = s$v$attempts, position = pos_no,
    position_count = length(order_positions[[s$scene]]), attempt_in_position = k,
    line = line_obj(attempt$pos, attempt$placed_full, attempt$placed_loose),
    foreign = json_arr(list()), correct = correct,
    error_codes = codes_json(attempt$codes), time_since_scene_enter_s = at))
  correct
}

# A red-button press the validator ignores (early return): not an attempt,
# not in confirm_count, only counted in confirm_ignored.
ignore_confirm <- function(s, reason, pos_no) {
  s$v$ignored <- s$v$ignored + 1
  emit(s, "confirm_ignored", json_obj(reason = reason, position = pos_no,
                                      time_since_scene_enter_s = active_s(s)))
}

# One rated quiz touch. Only ConceptScene counts quiz touches in summary.csv
# (its confirm_count); in ComplexScene the summary counts red-button presses
# only, the remainder quiz lives in the events. WRONG_ANSWER has item null.
answer_quiz <- function(s, quiz, question_id, selected, k, right) {
  correct <- selected == right
  codes <- if (correct) list() else list(err("WRONG_ANSWER"))
  at <- active_s(s)
  s$v$quiz_attempts <- s$v$quiz_attempts + 1
  if (s$scene == "ConceptScene") note_attempt(s, correct, codes, at)
  emit(s, "quiz_answer", json_obj(
    attempt_no = s$v$quiz_attempts, quiz = quiz, question_id = question_id,
    selected = selected, correct = correct, attempt_in_question = k,
    error_codes = codes_json(codes), time_since_scene_enter_s = at))
  correct
}

# One question, touched until right (a touched toggle is rated at once,
# decision 020). After 4 wrong touches the trainee finds the right one.
ask_question <- function(s, person, quiz, question_id, q) {
  k <- 0
  repeat {
    k <- k + 1
    wrong <- k < 5 && runif(1) < p_error(person, q$skill, k)
    selected <- if (wrong) sample(setdiff(q$keys, q$right), 1) else q$right
    if (answer_quiz(s, quiz, question_id, selected, k, q$right)) break
    advance(s, runif(1, 3, 10))  # read the feedback line, touch again
  }
  practise(person, q$skill)
}

# ---- 5b2. Interaction events (#29, spec 4.4) -------------------------------

# Scenes whose packages carry PackageData (grab_start/_end are logged there).
# WelcomeScene's practice box has no PackageData: no grab events, grabs empty.
grab_scenes <- c("ConceptScene", picking_scenes)

# Stable object_id (hierarchy path of the PackageData GameObject, like
# ResearchObjectId.cs) of the n-th package of a kind taken in a visit. Names
# follow the scenes (Basic "GG_Package (18)/SM_box_1", Mixed
# "Interactables/GG_Package (2)", Complex "Interactables/ring (3)"); the
# Complex carton names are made up.
object_id <- function(scene, item, form, n) {
  if (scene == "BasicPickingScene") return(sprintf("GG_Package (%d)/SM_box_1", 17 + n))
  if (scene == "MixedPickingScene" && item == "connector_a") {
    return(if (n == 1) "Interactables/GG_Package" else sprintf("Interactables/GG_Package (%d)", n - 1))
  }
  if (scene == "MixedPickingScene") return(sprintf("Interactables/PlasticBag_GG_%d", n))
  if (item == "ring_d" && form == "loose") {
    return(if (n == 1) "Interactables/ring" else sprintf("Interactables/ring (%d)", n - 1))
  }
  if (item == "ring_d") return(sprintf("Interactables/RingD_Carton (%d)", n))
  if (form == "loose") return(if (n == 1) "Interactables/clamp" else sprintf("Interactables/clamp (%d)", n - 1))
  sprintf("Interactables/SB_Carton (%d)", n)
}

# A new package of this kind (never one already in the box).
next_object <- function(s, item, form) {
  key <- paste(item, form)
  n <- if (is.null(s$v$taken[[key]])) 1 else s$v$taken[[key]] + 1
  s$v$taken[[key]] <- n
  object_id(s$scene, item, form, n)
}

grab_start <- function(s, obj, item) {
  force(obj)  # evaluate a lazy argument before s$v is replaced below
  s$v$grabs <- s$v$grabs + 1
  s$v$grab_open[[obj]] <- active_s(s)
  emit(s, "grab_start", json_obj(object_id = obj, material_key = item, hand = NULL, input = NULL))
}

# duration_s = active time since this object's grab_start in the visit.
grab_end <- function(s, obj, item) {
  start <- s$v$grab_open[[obj]]
  s$v$grab_open[[obj]] <- NULL
  dur <- if (is.null(start)) NULL else round(active_s(s) - start, 3)
  emit(s, "grab_end", json_obj(object_id = obj, material_key = item, hand = NULL, input = NULL,
                               duration_s = dur))
}

# The order container's set changed. count_after = every package in it now,
# finished positions included (the counter's whole set, like the C# counters).
container_change <- function(s, entered, obj, item, pos, form) {
  if (entered) {
    s$v$box <- rbind(s$v$box, data.frame(obj = obj, item = item, pos = pos, form = form,
                                         stringsAsFactors = FALSE))
    s$v$c_enter <- s$v$c_enter + 1
  } else {
    s$v$box <- s$v$box[s$v$box$obj != obj, , drop = FALSE]
    s$v$c_exit <- s$v$c_exit + 1
  }
  emit(s, if (entered) "container_enter" else "container_exit",
       json_obj(container = "order", object_id = obj, material_key = item,
                count_after = nrow(s$v$box)))
}

# Grab a package in the storage box, carry it into the order container (the
# trigger sees it while it is still held), let go.
put_in <- function(s, item, pos, form) {
  obj <- next_object(s, item, form)
  advance(s, round(side_runif(1, 3), 3))      # find the package
  grab_start(s, obj, item)
  advance(s, round(side_runif(0.8, 2.5), 3))  # carry it over
  container_change(s, TRUE, obj, item, pos, form)
  advance(s, round(side_runif(0.2, 0.8), 3))
  grab_end(s, obj, item)
}

# Take the last package of this kind out of the order container again.
take_out <- function(s, item, pos, form) {
  rows <- which(s$v$box$item == item & s$v$box$pos == pos & s$v$box$form == form)
  obj <- s$v$box$obj[rows[length(rows)]]
  advance(s, round(side_runif(1, 3), 3))
  grab_start(s, obj, item)
  advance(s, round(side_runif(0.3, 1.2), 3))
  container_change(s, FALSE, obj, item, pos, form)
  advance(s, round(side_runif(0.5, 1.5), 3))
  grab_end(s, obj, item)
}

# Picked up and put back where it was (hesitation, a re-grab): no container change.
pick_and_drop <- function(s, obj, item) {
  advance(s, round(side_runif(1, 3), 3))
  grab_start(s, obj, item)
  advance(s, round(side_runif(0.5, 2.5), 3))
  grab_end(s, obj, item)
}

# Before a rated press: bring the current position's packages in the order
# container to what the press reports (placed_full cartons, placed_loose
# pieces). Packages of finished positions stay in the box untouched.
place_to <- function(s, pos_no, placed_full, placed_loose) {
  if (!s$interaction) return(invisible())
  item <- order_positions[[s$scene]][[pos_no]]$item
  # Sometimes a package is picked up and put back first (hesitation). It is
  # one the trainee did not take, so it gets an id of its own kind.
  # (The id is taken before the call: next_object changes s$v, so it must not
  # run lazily inside another replacement of s$v.)
  if (side_chance(0.2)) {
    wrong_one <- next_object(s, item, "full")
    pick_and_drop(s, wrong_one, item)
  }
  changed <- FALSE
  targets <- c(full = placed_full, loose = if (is.null(placed_loose)) 0 else placed_loose)
  for (form in names(targets)) {
    repeat {
      have <- sum(s$v$box$item == item & s$v$box$pos == pos_no & s$v$box$form == form)
      if (have == targets[[form]]) break
      changed <- TRUE
      if (have < targets[[form]]) put_in(s, item, pos_no, form) else take_out(s, item, pos_no, form)
    }
  }
  # Basic, same contents as before (after EKZ_PLACEMENT): a carton inside is
  # straightened, a grab without a container change.
  if (!changed && s$scene == "BasicPickingScene" && nrow(s$v$box) > 0) {
    last <- nrow(s$v$box)
    pick_and_drop(s, s$v$box$obj[last], s$v$box$item[last])
  }
  advance(s, round(side_runif(1, 4), 3))  # look at the box and the WMS, then press
}

# ---- 5c. Station rides (decision 019) --------------------------------------

# Wait for a ride (~3 s). The trainee may press the red button meanwhile: the
# validator ignores it (reason station_moving). press_position = the position
# the logic already shows (it changes at once, the boxes follow).
ride_wait <- function(s, press_position) {
  ride_s <- runif(1, 2.8, 3.3)
  if (runif(1) < 0.2) {
    early <- runif(1, 0.8, 2.0)
    advance(s, early)
    ignore_confirm(s, "station_moving", press_position)
    advance(s, ride_s - early)
  } else {
    advance(s, ride_s)
  }
}

# Between positions: the old storage box leaves (storage_out, the position
# just done), the next one comes in (storage_in, the new position).
ride_between_positions <- function(s, done_pos) {
  advance(s, 0.6)  # PositionStockSwitcher startDelay
  emit(s, "station_move_start", json_obj(what = "storage_out", position = done_pos))
  advance(s, 0.001)
  emit(s, "station_move_start", json_obj(what = "storage_in", position = done_pos + 1))
  ride_wait(s, done_pos + 1)
  emit(s, "station_move_arrived", json_obj(what = "storage_out", position = done_pos))
  advance(s, 0.001)
  emit(s, "station_move_arrived", json_obj(what = "storage_in", position = done_pos + 1))
}

# ComplexScene: storage box 1 only rides in after the remainder quiz is right.
ride_first_box_in <- function(s) {
  advance(s, 0.001)
  emit(s, "station_move_start", json_obj(what = "storage_in", position = 1))
  ride_wait(s, 1)
  emit(s, "station_move_arrived", json_obj(what = "storage_in", position = 1))
}

# Order done: the last storage box leaves, the order box follows once the
# storage box has cleared the station (OrderBoxConveyor.stationStock).
ride_order_end <- function(s, last_pos) {
  advance(s, 0.6)
  emit(s, "station_move_start", json_obj(what = "storage_out", position = last_pos))
  advance(s, 1.2)
  emit(s, "station_move_start", json_obj(what = "order_box", position = last_pos))
  advance(s, runif(1, 1.6, 2.1))
  emit(s, "station_move_arrived", json_obj(what = "storage_out", position = last_pos))
  advance(s, 1.2)
  emit(s, "station_move_arrived", json_obj(what = "order_box", position = last_pos))
}

# ---- 6. What happens inside each kind of scene -----------------------------

# practice = FALSE: a resumed session (the keypad jumps straight to the
# offered scene, so the Welcome practice is not done again).
run_welcome <- function(s, practice = TRUE) {
  advance(s, runif(1, 8, 20))
  if (practice && s$schema == 2) {
    # First grab of the box, then first red-button press (#39).
    emit(s, "welcome_practice_done", json_obj(action = "grab"))
    advance(s, runif(1, 4, 15))
    emit(s, "welcome_practice_done", json_obj(action = "button"))
    advance(s, runif(1, 3, 10))
  }
  maybe_unity_error(s)
}

# ConceptScene: 11 tour steps ("Weiter"), the quiz opens at material_flow,
# then the 4 questions in order, each touched until right.
run_concept <- function(s, person) {
  for (i in seq_along(tour_steps)) {
    advance(s, runif(1, 5, 15))  # read the step, press "Weiter"
    emit(s, "tour_step", json_obj(step_id = tour_steps[i], step_index = i))
    if (tour_steps[i] == "material_flow") {
      advance(s, 0.001)
      emit(s, "quiz_unlocked")
    }
    if (i == 5) maybe_pause(s)
  }
  # Some trainees press "Von vorn" and skim the first steps again (step_index
  # back to 1). The quiz stays open (it opens for good).
  if (runif(1) < 0.2) {
    for (i in 1:3) {
      advance(s, runif(1, 2, 6))
      emit(s, "tour_step", json_obj(step_id = tour_steps[i], step_index = i))
    }
  }
  maybe_unity_error(s)
  # #29: a few packages picked up at the label board (no container here).
  if (s$interaction) {
    for (k in seq_len(floor(side_runif(1, 5)))) {
      if (side_chance(0.5)) {
        pick_and_drop(s, sprintf("Interactables/VPE_OpenedPackage/Rings/ring (%d)", k), "ring_d")
      } else {
        pick_and_drop(s, sprintf("Interactables/GG_Package (%d)", k), "connector_a")
      }
    }
  }
  for (qid in names(concept_questions)) {
    advance(s, runif(1, 5, 15))  # read the question and the three answers
    ask_question(s, person, "concept", qid, concept_questions[[qid]])
  }
  complete_task(s)
  advance(s, runif(1, 3, 10))  # read the feedback, then press the menu
}

# ComplexScene before position 1: the remainder quiz (full cartons, loose
# pieces). Some trainees press the red button first (the #58 reminder,
# confirm_ignored "quiz_first"). Both right -> storage box 1 rides in.
run_remainder_quiz <- function(s, person) {
  if (runif(1) < 0.15) {
    advance(s, runif(1, 3, 10))
    ignore_confirm(s, "quiz_first", 1)
  }
  for (qid in names(remainder_questions)) {
    advance(s, runif(1, 5, 15))
    ask_question(s, person, "complex_remainder", qid, remainder_questions[[qid]])
  }
  ride_first_box_in(s)
  advance(s, runif(1, 20, 50))  # pick the cartons and loose rings of position 1
}

# A picking scene, one position after the other.
# max_confirms: stop after this many rated presses in the visit (0 = never
#   confirms; then the Complex remainder quiz is not answered either).
# give_up: every press is wrong (for quit/crash endings).
# forced: fixed presses for edge cases, by position, e.g.
#   list("2" = list(press1, press2)); a press = list(placed_full, placed_loose,
#   codes, correct).
# crash_after_position: stop right after this position's ride (for a crash).
run_picking <- function(s, person, max_confirms = 20, give_up = FALSE,
                        forced = list(), end_on_correct = FALSE,
                        double_cause_pause = FALSE, crash_after_position = NULL) {
  scene <- s$scene
  positions <- order_positions[[scene]]
  advance(s, runif(1, 30, 70))  # read the WMS order and pick packages
  if (double_cause_pause) {
    # Two causes in the same frame: headset off and focus lost together.
    pause_app(s, c("hmd_unmounted", "focus_lost"), runif(1, 20, 60))
    advance(s, runif(1, 2, 8))
  } else {
    maybe_pause(s)
  }
  maybe_unity_error(s)
  if (scene == "ComplexScene" && max_confirms > 0) run_remainder_quiz(s, person)

  for (pos_no in seq_along(positions)) {
    k <- 0
    pos_done <- FALSE
    fixed <- forced[[as.character(pos_no)]]
    while (s$v$attempts < max_confirms) {
      k <- k + 1
      if (k <= length(fixed)) {
        f <- fixed[[k]]
        attempt <- list(pos = positions[[pos_no]], placed_full = f$placed_full,
                        placed_loose = f$placed_loose, codes = f$codes)
        place_to(s, pos_no, attempt$placed_full, attempt$placed_loose)
        pos_done <- press_confirm(s, pos_no, k, attempt, f$correct)
      } else {
        attempt <- draw_attempt(scene, pos_no, person, k, force_wrong = give_up,
                                force_right = (k >= 8 && !give_up))
        place_to(s, pos_no, attempt$placed_full, attempt$placed_loose)
        pos_done <- press_confirm(s, pos_no, k, attempt)
      }
      if (pos_done) break
      advance(s, runif(1, 8, 30))  # read the feedback and fix the box
    }
    if (!pos_done) break  # gave up, or no presses left

    # Position correct: position_completed, then the boxes ride.
    advance(s, 0.001)
    emit(s, "position_completed", json_obj(position = pos_no, attempts_in_position = k))
    for (sk in picking_skill_list[[scene]]) practise(person, sk)

    if (pos_no < length(positions)) {
      ride_between_positions(s, pos_no)
      if (!is.null(crash_after_position) && crash_after_position == pos_no) {
        advance(s, runif(1, 5, 15))  # starts reading position 2, then the crash
        # Holding a position-2 package when the app dies: a grab_start
        # without grab_end (an open grab, censored).
        if (s$interaction) {
          item <- positions[[pos_no + 1]]$item
          grab_start(s, next_object(s, item, "full"), item)
          advance(s, 0.8)
        }
        return(invisible(FALSE))
      }
      advance(s, runif(1, 20, 50))  # read the new WMS line and pick
    } else {
      complete_task(s)  # OrderCompleted
      if (end_on_correct) return(invisible(TRUE))  # Complex: EndSession("completed")
      ride_order_end(s, pos_no)
      # Now and then a press after the end: ignored, reason "completed".
      if (runif(1) < 0.1) {
        advance(s, runif(1, 1, 3))
        ignore_confirm(s, "completed", pos_no)
      }
    }
  }
  advance(s, runif(1, 3, 10))
  invisible(!is.na(s$v$correct_at))
}

run_visit <- function(s, person, ...) {
  if (s$scene == "WelcomeScene") run_welcome(s)
  else if (s$scene == "ConceptScene") run_concept(s, person)
  else run_picking(s, person, ...)
}

# A normal course: every scene in build order, ends "completed" when the
# ComplexScene order is complete (#28 calls EndSession("completed") there).
run_full_course <- function(person, build, start_utc, special = list(), start_mode = "manual",
                            resume_scene = NULL, resume_accepted = NULL, ended_warning = FALSE,
                            language = "DE") {
  s <- start_session(person, build, start_utc, start_mode, resume_scene, resume_accepted, ended_warning,
                     language)
  run_welcome(s)
  for (scene in scene_order[-1]) {
    go_to_scene(s, scene)
    args <- c(list(s, person), special[[scene]])
    if (scene == "ComplexScene") args$end_on_correct <- TRUE
    do.call(run_visit, args)
  }
  end_session(s, "completed")
  invisible(s)
}

# A fixed press for run_picking(forced = ...).
fixed_press <- function(placed_full, codes, correct = length(codes) == 0, placed_loose = NULL) {
  list(placed_full = placed_full, placed_loose = placed_loose, codes = codes, correct = correct)
}

# ---- 7. The fake participants ----------------------------------------------

# P950: facilitator's trial run with a test ID. It belongs in practice/, but
# here it sits in research/ on purpose: analyse.R must drop it anyway. Test
# IDs keep the chosen language, so this one ran in English (#11).
p950 <- new_person("P950")
s <- start_session(p950, "aaa1111", "2026-11-02T09:00:00Z", start_mode = "auto", language = "EN")
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_concept(s, p950)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p950)
end_session(s, "quit")

# ---- 7b. Legacy v1 format for the mixed-schema test (P115 only) ------------
# P115 ran on an older build that still wrote schema_version 1 (before
# decision 021): header without layout_version/event_groups, confirm_pressed
# with container/required package-type counts (gg, vpe, gga, ggb, loose,
# other, total_pieces), quiz_answer without quiz/question_id, NO_ANSWER still
# possible, summary.csv with the two dwell columns. analyse.R must warn about
# mixed schema versions. Do not use this writer for anything else.

legacy_counts <- function(gg = NULL, vpe = NULL, gga = NULL, ggb = NULL, loose = NULL,
                          other = NULL, total_pieces = NULL) {
  json_obj(gg = gg, vpe = vpe, gga = gga, ggb = ggb, loose = loose, other = other,
           total_pieces = total_pieces)
}

run_legacy_v1_session <- function(person, build, start_utc) {
  s <- start_session(person, build, start_utc, schema = 1)
  run_welcome(s)  # schema 1 had no welcome_practice_done

  # v1 Concept quiz: one question confirmed with a button. A confirm without
  # a selection gave NO_ANSWER; task_completed came at the first right answer.
  go_to_scene(s, "ConceptScene")
  advance(s, runif(1, 20, 60))
  for (selected in list(NULL, "gg", "vpe")) {
    correct <- identical(selected, "vpe")
    codes <- if (is.null(selected)) list(err("NO_ANSWER"))
             else if (!correct) list(err("WRONG_ANSWER"))
             else list()
    note_attempt(s, correct, codes, active_s(s))
    emit(s, "quiz_answer", json_obj(attempt_no = s$v$attempts, selected = selected,
                                    correct = correct, error_codes = codes_json(codes)))
    if (correct) complete_task(s) else advance(s, runif(1, 4, 15))
  }
  advance(s, runif(1, 3, 10))

  # v1 BasicPickingScene: 6 GG cartons required; first press one short.
  go_to_scene(s, "BasicPickingScene")
  advance(s, runif(1, 30, 70))
  required <- legacy_counts(gg = 6, vpe = 0, other = 0, total_pieces = 300)
  presses <- list(
    list(box = legacy_counts(gg = 5, vpe = 0, other = 0, total_pieces = 250),
         codes = list(err("TOO_FEW", "gg"))),
    list(box = legacy_counts(gg = 6, vpe = 0, other = 0, total_pieces = 300),
         codes = list()))
  for (p in presses) {
    correct <- length(p$codes) == 0
    at <- active_s(s)
    note_attempt(s, correct, p$codes, at)
    emit(s, "confirm_pressed", json_obj(
      attempt_no = s$v$attempts, container = p$box, required = required,
      correct = correct, error_codes = codes_json(p$codes), time_since_scene_enter_s = at))
    if (correct) complete_task(s) else advance(s, runif(1, 8, 30))
  }
  advance(s, runif(1, 3, 10))
  end_session(s, "quit")
  invisible(s)
}

# P115: the legacy schema-1 session (an older build, before the v2 builds).
run_legacy_v1_session(new_person("P115"), "ccc3333", "2026-11-02T14:00:00Z")

# ---- 7c. Participants in the current format --------------------------------

# P101: normal course.
run_full_course(new_person("P101"), "aaa1111", "2026-11-03T08:00:00Z")

# P102: goes back from MixedPickingScene to BasicPickingScene once
# (repeat visits: scene_visit 2 for Basic and Mixed). In WelcomeScene one error
# repeats for 9 s (a second line after the 5 s window, the rest written at the
# visit close). The trainee leaves Mixed visit 1 while the app is paused
# (focus lost), so that pause is split between Mixed 1 and Basic 2.
p102 <- new_person("P102")
s <- start_session(p102, "aaa1111", "2026-11-03T09:30:00Z")
run_welcome(s)
raise_error(s)
error_burst(s, 60, 0.15)
advance(s, 2)
mixed_seen <- 0
for (scene in c("ConceptScene", "BasicPickingScene", "MixedPickingScene",
                "BasicPickingScene", "MixedPickingScene", "ComplexScene")) {
  go_to_scene(s, scene)
  if (s$pause_open) {
    advance(s, 7.25)
    end_pause(s)
  }
  if (scene == "ComplexScene") run_picking(s, p102, end_on_correct = TRUE)
  else run_visit(s, p102)
  if (scene == "MixedPickingScene") {
    mixed_seen <- mixed_seen + 1
    if (mixed_seen == 1) {
      begin_pause(s, "focus_lost")
      advance(s, 4.5)
    }
  }
}
end_session(s, "completed")

# P103: crashes in MixedPickingScene with a pause still open (headset off,
# then the app dies), then a second session on build bbb2222 resumes there.
p103 <- new_person("P103")
s <- start_session(p103, "aaa1111", "2026-11-03T11:00:00Z")
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_concept(s, p103)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p103)
go_to_scene(s, "MixedPickingScene"); run_picking(s, p103, max_confirms = 1, give_up = TRUE)
begin_pause(s, "hmd_unmounted")
crash_session(s)
crash_epoch <- s$start + s$t
restart_utc <- format(as.POSIXct(round(crash_epoch + 6 * 60), origin = "1970-01-01", tz = "UTC"),
                      "%Y-%m-%dT%H:%M:%SZ", tz = "UTC")
# Resume: the keypad offers MixedPickingScene and P103 accepts; the scene
# restarts from its beginning (design section 4, resume after a crash) and
# scene_enter.via is "resume". No Welcome practice in a resumed session.
s <- start_session(p103, "bbb2222", restart_utc,
                   resume_scene = "MixedPickingScene", resume_accepted = TRUE)
run_welcome(s, practice = FALSE)
go_to_scene(s, "MixedPickingScene", via = "resume"); run_picking(s, p103)
go_to_scene(s, "ComplexScene"); run_picking(s, p103, end_on_correct = TRUE)
end_session(s, "completed")

# P104: gives up in ComplexScene (remainder quiz done, two wrong confirms at
# position 1), takes the headset off and the app is closed while still paused
# (quit callback runs: reason "quit", no app_resume; scene_exit.paused_s
# includes the open pause). P104 ran a build before #29: event_groups without
# "interaction", no grab/container/menu events, via null after the first
# scene, empty grabs/container_* cells. analyse.R must give NA, not 0.
p104 <- new_person("P104")
s <- start_session(p104, "aaa1111", "2026-11-03T14:00:00Z", interaction = FALSE)
run_welcome(s)
for (scene in c("ConceptScene", "BasicPickingScene", "MixedPickingScene")) {
  go_to_scene(s, scene); run_visit(s, p104)
}
go_to_scene(s, "ComplexScene"); run_picking(s, p104, max_confirms = 2, give_up = TRUE)
begin_pause(s, "hmd_unmounted")
advance(s, 12.5)
end_session(s, "quit")

# P105: crashes in BasicPickingScene right after an exception and never comes
# back. The next session on the device (P106) marks the file crash_recovered.
# Its header has language null: a log from a build before #11 (allowed).
p105 <- new_person("P105")
s <- start_session(p105, "aaa1111", "2026-11-04T08:00:00Z", language = NULL)
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_concept(s, p105)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p105, max_confirms = 1, give_up = TRUE)
raise_error(s)
crash_session(s)

# P106: one pause whose app_pause lists two causes. Also the first press at
# Complex position 2 has 2 loose clamps too many (EXTRA_LOOSE, so every
# picking code appears in the fake data whatever the random draws give).
run_full_course(new_person("P106"), "aaa1111", "2026-11-04T09:30:00Z",
                special = list(MixedPickingScene = list(double_cause_pause = TRUE),
                               ComplexScene = list(forced = list("2" = list(
                                 fixed_press(3, list(err("EXTRA_LOOSE", "training_b")),
                                             placed_loose = 8))))))

# P107: enters ComplexScene but never confirms (and never answers the
# remainder quiz); the facilitator ends the session (#44 control, opened once
# and cancelled, then opened again) -> a visit with 0 confirms (error rate
# must be NA). In WelcomeScene a broken loop raises 200+ different errors: the
# error cap is reached, later errors (also in later scenes) are only counted
# in scene_exit. One repeat folded before the cap is still written at the
# visit close.
p107 <- new_person("P107")
s <- start_session(p107, "aaa1111", "2026-11-04T11:00:00Z")
run_welcome(s)
raise_error(s)
advance(s, 0.5)
raise_error(s)
for (k in 1:230) {
  advance(s, 0.01)
  raise_error(s, paste0("MissingReferenceException: probe ", k), "Probe:Update () (at Probe.cs:9)")
}
for (scene in c("ConceptScene", "BasicPickingScene", "MixedPickingScene")) {
  go_to_scene(s, scene); run_visit(s, p107)
}
go_to_scene(s, "ComplexScene"); run_picking(s, p107, max_confirms = 0)
emit(s, "end_control_opened")
advance(s, 2.5)
emit(s, "end_control_cancelled")
advance(s, 40)
emit(s, "end_control_opened")
advance(s, 1.5)
end_session(s, "facilitator")

# P108: normal course on the newer build bbb2222, but the study build still had
# autoStart on (session_start_mode "auto"): analyse.R must warn. autoStart
# skips the keypad, so the research-ID lock to "DE" did not apply and the
# session ran in the saved language "EN" (#11): analyse.R must warn too.
run_full_course(new_person("P108"), "bbb2222", "2026-11-04T13:00:00Z", start_mode = "auto",
                language = "EN")

# P109: the first Mixed press carries FOO_BAR, a code missing from skill_map.csv.
run_full_course(new_person("P109"), "aaa1111", "2026-11-04T14:30:00Z",
  special = list(MixedPickingScene = list(forced = list("1" = list(
    fixed_press(2, list(err("FOO_BAR", "connector_a"))))))))

# P110: the first Basic press says correct = true although it has an error
# code (the 6 cartons are there but one lies loose, and a faulty validator
# accepts it). Breaks the invariant on purpose; the order then completes.
run_full_course(new_person("P110"), "aaa1111", "2026-11-05T08:00:00Z",
  special = list(BasicPickingScene = list(forced = list("1" = list(
    fixed_press(6, list(err("EKZ_PLACEMENT")), correct = TRUE))))))

# P111: crashes in BasicPickingScene after one wrong confirm. On restart the
# keypad offers BasicPickingScene, but the resume is declined, so the trainee
# repeats the whole course from WelcomeScene (same build, a few minutes later):
# repeat visits after a crash that analyse.R must warn about.
p111 <- new_person("P111")
s <- start_session(p111, "aaa1111", "2026-11-05T09:30:00Z")
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_concept(s, p111)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p111, max_confirms = 1, give_up = TRUE)
crash_session(s)
restart_utc <- format(as.POSIXct(round(s$start + s$t + 4 * 60), origin = "1970-01-01", tz = "UTC"),
                      "%Y-%m-%dT%H:%M:%SZ", tz = "UTC")
s <- run_full_course(p111, "aaa1111", restart_utc,
                     resume_scene = "BasicPickingScene", resume_accepted = FALSE)

# P112: the app is quit cleanly in ConceptScene (session_end "quit"). On
# restart the keypad shows no resume (no crash) but the "already finished"
# warning, because any session_end counts as finished (backlog #28/#44); the
# facilitator presses "Trotzdem starten" (ended_session_warning = true) and
# P112 runs the whole course. analyse.R must show ended_session_override and warn.
p112 <- new_person("P112")
s <- start_session(p112, "aaa1111", "2026-11-05T11:30:00Z")
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_concept(s, p112)
end_session(s, "quit")
restart_utc <- format(as.POSIXct(round(s$start + s$t + 3 * 60), origin = "1970-01-01", tz = "UTC"),
                      "%Y-%m-%dT%H:%M:%SZ", tz = "UTC")
run_full_course(p112, "aaa1111", restart_utc, ended_warning = TRUE)

# P113: in MixedPickingScene the first press at position 2 has nothing of
# SCHRAUBE B in the box (EMPTY at position > 1: the 3 KABELVERBINDER A cartons
# of position 1 still lie there but no longer count); the next press is right.
run_full_course(new_person("P113"), "aaa1111", "2026-11-05T13:00:00Z",
  special = list(MixedPickingScene = list(forced = list("2" = list(
    fixed_press(0, list(err("EMPTY"))),
    fixed_press(3, list()))))))

# P114: ComplexScene position 1 correct, the boxes ride to position 2, then
# the app crashes (no task_completed in ComplexScene: the course is not
# finished). Last session on the device, so no crash_recovered line.
p114 <- new_person("P114")
s <- start_session(p114, "aaa1111", "2026-11-05T14:30:00Z")
run_welcome(s)
for (scene in c("ConceptScene", "BasicPickingScene", "MixedPickingScene")) {
  go_to_scene(s, scene); run_visit(s, p114)
}
go_to_scene(s, "ComplexScene"); run_picking(s, p114, crash_after_position = 1)
crash_session(s)

# ---- 7d. Pilot participants (P800-P899, decision 023) ----------------------
# They run like study participants (Menu lock, warning, resume) but keep the
# language chosen on the keypad and log to research/pilot/. analyse.R must
# analyse them as their own set (output/pilot/), without a language warning,
# and never mix them into the study set. They ran on a second headset, so
# P114's crashed file (above) gets no crash_recovered line from them.
device$open <- list()

# P801: the English pilot (developer), normal course.
run_full_course(new_person("P801"), "aaa1111", "2026-11-06T08:00:00Z", language = "EN")

# P802: the German expert's last run with a pilot ID, in DE.
run_full_course(new_person("P802"), "aaa1111", "2026-11-06T09:30:00Z", language = "DE")

# P803: English pilot (friend) crashes in BasicPickingScene after one wrong
# confirm, the keypad offers BasicPickingScene and the resume is accepted.
p803 <- new_person("P803")
s <- start_session(p803, "aaa1111", "2026-11-06T11:00:00Z", language = "EN")
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_concept(s, p803)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p803, max_confirms = 1, give_up = TRUE)
crash_session(s)
restart_utc <- format(as.POSIXct(round(s$start + s$t + 5 * 60), origin = "1970-01-01", tz = "UTC"),
                      "%Y-%m-%dT%H:%M:%SZ", tz = "UTC")
s <- start_session(p803, "aaa1111", restart_utc, language = "EN",
                   resume_scene = "BasicPickingScene", resume_accepted = TRUE)
run_welcome(s, practice = FALSE)
go_to_scene(s, "BasicPickingScene", via = "resume"); run_picking(s, p803)
go_to_scene(s, "MixedPickingScene"); run_picking(s, p803)
go_to_scene(s, "ComplexScene"); run_picking(s, p803, end_on_correct = TRUE)
end_session(s, "completed")

# ---- 8. Write the files ----------------------------------------------------

# Binary mode keeps "\n" line endings on Windows (the logger writes LF).
write_lf <- function(lines, path) {
  con <- file(path, open = "wb")
  writeLines(enc2utf8(lines), con, sep = "\n", useBytes = TRUE)
  close(con)
}

for (s in device$sessions) {
  stem <- file.path(s$folder, paste0(s$pid, "_", s$sid))
  write_lf(s$lines, paste0(stem, "_events.jsonl"))
  # The logger creates the summary file with its first row (first scene_exit).
  # The legacy schema-1 file (P115) gets the old column header.
  if (length(s$summary) > 0) {
    header_row <- if (s$schema == 2) summary_header else summary_header_v1
    write_lf(c(header_row, s$summary), paste0(stem, "_summary.csv"))
  }
}

# A partial copy of P101's session in a second sub folder, as if the files
# had been copied off the headset once mid-session and again at the end.
# analyse.R must keep the longer (full) files and warn; results must equal a
# run without the copy (selftest.R checks this).
fake_backup <- file.path(fake_research, "backup")
dir.create(fake_backup, showWarnings = FALSE)
p101_session <- Filter(function(s) s$pid == "P101", device$sessions)[[1]]
stem_copy <- file.path(fake_backup, paste0(p101_session$pid, "_", p101_session$sid))
write_lf(head(p101_session$lines, length(p101_session$lines) - 6), paste0(stem_copy, "_events.jsonl"))
write_lf(c(summary_header, head(p101_session$summary, length(p101_session$summary) - 1)),
         paste0(stem_copy, "_summary.csv"))

# ---- 9. Paper forms (#31) ---------------------------------------------------

# The forms as a facilitator would type them into Excel on a German PC and
# save as "CSV": semicolons, dates mostly DD.MM.YYYY, Windows-1252 text.
# Columns: analysis/paper_forms_template.csv. Edge cases analyse.R must catch:
#   P101  known scores (pre all 0, post items 1, 3, 8 = 1), ISO date
#   P102  typed in twice (duplicate: none joined)
#   P103  end time with seconds (Excel), two sessions (last one counts)
#   P104  abbruchgrund "Zeit " (case and space are ignored -> zeit)
#   P105  abbruchgrund "absturz" (not in the list -> warned, empty)
#   P106  SSQ post item 8 = 4 and TLX effort = 120 (out of range -> empty)
#   P107  facilitator ended the session: abbruchgrund mued_schwindlig + note
#   P108  SSQ pre item 3 empty (oculomotor and total NA, nausea not)
#   P109  abbruchgrund "andere" although the course was finished
#   P110  end time 3 h off (swapped form?)
#   P112  NASA-TLX not filled
#   P113  ID typed as "P13" (invalid; P113 then has a log but no paper)
#   P114  no paper form at all
#   P120  paper form but no log
#   P950  test ID (dropped)
#   P801-P803  pilot IDs: join the pilot set only (output/pilot/), never the study
set.seed(31)
paper_tz <- "Europe/Berlin"
ssq_item_names <- c("01_general_discomfort", "02_fatigue", "03_headache", "04_eye_strain",
                    "05_difficulty_focusing", "06_salivation_increased", "07_sweating", "08_nausea",
                    "09_difficulty_concentrating", "10_fullness_of_head", "11_blurred_vision",
                    "12_dizzy_eyes_open", "13_dizzy_eyes_closed", "14_vertigo",
                    "15_stomach_awareness", "16_burping")
tlx_names <- c("mental", "physical", "temporal", "performance", "effort", "frustration")
paper_header <- c("participant_id", "session_date", "facilitator", "end_time_local",
                  paste0("ssq_pre_", ssq_item_names), paste0("ssq_post_", ssq_item_names),
                  paste0("tlx_", tlx_names), "abbruchgrund", "note")

# Local end of a participant's last session (what the facilitator writes down).
last_event_local <- function(pid) {
  ends <- vapply(Filter(function(s) s$pid == pid, device$sessions),
                 function(s) s$start + s$t, numeric(1))
  as.POSIXct(floor(max(ends)), origin = "1970-01-01", tz = "UTC")
}

paper_row <- function(pid, pre = NULL, post = NULL, tlx = NULL, abbruch = "", note = "",
                      end_offset_h = 0, date_iso = FALSE, time_seconds = FALSE, id_written = pid,
                      end_at = NULL) {
  end <- if (is.null(end_at)) last_event_local(pid) else end_at
  end <- end + end_offset_h * 3600
  if (is.null(pre)) pre <- sample(0:3, 16, replace = TRUE, prob = c(0.70, 0.20, 0.07, 0.03))
  if (is.null(post)) post <- sample(0:3, 16, replace = TRUE, prob = c(0.50, 0.30, 0.15, 0.05))
  if (is.null(tlx)) tlx <- sample(seq(0, 100, 5), 6, replace = TRUE)
  c(id_written,
    format(end, if (date_iso) "%Y-%m-%d" else "%d.%m.%Y", tz = paper_tz),
    sample(c("F1", "F2"), 1),
    format(end, if (time_seconds) "%H:%M:%S" else "%H:%M", tz = paper_tz),
    as.character(pre), as.character(post), as.character(tlx), abbruch, note)
}

blank <- function(x, i) { x[i] <- ""; x }
p106_post <- sample(0:3, 16, replace = TRUE); p106_post[8] <- 4
p106_tlx <- sample(seq(0, 100, 5), 6, replace = TRUE); p106_tlx[5] <- 120
p108_pre <- blank(as.character(sample(0:3, 16, replace = TRUE, prob = c(0.7, 0.2, 0.07, 0.03))), 3)
paper_rows <- list(
  paper_row("P101", pre = rep(0, 16), post = c(1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0),
            tlx = c(40, 10, 30, 20, 45, 15), date_iso = TRUE),
  paper_row("P102"),
  paper_row("P102"),
  paper_row("P103", time_seconds = TRUE),
  paper_row("P104", abbruch = "Zeit "),
  paper_row("P105", abbruch = "absturz", note = "App abgestürzt in Basic"),
  paper_row("P106", post = p106_post, tlx = p106_tlx),
  paper_row("P107", abbruch = "mued_schwindlig", note = "Übelkeit in Komplex"),
  paper_row("P108", pre = p108_pre),
  paper_row("P109", abbruch = "andere"),
  paper_row("P110", end_offset_h = 3),
  paper_row("P111"),
  paper_row("P112", tlx = rep("", 6)),
  paper_row("P113", id_written = "P13"),
  paper_row("P115"),
  paper_row("P120", end_at = as.POSIXct("2026-11-06 09:00:00", tz = "UTC")),
  paper_row("P950"),
  paper_row("P801"),
  paper_row("P802"),
  paper_row("P803"))
paper_lines <- c(paste(paper_header, collapse = ";"),
                 vapply(paper_rows, function(r) {
                   if (length(r) != length(paper_header)) stop("fake paper row has ", length(r), " cells")
                   paste(r, collapse = ";")
                 }, character(1)),
                 paste(rep("", length(paper_header)), collapse = ";"))  # a row Excel left empty
con <- file(file.path(fake_root, "paper_forms.csv"), open = "wb")
writeLines(iconv(paper_lines, from = "UTF-8", to = "windows-1252"), con, sep = "\r\n", useBytes = TRUE)
close(con)

# Short overview so the run can be checked by eye.
overview <- data.frame(
  participant_id = vapply(device$sessions, function(s) s$pid, character(1)),
  build = vapply(device$sessions, function(s) s$build, character(1)),
  schema = vapply(device$sessions, function(s) s$schema, numeric(1)),
  event_lines = vapply(device$sessions, function(s) length(s$lines), integer(1)),
  summary_rows = vapply(device$sessions, function(s) length(s$summary), integer(1)),
  last_event = vapply(device$sessions, function(s) {
    sub('.*"event":"([a-z_]+)".*', "\\1", s$lines[length(s$lines)])
  }, character(1)))
print(overview, row.names = FALSE)
cat("Wrote", nrow(overview), "events files to", normalizePath(fake_research), "\n")
