/// <summary>
/// Builds the data object of the header and unity_error lines. Pure C#, so the field
/// names and values can be checked in EditMode tests without a running session.
/// </summary>
public static class ResearchEventData
{
    // 2 = log contract v2 (decision 021).
    public const int SchemaVersion = 2;

    // Station layout of decision 019 (three lanes); R assigns head-direction zones per layout.
    public const string LayoutVersion = "019";

    // Event groups this build emits; R treats a missing group as NA, not 0 (#29 adds interaction, #30 pose/perf).
    public static readonly string[] EventGroups = { "session", "outcome" };

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
