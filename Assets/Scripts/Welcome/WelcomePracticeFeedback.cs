using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #39: WelcomeScene practises two actions (grab the carton, press the red button). Each one shows a check
/// mark on its instruction card the first time it is done; when both are done the next lesson in the Menu
/// is highlighted. Marks stay once shown (no undo in a warm-up).
/// </summary>
public class WelcomePracticeFeedback : MonoBehaviour
{
    [Header("Actions")]
    [Tooltip("The practice carton; its first grab counts.")]
    [SerializeField] private Grabbable box;

    [Header("Feedback")]
    [Tooltip("Check mark on card 1 (Karton greifen), inactive in the scene.")]
    [SerializeField] private GameObject boxCheck;

    [Tooltip("Check mark on card 2 (Knopf drücken), inactive in the scene.")]
    [SerializeField] private GameObject buttonCheck;

    [Tooltip("Highlight behind the next lesson's Menu item, inactive in the scene.")]
    [SerializeField] private GameObject nextLessonHighlight;

    private bool boxGrabbed;
    private bool buttonPressed;

    private void OnEnable()
    {
        if (box != null)
        {
            box.WhenPointerEventRaised += HandleBoxPointerEvent;
        }
    }

    private void OnDisable()
    {
        if (box != null)
        {
            box.WhenPointerEventRaised -= HandleBoxPointerEvent;
        }
    }

    private void Start()
    {
        RequiredReference.IsMissing(box, nameof(box), this);
        RequiredReference.IsMissing(boxCheck, nameof(boxCheck), this);
        RequiredReference.IsMissing(buttonCheck, nameof(buttonCheck), this);
        RequiredReference.IsMissing(nextLessonHighlight, nameof(nextLessonHighlight), this);
    }

    // Called by the red button's _whenSelect, after its press sound.
    public void MarkButtonPressed()
    {
        if (buttonPressed)
            return;

        buttonPressed = true;
        Show(buttonCheck);
        UpdateNextLesson();
    }

    private void HandleBoxPointerEvent(PointerEvent evt)
    {
        if (evt.Type != PointerEventType.Select || boxGrabbed)
            return;

        boxGrabbed = true;
        Show(boxCheck);
        UpdateNextLesson();
    }

    private void UpdateNextLesson()
    {
        if (boxGrabbed && buttonPressed)
        {
            Show(nextLessonHighlight);
        }
    }

    private static void Show(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(true);
        }
    }
}
