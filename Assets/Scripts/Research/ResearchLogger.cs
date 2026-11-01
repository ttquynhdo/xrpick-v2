using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Research log of one session (#26): header, scene visits, pauses and Unity errors as JSON
/// lines, plus one summary CSV row per scene visit. Scene scripts call the static API
/// unconditionally; without a running session every call is a silent no-op.
/// </summary>
public sealed class ResearchLogger : MonoBehaviour
{
    private const int SchemaVersion = ResearchEventData.SchemaVersion;
    private const string ResearchFolder = "research";
    private const string BuildInfoResource = "ResearchBuildInfo";
    private const string UtcFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
    private const int StackLines = 5;

    private static readonly string[] EndReasons = { "completed", "facilitator", "quit" };

    // Lines written right before the headset may sleep or power off: forced to the storage device.
    private static readonly string[] ForceToDiskEvents = { "app_pause", "scene_exit", "session_end" };

    // Errors Unity/OVR print on every Play start without a headset or Link; they say nothing
    // about the trainee's session. Matched as substrings of the message.
    private static readonly string[] NoiseFilter =
    {
        "ErrorFormFactorUnavailable xrGetSystem",
        "OnInstanceCreate returned an error",
        "xrGetInstanceProcAddr failed to get function pointer"
    };

    private static ResearchLogger instance;

    // True while the logger prints its own messages: they are developer errors, not the trainee's.
    private static bool loggingOwnMessage;

    // Settings (asset values or defaults when the asset is missing).
    private bool autoStart = true;
    private string autoStartParticipantId = "P000";
    private bool devSkipSessionEndOnQuit;

    // Session state.
    private bool sessionActive;
    private bool writing;
    private bool writeFailed;
    private SessionFiles files;
    private string participantId;
    private string sessionId;
    private string build;
    private double sessionStartRealtime;
    private PauseTracker pauses;
    private double pauseStartT;
    private ErrorFolder errors;
    private readonly Dictionary<string, string> errorTypes = new();
    private readonly Dictionary<string, int> visitsPerScene = new();

    // Open scene visit.
    private string sceneName;
    private int sceneVisit;
    private bool visitOpen;
    private string visitEnterUtc;
    private double visitEnterT;
    private double visitEnterPausedS;
    private int visitEnterPauseCount;
    private int visitEnterHmdCount;
    private int visitUnityErrors;

    public static bool IsSessionActive => instance != null && instance.sessionActive;

    public static string ParticipantId => IsSessionActive ? instance.participantId : null;

    private static string ResearchRoot => Path.Combine(Application.persistentDataPath, ResearchFolder);

    /// <summary>
    /// Starts a session for a valid participant ID ("P" + 3 digits). Returns false for an invalid
    /// ID or when a session is already running.
    /// </summary>
    public static bool StartSession(string participantId)
    {
        return StartSession(participantId, ResearchEventData.StartModeManual, null, false, false);
    }

    /// <summary>
    /// Same, and records the resume offer in session_start: the scene offered (null when none) and
    /// whether the participant accepted it.
    /// </summary>
    public static bool StartSession(string participantId, string resumeOfferedScene, bool resumeAccepted)
    {
        return StartSession(participantId, resumeOfferedScene, resumeAccepted, false);
    }

    /// <summary>
    /// Same, and records in session_start whether the facilitator started despite the
    /// "already finished" warning (ended_session_warning).
    /// </summary>
    public static bool StartSession(string participantId, string resumeOfferedScene, bool resumeAccepted,
        bool endedSessionWarning)
    {
        return StartSession(participantId, ResearchEventData.StartModeManual, resumeOfferedScene, resumeAccepted,
            endedSessionWarning);
    }

    /// <summary>True when an earlier session of this participant ended with session_end. False for invalid IDs.</summary>
    public static bool HasEndedSession(string participantId)
    {
        if (!ResearchIds.IsValid(participantId))
        {
            return false;
        }

        string exclude = IsSessionActive ? instance.sessionId : string.Empty;
        return SessionFiles.HasEndedSession(ResearchRoot, participantId, exclude);
    }

    private static bool StartSession(string participantId, string startMode, string resumeOfferedScene,
        bool resumeAccepted, bool endedSessionWarning)
    {
        if (!ResearchIds.IsValid(participantId))
        {
            LogOwnError("ResearchLogger: invalid participant ID, expected P + 3 digits.");
            return false;
        }

        if (IsSessionActive)
        {
            LogOwnError("ResearchLogger: a session is already running.");
            return false;
        }

        EnsureInstance().BeginSession(participantId, startMode, resumeOfferedScene, resumeAccepted,
            endedSessionWarning);
        return true;
    }

    /// <summary>Ends the session with reason completed, facilitator or quit. Later calls do nothing.</summary>
    public static void EndSession(string reason)
    {
        if (!IsSessionActive)
        {
            return;
        }

        if (Array.IndexOf(EndReasons, reason) < 0)
        {
            LogOwnError("ResearchLogger: unknown session end reason, expected completed, facilitator or quit.");
            return;
        }

        instance.FinishSession(reason);
    }

    /// <summary>Writes one event line in the current scene visit (for #27-#30).</summary>
    public static void Log(string eventName, JsonFields data = null)
    {
        if (!IsSessionActive || string.IsNullOrEmpty(eventName))
        {
            return;
        }

        instance.WriteEvent(eventName, data);
    }

    /// <summary>
    /// True with the last scene entered when the participant's newest earlier session ended
    /// without session_end (crash). The running session is never considered.
    /// </summary>
    public static bool TryFindResumePoint(string participantId, out string scene)
    {
        scene = null;
        if (!ResearchIds.IsValid(participantId))
        {
            return false;
        }

        string exclude = IsSessionActive ? instance.sessionId : string.Empty;
        return SessionFiles.TryFindResumePoint(ResearchRoot, participantId, exclude, out scene);
    }

    // Domain reload may be off in the Editor: drop a stale reference from the last Play run.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateAtAppStart()
    {
        ResearchLogger logger = EnsureInstance();
        if (AutoStartRule.ShouldAutoStart(logger.autoStart, SceneManager.GetActiveScene().buildIndex))
        {
            StartSession(logger.autoStartParticipantId, ResearchEventData.StartModeAuto, null, false, false);
        }
    }

    private static void LogOwnError(string message)
    {
        loggingOwnMessage = true;
        try
        {
            Debug.LogError(message);
        }
        finally
        {
            loggingOwnMessage = false;
        }
    }

    private static ResearchLogger EnsureInstance()
    {
        if (instance == null)
        {
            var go = new GameObject("ResearchLogger");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ResearchLogger>();
        }

        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        var settings = Resources.Load<ResearchSettings>(ResearchSettings.ResourcePath);
        if (settings != null)
        {
            autoStart = settings.AutoStart;
            autoStartParticipantId = settings.AutoStartParticipantId;
            devSkipSessionEndOnQuit = settings.DevSkipSessionEndOnQuit;
        }

        Application.logMessageReceived += HandleLogMessage;
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        SubscribeOvr(true);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLogMessage;
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        SubscribeOvr(false);
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnApplicationPause(bool paused)
    {
        SetPauseCause(PauseCauses.AppPause, paused);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetPauseCause(PauseCauses.AppFocus, !hasFocus);
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        // Developer option to leave the file open, so the next run reports crash_recovered.
        if (devSkipSessionEndOnQuit)
        {
            return;
        }
#endif
        EndSession("quit");
    }

    private void HandleHmdUnmounted()
    {
        SetPauseCause(PauseCauses.Hmd, true);
    }

    private void HandleHmdMounted()
    {
        SetPauseCause(PauseCauses.Hmd, false);
    }

    private void HandleInputFocusLost()
    {
        SetPauseCause(PauseCauses.InputFocus, true);
    }

    private void HandleInputFocusAcquired()
    {
        SetPauseCause(PauseCauses.InputFocus, false);
    }

    private void SubscribeOvr(bool subscribe)
    {
        // Without a headset the Meta SDK may not be initialised; logging must never throw.
        try
        {
            if (subscribe)
            {
                OVRManager.HMDUnmounted += HandleHmdUnmounted;
                OVRManager.HMDMounted += HandleHmdMounted;
                OVRManager.InputFocusLost += HandleInputFocusLost;
                OVRManager.InputFocusAcquired += HandleInputFocusAcquired;
            }
            else
            {
                OVRManager.HMDUnmounted -= HandleHmdUnmounted;
                OVRManager.HMDMounted -= HandleHmdMounted;
                OVRManager.InputFocusLost -= HandleInputFocusLost;
                OVRManager.InputFocusAcquired -= HandleInputFocusAcquired;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("ResearchLogger: OVR events unavailable (" + e.GetType().Name + ").");
        }
    }

    private void BeginSession(string id, string startMode, string resumeOfferedScene, bool resumeAccepted,
        bool endedSessionWarning)
    {
        participantId = id;
        sessionId = Guid.NewGuid().ToString("D");
        build = ReadBuild();
        pauses = new PauseTracker(PauseCauses.Hmd);
        errors = new ErrorFolder();
        errorTypes.Clear();
        visitsPerScene.Clear();
        writeFailed = false;
        visitOpen = false;
        sceneName = null;
        sceneVisit = 0;

        // Scan before the new header exists, otherwise the new file would be marked as crashed.
        SessionFiles.MarkCrashedSessions(ResearchRoot, sessionId, UtcNow());
        // Time origin after the scan, so the header's t = 0 matches its utc.
        sessionStartRealtime = Time.realtimeSinceStartupAsDouble;
        files = new SessionFiles(ResearchRoot, participantId, sessionId);
        sessionActive = true;

        string activeScene = SceneManager.GetActiveScene().name;
        JsonFields header = ResearchEventData.Header(Application.version, build, SystemInfo.deviceModel,
            SystemInfo.operatingSystem, participantId, sessionId, startMode, ReadInputMode(), ReadRefreshRate());
        WriteLine(SessionFiles.EventLine(0, UtcNow(), participantId, sessionId, null, null, "header", header));
        WriteLine(SessionFiles.EventLine(0, UtcNow(), participantId, sessionId, activeScene, null, "session_start",
            ResearchEventData.SessionStart(resumeOfferedScene, resumeAccepted, endedSessionWarning)));

        OpenVisit(activeScene, null, "start");
    }

    private void FinishSession(string reason)
    {
        // session_end must stay the last line: the crash scan only reads the last parseable line.
        CloseVisit();
        WriteEvent("session_end", new JsonFields().Add("reason", reason));
        sessionActive = false;
    }

    private void HandleActiveSceneChanged(Scene previous, Scene next)
    {
        if (!sessionActive)
        {
            return;
        }

        try
        {
            // The previous Scene struct is often already unloaded (empty name); use the tracked name.
            string from = sceneName;
            CloseVisit();
            OpenVisit(next.name, from, null);
        }
        catch (Exception e)
        {
            StopAfterException("scene change", e);
        }
    }

    private void OpenVisit(string scene, string fromScene, string via)
    {
        visitsPerScene.TryGetValue(scene, out int count);
        count++;
        visitsPerScene[scene] = count;

        double t = Now();
        sceneName = scene;
        sceneVisit = count;
        visitOpen = true;
        visitEnterUtc = UtcNow();
        visitEnterT = t;
        visitEnterPausedS = pauses.PausedSeconds(t);
        visitEnterPauseCount = pauses.PauseCount;
        visitEnterHmdCount = pauses.HmdRemovedCount;
        visitUnityErrors = 0;

        WriteEvent("scene_enter", new JsonFields()
            .Add("from_scene", fromScene)
            .Add("via", via));
    }

    private void CloseVisit()
    {
        if (!visitOpen)
        {
            return;
        }

        // Folded repeats belong to the visit they happened in.
        DrainFoldedErrors();

        double t = Now();
        double pausedS = pauses.PausedSeconds(t) - visitEnterPausedS;
        double durationS = (t - visitEnterT) - pausedS;
        int visitPauses = pauses.PauseCount - visitEnterPauseCount;
        int visitHmdRemoved = pauses.HmdRemovedCount - visitEnterHmdCount;

        // Same values as the summary row, so every #26 summary column can be rebuilt from events.
        WriteEvent("scene_exit", new JsonFields()
            .Add("duration_s", Math.Round(durationS, 3))
            .Add("paused_s", Math.Round(pausedS, 3))
            .Add("pauses", visitPauses)
            .Add("hmd_removed_count", visitHmdRemoved)
            .Add("unity_errors", visitUnityErrors));

        var row = new VisitSummary
        {
            ParticipantId = participantId,
            SessionId = sessionId,
            Build = build,
            SchemaVersion = SchemaVersion,
            Scene = sceneName,
            SceneVisit = sceneVisit,
            EnterUtc = visitEnterUtc,
            DurationS = durationS,
            PausedS = pausedS,
            Pauses = visitPauses,
            HmdRemovedCount = visitHmdRemoved,
            UnityErrors = visitUnityErrors
        };
        if (!writeFailed && !files.AppendSummary(SummaryCsv.FormatRow(row)))
        {
            ReportWriteFailure();
        }

        visitOpen = false;
    }

    private void DrainFoldedErrors()
    {
        foreach (KeyValuePair<string, int> pending in errors.DrainPending())
        {
            errorTypes.TryGetValue(pending.Key, out string type);
            WriteEvent("unity_error", ResearchEventData.DrainedError(type, pending.Key, pending.Value));
        }
    }

    private void SetPauseCause(string source, bool active)
    {
        if (!sessionActive)
        {
            return;
        }

        try
        {
            ApplyPauseCause(source, active);
        }
        catch (Exception e)
        {
            StopAfterException("pause tracking", e);
        }
    }

    private void ApplyPauseCause(string source, bool active)
    {
        double t = Now();
        PauseTransition transition = pauses.SetCause(source, active, t);
        if (transition == PauseTransition.Paused)
        {
            pauseStartT = t;
            WriteEvent("app_pause", new JsonFields().AddStringList("causes", PauseCauses.Labels(pauses.ActiveCauses)));
        }
        else if (transition == PauseTransition.Resumed)
        {
            // Every cause seen during this pause, so hmd_removed_count can be rebuilt from events.
            WriteEvent("app_resume", new JsonFields()
                .Add("paused_s", Math.Round(t - pauseStartT, 3))
                .AddStringList("causes", PauseCauses.Labels(pauses.CausesThisPause)));
        }
    }

    private void HandleLogMessage(string message, string stackTrace, LogType type)
    {
        // The guard drops messages raised while the logger itself is writing or reporting.
        if (!sessionActive || writing || writeFailed || loggingOwnMessage)
        {
            return;
        }

        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
        {
            return;
        }

        if (IsNoise(message))
        {
            return;
        }

        try
        {
            CaptureError(message, stackTrace, type);
        }
        catch (Exception e)
        {
            StopAfterException("error capture", e);
        }
    }

    private void StopAfterException(string what, Exception e)
    {
        // Logging must never break the app; stop writing instead of retrying on every call.
        // A warning, not an error: warnings aren't captured, so this can't loop back into the log.
        writeFailed = true;
        Debug.LogWarning("ResearchLogger: " + what + " failed (" + e.GetType().Name + "); logging stops for this session.");
    }

    private void CaptureError(string message, string stackTrace, LogType type)
    {
        string stripped = ErrorFolder.StripPaths(message) ?? string.Empty;
        visitUnityErrors++;
        ErrorDecision decision = errors.Decide(stripped, Now());
        if (decision.CapReachedNow)
        {
            WriteEvent("unity_error_cap_reached", null);
            return;
        }

        if (!decision.Write)
        {
            return;
        }

        string typeName = TypeName(type);
        errorTypes[stripped] = typeName;
        WriteEvent("unity_error", ResearchEventData.UnityError(typeName, stripped,
            FirstLines(ErrorFolder.StripPaths(stackTrace), StackLines), decision.FoldedCount, decision.ErrorCount));
    }

    private static bool IsNoise(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        foreach (string noise in NoiseFilter)
        {
            if (message.IndexOf(noise, StringComparison.Ordinal) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private void WriteEvent(string eventName, JsonFields data)
    {
        WriteLine(SessionFiles.EventLine(Now(), UtcNow(), participantId, sessionId, sceneName,
            visitOpen ? sceneVisit : (int?)null, eventName, data),
            Array.IndexOf(ForceToDiskEvents, eventName) >= 0);
    }

    private void WriteLine(string line, bool forceToDisk = false)
    {
        if (writeFailed || writing)
        {
            return;
        }

        writing = true;
        try
        {
            if (!files.AppendEvent(line, forceToDisk))
            {
                ReportWriteFailure();
            }
        }
        finally
        {
            writing = false;
        }
    }

    private void ReportWriteFailure()
    {
        writeFailed = true;
        // A warning, not an error: warnings aren't captured, so this can't loop back into the log.
        Debug.LogWarning("ResearchLogger: cannot write the research log; logging stops for this session.");
    }

    private double Now()
    {
        return Time.realtimeSinceStartupAsDouble - sessionStartRealtime;
    }

    private static string UtcNow()
    {
        return DateTime.UtcNow.ToString(UtcFormat, CultureInfo.InvariantCulture);
    }

    private static string ReadBuild()
    {
        if (Application.isEditor)
        {
            return "editor";
        }

        var info = Resources.Load<TextAsset>(BuildInfoResource);
        string text = info != null ? info.text.Trim() : string.Empty;
        return text.Length > 0 ? text : null;
    }

    private static string ReadInputMode()
    {
        try
        {
            OVRInput.Controller active = OVRInput.GetActiveController();
            if ((active & OVRInput.Controller.Hands) != 0)
            {
                return "hands";
            }

            const OVRInput.Controller controllers = OVRInput.Controller.Touch | OVRInput.Controller.Remote
                | OVRInput.Controller.Gamepad;
            return (active & controllers) != 0 ? "controllers" : "none";
        }
        catch (Exception e)
        {
            Debug.LogWarning("ResearchLogger: input mode unavailable (" + e.GetType().Name + ").");
            return null;
        }
    }

    private static double? ReadRefreshRate()
    {
        try
        {
            OVRDisplay display = OVRManager.display;
            if (display == null)
            {
                return null;
            }

            float hz = display.displayFrequency;
            return hz > 0f ? hz : (double?)null;
        }
        catch (Exception e)
        {
            Debug.LogWarning("ResearchLogger: refresh rate unavailable (" + e.GetType().Name + ").");
            return null;
        }
    }

    private static string TypeName(LogType type)
    {
        switch (type)
        {
            case LogType.Exception: return "exception";
            case LogType.Assert: return "assert";
            default: return "error";
        }
    }

    private static string FirstLines(string text, int count)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        string[] lines = text.Split('\n');
        int n = Math.Min(count, lines.Length);
        var kept = new List<string>(n);
        for (int i = 0; i < n; i++)
        {
            string line = lines[i].TrimEnd('\r');
            if (line.Length > 0)
            {
                kept.Add(line);
            }
        }

        return kept.Count > 0 ? string.Join("\n", kept) : null;
    }
}
