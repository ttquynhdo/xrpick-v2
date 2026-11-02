using System.Collections.Generic;

/// <summary>
/// #19, docs/decisions/016: the division question asked before position 1 of ComplexScene. Answers and
/// answer options are derived from the order line, so the order numbers live only in orderLines.
/// Distractors stand for the usual mistakes: rounding up, the rest left in the carton, all as loose pieces.
/// </summary>
public static class RemainderQuiz
{
    public static int FullCartons(OrderLine line) => line.soll / line.vpe;

    public static int LoosePieces(OrderLine line) => line.soll % line.vpe;

    public static IReadOnlyList<int> CartonOptions(OrderLine line)
    {
        int cartons = FullCartons(line);
        return cartons == 0 ? new[] { 0, 1, 2 } : new[] { cartons - 1, cartons, cartons + 1 };
    }

    public static IReadOnlyList<int> LooseOptions(OrderLine line)
    {
        int rest = LoosePieces(line);
        int[] candidates = { rest, line.vpe - rest, line.soll, rest + line.vpe, rest + 1 };

        var options = new List<int>();
        foreach (int candidate in candidates)
        {
            if (options.Count < 3 && !options.Contains(candidate))
            {
                options.Add(candidate);
            }
        }

        options.Sort();
        return options;
    }
}
