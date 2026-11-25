# usability_explore.R
# -----------------------------------------------------------------------------
# Exploratory usability look at the REAL research logs (2026-11-24).
# This is NOT the formal research-question analysis (that is analyse.R).
# It looks for interaction patterns and anything that looks wrong:
#   - packages grabbed again and again, packages that leave the order
#     container right after they were let go ("bounce"),
#   - red-button presses close together, wrong confirms and their reasons,
#   - where the time goes (per scene, per order position, per tour step),
#   - quiz first attempts,
#   - logging problems (no session_end, open grabs, summary != events).
#
# How to run (from the repo root or from analysis/):
#   "C:\Program Files\R\R-4.6.2\bin\Rscript.exe" analysis/usability_explore.R
#
# Input : analysis/data/real/*_events.jsonl (+ *_summary.csv)
# Output: analysis/output/usability-2026-11-24/ (CSV tables + PNG charts)
#
# Why base R and not tidyverse: on this PC Windows "Application Control"
# blocks rlang.dll, so dplyr/ggplot2 cannot load (2026-11-24). Base R +
# jsonlite still work, so this script only uses those.
# -----------------------------------------------------------------------------

suppressMessages(library(jsonlite))

# ---- 0. Folders ---------------------------------------------------------------
# Work from the repo root or from analysis/, whichever we are in.
base_dir <- if (dir.exists("data/real")) "." else "analysis"
data_dir <- file.path(base_dir, "data", "real")
out_dir  <- file.path(base_dir, "output", "usability-2026-11-24")
dir.create(out_dir, recursive = TRUE, showWarnings = FALSE)

# Fixed scene order (build order) and short names used in every chart.
scene_order <- c("WelcomeScene", "ConceptScene", "BasicPickingScene",
                 "MixedPickingScene", "ComplexScene")
scene_short <- c(WelcomeScene = "Welcome", ConceptScene = "Concept",
                 BasicPickingScene = "Basic", MixedPickingScene = "Mixed",
                 ComplexScene = "Complex")
picking_scenes <- c("BasicPickingScene", "MixedPickingScene", "ComplexScene")

# Okabe-Ito palette: safe for colour-blind readers.
okabe <- c(orange = "#E69F00", sky = "#56B4E9", green = "#009E73",
           yellow = "#F0E442", blue = "#0072B2", red = "#D55E00",
           pink = "#CC79A7", grey = "#999999", black = "#000000")
scene_col <- c(WelcomeScene = okabe[["grey"]], ConceptScene = okabe[["sky"]],
               BasicPickingScene = okabe[["green"]],
               MixedPickingScene = okabe[["orange"]],
               ComplexScene = okabe[["pink"]])

# Small helper: read a nested field from one event's data list, NA if absent.
get_field <- function(x, ...) {
  for (k in c(...)) {
    if (is.null(x) || !is.list(x) || is.null(x[[k]])) return(NA)
    x <- x[[k]]
  }
  if (length(x) == 0) return(NA)
  if (is.list(x)) return(NA)
  x[[1]]
}

# ---- 1. Read every events file ------------------------------------------------
# One JSON object per line. We keep the 'data' part as a list column and pull
# out the fields we need later.
files <- list.files(data_dir, pattern = "_events\\.jsonl$", full.names = TRUE)
if (length(files) == 0) stop("No *_events.jsonl in ", data_dir)

read_one <- function(path) {
  lines <- readLines(path, encoding = "UTF-8", warn = FALSE)
  keep  <- nzchar(trimws(lines))
  recs  <- lapply(lines[keep], function(l)
    tryCatch(fromJSON(l, simplifyVector = FALSE), error = function(e) NULL))
  bad <- vapply(recs, is.null, logical(1))
  recs <- recs[!bad]
  num <- function(v) if (is.null(v)) NA_real_ else as.numeric(v)
  chr <- function(v) if (is.null(v)) NA_character_ else as.character(v)
  df <- data.frame(
    file        = basename(path),
    line_no     = which(keep)[!bad],
    t           = vapply(recs, function(r) num(r$t), numeric(1)),
    utc         = vapply(recs, function(r) chr(r$utc), character(1)),
    participant = vapply(recs, function(r) chr(r$participant_id), character(1)),
    session_id  = vapply(recs, function(r) chr(r$session_id), character(1)),
    scene       = vapply(recs, function(r) chr(r$scene), character(1)),
    visit       = vapply(recs, function(r) num(r$scene_visit), numeric(1)),
    event       = vapply(recs, function(r) chr(r$event), character(1)),
    stringsAsFactors = FALSE)
  df$data <- I(lapply(recs, function(r) r$data))
  attr(df, "bad_lines") <- sum(bad)
  df
}

ev_list   <- lapply(files, read_one)
bad_lines <- setNames(vapply(ev_list, function(d) attr(d, "bad_lines"), numeric(1)),
                      basename(files))
ev <- do.call(rbind, ev_list)
ev <- ev[order(ev$file, ev$line_no), ]

# ---- 2. Sessions: header facts, ID class, how the session ended --------------
hdr <- ev[ev$event == "header", ]
sessions <- data.frame(
  session_id  = hdr$session_id,
  participant = hdr$participant,
  start_utc   = hdr$utc,
  layout      = vapply(hdr$data, get_field, character(1), "layout_version"),
  build       = vapply(hdr$data, get_field, character(1), "build"),
  language    = vapply(hdr$data, get_field, character(1), "language"),
  input_mode  = vapply(hdr$data, get_field, character(1), "input_mode_at_start"),
  stringsAsFactors = FALSE)

# ID class as decided in the design: P000 + P900-P999 test, P800-P899 pilot,
# P001-P799 study. NOTE: a study-range ID is not proof of a real trainee.
id_num <- as.integer(sub("^P", "", sessions$participant))
sessions$id_class <- ifelse(id_num == 0 | id_num >= 900, "test",
                     ifelse(id_num >= 800, "pilot", "study-range"))

# Short label for charts; two sessions of one person get a suffix.
dup <- duplicated(sessions$participant) |
       duplicated(sessions$participant, fromLast = TRUE)
sessions$label <- ifelse(dup,
  paste0(sessions$participant, "-", substr(sessions$session_id, 1, 4)),
  sessions$participant)

