using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #20, docs/decisions/010: walks the trainee through the WMS dialog one field at a time on a board beside the
/// monitor. Each step shows its markers outside the screen border; nothing is drawn on the dialog itself.
/// The "Weiter" control is a poke toggle used as a push button.
/// </summary>
public class WmsFieldTour : MonoBehaviour
{
    // Loc table UI (#54).
    private const string ProgressKey = "concept.tour.progress";
    private const string NextKey = "concept.tour.next";
    private const string RestartKey = "concept.tour.restart";
    private const string StepKeyPrefix = "concept.tour.";

    [Serializable]
    public class Step
    {
        // #55: the texts are the Loc keys concept.tour.<id>.title / .body.
        [Tooltip("Stable id, e.g. soll (for #28 logging later); also names the Loc keys concept.tour.<id>.title/.body.")]
        public string id;

        [Tooltip("Shown only during this step; placed outside the monitor border.")]
        public GameObject[] markers;
    }

    [SerializeField] private Step[] steps;

    [Header("Board")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text progressText;

    [Header("Next control")]
    [SerializeField] private Toggle nextToggle;
    [SerializeField] private TMP_Text nextLabel;

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
    }

    private void OnDisable()
    {
        if (nextToggle != null)
        {
            nextToggle.onValueChanged.RemoveListener(OnNextToggled);
        }
    }

    private void Start()
    {
        if (RequiredReference.IsMissing(titleText, nameof(titleText), this)
            | RequiredReference.IsMissing(bodyText, nameof(bodyText), this)
            | RequiredReference.IsMissing(nextToggle, nameof(nextToggle), this))
            return;

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

    private void OnNextToggled(bool isOn)
    {
        if (!isOn)
            return;

        // Push button: release at once so the next poke counts again.
        nextToggle.SetIsOnWithoutNotify(false);
        NextStep();
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
        titleText.text = Loc.Get(StepKeyPrefix + step.id + ".title");
        bodyText.text = Loc.Get(StepKeyPrefix + step.id + ".body");

        if (progressText != null)
        {
            progressText.text = Loc.Get(ProgressKey, ("n", index + 1), ("total", steps.Length));
        }

        if (nextLabel != null)
        {
            nextLabel.text = Loc.Get(index == steps.Length - 1 ? RestartKey : NextKey);
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
