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

    /// <summary>
    /// Errors the written line stands for (its n_errors): the swallowed repeats plus this one.
    /// 0 when nothing is written.
    /// </summary>
    public int ErrorCount => Write ? FoldedCount + 1 : 0;
}
