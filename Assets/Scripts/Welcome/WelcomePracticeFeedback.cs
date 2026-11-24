using System;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #39: WelcomeScene practises two actions (grab the carton, press the red button). Each one shows a check
/// mark on its instruction card the first time it is done; when both are done the next lesson in the Menu
/// is highlighted. Marks stay once shown (no undo in a warm-up). #42: both done = lesson done (Menu unlock).
/// #121/#124 (decision 038): the first step card not yet done has the blue frame of the picking scenes (decision 017):
/// card 1 at the start, card 2 after the first grab, card 3 „Lektion wählen“ once both are done, in the same moment as
/// the Menu highlight; the other step cards are dimmed.
/// #175: the practice desk holds a standard carton, a screw pouch and loose rings in a tray; the first grab of any
/// of them counts as the grab action (user, 2026-11-24).
/// </summary>
public class WelcomePracticeFeedback : MonoBehaviour, ILessonCompletion
{
    [Header("Actions")]
    [Tooltip("The practice carton; the first grab of it or of any other practice item counts.")]
    [SerializeField] private Grabbable box;

    [Tooltip("#175: the other practice items (pouch, loose rings); the first grab of any of them counts too.")]
    [SerializeField] private Grabbable[] otherItems = Array.Empty<Grabbable>();

    [Header("Feedback")]
    [Tooltip("Check mark on card 1 (Karton greifen), inactive in the scene.")]
    [SerializeField] private GameObject boxCheck;

    [Tooltip("Check mark on card 2 (Knopf drücken), inactive in the scene.")]
    [SerializeField] private GameObject buttonCheck;

    [Tooltip("Highlight behind the next lesson's Menu item, inactive in the scene.")]
    [SerializeField] private GameObject nextLessonHighlight;

    [Header("Step cards (InstructionShelf/Panel/Cards)")]
    [Tooltip("Card „1. Karton greifen“.")]
    [SerializeField] private RectTransform grabCard;

    [Tooltip("Card „2. Knopf drücken“.")]
    [SerializeField] private RectTransform buttonCard;

    [Tooltip("Card „3. Lektion wählen“.")]
    [SerializeField] private RectTransform lessonCard;

    [Header("Look (same as InstructionStepHighlighter)")]
    [SerializeField] private Color frameColor = new(0.012f, 0.569f, 0.867f, 1f);
    [SerializeField] private float frameThinness = 1f;
    [SerializeField] private float frameOutset = 3f;

    [Range(0.2f, 1f)]
    [SerializeField] private float dimFactor = 0.55f;

    private InstructionCardLook grabLook;
    private InstructionCardLook buttonLook;
    private InstructionCardLook lessonLook;

    private bool boxGrabbed;
    private bool buttonPressed;

    public bool IsLessonCompleted => boxGrabbed && buttonPressed;

    public event Action LessonCompleted;

    private void OnEnable()
    {
        foreach (Grabbable item in PracticeItems())
        {
            item.WhenPointerEventRaised += HandleBoxPointerEvent;
        }
    }

    private void OnDisable()
    {
        foreach (Grabbable item in PracticeItems())
        {
            item.WhenPointerEventRaised -= HandleBoxPointerEvent;
        }
    }

    private System.Collections.Generic.IEnumerable<Grabbable> PracticeItems()
    {
        if (box != null)
            yield return box;

        foreach (Grabbable item in otherItems)
        {
            if (item != null)
                yield return item;
        }
    }

    private void Start()
    {
        RequiredReference.IsMissing(box, nameof(box), this);
        RequiredReference.IsMissing(boxCheck, nameof(boxCheck), this);
        RequiredReference.IsMissing(buttonCheck, nameof(buttonCheck), this);
        RequiredReference.IsMissing(nextLessonHighlight, nameof(nextLessonHighlight), this);

        bool cardsMissing = RequiredReference.IsMissing(grabCard, nameof(grabCard), this)
                            | RequiredReference.IsMissing(buttonCard, nameof(buttonCard), this)
                            | RequiredReference.IsMissing(lessonCard, nameof(lessonCard), this);
        if (!cardsMissing)
        {
            grabLook = new InstructionCardLook(grabCard, frameOutset);
            buttonLook = new InstructionCardLook(buttonCard, frameOutset);
            lessonLook = new InstructionCardLook(lessonCard, frameOutset);

            // The frames are added as the cards' last children; the check marks must still draw on top of them.
            if (boxCheck != null)
            {
                boxCheck.transform.SetAsLastSibling();
            }
            if (buttonCheck != null)
            {
                buttonCheck.transform.SetAsLastSibling();
            }
        }

        ApplyFrame();
    }

    // Called by the red button's _whenSelect, after its press sound.
    public void MarkButtonPressed()
    {
        if (buttonPressed)
            return;

        buttonPressed = true;
        ResearchLogger.LogWelcomePracticeDone(OutcomeEvents.PracticeButton);
        Show(buttonCheck);
        UpdateNextLesson();
        ApplyFrame();
    }

    private void HandleBoxPointerEvent(PointerEvent evt)
    {
        if (evt.Type != PointerEventType.Select || boxGrabbed)
            return;

        boxGrabbed = true;
        ResearchLogger.LogWelcomePracticeDone(OutcomeEvents.PracticeGrab);
        Show(boxCheck);
        UpdateNextLesson();
        ApplyFrame();
    }

    private void UpdateNextLesson()
    {
        if (IsLessonCompleted)
        {
            Show(nextLessonHighlight);
            LessonCompleted?.Invoke();
        }
    }

    // Frame on the first step card not yet done; card 3 stays current once both actions are done.
    private void ApplyFrame()
    {
        if (grabLook == null)
            return;

        RectTransform current = !boxGrabbed ? grabCard : !buttonPressed ? buttonCard : lessonCard;
        ShowCard(grabLook, current == grabCard);
        ShowCard(buttonLook, current == buttonCard);
        ShowCard(lessonLook, current == lessonCard);
    }

    private void ShowCard(InstructionCardLook card, bool isCurrent)
    {
        card.Show(isCurrent ? frameColor : Color.clear, frameThinness, dimmed: !isCurrent, dimFactor);
    }

    private static void Show(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(true);
        }
    }
}
