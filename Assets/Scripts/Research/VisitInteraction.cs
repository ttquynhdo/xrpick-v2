using System.Collections.Generic;

/// <summary>
/// Interaction state of one scene visit (#29): open grabs for grab_end.duration_s and the counts of the
/// summary columns grabs, container_enters, container_exits (research-logging-spec §5). A column stays empty
/// in a scene that cannot produce its event (Welcome's box has no PackageData; Welcome and Concept have no order
/// container), so "not measured" is never written as 0. Pure C#, covered by VisitInteractionTests. Times are
/// active seconds since scene enter.
/// </summary>
public sealed class VisitInteraction
{
    private readonly Dictionary<string, double> grabStartS = new();
    private readonly bool hasGrabs;
    private readonly bool hasContainer;

    private int grabs;
    private int containerEnters;
    private int containerExits;

    public VisitInteraction(string sceneName)
    {
        hasGrabs = sceneName != VisitOutcome.WelcomeScene;
        hasContainer = hasGrabs && sceneName != VisitOutcome.ConceptScene;
    }

    public void GrabStarted(string objectId, double activeS)
    {
        grabs++;
        if (objectId != null)
        {
            grabStartS[objectId] = activeS;
        }
    }

    /// <summary>Active seconds since this object's grab_start in this visit; null when there was none.</summary>
    public double? GrabEnded(string objectId, double activeS)
    {
        if (objectId == null || !grabStartS.TryGetValue(objectId, out double start))
        {
            return null;
        }

        grabStartS.Remove(objectId);
        return activeS - start;
    }

    public void ContainerEntered()
    {
        containerEnters++;
    }

    public void ContainerExited()
    {
        containerExits++;
    }

    public void FillSummary(VisitSummary row)
    {
        row.Grabs = hasGrabs ? grabs : (int?)null;
        row.ContainerEnters = hasContainer ? containerEnters : (int?)null;
        row.ContainerExits = hasContainer ? containerExits : (int?)null;
    }
}
