using System.Collections.Generic;

/// <summary>
/// #19, docs/decisions/016: the division question asked before position 1 of ComplexScene. Answers and
/// answer options are derived from the order line, so the order numbers live only in orderLines.
/// Distractors stand for the usual mistakes: rounding up, the rest left in the carton, all as loose pieces.
/// #123, docs/decisions/039: a third question asks how the goods go into the order container; the right answer
/// follows the line's Empf Kz (000 → unordered allowed, as concept.quiz.ekz_000), the distractors are the EKZ 3 rule
/// and the separator rule of EKZ 4–6, both wrong for 000.
/// </summary>
public static class RemainderQuiz
{
    // Answer keys of the placement question, in button order A, B, C (logged as quiz_answer.selected). The right
    // answer for 000 sits in the middle, and the two distractors are worded differently („Nur …“, „Immer …“), so
    // neither its place nor its wording gives it away (data-analyst review of #123).
    public const string PlacementOrdered = "ordered";
    public const string PlacementSeparator = "separator";
    public const string PlacementPoured = "poured";

    private const string UnorderedEmpfKz = "000";

    public static readonly IReadOnlyList<string> PlacementOptions =
        new[] { PlacementOrdered, PlacementPoured, PlacementSeparator };

    /// <summary>Empf Kz 000 → unordered allowed; every other class (003 in this app) → one by one and in order.</summary>
    public static string PlacementAnswer(OrderLine line) =>
        line != null && line.empfKz == UnorderedEmpfKz ? PlacementPoured : PlacementOrdered;

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
