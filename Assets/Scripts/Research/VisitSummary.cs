/// <summary>
/// One row of the per-visit summary CSV: the #26 session columns and the #28 outcome columns
/// (null = empty cell). The remaining CSV columns stay empty until #29, #30 and #74 fill them.
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

    // #28 step 2, research-logging-spec §5.
    public int? ConfirmCount;
    public int? ConfirmIgnored;
    public bool? FirstAttemptCorrect;
    public int? AttemptsToCorrect;
    public double? TimeToFirstConfirmS;
    public double? TimeToCorrectS;
    public bool? Completed;
    public string ErrorCodesAll;
}
