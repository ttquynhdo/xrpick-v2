using System.Collections.Generic;

// Until #11/#12 the scenes are still single-language: Basic is German, Mixed and Complex are English.
public enum FeedbackLanguage
{
    English,
    German
}

/// <summary>
/// #14, docs/decisions/006 §7: the screen shows only the first error of an OrderResult (reason + what to do)
/// and one generic line if there are more. Never prints target numbers. Material names come from the
/// order lines (WMS/label names), never from keys or enum names.
/// </summary>
public static class OrderFeedback
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get. {name} = material name.
    private static readonly Dictionary<OrderErrorCode, string> English = new()
    {
        { OrderErrorCode.Empty, "The order container is empty. Put the material of the order into the container." }, // order.feedback.empty
        { OrderErrorCode.WrongMaterial, "The container holds wrong material. Take it out. Check the material number in the WMS." }, // order.feedback.wrong_material
        { OrderErrorCode.TooFew, "Not enough {name}. Calculate again: order quantity ÷ VPE." },                       // order.feedback.too_few
        { OrderErrorCode.TooMany, "Too much {name}. Calculate again: order quantity ÷ VPE." },                        // order.feedback.too_many
        { OrderErrorCode.MissingLoose, "Not enough loose {name}. Calculate again: order quantity minus the pieces in the full cartons." }, // order.feedback.missing_loose
        { OrderErrorCode.ExtraLoose, "Too much loose {name}. Calculate again: order quantity minus the pieces in the full cartons." }      // order.feedback.extra_loose
    };

    // german-reviewer (#14): "Material {name}" keeps "zu wenig" grammatical for any name; never "Rest" (WMS label).
    private static readonly Dictionary<OrderErrorCode, string> German = new()
    {
        { OrderErrorCode.Empty, "Der Auftragsbehälter ist leer. Legen Sie das Material des Auftrags hinein." },
        { OrderErrorCode.WrongMaterial, "Im Auftragsbehälter liegt falsches Material. Nehmen Sie es heraus. Prüfen Sie die Materialnummer im LVS." },
        { OrderErrorCode.TooFew, "Zu wenig Material {name}. Rechnen Sie nach: Sollmenge geteilt durch VPE." },
        { OrderErrorCode.TooMany, "Zu viel Material {name}. Rechnen Sie nach: Sollmenge geteilt durch VPE." },
        { OrderErrorCode.MissingLoose, "Zu wenig loses Material {name}. Rechnen Sie nach: Sollmenge minus Stückzahl der vollen Kartons." },
        { OrderErrorCode.ExtraLoose, "Zu viel loses Material {name}. Rechnen Sie nach: Sollmenge minus Stückzahl der vollen Kartons." }
    };

    private const string MoreErrorsEnglish = "There are more errors.";   // order.feedback.more_errors
    private const string MoreErrorsGerman = "Es gibt noch weitere Fehler.";
    private const string WrongTitleEnglish = "Not correct";                                // order.feedback.wrong_title
    private const string WrongTitleGerman = "Nicht richtig";

    public static string Title(FeedbackLanguage language) =>
        language == FeedbackLanguage.German ? WrongTitleGerman : WrongTitleEnglish;

    // Reason + what to do for the first error, plus the "more errors" line when needed. Empty when correct.
    public static string Explain(OrderResult result, FeedbackLanguage language)
    {
        if (result.Correct)
            return string.Empty;

        OrderError first = result.Errors[0];
        Dictionary<OrderErrorCode, string> table = language == FeedbackLanguage.German ? German : English;

        // WRONG_TYPE is reserved (A2) and the quiz codes never come from OrderRule: show only the generic line.
        if (!table.TryGetValue(first.Code, out string template))
        {
            UnityEngine.Debug.LogWarning($"OrderFeedback: no sentence for {OrderErrorCodes.ToLogName(first.Code)}.");
            return language == FeedbackLanguage.German ? MoreErrorsGerman : MoreErrorsEnglish;
        }

        string sentence = template.Replace("{name}", NameOf(result, first.Item));

        if (result.Errors.Count > 1)
        {
            sentence += "\n" + (language == FeedbackLanguage.German ? MoreErrorsGerman : MoreErrorsEnglish);
        }

        return sentence;
    }

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
