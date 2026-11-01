using System;

/// <summary>
/// Hold-to-confirm timer for the facilitator (#44). Pure C#, so time is passed in by the caller.
/// A press completes once it has been held for holdSeconds; releasing earlier cancels it.
/// </summary>
public sealed class HoldGesture
{
    private readonly double holdSeconds;
    private readonly double showProgressAfterSeconds;
    private double pressTime;
    private bool completedThisPress;

    /// <param name="holdSeconds">How long the button must be held to complete.</param>
    /// <param name="showProgressAfterSeconds">Hold time before the progress indicator appears, so quick taps show nothing.</param>
    public HoldGesture(double holdSeconds, double showProgressAfterSeconds)
    {
        this.holdSeconds = holdSeconds;
        this.showProgressAfterSeconds = showProgressAfterSeconds;
    }

    public bool IsHolding { get; private set; }

    public void Press(double t)
    {
        IsHolding = true;
        pressTime = t;
        // A new press starts over, so it can complete once more.
        completedThisPress = false;
    }

    public void Release()
    {
        IsHolding = false;
    }

    /// <summary>Hold progress from 0 to 1. Always 0 when not holding.</summary>
    public double Progress(double t)
    {
        if (!IsHolding)
        {
            return 0;
        }

        if (holdSeconds <= 0)
        {
            return 1;
        }

        return Math.Clamp((t - pressTime) / holdSeconds, 0, 1);
    }

    public bool ShouldShowProgress(double t)
    {
        return IsHolding && t - pressTime >= showProgressAfterSeconds;
    }

    /// <summary>True once per press, the first time the hold reaches holdSeconds.</summary>
    public bool Completed(double t)
    {
        if (!IsHolding || completedThisPress || t - pressTime < holdSeconds)
        {
            return false;
        }

        completedThisPress = true;
        return true;
    }
}
