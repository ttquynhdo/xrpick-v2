/// <summary>
/// #182: state of the „Hilfe“ button. Pure C#, so time is passed in by the caller (no per-frame code here).
/// A press is taken only while the button is not waiting; it then waits until a rated attempt, a scene change or
/// <see cref="DefaultTimeoutSeconds"/> have passed (design 2026-11-26-help-button-design.md).
/// </summary>
public sealed class HelpRequestState
{
    public const double DefaultTimeoutSeconds = 60.0;

    private readonly double timeoutSeconds;
    private double requestedAt;

    public HelpRequestState(double timeoutSeconds = DefaultTimeoutSeconds)
    {
        this.timeoutSeconds = timeoutSeconds;
    }

    public bool IsWaiting { get; private set; }

    /// <summary>True when the press is taken (the button was not waiting); false while waiting.</summary>
    public bool TryRequest(double now)
    {
        if (IsWaiting)
            return false;

        IsWaiting = true;
        requestedAt = now;
        return true;
    }

    /// <summary>Opens the button again; true only when it was waiting (so a reset is logged once).</summary>
    public bool Reset()
    {
        if (!IsWaiting)
            return false;

        IsWaiting = false;
        return true;
    }

    public bool IsTimedOut(double now)
    {
        return IsWaiting && now - requestedAt >= timeoutSeconds;
    }
}