# End reason: session_end.reason, or "crash" when the file has no session_end.
ends <- ev[ev$event == "session_end", ]
sessions$end_reason <- vapply(sessions$session_id, function(s) {
  e <- ends[ends$session_id == s, ]
  if (nrow(e) == 0) "crash (no session_end)" else get_field(e$data[[1]], "reason")
}, character(1))
sessions$crash_recovered_line <- sessions$session_id %in%
  ev$session_id[ev$event == "crash_recovered"]

# Join session facts to every event.
ev$label  <- sessions$label[match(ev$session_id, sessions$session_id)]
ev$build  <- sessions$build[match(ev$session_id, sessions$session_id)]
ev$layout <- sessions$layout[match(ev$session_id, sessions$session_id)]

# Pull frequently used fields out of 'data' once, so later code stays short.
pull_chr <- function(...) vapply(ev$data, function(d) as.character(get_field(d, ...)), character(1))
pull_num <- function(...) vapply(ev$data, function(d) suppressWarnings(as.numeric(get_field(d, ...))), numeric(1))
ev$object_id   <- pull_chr("object_id")
ev$material    <- pull_chr("material_key")
ev$container   <- pull_chr("container")
ev$count_after <- pull_num("count_after")
ev$position    <- pull_num("position")
ev$correct     <- pull_chr("correct")
ev$reason      <- pull_chr("reason")
ev$what        <- pull_chr("what")
ev$step_id     <- pull_chr("step_id")
ev$step_index  <- pull_num("step_index")
ev$question_id <- pull_chr("question_id")
ev$quiz        <- pull_chr("quiz")
ev$attempt_q   <- pull_num("attempt_in_question")
ev$feedback_key <- pull_chr("feedback_key")
ev$dur_field   <- pull_num("duration_s")
ev$paused_field <- pull_num("paused_s")
ev$error_codes <- vapply(ev$data, function(d) {
  ec <- d$error_codes
  if (is.null(ec) || length(ec) == 0) return(NA_character_)
  paste(vapply(ec, function(e) as.character(e$code), character(1)), collapse = ";")
}, character(1))
ev$causes <- vapply(ev$data, function(d)
  if (is.null(d$causes)) NA_character_ else paste(unlist(d$causes), collapse = "+"),
  character(1))
# Loose piece or full carton/bag, from the object name (ring / clamp = loose).
ev$form <- ifelse(is.na(ev$object_id), NA,
           ifelse(grepl("ring|clamp", ev$object_id), "loose", "full"))

# ---- 3. Pauses -> active time -------------------------------------------------
# 't' keeps running while the app is paused (headset off, system menu), so a
# time difference must subtract the paused parts to give "active" seconds.
pause_iv <- do.call(rbind, lapply(sessions$session_id, function(s) {
  e <- ev[ev$session_id == s & ev$event %in% c("app_pause", "app_resume") & !is.na(ev$t), ]
  if (nrow(e) == 0) return(NULL)
  out <- NULL; open <- NA
  for (i in seq_len(nrow(e))) {
    if (e$event[i] == "app_pause") open <- e$t[i]
    if (e$event[i] == "app_resume" && !is.na(open)) {
      out <- rbind(out, data.frame(session_id = s, from = open, to = e$t[i]))
      open <- NA
    }
  }
  if (!is.na(open)) out <- rbind(out, data.frame(session_id = s, from = open, to = Inf))
  out
}))
active_between <- function(s, a, b) {
  if (is.na(a) || is.na(b)) return(NA_real_)
  iv <- pause_iv[pause_iv$session_id == s, ]
  overlap <- if (is.null(iv) || nrow(iv) == 0) 0 else
    sum(pmax(0, pmin(b, iv$to) - pmax(a, iv$from)))
  (b - a) - overlap
}

# ---- 4. Scene visits ----------------------------------------------------------
# One row per (session, scene, visit) from scene_enter / scene_exit.
enters <- ev[ev$event == "scene_enter", ]
visits <- data.frame(session_id = enters$session_id, label = enters$label,
                     participant = enters$participant, layout = enters$layout,
                     scene = enters$scene, visit = enters$visit,
                     t_enter = enters$t, stringsAsFactors = FALSE)
visits$t_exit <- mapply(function(s, sc, v) {
  x <- ev[ev$session_id == s & ev$event == "scene_exit" & ev$scene == sc & ev$visit == v, ]
  if (nrow(x) == 0) NA_real_ else x$t[1]
}, visits$session_id, visits$scene, visits$visit)
# A visit without scene_exit (crash) ends at the last event with a time.
visits$t_last <- mapply(function(s, sc, v) {
  x <- ev[ev$session_id == s & ev$scene %in% sc & ev$visit %in% v & !is.na(ev$t), ]
  max(x$t)
}, visits$session_id, visits$scene, visits$visit)
visits$ended <- ifelse(is.na(visits$t_exit), "no scene_exit", "scene_exit")
visits$t_end <- ifelse(is.na(visits$t_exit), visits$t_last, visits$t_exit)
visits$active_s <- mapply(active_between, visits$session_id, visits$t_enter, visits$t_end)
visits$wall_s   <- visits$t_end - visits$t_enter
visits$paused_s <- visits$wall_s - visits$active_s
visits$n_pauses <- mapply(function(s, sc, v)
  sum(ev$session_id == s & ev$event == "app_pause" & ev$scene %in% sc & ev$visit %in% v),
  visits$session_id, visits$scene, visits$visit)
visits$scene_short <- scene_short[visits$scene]

# Session-level: which scenes were reached, total active time.
sessions$scenes_reached <- vapply(sessions$session_id, function(s)
  paste(scene_short[visits$scene[visits$session_id == s]], collapse = ">"), character(1))
sessions$active_min <- vapply(sessions$session_id, function(s)
  round(sum(visits$active_s[visits$session_id == s]) / 60, 1), numeric(1))
sessions$paused_min <- vapply(sessions$session_id, function(s)
  round(sum(visits$paused_s[visits$session_id == s]) / 60, 1), numeric(1))
sessions$n_events <- as.vector(table(factor(ev$session_id, levels = sessions$session_id)))
write.csv(sessions, file.path(out_dir, "sessions.csv"), row.names = FALSE)

