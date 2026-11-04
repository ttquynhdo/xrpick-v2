using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #20, docs/decisions/010: walks the trainee through the WMS dialog one field at a time. Each step shows its markers
/// outside the screen border; nothing is drawn on the dialog itself. #78, decision 024: a step about one field writes
/// its title and what the field is into a caption next to that field.
/// #89, decision 026: one bar centred below the screen holds the step text of the steps without a caption, the
/// "what to do" line with a hand icon for every step, and [Zurück] "Schritt n von 11" [Weiter] (poke toggles used as
/// push buttons, #80). The bar is hidden at the start and opens through <see cref="Open"/> (card 1 done) or after
/// <see cref="autoOpenSeconds"/>; step 1 is shown and logged only then.
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
        // #55: the texts are the Loc keys concept.tour.<id>.title / .body / .action (#89).
        [Tooltip("Stable id, e.g. soll (for #28 logging later); also names the Loc keys concept.tour.<id>.title/.body/.action.")]
        public string id;

        [Tooltip("Shown only during this step; placed outside the monitor border.")]
        public GameObject[] markers;

        // #78, decision 024: a caption next to the field (one of the markers). Empty = the text goes on the bar.
        [Tooltip("Caption title next to the field; leave empty to show the step text on the bar.")]
        public TMP_Text captionTitle;

        public TMP_Text captionBody;
    }

    [SerializeField] private Step[] steps;

    [Header("Bar (#89, decision 026)")]
    [Tooltip("Interactables/Tour Nav Bar; hidden until the tour opens.")]
    [SerializeField] private GameObject barRoot;

    [Tooltip("Step text of the steps without a caption (intro, cartons, not_used, material_flow).")]
    [SerializeField] private TMP_Text barText;

    [Tooltip("The \"what to do\" line next to the hand icon.")]
    [SerializeField] private TMP_Text actionText;

    [SerializeField] private TMP_Text progressText;

    [Tooltip("The tour opens by itself after this many seconds if card 1 is not done yet; 0 = never.")]
    [SerializeField] private float autoOpenSeconds = 60f;

    [Header("Next control")]
    [SerializeField] private Toggle nextToggle;
    [SerializeField] private TMP_Text nextLabel;

    [Header("Back control (decision 024)")]
    [SerializeField] private Toggle backToggle;
    [SerializeField] private TMP_Text backLabel;

    [Tooltip("Dimmed at step 1, where Zurück does nothing.")]
    [SerializeField] private CanvasGroup backGroup;

    private int current;
    private bool ready;

    // Step id of the step just shown, raised on every step change incl. the first (#73: the quiz unlocks at a step).
    public event Action<string> StepShown;

    public bool IsOpen { get; private set; }

    // Null until the bar is open (#89).
    public string CurrentStepId => IsOpen ? steps[current].id : null;

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
        if (RequiredReference.IsMissing(barRoot, nameof(barRoot), this)
            | RequiredReference.IsMissing(barText, nameof(barText), this)
            | RequiredReference.IsMissing(actionText, nameof(actionText), this)
            | RequiredReference.IsMissing(nextToggle, nameof(nextToggle), this)
            | RequiredReference.IsMissing(backToggle, nameof(backToggle), this))
            return;

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError("WmsFieldTour: steps is empty.", this);
            return;
        }

        if (backLabel != null)
        {
            backLabel.text = Loc.Get(BackKey);
        }

        foreach (Step step in steps)
        {
            SetMarkers(step, false);
        }

        ready = true;

        // Open() may already have run (card 1 done before this Start).
        if (IsOpen)
        {
            barRoot.SetActive(true);
            ShowStep(0);
            return;
        }

        barRoot.SetActive(false);
        if (autoOpenSeconds > 0f)
        {
            StartCoroutine(OpenAfterDelay());
        }
    }

    // #89: shows the bar at step 1; later calls do nothing. Called when card 1 is done (ConceptCardProgress).
    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        if (!ready)
            return;

        barRoot.SetActive(true);
        ShowStep(0);
    }

    // Also callable from a UnityEvent.
    public void NextStep()
    {
        if (!IsOpen)
            return;

        ShowStep((current + 1) % steps.Length);
    }

    // Decision 024: back one step; does nothing at step 1 (no step change, no log).
    public void PreviousStep()
    {
        if (!IsOpen || current == 0)
            return;

        ShowStep(current - 1);
    }

    private IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(autoOpenSeconds);
        Open();
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
        string title = Loc.Get(StepKeyPrefix + step.id + ".title");
        string body = Loc.Get(StepKeyPrefix + step.id + ".body");
        bool hasCaption = step.captionTitle != null && step.captionBody != null;
        if (hasCaption)
        {
            step.captionTitle.text = title;
            step.captionBody.text = body;
            barText.text = string.Empty;
        }
        else
        {
            barText.text = $"<b>{title}:</b> {body}";
        }

        actionText.text = Loc.Get(StepKeyPrefix + step.id + ".action");

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
