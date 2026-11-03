using System.Collections.Generic;

/// <summary>
/// #14, docs/decisions/006 §7: the screen shows only the first error of an OrderResult (reason + what to do)
/// and one generic line if there are more. Never prints target numbers. Material names come from the
/// order lines (WMS/label names), never from keys or enum names. Texts come from the Loc table (#54).
/// </summary>
public static class OrderFeedback
{
    // {name} = material name. german-reviewer (#14): "Material {name}" keeps "zu wenig" grammatical for any
    // name; never "Rest" (WMS label).
    private static readonly Dictionary<OrderErrorCode, string> Keys = new()
    {
        { OrderErrorCode.Empty, "order.feedback.empty" },
        { OrderErrorCode.WrongMaterial, "order.feedback.wrong_material" },
        { OrderErrorCode.TooFew, "order.feedback.too_few" },
        { OrderErrorCode.TooMany, "order.feedback.too_many" },
        { OrderErrorCode.MissingLoose, "order.feedback.missing_loose" },
        { OrderErrorCode.ExtraLoose, "order.feedback.extra_loose" }
    };

    private const string MoreErrorsKey = "order.feedback.more_errors";
    // #41, decision 003: quantity is right but a carton lies loose (EKZ 3). No count, no place.
    private const string Ekz3PlacementKey = "order.feedback.ekz3_loose";
    private const string WrongTitleKey = "order.feedback.wrong_title";

    public static string Title() => Loc.Get(WrongTitleKey);

    // Reason + what to do for the first error, plus the "more errors" line when needed. Empty when correct.
    public static string Explain(OrderResult result)
    {
        if (result.Correct)
            return string.Empty;

        OrderError first = result.Errors[0];

        // WRONG_TYPE is reserved (A2) and the quiz codes never come from OrderRule: show only the generic line.
        if (!Keys.TryGetValue(first.Code, out string key))
        {
            UnityEngine.Debug.LogWarning($"OrderFeedback: no sentence for {OrderErrorCodes.ToLogName(first.Code)}.");
            return Loc.Get(MoreErrorsKey);
        }

        // EMPTY and WRONG_MATERIAL name no material ({name} only in the quantity sentences).
        string sentence = NamesMaterial(first.Code)
            ? Loc.Get(key, ("name", NameOf(result, first.Item)))
            : Loc.Get(key);

        if (result.Errors.Count > 1)
        {
            sentence += "\n" + Loc.Get(MoreErrorsKey);
        }

        return sentence;
    }

    // #21: at a later position the container still holds the material of earlier positions, so "the container
    // is empty" would be wrong; say what is missing for this position instead (the result code stays EMPTY).
    public static string ExplainEmptyPosition(OrderLine line)
    {
        return Loc.Get(Keys[OrderErrorCode.TooFew], ("name", line != null ? line.materialName : string.Empty));
    }

    public static string ExplainEkz3Placement() => Loc.Get(Ekz3PlacementKey);

    private static bool NamesMaterial(OrderErrorCode code) =>
        code != OrderErrorCode.Empty && code != OrderErrorCode.WrongMaterial;

    private static string NameOf(OrderResult result, string materialKey)
    {
        foreach (OrderLineResult line in result.Lines)
        {
            if (line.Line.materialKey == materialKey)
                return line.Line.materialName;
        }

        return materialKey;
    }
}
