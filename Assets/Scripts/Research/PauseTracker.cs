using System.Collections.Generic;

/// <summary>
/// Merges overlapping pause causes (focus lost, headset removed, app pause) into one
/// pause interval. Pure C#, so time is passed in by the caller.
/// </summary>
public sealed class PauseTracker
{
    private const string DefaultHmdCause = "hmd_unmounted";

    private readonly string hmdCause;
    private readonly List<string> activeCauses = new List<string>();
    private readonly List<string> causesThisPause = new List<string>();
    private double pauseStart;
    private double closedSeconds;
    private bool hmdInCurrentPause;

    /// <param name="hmdCause">The cause that counts as "headset removed" for HmdRemovedCount.</param>
    public PauseTracker(string hmdCause = DefaultHmdCause)
    {
        this.hmdCause = hmdCause;
    }

    public IReadOnlyList<string> ActiveCauses => activeCauses;

    /// <summary>
    /// Every cause seen during the current pause, or during the last one after it resumed,
    /// in the order they started. Cleared when the next pause starts.
    /// </summary>
    public IReadOnlyList<string> CausesThisPause => causesThisPause;

    public bool IsPaused => activeCauses.Count > 0;

    public int PauseCount { get; private set; }

    public int HmdRemovedCount { get; private set; }

    public PauseTransition SetCause(string cause, bool active, double t)
    {
        bool wasPaused = IsPaused;
        bool known = activeCauses.Contains(cause);

        if (active && !known)
        {
            activeCauses.Add(cause);
        }
        else if (!active && known)
        {
            activeCauses.Remove(cause);
        }
        else
        {
            return PauseTransition.None;
        }

        if (!wasPaused && IsPaused)
        {
            causesThisPause.Clear();
        }

        if (active && !causesThisPause.Contains(cause))
        {
            causesThisPause.Add(cause);
        }

        if (active && cause == hmdCause && !hmdInCurrentPause)
        {
            hmdInCurrentPause = true;
            HmdRemovedCount++;
        }

        if (!wasPaused && IsPaused)
        {
            pauseStart = t;
            PauseCount++;
            return PauseTransition.Paused;
        }

        if (wasPaused && !IsPaused)
        {
            closedSeconds += t - pauseStart;
            hmdInCurrentPause = false;
            return PauseTransition.Resumed;
        }

        return PauseTransition.None;
    }

    /// <summary>Total paused time, including a still-open pause up to <paramref name="now"/>.</summary>
    public double PausedSeconds(double now)
    {
        return IsPaused ? closedSeconds + (now - pauseStart) : closedSeconds;
    }
}
