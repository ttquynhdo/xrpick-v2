using System.Collections;
using UnityEngine;

/// <summary>
/// #83, docs/decisions/025: at the tour step "cartons" three static STECKER C cartons appear one after the other in
/// the order container, each with its running total (40, 80, 120 = Soll), so Soll : Verp.-Einh. is seen, not only
/// read. Leaving the step hides them; coming back starts again from the first. View only: no Grabbable, no log.
/// #91, decisions 026/030: each carton that appears is taken from the storage container (one static carton hides) and
/// Ist on the sample WMS counts along. At the step "material_flow" all three are in the order container at once, so
/// the order container leaves with them (LaneFlowDemo).
/// </summary>
public class CartonCountDemo : MonoBehaviour
{
    [SerializeField] private WmsFieldTour tour;

    [Tooltip("Tour step id that shows the cartons one after the other.")]
    [SerializeField] private string stepId = "cartons";

    [Tooltip("Tour step id that shows all cartons at once (the order is complete).")]
    [SerializeField] private string fullStepId = "material_flow";

    [Tooltip("The cartons in the order they appear, each with its total label as a child; inactive in the scene.")]
    [SerializeField] private GameObject[] cartons;

    [Tooltip("Static cartons in the storage container, hidden in this order as the cartons appear.")]
    [SerializeField] private GameObject[] storageCartons;

    [Tooltip("Sample WMS order; its Ist shows the pieces of the cartons shown.")]
    [SerializeField] private SampleWmsOrder order;

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
        ShowCount(0);
    }

    private void Start()
    {
        RequiredReference.IsMissing(tour, nameof(tour), this);
        RequiredReference.IsMissing(order, nameof(order), this);
        if (cartons == null || cartons.Length == 0)
        {
            Debug.LogError("CartonCountDemo: cartons is empty.", this);
        }
    }

    private void HandleStepShown(string shownStepId)
    {
        StopAllCoroutines();

        if (shownStepId == stepId)
        {
            ShowCount(0);
            StartCoroutine(ShowInTurn());
        }
        else if (shownStepId == fullStepId)
        {
            ShowCount(cartons.Length);
        }
        else
        {
            ShowCount(0);
        }
    }

    private IEnumerator ShowInTurn()
    {
        for (int i = 1; i <= cartons.Length; i++)
        {
            if (i > 1)
            {
                yield return new WaitForSeconds(interval);
            }

            ShowCount(i);
        }
    }

    // The first count cartons in the order container, as many static ones gone from the storage container.
    private void ShowCount(int count)
    {
        if (cartons != null)
        {
            for (int i = 0; i < cartons.Length; i++)
            {
                SetActive(cartons[i], i < count);
            }
        }

        if (storageCartons != null)
        {
            for (int i = 0; i < storageCartons.Length; i++)
            {
                SetActive(storageCartons[i], i >= count);
            }
        }

        if (order != null)
        {
            order.SetIstPieces(count * order.CurrentLine.vpe);
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}
