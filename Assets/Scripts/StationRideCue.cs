using System.Collections;
using UnityEngine;

/// <summary>
/// #99, decision 033: an orange arrow over the middle lane that shows where to look while the containers change.
/// Only in BasicPickingScene, the first real container change of the course. Shown from the correct confirm,
/// hidden a moment after the order container has started to leave.
/// </summary>
public class StationRideCue : MonoBehaviour
{
    [Tooltip("Validator of this scene (IOrderCompletion); the cue appears when the whole order is correct.")]
    [SerializeField] private MonoBehaviour validator;

    [Tooltip("Conveyor of the order container; the cue hides after it has started to move.")]
    [SerializeField] private OrderBoxConveyor orderBoxConveyor;

    [Tooltip("The arrow (inactive in the scene).")]
    [SerializeField] private GameObject cue;

    [Tooltip("Seconds after the order container starts to move before the cue hides.")]
    [SerializeField] private float hideDelay = 2f;

    private IOrderCompletion completion;

    private void OnEnable()
    {
        completion = validator as IOrderCompletion;

        if (completion == null)
        {
            Debug.LogError("StationRideCue: validator is not assigned or does not implement IOrderCompletion.", this);
            return;
        }
        if (cue == null)
        {
            Debug.LogError("StationRideCue: cue is not assigned.", this);
            return;
        }

        cue.SetActive(false);
        completion.OrderCompleted += Show;
        if (orderBoxConveyor != null)
        {
            orderBoxConveyor.Departed += HideLater;
        }
    }

    private void OnDisable()
    {
        if (completion != null)
        {
            completion.OrderCompleted -= Show;
        }
        if (orderBoxConveyor != null)
        {
            orderBoxConveyor.Departed -= HideLater;
        }
        if (cue != null)
        {
            cue.SetActive(false);
        }
    }

    private void Show()
    {
        cue.SetActive(true);
    }

    private void HideLater()
    {
        StartCoroutine(Hide());
    }

    private IEnumerator Hide()
    {
        if (hideDelay > 0f)
        {
            yield return new WaitForSeconds(hideDelay);
        }

        cue.SetActive(false);
    }
}
