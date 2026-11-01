using System.Collections.Generic;

/// <summary>
/// The order rule (docs/decisions/005, 006): compares the container with the order lines.
/// Checks every skill on every confirm (it never stops at the first error), so the log can use
/// confirm_count as denominator. Pure C#, covered by the XRPick.Orders.Tests EditMode tests.
/// </summary>
public static class OrderRule
{
    /// <param name="countsLoose">True when the scene counts loose pieces (ComplexScene).</param>
    public static OrderResult Check(IReadOnlyList<OrderLine> lines, ContainerContents contents, bool countsLoose)
    {
        var errors = new List<OrderError>();
        var lineResults = new List<OrderLineResult>();
        var foreign = new List<ForeignMaterial>();
        var orderedKeys = new HashSet<string>();

        foreach (OrderLine line in lines)
        {
            orderedKeys.Add(line.materialKey);
            int placedFull = contents.FullOf(line.materialKey);
            int? placedLoose = countsLoose ? contents.LooseOf(line.materialKey) : (int?)null;
            lineResults.Add(new OrderLineResult(line, placedFull, placedLoose));
        }

        // EMPTY is logged alone (research-logging-spec §6).
        if (contents.IsEmpty)
        {
            errors.Add(new OrderError(OrderErrorCode.Empty, null));
            return new OrderResult(errors, lineResults, foreign);
        }

        // Ordinal key order, so the same container always gives the same list, whatever the trainee did first.
        var foreignKeys = new List<string>();
        foreach (string key in contents.Keys)
        {
            if (!orderedKeys.Contains(key))
            {
                foreignKeys.Add(key);
            }
        }
        foreignKeys.Sort(System.StringComparer.Ordinal);

        foreach (string key in foreignKeys)
        {
            foreign.Add(new ForeignMaterial(key, contents.FullOf(key), contents.LooseOf(key)));
            errors.Add(new OrderError(OrderErrorCode.WrongMaterial, key));
        }

        foreach (OrderLineResult result in lineResults)
        {
            OrderLine line = result.Line;

            if (result.PlacedFull < line.RequiredFull)
            {
                errors.Add(new OrderError(OrderErrorCode.TooFew, line.materialKey));
            }
            else if (result.PlacedFull > line.RequiredFull)
            {
                errors.Add(new OrderError(OrderErrorCode.TooMany, line.materialKey));
            }

            if (result.PlacedLoose.HasValue)
            {
                if (result.PlacedLoose.Value < line.RequiredLoose)
                {
                    errors.Add(new OrderError(OrderErrorCode.MissingLoose, line.materialKey));
                }
                else if (result.PlacedLoose.Value > line.RequiredLoose)
                {
                    errors.Add(new OrderError(OrderErrorCode.ExtraLoose, line.materialKey));
                }
            }
            else if (contents.LooseOf(line.materialKey) > 0)
            {
                // A scene without loose counting still must not accept loose pieces of an ordered material.
                errors.Add(new OrderError(OrderErrorCode.ExtraLoose, line.materialKey));
            }
        }

        SortByPriority(errors);
        return new OrderResult(errors, lineResults, foreign);
    }

    /// <summary>
    /// Developer messages (English) for order lines entered wrongly in the Inspector; empty when all lines are valid.
    /// Without loose counting, Soll must be a whole number of cartons, or the remainder would never be checked.
    /// </summary>
    public static List<string> FindLineProblems(IReadOnlyList<OrderLine> lines, bool countsLoose)
    {
        var problems = new List<string>();
        var keys = new HashSet<string>();

        for (int i = 0; i < lines.Count; i++)
        {
            OrderLine line = lines[i];

            if (string.IsNullOrEmpty(line.materialKey))
            {
                problems.Add($"order line {i}: materialKey is empty.");
            }
            else if (!keys.Add(line.materialKey))
            {
                problems.Add($"order line {i}: materialKey '{line.materialKey}' is used twice.");
            }

            if (line.vpe <= 0)
            {
                problems.Add($"order line {i}: vpe must be greater than 0.");
            }
            else if (!countsLoose && line.soll % line.vpe != 0)
            {
                problems.Add($"order line {i}: soll {line.soll} is not a multiple of vpe {line.vpe}, but this scene does not count loose pieces.");
            }

            if (line.soll <= 0)
            {
                problems.Add($"order line {i}: soll must be greater than 0.");
            }
        }

        return problems;
    }

    // Stable insertion sort: keeps material order inside the same priority.
    private static void SortByPriority(List<OrderError> errors)
    {
        for (int i = 1; i < errors.Count; i++)
        {
            OrderError current = errors[i];
            int j = i - 1;

            while (j >= 0 && OrderErrorCodes.Priority(errors[j].Code) > OrderErrorCodes.Priority(current.Code))
            {
                errors[j + 1] = errors[j];
                j--;
            }

            errors[j + 1] = current;
        }
    }
}
