# make_fake_data.R
#
# Writes fake research logs in exactly the format of the C# ResearchLogger
# (Assets/Scripts/Research/), plus the outcome events that #28 will add
# (confirm_pressed, quiz_answer, task_completed; design section 6).
# Why: analyse.R must be tested before real participants exist, and the
# fake files are the contract between the C# logger and R.
#
# Run from the repo root:   Rscript analysis/make_fake_data.R
# Output (git-ignored):     analysis/data/fake/research/ (+ research/practice/,
#                           research/backup/ with a partial duplicate of P101)
#
# Base R only (no packages), so it runs right after installing R.
# If the logger format changes, change this script in the same commit.

set.seed(26)  # same fake data on every run

# ---- 1. Paths --------------------------------------------------------------

# Works from the repo root (Rscript, source) and from inside analysis/.
analysis_folder <- if (file.exists("skill_map.csv")) "." else "analysis"
fake_root <- file.path(analysis_folder, "data", "fake")
fake_research <- file.path(fake_root, "research")
fake_practice <- file.path(fake_research, "practice")

# Start clean so a rerun never mixes old and new files.
unlink(fake_root, recursive = TRUE)
dir.create(fake_practice, recursive = TRUE, showWarnings = FALSE)

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

summary_header <- paste(
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

# Required counts per scene (current validators). Keys that don't apply in a
# scene are left out here and become null in the log (design section 6).
order_required <- list(
  BasicPickingScene = list(gg = 6, vpe = 0, other = 0),
  MixedPickingScene = list(vpe = 0, gga = 2, ggb = 2, other = 0),
  ComplexScene      = list(gga = 0, ggb = 2, loose = 5, other = 0))

# Total pieces as the WMS "Ist" shows it (Basic: GG x 50; Complex: GGA x 50 +
# GGB x 25 + loose). MixedPickingScene has no piece total -> null.
total_pieces <- function(scene, box) {
  if (scene == "BasicPickingScene") return(50 * box$gg)
  if (scene == "ComplexScene") return(50 * box$gga + 25 * box$ggb + box$loose)
  NULL
}

# The fixed keys of container/required, in the agreed order.
counts_obj <- function(box) {
  json_obj(gg = box$gg, vpe = box$vpe, gga = box$gga, ggb = box$ggb,
           loose = box$loose, other = box$other, total_pieces = box$total_pieces)
}

# Skills each scene tests (same content as scene_skills.csv).
scene_skill_list <- list(
  ConceptScene      = "package_concept",
  BasicPickingScene = "calculate_quantity",
  MixedPickingScene = c("calculate_quantity", "check_material"),
  ComplexScene      = c("calculate_quantity", "check_material", "calculate_loose"))

# Chance of an error on attempt k: lower for able trainees, for later attempts
# in the same visit, and for skills practised in earlier visits. This gives
# the fake data a visible "improvement" signal to test the charts with.
skill_base_logit <- c(package_concept = 0.3, calculate_quantity = 0.2,
                      check_material = 0.0, calculate_loose = 0.4)

p_error <- function(person, skill, k) {
  practised <- person$seen[[skill]]
  if (is.null(practised)) practised <- 0
  plogis(skill_base_logit[[skill]] - person$ability - 0.9 * (k - 1) - 0.6 * practised)
}

err <- function(code, item = NULL) list(code = code, item = item)

# One confirm press: what is in the container and which error codes it gets.
# force_wrong makes sure the press is wrong (for "gave up"/crash cases);
# force_right is the safety stop after many attempts.
draw_attempt <- function(scene, person, k, force_wrong = FALSE, force_right = FALSE) {
  req <- order_required[[scene]]
  box <- req
  codes <- list()

  if (!force_right && runif(1) < 0.03) {
    # EMPTY is logged alone, without an extra TOO_FEW (design section 6).
    for (key in names(box)) box[[key]] <- 0
    codes <- list(err("EMPTY"))
  } else if (!force_right) {
    skills <- scene_skill_list[[scene]]
    wrong <- vapply(skills, function(sk) runif(1) < p_error(person, sk, k), logical(1))
    if (force_wrong && !any(wrong)) wrong[1] <- TRUE

    if (wrong["calculate_quantity"]) {
      item <- switch(scene, BasicPickingScene = "gg", ComplexScene = "ggb",
                     MixedPickingScene = sample(c("gga", "ggb"), 1))
      delta <- sample(c(-2, -1, 1, 2), 1)
      box[[item]] <- req[[item]] + delta
      codes <- c(codes, list(err(if (delta < 0) "TOO_FEW" else "TOO_MANY", item)))
    }
    if (isTRUE(wrong["check_material"])) {
      if (scene == "MixedPickingScene") {
        box$vpe <- 1  # VPE is not in the order
        codes <- c(codes, list(err("WRONG_TYPE")))
      } else {
        box$gga <- 1  # GGA instead of / next to the ordered GGB
        codes <- c(codes, list(err("WRONG_MATERIAL", "gga")))
      }
    }
    if (isTRUE(wrong["calculate_loose"])) {
      delta <- sample(c(-3, -2, -1, 1, 2), 1)
      box$loose <- req$loose + delta
      codes <- c(codes, list(err(if (delta < 0) "MISSING_LOOSE" else "EXTRA_LOOSE", "loose")))
    }
  }

  box$total_pieces <- total_pieces(scene, box)
  req$total_pieces <- total_pieces(scene, req)
  list(box = box, req = req, codes = codes)
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
start_session <- function(person, build, start_utc, start_mode = "manual") {
  s <- new.env()
  s$pid <- person$pid
  s$sid <- make_uuid()
  s$build <- build
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

  # Header and session_start: t = 0, scene_visit null.
  s$lines <- event_line(0, fmt_utc(s$start, 0), s$pid, s$sid, NULL, NULL, "header",
    json_obj(schema_version = 1, app_version = "0.1.0", build = build,
             device_model = "Oculus Quest", os_version = "Android OS 14 / API-34",
             participant_id = s$pid, session_id = s$sid, session_start_mode = start_mode,
             language = NULL,
             input_mode_at_start = "controllers", target_refresh_rate = 72))
  s$scene <- "WelcomeScene"
  emit(s, "session_start")
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
  s$v <- list(enter_t = s$t, enter_utc = fmt_utc(s$start, s$t), paused_s = 0,
              pauses = 0, hmd = 0, errors = 0, attempts = 0,
              codes = character(0), first_correct = NA, correct_at = NA,
              t_first = NA, t_correct = NA)
  emit(s, "scene_enter", json_obj(from_scene = from, via = via))
}

# Error code tally for summary.csv, e.g. "TOO_FEW:2;WRONG_TYPE:1".
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
  # #29/#30 columns stay empty.
  v <- s$v
  has_outcome <- s$scene != "WelcomeScene"
  row <- c(s$pid, s$sid, s$build, "1", s$scene, cell(s$visit), v$enter_utc,
           cell(duration), cell(v$paused_s),
           if (has_outcome) cell(v$attempts) else "",
           if (has_outcome) cell(v$first_correct) else "",
           if (has_outcome) cell(v$correct_at) else "",
           if (has_outcome) cell(v$t_first) else "",
           if (has_outcome) cell(v$t_correct) else "",
           if (has_outcome) cell(!is.na(v$correct_at)) else "",
           if (has_outcome) codes_tally(v$codes) else "",
           "", "", "", "", "", "",
           cell(v$pauses), cell(v$hmd), "", "", cell(v$errors))
  s$summary <- c(s$summary, paste(row, collapse = ","))
  s$visit_open <- FALSE
}

go_to_scene <- function(s, scene) {
  from <- s$scene
  exit_scene(s)
  advance(s, 0.001)
  enter_scene(s, scene, from, NULL)
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

# Shared bookkeeping for confirm_pressed and quiz_answer; writes task_completed
# on the first correct press of the visit. Returns TRUE when correct.
record_outcome <- function(s, k, correct, codes, at) {
  if (k == 1) {
    s$v$first_correct <- correct
    s$v$t_first <- at
  }
  s$v$codes <- c(s$v$codes, vapply(codes, function(e) e$code, character(1)))
  if (correct && is.na(s$v$correct_at)) {
    s$v$correct_at <- k
    s$v$t_correct <- at
    advance(s, 0.001)
    emit(s, "task_completed", json_obj(attempts_to_correct = k,
                                       time_to_first_confirm_s = s$v$t_first,
                                       time_to_correct_s = at))
  }
  correct
}

codes_json <- function(codes) {
  json_arr(lapply(codes, function(e) json_obj(code = e$code, item = e$item)))
}

# correct is normally "no error codes" (the invariant). Passing correct
# explicitly lets one fake press break the invariant on purpose (P110).
press_confirm <- function(s, attempt, correct = length(attempt$codes) == 0) {
  s$v$attempts <- s$v$attempts + 1
  k <- s$v$attempts
  at <- active_s(s)
  emit(s, "confirm_pressed", json_obj(
    attempt_no = k, container = counts_obj(attempt$box),
    required = counts_obj(attempt$req), correct = correct,
    error_codes = codes_json(attempt$codes), time_since_scene_enter_s = at))
  record_outcome(s, k, correct, attempt$codes, at)
}

# ---- 6. What happens inside each kind of scene -----------------------------

run_welcome <- function(s) {
  advance(s, runif(1, 15, 45))
  maybe_unity_error(s)
}

run_quiz <- function(s, person) {
  advance(s, runif(1, 20, 60))  # read the package descriptions
  maybe_pause(s)
  maybe_unity_error(s)
  k <- 0
  repeat {
    k <- k + 1
    if (k == 1 && runif(1) < 0.05) {
      selected <- NULL  # confirm without a selection
      codes <- list(err("NO_ANSWER"))
    } else if (k < 6 && runif(1) < p_error(person, "package_concept", k)) {
      selected <- sample(c("gg", "both"), 1)
      codes <- list(err("WRONG_ANSWER"))
    } else {
      selected <- "vpe"  # the right answer
      codes <- list()
    }
    s$v$attempts <- s$v$attempts + 1
    at <- active_s(s)
    correct <- length(codes) == 0
    emit(s, "quiz_answer", json_obj(attempt_no = s$v$attempts, selected = selected,
                                    correct = correct, error_codes = codes_json(codes)))
    if (record_outcome(s, s$v$attempts, correct, codes, at)) break
    advance(s, runif(1, 4, 15))
  }
  advance(s, runif(1, 3, 10))  # read the feedback, then press the menu
  person$seen[["package_concept"]] <- c(person$seen[["package_concept"]], 0)[1] + 1
}

# max_confirms: stop after this many presses (0 = never confirms).
# give_up: every press is wrong (for quit/crash endings).
# first_press: a fixed first press (list(box, codes, correct)) for edge cases.
run_picking <- function(s, person, max_confirms = 8, give_up = FALSE,
                        first_press = NULL, end_on_correct = FALSE,
                        double_cause_pause = FALSE) {
  scene <- s$scene
  advance(s, runif(1, 30, 70))  # read the WMS order and pick packages
  if (double_cause_pause) {
    # Two causes in the same frame: headset off and focus lost together.
    pause_app(s, c("hmd_unmounted", "focus_lost"), runif(1, 20, 60))
    advance(s, runif(1, 2, 8))
  } else {
    maybe_pause(s)
  }
  maybe_unity_error(s)

  done <- FALSE
  k <- 0
  while (k < max_confirms) {
    k <- k + 1
    if (k == 1 && !is.null(first_press)) {
      req <- order_required[[scene]]
      req$total_pieces <- total_pieces(scene, req)
      attempt <- list(box = first_press$box, req = req, codes = first_press$codes)
      done <- press_confirm(s, attempt, first_press$correct)
    } else {
      attempt <- draw_attempt(scene, person, k, force_wrong = give_up,
                              force_right = (k == 8 && !give_up))
      done <- press_confirm(s, attempt)
    }
    if (done) break
    advance(s, runif(1, 8, 30))  # read the feedback and fix the container
  }

  # Practice counts for the skills this scene tests.
  for (sk in scene_skill_list[[scene]]) {
    person$seen[[sk]] <- c(person$seen[[sk]], 0)[1] + 1
  }
  if (done && end_on_correct) return(invisible(TRUE))  # session ends right away
  advance(s, runif(1, 3, 10))
  invisible(done)
}

run_visit <- function(s, person, ...) {
  if (s$scene == "WelcomeScene") run_welcome(s)
  else if (s$scene == "ConceptScene") run_quiz(s, person)
  else run_picking(s, person, ...)
}

# A normal course: every scene in build order, ends "completed" on the first
# correct confirm in ComplexScene (#28 calls EndSession("completed") there).
run_full_course <- function(person, build, start_utc, special = list(), start_mode = "manual") {
  s <- start_session(person, build, start_utc, start_mode)
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

# ---- 7. The fake participants ----------------------------------------------

# P950: facilitator's trial run with a test ID. It belongs in practice/, but
# here it sits in research/ on purpose: analyse.R must drop it anyway.
p950 <- new_person("P950")
s <- start_session(p950, "aaa1111", "2026-11-02T09:00:00Z", start_mode = "auto")
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_quiz(s, p950)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p950)
end_session(s, "quit")

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
go_to_scene(s, "ConceptScene"); run_quiz(s, p103)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p103)
go_to_scene(s, "MixedPickingScene"); run_picking(s, p103, max_confirms = 1, give_up = TRUE)
begin_pause(s, "hmd_unmounted")
crash_session(s)
crash_epoch <- s$start + s$t
restart_utc <- format(as.POSIXct(round(crash_epoch + 6 * 60), origin = "1970-01-01", tz = "UTC"),
                      "%Y-%m-%dT%H:%M:%SZ", tz = "UTC")
# Resume: the menu is unlocked up to MixedPickingScene, which restarts from
# its beginning (design section 4, resume after a crash).
s <- start_session(p103, "bbb2222", restart_utc)
run_welcome(s)
go_to_scene(s, "MixedPickingScene"); run_picking(s, p103)
go_to_scene(s, "ComplexScene"); run_picking(s, p103, end_on_correct = TRUE)
end_session(s, "completed")

# P104: gives up in ComplexScene after two wrong confirms, takes the headset
# off and the app is closed while still paused (quit callback runs: reason
# "quit", no app_resume; scene_exit.paused_s includes the open pause).
p104 <- new_person("P104")
s <- start_session(p104, "aaa1111", "2026-11-03T14:00:00Z")
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
p105 <- new_person("P105")
s <- start_session(p105, "aaa1111", "2026-11-04T08:00:00Z")
run_welcome(s)
go_to_scene(s, "ConceptScene"); run_quiz(s, p105)
go_to_scene(s, "BasicPickingScene"); run_picking(s, p105, max_confirms = 1, give_up = TRUE)
raise_error(s)
crash_session(s)

# P106: one pause whose app_pause lists two causes.
run_full_course(new_person("P106"), "aaa1111", "2026-11-04T09:30:00Z",
                special = list(MixedPickingScene = list(double_cause_pause = TRUE)))

# P107: enters ComplexScene but never confirms; the facilitator ends the
# session (#27 control) -> a visit with 0 confirms (error rate must be NA).
# In WelcomeScene a broken loop raises 200+ different errors: the error cap is
# reached, later errors (also in later scenes) are only counted in scene_exit.
# One repeat folded before the cap is still written at the visit close.
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
end_session(s, "facilitator")

# P108: normal course on the newer build bbb2222, but the study build still had
# autoStart on (session_start_mode "auto"): analyse.R must warn.
run_full_course(new_person("P108"), "bbb2222", "2026-11-04T13:00:00Z", start_mode = "auto")

# P109: one confirm carries FOO_BAR, a code missing from skill_map.csv.
run_full_course(new_person("P109"), "aaa1111", "2026-11-04T14:30:00Z",
  special = list(MixedPickingScene = list(first_press = list(
    box = list(vpe = 0, gga = 2, ggb = 1, other = 0),
    codes = list(err("FOO_BAR")), correct = FALSE))))

# P110: one confirm says correct = true although it has an error code
# (today's OrderConfirm accepts 6 GG + 1 VPE). Breaks the invariant on purpose.
run_full_course(new_person("P110"), "aaa1111", "2026-11-05T08:00:00Z",
  special = list(BasicPickingScene = list(first_press = list(
    box = list(gg = 6, vpe = 1, other = 0, total_pieces = 300),
    codes = list(err("WRONG_TYPE")), correct = TRUE))))

# ---- 8. Write the files ----------------------------------------------------

# Binary mode keeps "\n" line endings on Windows (the logger writes LF).
write_lf <- function(lines, path) {
  con <- file(path, open = "wb")
  writeLines(enc2utf8(lines), con, sep = "\n", useBytes = TRUE)
  close(con)
}

for (s in device$sessions) {
  stem <- file.path(fake_research, paste0(s$pid, "_", s$sid))
  write_lf(s$lines, paste0(stem, "_events.jsonl"))
  # The logger creates the summary file with its first row (first scene_exit).
  if (length(s$summary) > 0) {
    write_lf(c(summary_header, s$summary), paste0(stem, "_summary.csv"))
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

# Short overview so the run can be checked by eye.
overview <- data.frame(
  participant_id = vapply(device$sessions, function(s) s$pid, character(1)),
  build = vapply(device$sessions, function(s) s$build, character(1)),
  event_lines = vapply(device$sessions, function(s) length(s$lines), integer(1)),
  summary_rows = vapply(device$sessions, function(s) length(s$summary), integer(1)),
  last_event = vapply(device$sessions, function(s) {
    sub('.*"event":"([a-z_]+)".*', "\\1", s$lines[length(s$lines)])
  }, character(1)))
print(overview, row.names = FALSE)
cat("Wrote", nrow(overview), "events files to", normalizePath(fake_research), "\n")