# ---- 5. Order position of every event (Basic / Mixed / Complex) --------------
# Mixed and Complex work one order line at a time. Position k starts after
# position k-1 was completed, so position = 1 + completed positions before t.
pc <- ev[ev$event == "position_completed", ]
ev$pos_now <- NA_real_
pick_rows <- which(ev$scene %in% picking_scenes & !is.na(ev$t))
for (i in pick_rows) {
  done <- pc$session_id == ev$session_id[i] & pc$scene == ev$scene[i] &
          pc$visit == ev$visit[i] & pc$t < ev$t[i]
  ev$pos_now[i] <- 1 + sum(done)
}
# Confirm presses carry their own position: trust the logged one.
cp_rows <- ev$event %in% c("confirm_pressed", "position_completed") & !is.na(ev$position)
ev$pos_now[cp_rows] <- ev$position[cp_rows]

# ---- 6. Grabs: pair grab_start with grab_end ---------------------------------
# One row per grab. Snap-place artefacts of old builds (#114, builds up to
# 727eee1): an extra grab_start 0.01-0.02 s after the hand's grab_end of a
# snapped carton in Basic/Mixed. We flag and drop them.
gs <- ev[ev$event == "grab_start", ]
ge <- ev[ev$event == "grab_end", ]
grabs <- gs[, c("session_id", "label", "layout", "build", "scene", "visit",
                "object_id", "material", "form", "t", "pos_now")]
names(grabs)[names(grabs) == "t"] <- "t_start"
grabs$t_end <- NA_real_
used_end <- rep(FALSE, nrow(ge))
for (i in seq_len(nrow(grabs))) {
  cand <- which(!used_end & ge$session_id == grabs$session_id[i] &
                ge$scene == grabs$scene[i] & ge$visit == grabs$visit[i] &
                ge$object_id == grabs$object_id[i] & ge$t >= grabs$t_start[i])
  if (length(cand) > 0) { grabs$t_end[i] <- ge$t[cand[1]]; used_end[cand[1]] <- TRUE }
}
grabs$hold_s <- grabs$t_end - grabs$t_start
orphan_ends <- ge[!used_end, ]   # grab_end without a grab_start: logging issue

# Previous release of the same object in the same visit.
grabs <- grabs[order(grabs$session_id, grabs$scene, grabs$visit, grabs$t_start), ]
grabs$prev_end <- NA_real_
for (i in seq_len(nrow(grabs))) {
  same <- grabs$session_id == grabs$session_id[i] & grabs$scene == grabs$scene[i] &
          grabs$visit == grabs$visit[i] & grabs$object_id == grabs$object_id[i] &
          !is.na(grabs$t_end) & grabs$t_end <= grabs$t_start[i] &
          seq_len(nrow(grabs)) != i
  if (any(same)) grabs$prev_end[i] <- max(grabs$t_end[same])
}
grabs$gap_since_release_s <- grabs$t_start - grabs$prev_end
grabs$snap_artefact <- grabs$build == "727eee1" &
  grabs$scene %in% c("BasicPickingScene", "MixedPickingScene") &
  !is.na(grabs$gap_since_release_s) & grabs$gap_since_release_s <= 0.05
grabs$regrab_3s  <- !grabs$snap_artefact & !is.na(grabs$gap_since_release_s) &
                    grabs$gap_since_release_s <= 3
grabs$regrab_10s <- !grabs$snap_artefact & !is.na(grabs$gap_since_release_s) &
                    grabs$gap_since_release_s <= 10
write.csv(grabs, file.path(out_dir, "grabs.csv"), row.names = FALSE)
g_ok <- grabs[!grabs$snap_artefact, ]

# ---- 7. What happens right after a release (picking scenes) ------------------
# The PackageDropper drops a package 0.3 s after release (fall <= 0.4 s).
# We compare "in the order container?" at the release moment with 1.5 s later.
#   inside -> outside = let go inside the box but it ended outside ("bounce out")
#   outside -> inside = let go outside / above and it dropped in
cont <- ev[ev$event %in% c("container_enter", "container_exit") & ev$container %in% "order", ]
in_order_at <- function(s, sc, v, obj, tt) {
  x <- cont[cont$session_id == s & cont$scene == sc & cont$visit == v &
            cont$object_id == obj & cont$t <= tt, ]
  if (nrow(x) == 0) return(FALSE)
  x$event[nrow(x)] == "container_enter"
}
rel <- g_ok[g_ok$scene %in% picking_scenes & !is.na(g_ok$t_end), ]
rel$next_start <- vapply(seq_len(nrow(rel)), function(i) {
  later <- g_ok$t_start[g_ok$session_id == rel$session_id[i] & g_ok$scene == rel$scene[i] &
                        g_ok$visit == rel$visit[i] & g_ok$object_id == rel$object_id[i] &
                        g_ok$t_start > rel$t_end[i]]
  if (length(later) == 0) NA_real_ else min(later)
}, numeric(1))
rel$in_before <- mapply(in_order_at, rel$session_id, rel$scene, rel$visit, rel$object_id, rel$t_start - 0.001)
rel$in_at_release <- mapply(in_order_at, rel$session_id, rel$scene, rel$visit, rel$object_id, rel$t_end)
rel$check_t <- pmin(rel$t_end + 1.5, ifelse(is.na(rel$next_start), Inf, rel$next_start - 0.001))
rel$in_after <- mapply(in_order_at, rel$session_id, rel$scene, rel$visit, rel$object_id, rel$check_t)
rel$outcome <- ifelse(rel$in_at_release & rel$in_after, "let go inside, stayed in",
               ifelse(rel$in_at_release & !rel$in_after, "let go inside, ended OUTSIDE",
               ifelse(!rel$in_at_release & rel$in_after, "let go outside, dropped in",
                      "let go outside, stayed out")))
# Delay of the exit after release: the dropper moves 0.3-0.7 s after release.
rel$exit_delay_s <- vapply(seq_len(nrow(rel)), function(i) {
  x <- cont[cont$session_id == rel$session_id[i] & cont$scene == rel$scene[i] &
            cont$visit == rel$visit[i] & cont$object_id == rel$object_id[i] &
            cont$event == "container_exit" & cont$t > rel$t_end[i] & cont$t <= rel$check_t[i], ]
  if (nrow(x) == 0) NA_real_ else x$t[1] - rel$t_end[i]
}, numeric(1))
rel$regrab_within_3s <- !is.na(rel$next_start) & rel$next_start - rel$t_end <= 3
# Kind of move from the grab's point of view.
rel$move <- ifelse(!rel$in_before & rel$in_after, "put in",
            ifelse(rel$in_before & rel$in_after, "moved inside box",
            ifelse(rel$in_before & !rel$in_after, "taken out", "never in box")))
