using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #105, docs/decisions/036 (5): a marker that points at a package switches itself off once one of the given packages
/// is grabbed, so it never stands between the eyes and the label the trainee reads.
/// #136: once one of the packages has been grabbed, the marker stays off for the rest of the scene visit, also when a
/// later step (or „Zurück“ / „Von vorn“) would show it again: the trainee already knows which object to take.
/// #156: the tour asks <see cref="MayShow"/> before it activates the marker, so a done marker is never switched on.
/// If anything else activates it anyway, its canvases and renderers are switched off in the same call (no frame drawn)
/// and the GameObject is deactivated in the next Update (it can't be deactivated while it is being activated).
/// </summary>
public class MarkerHideOnGrab : MonoBehaviour, IStepMarkerGate
{
    [Tooltip("Grabbing any of these switches the marker off.")]
    [SerializeField] private Grabbable[] packages;

    // Survives OnDisable/OnEnable; a new scene visit creates a new component, so it starts false again.
    private bool grabbedOnce;

    private bool hideNow;

    public bool MayShow
    {
        get
        {
            if (!grabbedOnce && AnyPackageHeld())
            {
                grabbedOnce = true;
            }

            return !grabbedOnce;
        }
    }

    private void OnEnable()
    {
        if (packages == null || packages.Length == 0)
        {
            Debug.LogError("MarkerHideOnGrab: packages is empty.", this);
            return;
        }

        foreach (Grabbable package in packages)
        {
            if (package != null)
            {
                package.WhenPointerEventRaised += HandlePointerEvent;
            }
        }

        hideNow = !MayShow;
        if (hideNow)
        {
            StopDrawing();
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
            hideNow = false;
            gameObject.SetActive(false);
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            grabbedOnce = true;
            gameObject.SetActive(false);
        }
    }

    private bool AnyPackageHeld()
    {
        if (packages == null)
            return false;

        foreach (Grabbable package in packages)
        {
            if (package != null && package.SelectingPointsCount > 0)
                return true;
        }

        return false;
    }

    // Only ever switches drawing off: once grabbed, the marker never shows again in this visit.
    private void StopDrawing()
    {
        foreach (Canvas canvas in GetComponentsInChildren<Canvas>(true))
        {
            canvas.enabled = false;
        }

        foreach (Renderer marker in GetComponentsInChildren<Renderer>(true))
        {
            marker.enabled = false;
        }
    }
}
