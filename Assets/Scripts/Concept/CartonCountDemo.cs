using System.Collections;
using UnityEngine;

/// <summary>
/// #83, docs/decisions/025: at the tour step "cartons" three static STECKER C cartons appear one after the other in
/// the order container, each with its running total (40, 80, 120 = Soll), so Soll : Verp.-Einh. is seen, not only
/// read. Leaving the step hides them; coming back starts again from the first. View only: no Grabbable, no log.
/// </summary>
public class CartonCountDemo : MonoBehaviour
{
    [SerializeField] private WmsFieldTour tour;

    [Tooltip("Tour step id that shows the cartons.")]
    [SerializeField] private string stepId = "cartons";

    [Tooltip("The cartons in the order they appear, each with its total label as a child; inactive in the scene.")]
    [SerializeField] private GameObject[] cartons;

    [Tooltip("Seconds between two cartons.")]
    [SerializeField] private float interval = 0.6f;

    private void OnEnable()
    {
        if (tour != null)
        {
            tour.StepShown += HandleStepShown;
        }
    }

    private void OnDisable()
    {
        if (tour != null)
        {
            tour.StepShown -= HandleStepShown;
        }

        StopAllCoroutines();
        HideAll();
    }

    private void Start()
    {
        RequiredReference.IsMissing(tour, nameof(tour), this);
        if (cartons == null || cartons.Length == 0)
        {
            Debug.LogError("CartonCountDemo: cartons is empty.", this);
        }
    }

    private void HandleStepShown(string shownStepId)
    {
        StopAllCoroutines();
        HideAll();

        if (shownStepId == stepId)
        {
            StartCoroutine(ShowInTurn());
        }
    }

    private IEnumerator ShowInTurn()
    {
        for (int i = 0; i < cartons.Length; i++)
        {
            if (i > 0)
            {
                yield return new WaitForSeconds(interval);
            }

            if (cartons[i] != null)
            {
                cartons[i].SetActive(true);
            }
        }
    }

    private void HideAll()
    {
        if (cartons == null)
            return;

        foreach (GameObject carton in cartons)
        {
            if (carton != null)
            {
                carton.SetActive(false);
            }
        }
    }
}