write.csv(rel, file.path(out_dir, "releases.csv"), row.names = FALSE)

# ---- 8. Container events that happen without a hand --------------------------
# A container event is "hand-driven" when the object is held or was let go
# less than 1 s before (the drop). Otherwise something else moved it
# (another package pushed it, physics, counter flicker).
cont$held_or_drop <- vapply(seq_len(nrow(cont)), function(i) {
  g <- g_ok[g_ok$session_id == cont$session_id[i] & g_ok$scene == cont$scene[i] &
            g_ok$visit == cont$visit[i] & g_ok$object_id == cont$object_id[i], ]
  if (nrow(g) == 0) return(FALSE)
  te <- ifelse(is.na(g$t_end), Inf, g$t_end)
  any(cont$t[i] >= g$t_start - 0.05 & cont$t[i] <= te + 1.0)
}, logical(1))
# Flicker: exit and re-enter (or enter and exit) of one object within 0.5 s.
cont <- cont[order(cont$session_id, cont$scene, cont$visit, cont$object_id, cont$t), ]
cont$flip_05s <- FALSE
for (i in seq_len(nrow(cont))[-1]) {
  if (cont$session_id[i] == cont$session_id[i - 1] && cont$scene[i] == cont$scene[i - 1] &&
      cont$visit[i] == cont$visit[i - 1] && cont$object_id[i] == cont$object_id[i - 1] &&
      cont$event[i] != cont$event[i - 1] && cont$t[i] - cont$t[i - 1] <= 0.5)
    cont$flip_05s[i] <- TRUE
}
write.csv(cont[, setdiff(names(cont), "data")], file.path(out_dir, "container_events.csv"), row.names = FALSE)

# ---- 9. Per scene x position: grab churn numbers -----------------------------
key <- unique(rbind(
  g_ok[g_ok$scene %in% picking_scenes, c("session_id", "label", "scene", "pos_now")]))
churn <- do.call(rbind, lapply(seq_len(nrow(key)), function(i) {
  k <- key[i, ]
  g <- g_ok[g_ok$session_id == k$session_id & g_ok$scene == k$scene & g_ok$pos_now %in% k$pos_now, ]
  r <- rel[rel$session_id == k$session_id & rel$scene == k$scene & rel$pos_now %in% k$pos_now, ]
  c2 <- cont[cont$session_id == k$session_id & cont$scene == k$scene & cont$pos_now %in% k$pos_now, ]
  data.frame(k, grabs = nrow(g), objects = length(unique(g$object_id)),
             grabs_per_object = round(nrow(g) / max(1, length(unique(g$object_id))), 2),
             regrab_3s = sum(g$regrab_3s), regrab_10s = sum(g$regrab_10s),
             released_inside = sum(r$in_at_release),
             inside_then_out = sum(r$outcome == "let go inside, ended OUTSIDE"),
             container_enters = sum(c2$event == "container_enter"),
             container_exits = sum(c2$event == "container_exit"),
             exits_without_hand = sum(c2$event == "container_exit" & !c2$held_or_drop),
             flips_05s = sum(c2$flip_05s))
}))
churn$scene_short <- scene_short[churn$scene]
churn <- churn[order(churn$label, match(churn$scene, scene_order), churn$pos_now), ]
write.csv(churn, file.path(out_dir, "churn_by_position.csv"), row.names = FALSE)

# ---- 10. Red-button presses ---------------------------------------------------
cf <- ev[ev$event %in% c("confirm_pressed", "confirm_ignored"), ]
cf <- cf[order(cf$session_id, cf$t), ]
cf$required_full  <- vapply(cf$data, get_field, numeric(1), "line", "required_full")
cf$placed_full    <- vapply(cf$data, get_field, numeric(1), "line", "placed_full")
cf$required_loose <- vapply(cf$data, get_field, numeric(1), "line", "required_loose")
cf$placed_loose   <- vapply(cf$data, get_field, numeric(1), "line", "placed_loose")
cf$empf_kz        <- vapply(cf$data, function(d) as.character(get_field(d, "line", "empf_kz")), character(1))
# Material of the press's order line (#174: ComplexScene swaps its lines in
# layout "040", so never assume a material per position).
cf$line_item      <- vapply(cf$data, function(d) as.character(get_field(d, "line", "item")), character(1))
cf$n_loose_places <- vapply(cf$data, function(d)
  if (is.null(d$placement)) NA_real_ else length(d$placement$loose), numeric(1))
cf$since_enter_s  <- vapply(cf$data, get_field, numeric(1), "time_since_scene_enter_s")
# Gap to the previous press (pressed or ignored) in the same visit, active s.
cf$gap_prev_s <- NA_real_
cf$container_events_since_prev <- NA_real_
for (i in seq_len(nrow(cf))[-1]) {
  if (cf$session_id[i] == cf$session_id[i - 1] && cf$scene[i] == cf$scene[i - 1] &&
      cf$visit[i] == cf$visit[i - 1]) {
    cf$gap_prev_s[i] <- active_between(cf$session_id[i], cf$t[i - 1], cf$t[i])
    cf$container_events_since_prev[i] <- sum(cont$session_id == cf$session_id[i] &
      cont$scene == cf$scene[i] & cont$visit == cf$visit[i] &
      cont$t > cf$t[i - 1] & cont$t < cf$t[i])
  }
}
# Counts were right but the press was still wrong (e.g. placement).
cf$counts_right_but_wrong <- cf$event == "confirm_pressed" & cf$correct %in% "FALSE" &
  !is.na(cf$placed_full) & cf$placed_full == cf$required_full &
  (is.na(cf$required_loose) | (!is.na(cf$placed_loose) & cf$placed_loose == cf$required_loose))
cf$scene_short <- scene_short[cf$scene]
write.csv(cf[, setdiff(names(cf), "data")], file.path(out_dir, "confirms.csv"), row.names = FALSE)

