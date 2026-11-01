using System.Collections.Generic;

/// <summary>
/// #21, docs/decisions/012: an order worked one position (order line) at a time, as in the real
/// Kommissionierung dialog. A confirm rates only the current line; the caller advances only after a
/// correct result. Pure C#, covered by OrderSequenceTests.
/// </summary>
public sealed class OrderSequence
{
    private readonly IReadOnlyList<OrderLine> lines;
    private int index;

    public OrderSequence(IReadOnlyList<OrderLine> lines)
    {
        this.lines = lines;
    }

    // Position number as the WMS counts it, from 1; 0 when the order has no lines.
    public int Position => lines.Count > 0 ? index + 1 : 0;

    public int Count => lines.Count;

    public bool IsLast => index >= lines.Count - 1;

    public OrderLine CurrentLine => lines.Count > 0 ? lines[index] : null;

    // Material of any other position counts as foreign: it does not belong in this pick.
    public OrderResult CheckCurrent(ContainerContents contents, bool countsLoose)
    {
        var current = new List<OrderLine>();

        if (CurrentLine != null)
        {
            current.Add(CurrentLine);
        }

        return OrderRule.Check(current, contents, countsLoose);
    }

    public bool TryAdvance()
    {
        if (lines.Count == 0 || IsLast)
            return false;

        index++;
        return true;
    }
}
