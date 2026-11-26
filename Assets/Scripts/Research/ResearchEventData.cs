/// <summary>
/// Builds the data object of the header and unity_error lines. Pure C#, so the field
/// names and values can be checked in EditMode tests without a running session.
/// </summary>
public static class ResearchEventData
{
    // 2 = log contract v2 (decision 021).
    public const int SchemaVersion = 2;

    // Station layout: "019" = three lanes (decision 019); "025" = plus the ConceptScene of decisions 024/025
    // (three lanes in Concept, tour with Zurück, #80); "029" = plus the Concept containers that count and hint
    // (decision 030, #91/#100: Ist follows the order container, concept_hint_* events); "032" = Concept picks from the storage container (decision 032, #92: grabbable
    // storage cartons, container_* in Concept for order and storage, quiz label_by_matnr); "033" = picking scenes wait 4 s
    // after a correct confirm before the containers ride (decision 033, #99); "034" = Empf Kz 003 places shown with one
    // carton per place and loose-carton marks (decision 037, #108), result card at the order box, Menu highlight instead
    // of the Menu sentence (#109), box signs in one pose (#111); "035" = slower Concept lane demo (8 s wait, slower
    // boxes) and reworded tour steps 1, 5, 6, 9 (#118); "036" = WelcomeScene in two steps (decision 038, #121/#124:
    // objectives first, „Zu den Aufgaben“ → welcome_tasks_opened, step frame, card 3 „Lektion wählen“); "037" = read far,
    // touch near (decision 040, #129/#130: Concept step text, action line and quiz question at the screen, buttons and
    // A/B/C lower near the hands; Complex worked example and question at the screen); "038" = #157: Mixed EKZ place
    // outlines from the start (#152), cartons leave snap places cleanly and snap when let go above a free place (#140),
    // packages let go in the air drop straight down or return to where they were grabbed (#153); "039" = one text rule
    // (decision 041, #155): shorter shelf cards, Concept with 2 step cards and the tour opening by itself after 2 s,
    // Concept and Complex questions on the A/B/C cluster near the hands, tour step 8 without action line; "040" = #168:
    // a package let go over a container lies on top only on a thin package or with >= 50 % of its base carried, else it
    // is pushed aside by the overlap; its top never above the rim; lifting a package lets those lying on it fall; and
    // #173 (same build): Mixed SCHRAUBE B bags are flat clear pouches with the screws visible; #174 (same build):
    // Complex position 1 = SCHLAUCHSCHELLE B 152/50, position 2 = RING D 53/25 in pouches, quiz with the new numbers,
    // trays instead of opened cartons; #175 (same build): Welcome practice carton, pouch and rings, not logged;
    // #169 (same build): the red button takes diagonal presses (no event changes).
    // "041" = decision 043 (P811 headset fixes, one build): #177 flat canvas buttons (Concept tour and quiz, Complex
    // remainder quiz, Welcome „Zu den Aufgaben“) take presses that first touch the button's edge (no event changes);
    // #180: a ring or clamp let go onto a package near the rim lies on it, up to 1 cm above the rim (no event changes);
    // #181: short result-card bodies, same keys (feedback_key unchanged);
    // #178: pouches 14 x 14 x 4.5 cm, stacked in the storage containers (new object_ids, no event changes).
    // R tells logs apart by it.
    public const string LayoutVersion = "041";

    // Event groups this build emits; R treats a missing group as NA, not 0 (#29 adds interaction, #30 pose/perf).
    public static readonly string[] EventGroups = { "session", "outcome", "interaction" };

    /// <summary>Header value when ResearchSettings.autoStart started the session.</summary>
    public const string StartModeAuto = "auto";

    /// <summary>Header value when other code (the #27 keypad) called StartSession.</summary>
    public const string StartModeManual = "manual";

    public static JsonFields Header(string appVersion, string build, string deviceModel, string osVersion,
        string participantId, string sessionId, string sessionStartMode, string language,
        string inputModeAtStart, double? targetRefreshRate)
    {
        return new JsonFields()
            .Add("schema_version", SchemaVersion)
            .Add("layout_version", LayoutVersion)
            .AddStringList("event_groups", EventGroups)
            .Add("app_version", appVersion)
            .Add("build", build)
            .Add("device_model", deviceModel)
            .Add("os_version", osVersion)
            .Add("participant_id", participantId)
            .Add("session_id", sessionId)
            .Add("session_start_mode", sessionStartMode)
            .Add("language", language)
            .Add("input_mode_at_start", inputModeAtStart)
            .Add("target_refresh_rate", targetRefreshRate);
    }

    /// <summary>
    /// The session_start data. resume_accepted is null when no resume point was offered
    /// (resumeOfferedScene null), so "declined" and "never asked" stay distinguishable.
    /// </summary>
    public static JsonFields SessionStart(string resumeOfferedScene, bool resumeAccepted)
    {
        return SessionStart(resumeOfferedScene, resumeAccepted, false);
    }

    /// <summary>
    /// Same, plus ended_session_warning: true when the "already finished" warning was shown and the
    /// facilitator started anyway, false otherwise.
    /// </summary>
    public static JsonFields SessionStart(string resumeOfferedScene, bool resumeAccepted, bool endedSessionWarning)
    {
        return new JsonFields()
            .Add("resume_offered_scene", resumeOfferedScene)
            .Add("resume_accepted", resumeOfferedScene != null ? resumeAccepted : (bool?)null)
            .Add("ended_session_warning", endedSessionWarning);
    }

    /// <summary>
    /// One unity_error line. <paramref name="errorCount"/> is n_errors: how many errors the line
    /// stands for (1 for a first occurrence, folded_count + 1 after the fold window,
    /// folded_count for a line that only reports folded repeats).
    /// </summary>
    public static JsonFields UnityError(string type, string message, string stack, int foldedCount, int errorCount)
    {
        return new JsonFields()
            .Add("type", type)
            .Add("message", message)
            .Add("stack", stack)
            .Add("folded_count", foldedCount)
            .Add("n_errors", errorCount);
    }

    /// <summary>The line that reports repeats still folded when a visit closes: no stack, n_errors = folded.</summary>
    public static JsonFields DrainedError(string type, string message, int foldedCount)
    {
        return UnityError(type, message, null, foldedCount, foldedCount);
    }
}
