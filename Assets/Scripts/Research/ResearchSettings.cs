using UnityEngine;

/// <summary>
/// Research logger settings, loaded from Resources/ResearchSettings. Until the participant
/// keypad exists (#27), the logger starts a session at app start with the configured test ID.
/// </summary>
[CreateAssetMenu(fileName = "ResearchSettings", menuName = "XRPick/Research Settings")]
public sealed class ResearchSettings : ScriptableObject
{
    public const string ResourcePath = "ResearchSettings";

    [Header("Session start")]
    [Tooltip("Start a session automatically at app start (until #27 adds the keypad).")]
    [SerializeField] private bool autoStart = true;

    [Tooltip("Participant ID for the automatic start: P + 3 digits. P000 and P900-P999 log to research/practice/, P800-P899 to research/pilot/.")]
    [SerializeField] private string autoStartParticipantId = "P000";

    [Header("Developer")]
    [Tooltip("Editor only: skip session_end when Play stops, to test crash recovery. Ignored in builds.")]
    [SerializeField] private bool devSkipSessionEndOnQuit = false;

    public bool AutoStart => autoStart;

    public string AutoStartParticipantId => autoStartParticipantId;

    public bool DevSkipSessionEndOnQuit => devSkipSessionEndOnQuit;
}
