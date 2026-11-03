using System;
using TMPro;
using UnityEngine;

public class MixedPickingValidator : MonoBehaviour, IOrderCompletion
{
    // #47: raised once when the last position is correct; later confirms and content changes are ignored.
    public bool IsCompleted { get; private set; }

    public event Action OrderCompleted;

    // Loc table UI (#54). {next_lesson} = menu label of the next lesson.
    private const string CorrectKey = "mixed.feedback.correct";

    // #21, decision 012: {position} = finished position, {next_position} = next position.
    private const string PositionDoneKey = "mixed.feedback.position_done";

    private const string NextScene = "ComplexScene";

    [SerializeField] private MixedPickingContainer container;
    [SerializeField] private TMP_Text feedbackText;

    // #21 (#33 Mixed part): result card next to the red button; hidden until a confirm.
    [Tooltip("Optional card that holds feedbackText; shown on confirm, hidden when the container changes.")]
    [SerializeField] private GameObject resultCard;

    // #21: the WMS dialog has no instruction line; left empty in MixedPickingScene.
    [Tooltip("Optional. Left empty when the scene uses the WMS dialog.")]
    [SerializeField] private TMP_Text instructionText;

    private string initialInstruction;

    private void Awake()
    {
        if (instructionText != null)
        {
            initialInstruction = instructionText.text;
        }
    }

    private void OnEnable()
    {
        if (container != null)
        {
            container.ContentsChanged += ClearResult;
        }
    }

    private void OnDisable()
    {
        if (container != null)
        {
            container.ContentsChanged -= ClearResult;
        }
    }

    private void Start()
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
        if (resultCard != null)
        {
            resultCard.SetActive(false);
        }
    }

    // #4: an old result must not stay visible once the container content changes.
    private void ClearResult()
    {
        if (IsCompleted)
            return;

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
        if (instructionText != null)
        {
            instructionText.text = initialInstruction;
        }
        if (resultCard != null)
        {
            resultCard.SetActive(false);
        }
    }

    public void ConfirmOrder()
    {
        // #47: the order is shipped; the result stays as it is.
        if (IsCompleted)
            return;

        // Non-short-circuit | so every missing reference is logged at once.
        bool missing =
            RequiredReference.IsMissing(container, nameof(container), this) |
            RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this);

        if (missing)
            return;

        // #6: one rule decides right/wrong (docs/decisions/005, 006); #21: only the current position.
        OrderResult result = container.CheckOrder();

        // #14: the instruction stays visible in both cases; the feedback never prints target numbers.
        if (result.Correct && !container.IsLastPosition)
        {
            int finished = container.CurrentPosition;

            // Advancing raises ContentsChanged, which clears the card: write the result after it.
            container.AdvanceToNextLine();
            ShowResult(Loc.Get(PositionDoneKey, ("position", finished), ("next_position", container.CurrentPosition)),
                Color.green);
            return;
        }

        if (result.Correct)
        {
            if (!LessonNames.TryGetDisplayName(NextScene, out string nextLesson))
            {
                Debug.LogError($"MixedPickingValidator: no lesson name for {NextScene}.", this);
            }
            ShowResult(Loc.Get(CorrectKey, ("next_lesson", nextLesson)), Color.green);
            IsCompleted = true;
            OrderCompleted?.Invoke();
            return;
        }

        bool emptyLaterPosition = container.CurrentPosition > 1 && result.Errors[0].Code == OrderErrorCode.Empty;
        string reason = emptyLaterPosition
            ? OrderFeedback.ExplainEmptyPosition(container.CurrentLine)
            : OrderFeedback.Explain(result);

        ShowResult(OrderFeedback.Title() + "\n" + reason, Color.red);
    }

    private void ShowResult(string text, Color color)
    {
        if (resultCard != null)
        {
            resultCard.SetActive(true);
        }
        feedbackText.text = text;
        feedbackText.color = color;
    }
}
