/// <summary>
/// One row of the per-visit summary CSV. Only the fields #26 measures live here;
/// the remaining CSV columns stay empty until later items fill them.
/// </summary>
public sealed class VisitSummary
{
    public string ParticipantId;
    public string SessionId;
    public string Build;
    public int SchemaVersion;
    public string Scene;
    public int SceneVisit;
    public string EnterUtc;
    public double DurationS;
    public double PausedS;
    public int Pauses;
    public int HmdRemovedCount;
    public int UnityErrors;
}
