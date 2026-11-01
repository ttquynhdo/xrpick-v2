using System.Collections.Generic;

/// <summary>
/// Pause sources tracked separately, so two signals that mean the same thing (Unity app
/// focus and OVR input focus) can't end each other's pause. The log shows the design's
/// labels (focus_lost, pause, hmd_unmounted).
/// </summary>
public static class PauseCauses
{
    public const string AppFocus = "app_focus";
    public const string InputFocus = "input_focus";
    public const string AppPause = "app_pause";
    public const string Hmd = "hmd";

    public const string FocusLostLabel = "focus_lost";
    public const string PauseLabel = "pause";
    public const string HmdUnmountedLabel = "hmd_unmounted";

    /// <summary>Logged label of one source; an unknown source keeps its own name.</summary>
    public static string Label(string source)
    {
        switch (source)
        {
            case AppFocus:
            case InputFocus:
                return FocusLostLabel;
            case AppPause:
                return PauseLabel;
            case Hmd:
                return HmdUnmountedLabel;
            default:
                return source;
        }
    }

    /// <summary>Labels of the sources in their order, each label once.</summary>
    public static List<string> Labels(IReadOnlyList<string> sources)
    {
        var labels = new List<string>();
        if (sources == null)
        {
            return labels;
        }

        foreach (string source in sources)
        {
            string label = Label(source);
            if (!labels.Contains(label))
            {
                labels.Add(label);
            }
        }

        return labels;
    }
}
