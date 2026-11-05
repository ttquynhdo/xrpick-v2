using System.Collections;
using UnityEngine;

/// <summary>
/// #109, decision 037 (4): after the last correct confirm the result card at the order container's front wall
/// stays while the trainee reads it, and hides a moment after the order container has started to leave.
/// In Basic and Mixed the Menu item of the next lesson is highlighted at the same moment, instead of a
/// „Wählen Sie … im Menü“ sentence on the card. Replaces Basic's middle-lane arrow (#99).
/// </summary>
public class ResultCardDeparture : MonoBehaviour
{
    [Tooltip("Conveyor of the order container; its Departed event starts the countdown.")]
    [SerializeField] private OrderBoxConveyor orderBoxConveyor;

    [Tooltip("Result cards of this scene (hidden after the delay).")]
    [SerializeField] private GameObject[] resultCards;

    [Tooltip("Optional. Blue highlight behind the Menu item of the next lesson (inactive in the scene).")]
    [SerializeField] private GameObject nextLessonHighlight;

    [Tooltip("Seconds after the order container starts to move before the card hides.")]
    [SerializeField] private float hideDelay = 2f;

    private void OnEnable()
    {
        if (orderBoxConveyor == null)
        {
            Debug.LogError("ResultCardDeparture: orderBoxConveyor is not assigned.", this);
            return;
        }

        orderBoxConveyor.Departed += OnDeparted;
    }

    private void OnDisable()
    {
        if (orderBoxConveyor != null)
        {
            orderBoxConveyor.Departed -= OnDeparted;
        }
    }

    private void OnDeparted()
    {
        StartCoroutine(HideLater());
    }

    private IEnumerator HideLater()
    {
        if (hideDelay > 0f)
        {
            yield return new WaitForSeconds(hideDelay);
        }

        foreach (GameObject card in resultCards)
        {
            if (card != null)
            {
                card.SetActive(false);
            }
        }
        if (nextLessonHighlight != null)
        {
            nextLessonHighlight.SetActive(true);
        }
    }
}
