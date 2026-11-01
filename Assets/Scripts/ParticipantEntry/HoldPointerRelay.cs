using UnityEngine;
using UnityEngine.EventSystems;

// #44: forwards pointer down/up/exit on the facilitator disc to FacilitatorEndControl.
// Leaving the disc counts as release, so a drifting hand cancels the hold.
public class HoldPointerRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private FacilitatorEndControl target;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target != null)
            target.HoldStart();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (target != null)
            target.HoldEnd();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target != null)
            target.HoldEnd();
    }
}