# ---- 11. What lies in the order box at the last (correct) press --------------
# Checks T6: the last press only rates the current line. Were the goods of the
# earlier position still in the box?
in_box_at <- function(s, sc, v, tt) {
  x <- cont[cont$session_id == s & cont$scene == sc & cont$visit == v & cont$t <= tt, ]
  if (nrow(x) == 0) return(character(0))
  x <- x[order(x$t), ]
  last <- tapply(x$event, x$object_id, function(e) e[length(e)])
  names(last)[last == "container_enter"]
}
final <- cf[cf$event == "confirm_pressed" & cf$correct %in% "TRUE" &
            cf$scene %in% c("MixedPickingScene", "ComplexScene"), ]
final <- final[final$position == ave(final$position, final$session_id, final$scene, FUN = max), ]
# Material of position k in a visit = line.item of its presses (logged).
pos_material <- function(s, sc, v, k) {
  m <- cf$line_item[cf$event == "confirm_pressed" & cf$session_id == s & cf$scene == sc &
                      cf$visit == v & cf$position %in% k & !is.na(cf$line_item)]
  if (length(m) == 0) NA_character_ else m[1]
}
final_box <- do.call(rbind, lapply(seq_len(nrow(final)), function(i) {
  objs <- in_box_at(final$session_id[i], final$scene[i], final$visit[i], final$t[i])
  mats <- ev$material[match(objs, ev$object_id)]
  form <- ifelse(grepl("ring|clamp", objs, ignore.case = TRUE), "loose", "full")
  m1 <- pos_material(final$session_id[i], final$scene[i], final$visit[i], 1)
  m2 <- pos_material(final$session_id[i], final$scene[i], final$visit[i], 2)
  # A position without a press has no material: its counts are unknown (NA), not 0.
  n_in <- function(m, f) if (is.na(m)) NA_integer_ else sum(mats %in% m & form == f)
  data.frame(label = final$label[i], scene = final$scene[i], position = final$position[i],
             pos1_material = m1, pos2_material = m2,
             pos1_full_in_box = n_in(m1, "full"),
             pos1_loose_in_box = n_in(m1, "loose"),
             pos2_full_in_box = n_in(m2, "full"),
             pos2_loose_in_box = n_in(m2, "loose"))
}))
if (!is.null(final_box))
  write.csv(final_box, file.path(out_dir, "final_box_contents.csv"), row.names = FALSE)

# ---- 12. Time per order position ---------------------------------------------
# Start of position 1 = scene enter; start of position k = arrival of its
# storage box (station_move_arrived storage_in), or completion of k-1.
pos_time <- do.call(rbind, lapply(which(visits$scene %in% picking_scenes), function(j) {
  v <- visits[j, ]
  e <- ev[ev$session_id == v$session_id & ev$scene == v$scene & ev$visit %in% v$visit, ]
  n_pos <- max(c(1, e$position[e$event == "confirm_pressed"]), na.rm = TRUE)
  do.call(rbind, lapply(seq_len(n_pos), function(k) {
    start <- if (k == 1) v$t_enter else {
      a <- e$t[e$event == "station_move_arrived" & e$what %in% "storage_in" & e$position %in% k]
      if (length(a) > 0) a[1] else e$t[e$event == "position_completed" & e$position %in% (k - 1)][1]
    }
    done <- e$t[e$event == "position_completed" & e$position %in% k]
    end  <- if (length(done) > 0) done[1] else v$t_end
    p <- e[e$event == "confirm_pressed" & e$position %in% k, ]
    data.frame(label = v$label, layout = v$layout, scene = v$scene, position = k,
               completed = length(done) > 0,
               active_s = round(active_between(v$session_id, start, end), 1),
               presses = nrow(p), wrong_presses = sum(p$correct %in% "FALSE"))
  }))
}))
write.csv(pos_time, file.path(out_dir, "time_per_position.csv"), row.names = FALSE)

# ---- 13. Concept tour steps ----------------------------------------------------
ts <- ev[ev$event == "tour_step", ]
ts <- ts[order(ts$session_id, ts$t), ]
ts$step_s <- NA_real_; ts$is_back <- FALSE
for (i in seq_len(nrow(ts))) {
  nxt <- ev$t[ev$session_id == ts$session_id[i] & ev$scene == ts$scene[i] &
              ev$visit == ts$visit[i] & ev$event %in% c("tour_step", "scene_exit") &
              ev$t > ts$t[i]]
  if (length(nxt) > 0) ts$step_s[i] <- active_between(ts$session_id[i], ts$t[i], min(nxt))
  if (i > 1 && ts$session_id[i] == ts$session_id[i - 1] &&
      ts$step_index[i] < ts$step_index[i - 1] &&
      !(ts$step_index[i - 1] == max(ts$step_index) && ts$step_index[i] == 1)) ts$is_back[i] <- TRUE
  # (11 -> 1 is the "Von vorn" restart, not a Back press)
}
# The last step (material_flow) lasts through the lane demo and the quiz,
# so its time is not reading time: we keep it but mark it.
ts$last_in_visit <- !duplicated(paste(ts$session_id, ts$visit), fromLast = TRUE)
write.csv(ts[, c("label", "layout", "step_index", "step_id", "t", "step_s", "is_back", "last_in_visit")],
          file.path(out_dir, "tour_steps.csv"), row.names = FALSE)

# ---- 14. Quiz answers -----------------------------------------------------------
qa <- ev[ev$event == "quiz_answer", ]
quiz <- aggregate(cbind(attempts = rep(1, nrow(qa))) ~ label + quiz + question_id, data = qa, FUN = sum)
first <- qa[qa$attempt_q %in% 1, c("label", "quiz", "question_id", "correct")]
names(first)[4] <- "first_correct"
quiz <- merge(quiz, first, all.x = TRUE)
write.csv(quiz, file.path(out_dir, "quiz.csv"), row.names = FALSE)

# ---- 15. Data / logging checks --------------------------------------------------
summ_files <- list.files(data_dir, pattern = "_summary\\.csv$", full.names = TRUE)
summ <- do.call(rbind, lapply(summ_files, function(f)
  read.csv(f, colClasses = "character", na.strings = "")))
