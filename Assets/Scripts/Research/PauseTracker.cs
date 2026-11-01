using System.Collections.Generic;

/// <summary>
/// Merges overlapping pause causes (focus lost, headset removed, app pause) into one
/// pause interval. Pure C#, so time is passed in by the caller.
/// </summary>
public sealed class PauseTracker
{
    private const string DefaultHmdCause = "hmd_unmounted";

    private readonly string _hmdCause;
    private readonly List<string> _activeCauses = new List<string>();
    private readonly List<string> _causesThisPause = new List<string>();
    private double _pauseStart;
    private double _closedSeconds;
    private bool _hmdInCurrentPause;

    /// <param name="hmdCause">The cause that counts as "headset removed" for HmdRemovedCount.</param>
    public PauseTracker(string hmdCause = DefaultHmdCause)
    {
        _hmdCause = hmdCause;
    }

    public IReadOnlyList<string> ActiveCauses => _activeCauses;

    /// <summary>
    /// Every cause seen during the current pause, or during the last one after it resumed,
    /// in the order they started. Cleared when the next pause starts.
    /// </summary>
    public IReadOnlyList<string> CausesThisPause => _causesThisPause;

    public bool IsPaused => _activeCauses.Count > 0;

    public int PauseCount { get; private set; }

    public int HmdRemovedCount { get; private set; }

    public PauseTransition SetCause(string cause, bool active, double t)
    {
        bool wasPaused = IsPaused;
        bool known = _activeCauses.Contains(cause);

        if (active && !known)
        {
            _activeCauses.Add(cause);
        }
        else if (!active && known)
        {
            _activeCauses.Remove(cause);
        }
        else
        {
            return PauseTransition.None;
        }

        if (!wasPaused && IsPaused)
        {
            _causesThisPause.Clear();
        }

        if (active && !_causesThisPause.Contains(cause))
        {
            _causesThisPause.Add(cause);
        }

        if (active && cause == _hmdCause && !_hmdInCurrentPause)
        {
            _hmdInCurrentPause = true;
            HmdRemovedCount++;
        }

        if (!wasPaused && IsPaused)
        {
            _pauseStart = t;
            PauseCount++;
            return PauseTransition.Paused;
        }

        if (wasPaused && !IsPaused)
        {
            _closedSeconds += t - _pauseStart;
            _hmdInCurrentPause = false;
            return PauseTransition.Resumed;
        }

        return PauseTransition.None;
    }

    /// <summary>Total paused time, including a still-open pause up to <paramref name="now"/>.</summary>
    public double PausedSeconds(double now)
    {
        return IsPaused ? _closedSeconds + (now - _pauseStart) : _closedSeconds;
    }
}
