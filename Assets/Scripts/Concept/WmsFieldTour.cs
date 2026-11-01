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
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string ProgressFormat = "Schritt {0} von {1}";   // concept.tour.progress
    private const string NextLabel = "Weiter";                     // concept.tour.next
    private const string RestartLabel = "Von vorn";                // concept.tour.restart

    [Serializable]
    public class Step
    {
        [Tooltip("Stable id, e.g. soll (for #28 logging later).")]
        public string id;

        public string title;

        [TextArea(2, 5)]
        public string body;

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
        titleText.text = step.title;
        bodyText.text = step.body;

        if (progressText != null)
        {
            progressText.text = string.Format(ProgressFormat, index + 1, steps.Length);
        }

        if (nextLabel != null)
        {
            nextLabel.text = index == steps.Length - 1 ? RestartLabel : NextLabel;
        }
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