cmp <- do.call(rbind, lapply(seq_len(nrow(summ)), function(i) {
  s <- summ[i, ]
  e <- ev[ev$session_id == s$session_id & ev$scene %in% s$scene & ev$visit %in% as.numeric(s$scene_visit), ]
  n_conf <- if (s$scene == "ConceptScene") sum(e$event == "quiz_answer") else sum(e$event == "confirm_pressed")
  data.frame(label = sessions$label[match(s$session_id, sessions$session_id)], scene = s$scene,
             grabs_summary = as.numeric(s$grabs), grabs_events = sum(e$event == "grab_start"),
             enters_summary = as.numeric(s$container_enters), enters_events = sum(e$event == "container_enter"),
             exits_summary = as.numeric(s$container_exits), exits_events = sum(e$event == "container_exit"),
             confirms_summary = as.numeric(s$confirm_count), confirms_events = n_conf,
             pauses_summary = as.numeric(s$pauses), pauses_events = sum(e$event == "app_pause"))
}))
cmp$mismatch <- with(cmp,
  (!is.na(grabs_summary) & grabs_summary != grabs_events) |
  (!is.na(enters_summary) & enters_summary != enters_events) |
  (!is.na(exits_summary) & exits_summary != exits_events) |
  (!is.na(confirms_summary) & confirms_summary != confirms_events) |
  (!is.na(pauses_summary) & pauses_summary != pauses_events))
write.csv(cmp, file.path(out_dir, "summary_vs_events.csv"), row.names = FALSE)

# Visits in the events with no summary row (e.g. the visit that crashed).
visits$has_summary_row <- paste(visits$session_id, visits$scene, visits$visit) %in%
  paste(summ$session_id, summ$scene, summ$scene_visit)

checks <- do.call(rbind, lapply(sessions$session_id, function(s) {
  e <- ev[ev$session_id == s, ]
  tt <- e$t[!is.na(e$t)]
  g <- grabs[grabs$session_id == s, ]
  data.frame(label = sessions$label[sessions$session_id == s],
             bad_json_lines = bad_lines[[e$file[1]]],
             t_goes_backwards = sum(diff(tt) < 0),
             end_reason = sessions$end_reason[sessions$session_id == s],
             open_grabs = sum(is.na(g$t_end) & !g$snap_artefact),
             grab_end_without_start = sum(orphan_ends$session_id == s),
             snap_artefacts_dropped = sum(g$snap_artefact),
             negative_durations = sum(e$dur_field < 0, na.rm = TRUE) + sum(e$paused_field < 0, na.rm = TRUE),
             unity_error_lines = sum(e$event == "unity_error"),
             visits_without_summary_row = sum(!visits$has_summary_row[visits$session_id == s]),
             summary_mismatch_rows = sum(cmp$mismatch[cmp$label == sessions$label[sessions$session_id == s]]),
             build_dirty = grepl("dirty", sessions$build[sessions$session_id == s]))
}))
excl_file <- file.path(base_dir, "data_exclusions.csv")
excl <- if (file.exists(excl_file)) read.csv(excl_file, colClasses = "character") else NULL
checks$in_data_exclusions <- sessions$session_id %in% excl$session_id
write.csv(visits, file.path(out_dir, "visits.csv"), row.names = FALSE)
write.csv(checks, file.path(out_dir, "data_checks.csv"), row.names = FALSE)

# =============================== CHARTS =========================================
# Order sessions by date, so the oldest build is at the top of every chart.
sess_ord <- sessions[order(sessions$start_utc), ]
sess_lab <- paste0(sess_ord$label, "  [", sess_ord$layout, " / ", sess_ord$build, "]")

# ---- Chart 1: where the time goes, per session --------------------------------
png(file.path(out_dir, "01_time_per_scene.png"), width = 1300, height = 650, res = 110)
par(mar = c(5, 17, 4, 12), xpd = NA)
mat <- sapply(sess_ord$session_id, function(s)
  sapply(scene_order, function(sc) sum(visits$active_s[visits$session_id == s & visits$scene == sc]) / 60))
paused <- sapply(sess_ord$session_id, function(s) sum(visits$paused_s[visits$session_id == s]) / 60)
mat <- rbind(mat, paused = paused)
bp <- barplot(mat, horiz = TRUE, xlim = c(0, max(colSums(mat)) * 1.25), col = c(scene_col[scene_order], "#E8E8E8"),
              names.arg = sess_lab, las = 1,
              xlab = "Minutes (active time per scene; light grey = paused / headset off / focus lost)",
              main = "Where does the time go? Active minutes per scene and session")
text(colSums(mat) + 0.3, bp, labels = sess_ord$end_reason, adj = 0, cex = 0.75)
legend("topright", inset = c(-0.22, 0), legend = c(scene_short[scene_order], "paused"),
       fill = c(scene_col[scene_order], "#E8E8E8"), cex = 0.8, bty = "n")
invisible(dev.off())

# ---- Chart 2: what happens after a package is let go -----------------------------
# One bar per session x picking scene; colours = release outcome.
out_levels <- c("let go inside, stayed in", "let go outside, dropped in",
                "let go outside, stayed out", "let go inside, ended OUTSIDE")
out_cols <- c(okabe[["green"]], okabe[["sky"]], okabe[["grey"]], okabe[["red"]])
# Bar name shows the layout: P802 (layout 033) has no PackageDropper yet (#153
# came with layout 038), so it is the "before" picture.
rel$bar <- paste0(rel$label, " ", scene_short[rel$scene], " [", rel$layout, "]")
bar_ord <- unique(rel$bar[order(match(rel$label, sess_ord$label), match(rel$scene, scene_order))])
tab <- table(factor(rel$outcome, levels = out_levels), factor(rel$bar, levels = bar_ord))
png(file.path(out_dir, "02_release_outcomes.png"), width = 1300, height = 750, res = 110)
par(mar = c(5, 12, 4, 2), xpd = NA)
bp <- barplot(tab, horiz = TRUE, xlim = c(0, max(colSums(tab)) * 1.35), col = out_cols, las = 1,
              xlab = "Number of releases (package let go by the hand)",
              main = "After letting go: does the package stay in the order container?")
