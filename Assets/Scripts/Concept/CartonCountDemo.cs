using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// #83 / #91, docs/decisions/025, 030: the tour step "cartons" makes Soll : Verp.-Einh. seen, not only read.
/// #92, docs/decisions/032: the trainee puts the cartons into the order container from the storage container; nothing
/// appears by itself. At this step a running total in LVS-field style (40, 80, „120 = Soll“) stands above the carton
/// laid down last, as long as the count does not exceed the order (then ConceptBoxHints shows its too-many hint).
/// Ist itself comes from ConceptBoxHints → SampleWmsOrder. View only: no log.
/// </summary>
public class CartonCountDemo : MonoBehaviour
{
    [SerializeField] private WmsFieldTour tour;

    [Tooltip("Tour step id that shows the running total.")]
    [SerializeField] private string stepId = "cartons";

    [Tooltip("Reads which sample cartons lie in the order container.")]
    [SerializeField] private ConceptBoxHints boxHints;

    [Tooltip("Sample WMS order; Soll and Verp.-Einh. give how many totals there are.")]
    [SerializeField] private SampleWmsOrder order;

    [Tooltip("Total tags in order (1 carton, 2 cartons, …), inactive world-space canvases.")]
    [SerializeField] private RectTransform[] totals;

    [Tooltip("Eyes; empty = Camera.main.")]
    [SerializeField] private Transform head;

    [Tooltip("Gap between the carton's top and the tag's bottom edge, in metres.")]
    [SerializeField] private float gap = 0.015f;

    private bool atStep;

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

        atStep = false;
        ShowTotal(-1, null);
    }

    private void Start()
    {
        RequiredReference.IsMissing(tour, nameof(tour), this);
        RequiredReference.IsMissing(boxHints, nameof(boxHints), this);
        RequiredReference.IsMissing(order, nameof(order), this);
        if (totals == null || totals.Length == 0)
        {
            Debug.LogError("CartonCountDemo: totals is empty.", this);
        }

        ShowTotal(-1, null);
    }

    private void HandleStepShown(string shownStepId)
    {
        atStep = shownStepId == stepId;
        if (!atStep)
        {
            ShowTotal(-1, null);
        }
    }

    private void LateUpdate()
    {
        if (!atStep || boxHints == null || order == null || totals == null)
            return;

        List<Transform> placed = boxHints.SampleIn(ConceptBox.Order);
        int vpe = Mathf.Max(order.CurrentLine.vpe, 1);
        int needed = Mathf.Min(order.CurrentLine.soll / vpe, totals.Length);
        int count = placed.Count;

        if (count == 0 || count > needed)
        {
            ShowTotal(-1, null);
            return;
        }

        ShowTotal(count - 1, placed[count - 1]);
    }

    // Shows totals[index] above the carton (index -1 = none).
    private void ShowTotal(int index, Transform carton)
    {
        if (totals == null)
            return;

        for (int i = 0; i < totals.Length; i++)
        {
            if (totals[i] != null)
            {
                totals[i].gameObject.SetActive(i == index);
            }
        }

        if (index < 0 || carton == null || totals[index] == null)
            return;

        Transform eyes = head != null ? head : Camera.main != null ? Camera.main.transform : null;
        Bounds bounds = TagPlacement.Of(carton.GetComponentsInChildren<Renderer>(), carton.position);
        RectTransform tag = totals[index];
        float halfHeight = tag.rect.height * tag.lossyScale.y * 0.5f;
        Vector3 position = new(bounds.center.x, bounds.max.y + gap + halfHeight, bounds.center.z);
        Quaternion rotation = eyes != null
            ? Quaternion.LookRotation(position - eyes.position, Vector3.up)
            : tag.rotation;
        tag.SetPositionAndRotation(position, rotation);
    }
}
