using TMPro;
using UnityEngine;
using UnityEngine.UI;

// #27: participant keypad in WelcomeScene. The course content stays hidden until a
// research session has started for the typed code (decision 004).
public class ParticipantEntryController : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string ConfirmQuestionFormat = "Ist der Code {participant_id} richtig?";                // entry.confirm.question
    private const string ResumeQuestionFormat = "Kurs von {participant_id} fortsetzen?";                 // entry.resume.question
    private const string ResumeWhereFormat = "Unterbrochen in: {scene_name}";                           // entry.resume.where
    private const string WarningFinishedFormat = "Für {participant_id} gibt es bereits eine abgeschlossene Sitzung."; // entry.warning.finished
    private const string StartError = "Die Sitzung konnte nicht gestartet werden. Bitte starten Sie die App neu.";  // entry.error.start

    [Header("Panels")]
    [SerializeField] private GameObject keypadPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject resumePanel;
    [SerializeField] private GameObject warningPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private TMP_Text resumeText;
    [SerializeField] private TMP_Text warningText;
    // Must NOT be a child of the four entry panels: it stays visible after they are hidden.
    [Tooltip("Must NOT be a child of the four entry panels, or it is hidden together with them.")]
    [SerializeField] private TMP_Text errorText;

    [Header("Controls")]
    [SerializeField] private Button okButton;
    // Optional: dimmed together with okButton, because the button tint only reaches its background.
    [SerializeField] private TMP_Text okLabel;

    // The poke canvas (surface, collider, raycast image). Hidden once a session runs so no
    // invisible slab stays in the workspace; kept on the error path so errorText stays visible.
    [SerializeField] private GameObject entrySurface;

    // One badge on the keypad, one on the confirm panel; shown only for test codes.
    [SerializeField] private GameObject[] testModeBadges;

    [Header("Course")]
    [SerializeField] private GameObject[] courseRoots;
    [SerializeField] private SceneMenuController sceneMenu;

    private const float DisabledLabelAlpha = 0.3f;

    private readonly ParticipantCodeBuffer buffer = new();

    private string pendingCode;
    private string resumeScene;
    private bool started;
    private bool broken;

    private void Start()
    {
        // Fail closed: the course stays hidden unless a session is already running.
        bool sessionActive = ResearchLogger.IsSessionActive;
        if (!sessionActive)
            SetCourseVisible(false);

        if (RequiredReference.IsMissing(keypadPanel, nameof(keypadPanel), this)
            | RequiredReference.IsMissing(confirmPanel, nameof(confirmPanel), this)
            | RequiredReference.IsMissing(resumePanel, nameof(resumePanel), this)
            | RequiredReference.IsMissing(warningPanel, nameof(warningPanel), this)
            | RequiredReference.IsMissing(codeDisplay, nameof(codeDisplay), this)
            | RequiredReference.IsMissing(confirmText, nameof(confirmText), this)
            | RequiredReference.IsMissing(resumeText, nameof(resumeText), this)
            | RequiredReference.IsMissing(warningText, nameof(warningText), this)
            | RequiredReference.IsMissing(errorText, nameof(errorText), this)
            | RequiredReference.IsMissing(okButton, nameof(okButton), this)
            | RequiredReference.IsMissing(entrySurface, nameof(entrySurface), this)
            | RequiredReference.IsMissing(sceneMenu, nameof(sceneMenu), this)
            | !HasCourseRoots())
        {
            broken = true;
            return;
        }

        errorText.text = string.Empty;

        // A session started earlier (e.g. Editor auto-start in another scene): never show the keypad.
        if (sessionActive)
        {
            started = true;
            HideEntryPanels();
            entrySurface.SetActive(false);
            SetCourseVisible(true);
            return;
        }

        ShowKeypad();
    }

    private bool HasCourseRoots()
    {
        if (courseRoots == null || courseRoots.Length == 0)
        {
            Debug.LogError($"{nameof(ParticipantEntryController)} on '{gameObject.name}': {nameof(courseRoots)} is empty, nothing would be gated.", this);
            return false;
        }

        bool valid = true;
        for (int i = 0; i < courseRoots.Length; i++)
        {
            if (courseRoots[i] != null)
                continue;

            Debug.LogError($"{nameof(ParticipantEntryController)} on '{gameObject.name}': {nameof(courseRoots)}[{i}] is not assigned.", this);
            valid = false;
        }

        return valid;
    }

    private bool Locked => started || broken;

    public void PressDigit(int digit)
    {
        if (Locked)
            return;

        buffer.AddDigit(digit);
        RefreshKeypad();
    }

    public void PressDelete()
    {
        if (Locked)
            return;

        buffer.DeleteLast();
        RefreshKeypad();
    }

    public void PressOk()
    {
        if (Locked || !buffer.IsComplete)
            return;

        pendingCode = buffer.Code;
        confirmText.text = ConfirmQuestionFormat.Replace("{participant_id}", pendingCode);
        SetBadges(ResearchIds.IsTestId(pendingCode));
        ShowOnly(confirmPanel);
    }

    // Back to the keypad with the typed code kept, so one digit can be fixed.
    public void PressCorrect()
    {
        if (Locked)
            return;

        pendingCode = null;
        resumeScene = null;
        ShowKeypad();
    }

    public void PressYes()
    {
        if (Locked || pendingCode == null)
            return;

        string code = pendingCode;

        if (ResearchLogger.TryFindResumePoint(code, out string scene)
            && LessonNames.TryGetDisplayName(scene, out string lessonName)
            && Application.CanStreamedLevelBeLoaded(scene))
        {
            resumeScene = scene;
            resumeText.text = ResumeQuestionFormat.Replace("{participant_id}", code)
                + "\n" + ResumeWhereFormat.Replace("{scene_name}", lessonName);
            ShowOnly(resumePanel);
            return;
        }

        if (!ResearchIds.IsTestId(code) && ResearchLogger.HasEndedSession(code))
        {
            warningText.text = WarningFinishedFormat.Replace("{participant_id}", code);
            ShowOnly(warningPanel);
            return;
        }

        Begin(code, null, false, null);
    }

    public void PressResumeContinue()
    {
        if (Locked || pendingCode == null || resumeScene == null)
            return;

        if (!Application.CanStreamedLevelBeLoaded(resumeScene))
        {
            // Never log "accepted" when the scene cannot load.
            Debug.LogWarning($"Resume scene '{resumeScene}' cannot be loaded; starting the course without resuming.", this);
            Begin(pendingCode, resumeScene, false, null);
            return;
        }

        Begin(pendingCode, resumeScene, true, resumeScene);
    }

    public void PressResumeRestart()
    {
        if (Locked || pendingCode == null || resumeScene == null)
            return;

        Begin(pendingCode, resumeScene, false, null);
    }

    public void PressWarningCorrect()
    {
        // PressCorrect also clears pendingCode and resumeScene.
        PressCorrect();
    }

    public void PressWarningContinue()
    {
        if (Locked || pendingCode == null)
            return;

        Begin(pendingCode, null, false, null);
    }

    private void Begin(string code, string offeredScene, bool accepted, string loadScene)
    {
        // Set first so a second press cannot start another session.
        started = true;

        bool ok = offeredScene == null
            ? ResearchLogger.StartSession(code)
            : ResearchLogger.StartSession(code, offeredScene, accepted);

        // Course content opens only for a started session that carries the typed code.
        ok = ok && ResearchLogger.ParticipantId == code;

        if (!ok)
        {
            // No retry loop: the session cannot be recovered without an app restart.
            errorText.text = StartError;
            HideEntryPanels();
            SetCourseVisible(false);
            return;
        }

        errorText.text = string.Empty;
        HideEntryPanels();
        entrySurface.SetActive(false);

        if (loadScene != null)
        {
            if (!sceneMenu.OpenScene(loadScene))
                SetCourseVisible(true);
            return;
        }

        SetCourseVisible(true);
    }

    private void ShowKeypad()
    {
        RefreshKeypad();
        ShowOnly(keypadPanel);
    }

    private void RefreshKeypad()
    {
        codeDisplay.text = buffer.Display;
        okButton.interactable = buffer.IsComplete;
        if (okLabel != null)
            okLabel.alpha = buffer.IsComplete ? 1f : DisabledLabelAlpha;
        SetBadges(buffer.IsComplete && ResearchIds.IsTestId(buffer.Code));
    }

    private void ShowOnly(GameObject panel)
    {
        keypadPanel.SetActive(panel == keypadPanel);
        confirmPanel.SetActive(panel == confirmPanel);
        resumePanel.SetActive(panel == resumePanel);
        warningPanel.SetActive(panel == warningPanel);
    }

    private void HideEntryPanels()
    {
        ShowOnly(null);
        SetBadges(false);
    }

    private void SetBadges(bool visible)
    {
        if (testModeBadges == null)
            return;

        foreach (GameObject badge in testModeBadges)
        {
            if (badge != null)
                badge.SetActive(visible);
        }
    }

    private void SetCourseVisible(bool visible)
    {
        if (courseRoots == null)
            return;

        foreach (GameObject root in courseRoots)
        {
            if (root != null)
                root.SetActive(visible);
        }
    }
}
