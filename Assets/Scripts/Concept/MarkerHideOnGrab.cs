using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #105, docs/decisions/036 (5): a marker that points at a package switches itself off once one of the given packages
/// is grabbed, so it never stands between the eyes and the label the trainee reads. The tour shows it again when the
/// step is shown again (WmsFieldTour sets its markers active on every step change).
/// </summary>
public class MarkerHideOnGrab : MonoBehaviour
{
    [Tooltip("Grabbing any of these switches the marker off.")]
    [SerializeField] private Grabbable[] packages;

    private bool hideNow;

    private void OnEnable()
    {
        hideNow = false;
        if (packages == null || packages.Length == 0)
        {
            Debug.LogError("MarkerHideOnGrab: packages is empty.", this);
            return;
        }

        foreach (Grabbable package in packages)
        {
            if (package == null)
                continue;

            // Already in hand when the step is shown: nothing left to point at (switched off next frame, since a
            // GameObject can't be deactivated while it is being activated).
            if (package.SelectingPointsCount > 0)
            {
                hideNow = true;
            }

            package.WhenPointerEventRaised += HandlePointerEvent;
        }
    }

    private void OnDisable()
    {
        if (packages == null)
            return;

        foreach (Grabbable package in packages)
        {
            if (package != null)
            {
                package.WhenPointerEventRaised -= HandlePointerEvent;
            }
        }
    }

    private void Update()
    {
        if (hideNow)
        {
            gameObject.SetActive(false);
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            gameObject.SetActive(false);
        }
    }
}