text(colSums(tab) + 1, bp, paste0("n=", colSums(tab), ", out after in: ", tab[4, ]), adj = 0, cex = 0.7)
legend("bottomright", legend = out_levels, fill = out_cols, cex = 0.8, bty = "n")
invisible(dev.off())

# ---- Chart 2b: how long after the release does the package leave the box? ------
# If almost all exits come ~0.3 s after release, it is the PackageDropper
# (0.3 s timer) that moves the package, not the hand.
dl <- rel$exit_delay_s[rel$outcome == "let go inside, ended OUTSIDE" & !is.na(rel$exit_delay_s)]
if (length(dl) > 0) {
  png(file.path(out_dir, "02b_exit_delay_after_release.png"), width = 1000, height = 600, res = 110)
  par(mar = c(5, 5, 4, 2))
  hist(dl, breaks = seq(0, 1.5, by = 0.025), col = okabe[["red"]], border = "white",
       xlab = "Seconds from release (grab_end) to container_exit",
       ylab = "Number of releases",
       main = paste0("Packages let go inside the order container that then left it (n=", length(dl), ")"))
  abline(v = 0.3, lty = 2)
  mtext("dashed line = 0.3 s, the PackageDropper delay before it drops a released package", side = 3, line = 0.3, cex = 0.8)
  invisible(dev.off())
}

# ---- Chart 3: grabs per package, per scene and position --------------------------
ch <- churn
ch$x <- paste0(ch$scene_short, ifelse(ch$scene == "BasicPickingScene", "", paste0(" pos ", ch$pos_now)))
x_levels <- c("Basic", "Mixed pos 1", "Mixed pos 2", "Complex pos 1", "Complex pos 2")
ch <- ch[ch$x %in% x_levels, ]
ch$xi <- match(ch$x, x_levels)
png(file.path(out_dir, "03_grabs_per_package.png"), width = 1200, height = 700, res = 110)
par(mar = c(5, 5, 4, 2))
set.seed(1)
jit <- ch$xi + runif(nrow(ch), -0.15, 0.15)
plot(jit, ch$grabs_per_object, xlim = c(0.5, 5.5), ylim = c(0, max(ch$grabs_per_object) * 1.15),
     xaxt = "n", pch = 19, col = scene_col[ch$scene], cex = 1.4,
     xlab = "Scene and order position", ylab = "Grabs per package touched (1 = each package once)",
     main = "How often is the same package grabbed again? (one dot = one session)")
axis(1, at = 1:5, labels = x_levels)
abline(h = 1, lty = 2, col = "grey40")
text(jit, ch$grabs_per_object, ch$label, pos = 4, cex = 0.7)
n_per_x <- table(factor(ch$x, levels = x_levels))
mtext(paste0("n=", n_per_x), side = 1, at = 1:5, line = 2.2, cex = 0.75)
invisible(dev.off())

# ---- Chart 4: order-container count over time (Mixed and Complex) ----------------
# Step line = packages in the order container (count_after); ticks = grabs;
# vertical lines = red-button presses (green correct, red wrong).
plot_count_timeline <- function(scene, file) {
  ss <- sess_ord$session_id[sess_ord$session_id %in% cont$session_id[cont$scene == scene]]
  if (length(ss) == 0) return(invisible())
  png(file.path(out_dir, file), width = 1300, height = 260 * length(ss) + 120, res = 110)
  par(mfrow = c(length(ss), 1), mar = c(3, 5, 2.5, 1), oma = c(2, 0, 2, 0))
  for (s in ss) {
    v <- visits[visits$session_id == s & visits$scene == scene, ][1, ]
    x <- cont[cont$session_id == s & cont$scene == scene, ]; x <- x[order(x$t), ]
    g <- g_ok[g_ok$session_id == s & g_ok$scene == scene, ]
    p <- cf[cf$session_id == s & cf$scene == scene, ]
    xr <- c(0, v$t_end - v$t_enter)
    plot(c(0, x$t - v$t_enter), c(0, x$count_after), type = "s", lwd = 2, col = okabe[["blue"]],
         xlim = xr, ylim = c(0, max(x$count_after, 1) + 1), xlab = "", ylab = "packages in box",
         main = paste0(sessions$label[sessions$session_id == s], " (layout ",
                       sessions$layout[sessions$session_id == s], "): ", sum(x$event == "container_enter"),
                       " enters, ", sum(x$event == "container_exit"), " exits, ", nrow(g), " grabs"),
         cex.main = 0.95)
    rug(g$t_start - v$t_enter, col = okabe[["grey"]], ticksize = 0.06)
    abline(v = p$t[p$event == "confirm_pressed" & p$correct %in% "TRUE"] - v$t_enter, col = okabe[["green"]], lwd = 2)
    abline(v = p$t[p$event == "confirm_pressed" & p$correct %in% "FALSE"] - v$t_enter, col = okabe[["red"]], lwd = 2, lty = 2)
    abline(v = p$t[p$event == "confirm_ignored"] - v$t_enter, col = okabe[["orange"]], lwd = 1, lty = 3)
  }
  mtext("Seconds since scene enter (wall clock, pauses included)", side = 1, outer = TRUE)
  mtext(paste(scene_short[scene], ": packages in the order container over time (grey ticks = grabs;",
              "green = correct press, red dashed = wrong press, orange dotted = ignored press)"),
        side = 3, outer = TRUE, cex = 0.85)
  invisible(dev.off())
}
plot_count_timeline("MixedPickingScene", "04a_box_count_timeline_mixed.png")
plot_count_timeline("ComplexScene", "04b_box_count_timeline_complex.png")

# ---- Chart 5: red-button presses over time ---------------------------------------
pp <- cf[cf$scene %in% picking_scenes, ]
pp$since <- pp$t - visits$t_enter[match(paste(pp$session_id, pp$scene, pp$visit),
                                        paste(visits$session_id, visits$scene, visits$visit))]
