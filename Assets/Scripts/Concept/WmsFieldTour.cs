using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #20, docs/decisions/010: walks the trainee through the WMS dialog one field at a time. Each step shows its markers
/// outside the screen border; nothing is drawn on the dialog itself. #78, decision 024: a step about one field writes
/// its text into a caption next to that field; the other steps use the board right of the screen.
/// The "Weiter" control is a poke toggle used as a push button. #80, docs/decisions/024: a fixed bar below the
/// screen holds [Zurück] "Schritt n von 11" [Weiter]; "Zurück" is dimmed and not pressable at step 1.
/// </summary>
public class WmsFieldTour : MonoBehaviour
{
    // Loc table UI (#54).
    private const string ProgressKey = "concept.tour.progress";
    private const string NextKey = "concept.tour.next";
    private const string RestartKey = "concept.tour.restart";
    private const string BackKey = "concept.tour.back";
    private const float DimmedAlpha = 0.5f;
    private const string StepKeyPrefix = "concept.tour.";

    [Serializable]
    public class Step
    {
        // #55: the texts are the Loc keys concept.tour.<id>.title / .body.
        [Tooltip("Stable id, e.g. soll (for #28 logging later); also names the Loc keys concept.tour.<id>.title/.body.")]
        public string id;

        [Tooltip("Shown only during this step; placed outside the monitor border.")]
        public GameObject[] markers;

        // #78, decision 024: a caption next to the field (one of the markers). Empty = the text goes on the board.
        [Tooltip("Caption title next to the field; leave empty to use the board right of the screen.")]
        public TMP_Text captionTitle;

        public TMP_Text captionBody;
    }

    [SerializeField] private Step[] steps;

    [Header("Board")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Tooltip("Board panel with titleText/bodyText; hidden while a step shows its own caption (decision 024).")]
    [SerializeField] private GameObject boardPanel;
    [SerializeField] private TMP_Text progressText;

    [Header("Next control")]
    [SerializeField] private Toggle nextToggle;
    [SerializeField] private TMP_Text nextLabel;

    [Header("Back control (decision 024)")]
    [SerializeField] private Toggle backToggle;
    [SerializeField] private TMP_Text backLabel;

    [Tooltip("Dimmed at step 1, where Zurück does nothing.")]
    [SerializeField] private CanvasGroup backGroup;

    private int current;

    // Step id of the step just shown, raised on every step change incl. the first (#73: the quiz unlocks at a step).
    public event Action<string> StepShown;

    public string CurrentStepId => steps != null && steps.Length > 0 ? steps[current].id : null;

    private void OnEnable()
    {
        if (nextToggle != null)
        {
            nextToggle.onValueChanged.AddListener(OnNextToggled);
        }

        if (backToggle != null)
        {
            backToggle.onValueChanged.AddListener(OnBackToggled);
        }
    }

    private void OnDisable()
    {
        if (nextToggle != null)
        {
            nextToggle.onValueChanged.RemoveListener(OnNextToggled);
        }

        if (backToggle != null)
        {
            backToggle.onValueChanged.RemoveListener(OnBackToggled);
        }
    }

    private void Start()
    {
        if (RequiredReference.IsMissing(titleText, nameof(titleText), this)
            | RequiredReference.IsMissing(bodyText, nameof(bodyText), this)
            | RequiredReference.IsMissing(nextToggle, nameof(nextToggle), this)
            | RequiredReference.IsMissing(backToggle, nameof(backToggle), this))
            return;

        if (backLabel != null)
        {
            backLabel.text = Loc.Get(BackKey);
        }

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError("WmsFieldTour: steps is empty.", this);
            return;
        }

        ShowStep(0);
    }

    // Also callable from a UnityEvent.
    public void NextStep()
    {
        if (steps == null || steps.Length == 0)
            return;

        ShowStep((current + 1) % steps.Length);
    }

    // Decision 024: back one step; does nothing at step 1 (no step change, no log).
    public void PreviousStep()
    {
        if (steps == null || steps.Length == 0 || current == 0)
            return;

        ShowStep(current - 1);
    }

    private void OnNextToggled(bool isOn)
    {
        if (!isOn)
            return;

        // Push button: release at once so the next poke counts again.
        nextToggle.SetIsOnWithoutNotify(false);
        NextStep();
    }

    private void OnBackToggled(bool isOn)
    {
        if (!isOn)
            return;

        backToggle.SetIsOnWithoutNotify(false);
        PreviousStep();
    }

    private void ShowStep(int index)
    {
        current = index;

        // Steps may share a marker group: hide all first, then show the current step's.
        foreach (Step other in steps)
        {
            SetMarkers(other, false);
        }
        SetMarkers(steps[index], true);

        Step step = steps[index];
        bool hasCaption = step.captionTitle != null && step.captionBody != null;
        if (boardPanel != null)
        {
            boardPanel.SetActive(!hasCaption);
        }

        (hasCaption ? step.captionTitle : titleText).text = Loc.Get(StepKeyPrefix + step.id + ".title");
        (hasCaption ? step.captionBody : bodyText).text = Loc.Get(StepKeyPrefix + step.id + ".body");

        if (progressText != null)
        {
            progressText.text = Loc.Get(ProgressKey, ("n", index + 1), ("total", steps.Length));
        }

        if (nextLabel != null)
        {
            nextLabel.text = Loc.Get(index == steps.Length - 1 ? RestartKey : NextKey);
        }

        bool canGoBack = index > 0;
        backToggle.interactable = canGoBack;
        if (backGroup != null)
        {
            backGroup.alpha = canGoBack ? 1f : DimmedAlpha;
        }

        ResearchLogger.LogTourStep(step.id, index + 1);
        StepShown?.Invoke(step.id);
    }

    private static void SetMarkers(Step step, bool active)
    {
        if (step.markers == null)
            return;

        foreach (GameObject marker in step.markers)
        {
            if (marker != null)
            {
                marker.SetActive(active);
            }
        }
    }
}
