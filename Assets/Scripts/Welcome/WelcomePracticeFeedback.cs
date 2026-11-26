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
/// #179 (decision 043 item 5): the practice items lie in a storage container, an empty order container stands right
/// of it; step 1 is done once any practice item has lain in the order container, not held, for <see cref="settleSeconds"/>
/// (taking it out again keeps the mark). The items are kinematic, so the container is read by bounds, not by triggers.
/// </summary>
public class WelcomePracticeFeedback : MonoBehaviour, ILessonCompletion
{
    [Header("Actions")]
    [Tooltip("A practice carton; once it or any other practice item lies in the order container, step 1 is done.")]
    [SerializeField] private Grabbable box;

    [Tooltip("#175/#179: the other practice items (second carton, pouches).")]
    [SerializeField] private Grabbable[] otherItems = Array.Empty<Grabbable>();

    [Tooltip("#179: the solid collider of the empty order container (PlasticBox_Order).")]
    [SerializeField] private Collider orderBox;

    [Tooltip("#179: how long an item must lie in the order container, not held, before step 1 counts.")]
    [SerializeField] private float settleSeconds = 0.5f;

    // Item centres may lie this far inside the container's outer walls and this far above its rim.
    private const float WallMargin = 0.01f;
    private const float RimMargin = 0.02f;

    [Header("Feedback")]
    [Tooltip("Check mark on card 1 (Ware umlegen), inactive in the scene.")]
    [SerializeField] private GameObject boxCheck;

    [Tooltip("Check mark on card 2 (Knopf drücken), inactive in the scene.")]
    [SerializeField] private GameObject buttonCheck;

    [Tooltip("Highlight behind the next lesson's Menu item, inactive in the scene.")]
    [SerializeField] private GameObject nextLessonHighlight;

    [Header("Step cards (InstructionShelf/Panel/Cards)")]
    [Tooltip("Card „1. Ware umlegen“.")]
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

    private bool itemPlaced;
    private bool buttonPressed;
    private float placedSeconds;

    public bool IsLessonCompleted => itemPlaced && buttonPressed;

    public event Action LessonCompleted;

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
        RequiredReference.IsMissing(orderBox, nameof(orderBox), this);
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

    private void Update()
    {
        if (itemPlaced || orderBox == null)
            return;

        if (!AnyItemInOrderBox())
        {
            placedSeconds = 0f;
            return;
        }

        placedSeconds += Time.deltaTime;
        if (placedSeconds >= settleSeconds)
        {
            MarkItemPlaced();
        }
    }

    // True when a practice item that no hand holds has its centre inside the order container.
    private bool AnyItemInOrderBox()
    {
        Bounds container = orderBox.bounds;
        foreach (Grabbable item in PracticeItems())
        {
            if (item.SelectingPointsCount > 0 || !item.isActiveAndEnabled)
                continue;

            Collider itemCollider = item.GetComponent<Collider>();
            if (itemCollider == null)
                continue;

            Vector3 centre = itemCollider.bounds.center;
            bool inside = centre.x > container.min.x + WallMargin && centre.x < container.max.x - WallMargin
                          && centre.z > container.min.z + WallMargin && centre.z < container.max.z - WallMargin
                          && centre.y > container.min.y && centre.y < container.max.y + RimMargin;
            if (inside)
                return true;
        }

        return false;
    }

    private void MarkItemPlaced()
    {
        itemPlaced = true;
        ResearchLogger.LogWelcomePracticeDone(OutcomeEvents.PracticePlace);
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

        RectTransform current = !itemPlaced ? grabCard : !buttonPressed ? buttonCard : lessonCard;
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
