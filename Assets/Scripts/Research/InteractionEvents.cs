using System;

/// <summary>
/// Data objects of the interaction events of #29 (decision 021, research-logging-spec §4.1, §4.4). Pure C#, so
/// field names and values are covered by InteractionEventsTests. hand and input are always null (decision 022: #74 dropped).
/// </summary>
public static class InteractionEvents
{
    // scene_enter.via values.
    public const string ViaStart = "start";
    public const string ViaMenu = "menu";
    public const string ViaResume = "resume";
    // No facilitator_end: Beenden ends the session before WelcomeScene loads, so no scene_enter follows.

    // container_enter / container_exit .container: order in Basic/Mixed/Complex; ConceptScene also storage (#92,
    // layout "032"). concept_hint_* .container (#100): order or storage.
    public const string ContainerOrder = "order";
    public const string ContainerStorage = "storage";

    // concept_hint_* .reason (#100, decision 030); same codes as OrderRule's errors.
    public const string HintWrongMaterial = "WRONG_MATERIAL";
    public const string HintTooMany = "TOO_MANY";

    public static JsonFields GrabStart(string objectId, string materialKey)
    {
        return Grab(objectId, materialKey);
    }

    /// <param name="durationS">Active seconds; null when the matching grab_start is not in this visit.</param>
    public static JsonFields GrabEnd(string objectId, string materialKey, double? durationS)
    {
        return Grab(objectId, materialKey)
            .Add("duration_s", durationS.HasValue ? Math.Round(durationS.Value, 3) : (double?)null);
    }

    public static JsonFields Container(string container, string objectId, string materialKey, int countAfter)
    {
        return new JsonFields()
            .Add("container", container)
            .Add("object_id", objectId)
            .Add("material_key", materialKey)
            .Add("count_after", countAfter);
    }

    /// <summary>#100: a hint tag beside a wrong or extra package in a ConceptScene container.</summary>
    /// <param name="tourStepId">Tour step shown at that moment; null before the tour bar opens.</param>
    public static JsonFields ConceptHint(string container, string reason, string objectId, string materialKey,
        string tourStepId)
    {
        return new JsonFields()
            .Add("container", container)
            .Add("reason", reason)
            .Add("object_id", objectId)
            .Add("material_key", materialKey)
            .Add("tour_step_id", tourStepId);
    }

    public static JsonFields MenuSelect(string targetScene, bool blocked)
    {
        return new JsonFields()
            .Add("target_scene", targetScene)
            .Add("blocked", blocked);
    }

    private static JsonFields Grab(string objectId, string materialKey)
    {
        return new JsonFields()
            .Add("object_id", objectId)
            .Add("material_key", materialKey)
            .Add("hand", (string)null)
            .Add("input", (string)null);
    }
}
