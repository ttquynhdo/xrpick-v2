/// <summary>What to do with one error message: write it, and how many repeats were swallowed before it.</summary>
public readonly struct ErrorDecision
{
    public ErrorDecision(bool write, int foldedCount, bool capReachedNow)
    {
        Write = write;
        FoldedCount = foldedCount;
        CapReachedNow = capReachedNow;
    }

    public bool Write { get; }

    public int FoldedCount { get; }

    public bool CapReachedNow { get; }
}
