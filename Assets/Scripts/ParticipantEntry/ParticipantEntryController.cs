using TMPro;
using UnityEngine;
using UnityEngine.UI;

// #27: participant keypad in WelcomeScene. The course content stays hidden until a
// research session has started for the typed code (decision 004).
public class ParticipantEntryController : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string ConfirmQuestionFormat = "Ist {participant_id} richtig?";                        // entry.confirm.question
    private const string ResumeQuestionFormat = "Kurs von {participant_id} fortsetzen?";                 // entry.resume.question
    private const string ResumeWhereFormat = "Abgebrochen in: {scene_name}";                             // entry.resume.where
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
    [SerializeField] private TMP_Text errorText;

    [Header("Controls")]
    [SerializeField] private Button okButton;

    // One badge on the keypad, one on the confirm panel; shown only for test codes.
    [SerializeField] private GameObject[] testModeBadges;

    [Header("Course")]
    [SerializeField] private GameObject[] courseRoots;
    [SerializeField] private SceneMenuController sceneMenu;

    private readonly ParticipantCodeBuffer buffer = new();

    private string pendingCode;
    private string resumeScene;
    private bool started;

    private void Start()
    {
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
            | RequiredReference.IsMissing(sceneMenu, nameof(sceneMenu), this))
        {
            return;
        }

        errorText.text = string.Empty;

        // A session started earlier (e.g. Editor auto-start in another scene): never show the keypad.
        if (ResearchLogger.IsSessionActive)
        {
            started = true;
            HideEntryPanels();
            SetCourseVisible(true);
            return;
        }

        SetCourseVisible(false);
        ShowKeypad();
    }

    public void PressDigit(int digit)
    {
        if (started)
            return;

        buffer.AddDigit(digit);
        RefreshKeypad();
    }

    public void PressDelete()
    {
        if (started)
            return;

        buffer.DeleteLast();
        RefreshKeypad();
    }

    public void PressOk()
    {
        if (started || !buffer.IsComplete)
            return;

        pendingCode = buffer.Code;
        confirmText.text = ConfirmQuestionFormat.Replace("{participant_id}", pendingCode);
        SetBadges(ResearchIds.IsTestId(pendingCode));
        ShowOnly(confirmPanel);
    }

    // Back to the keypad with the typed code kept, so one digit can be fixed.
    public void PressCorrect()
    {
        if (started)
            return;

        ShowKeypad();
    }

    public void PressYes()
    {
        if (started || pendingCode == null)
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
        if (started || pendingCode == null || resumeScene == null)
            return;

        Begin(pendingCode, resumeScene, true, resumeScene);
    }

    public void PressResumeRestart()
    {
        if (started || pendingCode == null || resumeScene == null)
            return;

        Begin(pendingCode, resumeScene, false, null);
    }

    public void PressWarningCorrect()
    {
        PressCorrect();
    }

    public void PressWarningContinue()
    {
        if (started || pendingCode == null)
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
        SetBadges(buffer.IsComplete && ResearchIds.IsTestId(buffer.Code));
        RefreshKeypad();
        ShowOnly(keypadPanel);
    }

    private void RefreshKeypad()
    {
        codeDisplay.text = buffer.Display;
        okButton.interactable = buffer.IsComplete;
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