png(file.path(out_dir, "05_confirm_presses.png"), width = 1400, height = 650, res = 110)
par(mfrow = c(1, 3), mar = c(5, 6, 3, 1), oma = c(0, 0, 2.5, 0))
sl <- sess_ord$label[sess_ord$label %in% pp$label]
for (sc in picking_scenes) {
  x <- pp[pp$scene == sc, ]
  y <- match(x$label, sl)
  plot(NA, xlim = c(0, max(c(60, x$since), na.rm = TRUE) * 1.1), ylim = c(0.5, length(sl) + 0.5),
       yaxt = "n", xlab = "Seconds since scene enter (wall clock)", ylab = "", main = scene_short[sc])
  axis(2, at = seq_along(sl), labels = sl, las = 1)
  col <- ifelse(x$event == "confirm_ignored", okabe[["orange"]],
         ifelse(x$correct %in% "TRUE", okabe[["green"]], okabe[["red"]]))
  pch <- ifelse(x$event == "confirm_ignored", 2, ifelse(x$correct %in% "TRUE", 19, 4))
  points(x$since, y, col = col, pch = pch, cex = 1.6, lwd = 2)
  lab <- ifelse(x$event == "confirm_ignored", x$reason, ifelse(is.na(x$error_codes), "", x$error_codes))
  text(x$since, y, lab, pos = 3, cex = 0.6, col = col)
}
mtext("Red-button presses: green dot = correct, red x = wrong (with error code), orange triangle = ignored",
      side = 3, outer = TRUE, cex = 0.9)
invisible(dev.off())

# ---- Chart 6: time per order position ----------------------------------------------
pt <- pos_time
pt$x <- paste0(scene_short[pt$scene], ifelse(pt$scene == "BasicPickingScene", "", paste0(" pos ", pt$position)))
pt <- pt[pt$x %in% x_levels, ]
pt$xi <- match(pt$x, x_levels)
png(file.path(out_dir, "06_time_per_position.png"), width = 1200, height = 700, res = 110)
par(mar = c(5, 5, 4, 2))
set.seed(2)
jit <- pt$xi + runif(nrow(pt), -0.15, 0.15)
plot(jit, pt$active_s, xlim = c(0.5, 5.5), ylim = c(0, max(pt$active_s, na.rm = TRUE) * 1.1),
     xaxt = "n", pch = ifelse(pt$completed, 19, 1), col = scene_col[pt$scene], cex = 1.4,
     xlab = "Scene and order position (open circle = not completed)",
     ylab = "Active seconds (pauses removed)",
     main = "How long does each order position take? (one dot = one session)")
axis(1, at = 1:5, labels = x_levels)
text(jit, pt$active_s, paste0(pt$label, " (", pt$presses, ")"), pos = 4, cex = 0.65)
mtext(paste0("n=", table(factor(pt$x, levels = x_levels))), side = 1, at = 1:5, line = 2.2, cex = 0.75)
mtext("(number) = red-button presses in that position; Complex pos 1 includes the 3 quiz questions",
      side = 3, line = 0.3, cex = 0.75)
invisible(dev.off())

# ---- Chart 7: Concept tour step times ------------------------------------------
tsr <- ts[!ts$last_in_visit & !is.na(ts$step_s), ]
step_levels <- unique(ts$step_id[order(ts$step_index)])
tsr$xi <- match(tsr$step_id, step_levels)
png(file.path(out_dir, "07_tour_step_times.png"), width = 1300, height = 700, res = 110)
par(mar = c(9, 5, 4, 2))
set.seed(3)
jit <- tsr$xi + runif(nrow(tsr), -0.2, 0.2)
plot(jit, tsr$step_s, log = "y", xlim = c(0.5, length(step_levels) + 0.5), xaxt = "n",
     pch = ifelse(tsr$is_back, 17, 19), col = adjustcolor(okabe[["blue"]], 0.7), cex = 1.2,
     xlab = "", ylab = "Active seconds on the step (log scale)",
     main = "Concept tour: seconds per step (triangle = reached by 'Zurück'); last step left out")
axis(1, at = seq_along(step_levels), labels = paste0(seq_along(step_levels), " ", step_levels), las = 2, cex.axis = 0.8)
slow <- tsr$step_s > 60
text(jit[slow], tsr$step_s[slow], tsr$label[slow], pos = 4, cex = 0.65)
n_back <- sum(ts$is_back)
mtext(paste0("Back presses in all sessions: ", n_back, ";  n sessions with a tour: ",
             length(unique(ts$session_id))), side = 3, line = 0.3, cex = 0.8)
invisible(dev.off())

# ---- Chart 8: quiz attempts until right -------------------------------------------
qz <- quiz
qz$q <- paste0(qz$quiz, ": ", qz$question_id)
q_levels <- unique(qz$q[order(qz$quiz != "concept", qz$q)])
s_levels <- sess_ord$label[sess_ord$label %in% qz$label]
m <- matrix(NA, length(q_levels), length(s_levels), dimnames = list(q_levels, s_levels))
for (i in seq_len(nrow(qz))) m[qz$q[i], qz$label[i]] <- qz$attempts[i]
png(file.path(out_dir, "08_quiz_attempts.png"), width = 1100, height = 650, res = 110)
par(mar = c(4, 17, 4, 2))
cols <- c("1" = "#C7E9C0", "2" = "#FDD49E", "3" = "#FC8D59", "4" = "#D7301F")
image(seq_along(s_levels), seq_along(q_levels), t(pmin(m, 4)), col = cols, breaks = c(0.5, 1.5, 2.5, 3.5, 4.5),
      xaxt = "n", yaxt = "n", xlab = "Session", ylab = "",
      main = "Quiz: answers needed until right (1 = right first time)")
axis(1, at = seq_along(s_levels), labels = s_levels)
axis(2, at = seq_along(q_levels), labels = q_levels, las = 1, cex.axis = 0.8)
for (i in seq_along(q_levels)) for (j in seq_along(s_levels))
  if (!is.na(m[i, j])) text(j, i, m[i, j])
invisible(dev.off())

# ---- Console summary ----------------------------------------------------------------
cat("Sessions:", nrow(sessions), " events:", nrow(ev), "\n")
cat("Grabs:", nrow(grabs), " snap artefacts dropped:", sum(grabs$snap_artefact), "\n")
cat("Releases in picking scenes:", nrow(rel), "\n"); print(table(rel$outcome, rel$scene))
cat("\nExit delay after release when it ended outside (s):\n")
print(summary(rel$exit_delay_s[rel$outcome == "let go inside, ended OUTSIDE"]))
cat("\nWritten to", normalizePath(out_dir), "\n")
print(list.files(out_dir))
