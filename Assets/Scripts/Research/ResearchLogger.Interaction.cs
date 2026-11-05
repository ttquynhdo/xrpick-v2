using System;

/// <summary>
/// Interaction events of #29 (decision 021, research-logging-spec §4.1, §4.4). Scene scripts call these
/// unconditionally; without a running session or open scene visit every call is a silent no-op.
/// </summary>
public sealed partial class ResearchLogger
{
    // via of the next scene_enter; set by the scene script right before it loads a scene.
    private static string pendingVia;

    /// <summary>The next scene load is caused by via (one of the InteractionEvents.Via* values).</summary>
    public static void SetNextSceneVia(string via)
    {
        pendingVia = IsSessionActive ? via : null;
    }

    /// <summary>First pointer selects a package (objectId stable across runs, from the scene hierarchy).</summary>
    public static void LogGrabStart(string objectId, string materialKey)
    {
        LogInteraction("grab_start", i =>
        {
            i.GrabStarted(objectId, instance.SceneActiveSeconds());
            return InteractionEvents.GrabStart(objectId, materialKey);
        });
    }

    /// <summary>Last pointer released the package.</summary>
    public static void LogGrabEnd(string objectId, string materialKey)
    {
        LogInteraction("grab_end",
            i => InteractionEvents.GrabEnd(objectId, materialKey, i.GrabEnded(objectId, instance.SceneActiveSeconds())));
    }

    /// <summary>The order container's set of packages actually changed; countAfter = packages in it now.</summary>
    public static void LogContainerChange(bool entered, string objectId, string materialKey, int countAfter)
    {
        LogInteraction(entered ? "container_enter" : "container_exit", i =>
        {
            if (entered)
            {
                i.ContainerEntered();
            }
            else
            {
                i.ContainerExited();
            }

            return InteractionEvents.Container(InteractionEvents.ContainerOrder, objectId, materialKey, countAfter);
        });
    }

    /// <summary>
    /// #100: a ConceptScene hint tag appeared beside a package (shown) or went away (cleared: the package was taken out
    /// or picked up, or a hint on another package replaced it). Leaving the scene while a tag is shown logs nothing.
    /// </summary>
    public static void LogConceptHint(bool shown, string container, string reason, string objectId,
        string materialKey, string tourStepId)
    {
        Log(shown ? "concept_hint_shown" : "concept_hint_cleared",
            InteractionEvents.ConceptHint(container, reason, objectId, materialKey, tourStepId));
    }

    /// <param name="blocked">The item was locked by the research lock (#42); the scene does not change.</param>
    public static void LogMenuSelect(string targetScene, bool blocked)
    {
        Log("menu_select", InteractionEvents.MenuSelect(targetScene, blocked));
    }

    /// <summary>Facilitator end control (#44): opened = the 3 s hold showed the confirmation; else Fortsetzen.</summary>
    public static void LogEndControl(bool opened)
    {
        Log(opened ? "end_control_opened" : "end_control_cancelled", new JsonFields());
    }

    private static string TakePendingVia()
    {
        string via = pendingVia;
        pendingVia = null;
        return via;
    }

    private static void LogInteraction(string eventName, Func<VisitInteraction, JsonFields> build)
    {
        if (!IsSessionActive || !instance.visitOpen || instance.writeFailed)
        {
            return;
        }

        JsonFields data;
        try
        {
            data = build(instance.interaction);
        }
        catch (Exception e)
        {
            instance.StopAfterException(eventName, e);
            return;
        }

        instance.WriteEvent(eventName, data);
    }
}
