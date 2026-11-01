using TMPro;
using UnityEngine;
using UnityEngine.UI;

// #44: facilitator control to end the research session in any scene. A 3 s hold on a small
// disc opens a confirm panel; "Beenden" ends the session and returns to the keypad (decision 004 §9).
public class FacilitatorEndControl : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string QuestionText = "Sitzung wirklich beenden?"; // end.confirm.question
    private const string CancelText = "Abbrechen";                   // end.confirm.cancel
    private const string EndText = "Beenden";                        // end.confirm.end
    private const string EndReason = "facilitator";

    private const double HoldSeconds = 3.0;
    private const double ShowProgressAfterSeconds = 0.5;

    [SerializeField] private GameObject confirmPanel;
    [Tooltip("Filled radial/horizontal Image; hidden until the hold has lasted 0.5 s.")]
    [SerializeField] private Image progressIndicator;
    [SerializeField] private SceneMenuController sceneMenu;

    // Optional: the prefab's texts are set here so the strings live in one place.
    [SerializeField] private TMP_Text questionLabel;
    [SerializeField] private TMP_Text cancelLabel;
    [SerializeField] private TMP_Text endLabel;

    private readonly HoldGesture gesture = new(HoldSeconds, ShowProgressAfterSeconds);

    private bool broken;
    private bool ended;
    private bool refsChecked;

    private void Start()
    {
        EnsureRefsChecked();
        if (broken)
            return;

        if (questionLabel != null)
            questionLabel.text = QuestionText;
        if (cancelLabel != null)
            cancelLabel.text = CancelText;
        if (endLabel != null)
            endLabel.text = EndText;

        confirmPanel.SetActive(false);
        SetProgressVisible(false);
    }

    private void EnsureRefsChecked()
    {
        if (refsChecked)
            return;

        refsChecked = true;
        broken = RequiredReference.IsMissing(confirmPanel, nameof(confirmPanel), this)
            | RequiredReference.IsMissing(progressIndicator, nameof(progressIndicator), this)
            | RequiredReference.IsMissing(sceneMenu, nameof(sceneMenu), this);
    }

    private bool Locked => broken || ended;

    public void HoldStart()
    {
        EnsureRefsChecked();
        if (Locked || !ResearchLogger.IsSessionActive || confirmPanel.activeSelf)
            return;

        gesture.Press(Time.realtimeSinceStartupAsDouble);
    }

    public void HoldEnd()
    {
        gesture.Release();
        if (!broken)
            SetProgressVisible(false);
    }

    // Per-frame work only while holding: no allocations.
    private void Update()
    {
        if (!gesture.IsHolding || Locked)
            return;

        if (!ResearchLogger.IsSessionActive)
        {
            gesture.Release();
            SetProgressVisible(false);
            confirmPanel.SetActive(false);
            return;
        }

        double now = Time.realtimeSinceStartupAsDouble;

        if (gesture.Completed(now))
        {
            gesture.Release();
            SetProgressVisible(false);
            confirmPanel.SetActive(true);
            return;
        }

        if (gesture.ShouldShowProgress(now))
        {
            progressIndicator.fillAmount = (float)gesture.Progress(now);
            SetProgressVisible(true);
        }
    }

    public void PressCancel()
    {
        if (Locked)
            return;

        gesture.Release();
        confirmPanel.SetActive(false);
        SetProgressVisible(false);
    }

    public void PressEnd()
    {
        if (Locked)
            return;

        if (!ResearchLogger.IsSessionActive)
        {
            // Nothing to end: just close the panel.
            confirmPanel.SetActive(false);
            return;
        }

        // Set first so a second press cannot end the session twice.
        ended = true;
        gesture.Release();
        SetProgressVisible(false);

        ResearchLogger.EndSession(EndReason);
        sceneMenu.OpenWelcome(true);
    }

    private void SetProgressVisible(bool visible)
    {
        if (progressIndicator != null && progressIndicator.gameObject.activeSelf != visible)
            progressIndicator.gameObject.SetActive(visible);
    }
}
