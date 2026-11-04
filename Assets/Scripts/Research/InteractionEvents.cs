using System;

/// <summary>
/// Data objects of the interaction events of #29 (decision 021, research-logging-spec §4.1, §4.4). Pure C#, so
/// field names and values are covered by InteractionEventsTests. hand and input stay null until #74.
/// </summary>
public static class InteractionEvents
{
    // scene_enter.via values.
    public const string ViaStart = "start";
    public const string ViaMenu = "menu";
    public const string ViaResume = "resume";
    // No facilitator_end: Beenden ends the session before WelcomeScene loads, so no scene_enter follows.

    // container_enter / container_exit .container: the order container is the only counted one.
    public const string ContainerOrder = "order";

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
