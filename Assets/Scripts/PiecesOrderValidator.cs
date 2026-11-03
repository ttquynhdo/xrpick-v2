using System;
using TMPro;
using UnityEngine;

public class PiecesOrderValidator : MonoBehaviour, IOrderCompletion
{
    // #47: raised once when the last position is correct (#19); later confirms and content changes are ignored.
    public bool IsCompleted { get; private set; }

    public event Action OrderCompleted;

    // Loc table UI (#54). Last lesson: the Congratulations panel tells the trainee what to do next.
    private const string CorrectKey = "complex.feedback.correct";

    // #19, decision 016: {done} = finished position, {next} = next position. The middle line is the
    // Anbruchmenge reminder (Detail Description 5.12); it is not rated.
    private const string PositionDoneKey = "complex.feedback.position_done";

    // #58: red button before the questions below the screen are answered; not a wrong order.
    private const string AnswerQuizFirstKey = "complex.feedback.answer_quiz_first";

    [Header("References")]
    [SerializeField] private PiecesContainerCounter containerCounter;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject congratulationsPanel;

    // #10, decision 008: the WMS dialog has no instruction line; the task text stays on the Instruction Table.
    [Tooltip("Optional. Left empty when the scene uses the WMS dialog.")]
    [SerializeField] private TMP_Text instructionText;

    // #10 (#33 Complex part): result card next to the red button; hidden until a confirm.
    [Tooltip("Optional card that holds feedbackText; shown on confirm, hidden when the container changes.")]
    [SerializeField] private GameObject resultCard;

    // #58, decision 016: the storage container stays hidden until both questions are answered right.
    [Tooltip("Optional. Questions below the screen that must be answered before position 1 is rated.")]
    [SerializeField] private RemainderQuizController remainderQuiz;

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
        if (containerCounter != null)
        {
            containerCounter.ContentsChanged += ClearResult;
        }
    }

    private void OnDisable()
    {
        if (containerCounter != null)
        {
            containerCounter.ContentsChanged -= ClearResult;
        }
    }

    private void Start()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(false);
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
            RequiredReference.IsMissing(containerCounter, nameof(containerCounter), this) |
            RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this) |
            RequiredReference.IsMissing(congratulationsPanel, nameof(congratulationsPanel), this);

        if (missing)
            return;

        // #58: the order can't be picked yet, so tell the trainee what to do instead of rating an empty box.
        if (containerCounter.CurrentPosition == 1 && remainderQuiz != null && !remainderQuiz.IsAnswered)
        {
            ShowCard();
            feedbackText.text = Loc.Get(AnswerQuizFirstKey);
            feedbackText.color = Color.yellow;
            return;
        }

        // #6: one rule decides right/wrong (docs/decisions/005, 006); #19: only the current position.
        OrderResult result = containerCounter.CheckOrder();

        if (result.Correct && !containerCounter.IsLastPosition)
        {
            int finished = containerCounter.CurrentPosition;
            // Advancing raises ContentsChanged, which clears the card: write the result after it.
            containerCounter.AdvanceToNextLine();
            ShowCard();
            feedbackText.text = Loc.Get(PositionDoneKey, ("done", finished), ("next", containerCounter.CurrentPosition));
            feedbackText.color = Color.green;
            return;
        }

        ShowCard();

        if (result.Correct)
        {
            feedbackText.text = Loc.Get(CorrectKey);
            feedbackText.color = Color.green;
            congratulationsPanel.SetActive(true);
            if (instructionText != null)
            {
                instructionText.text = string.Empty;
            }
            IsCompleted = true;
            OrderCompleted?.Invoke();
            return;
        }

        // #14: the instruction stays visible; the feedback names the first error, never the target numbers.
        bool emptyLaterPosition = containerCounter.CurrentPosition > 1 && result.Errors[0].Code == OrderErrorCode.Empty;
        string reason = emptyLaterPosition
            ? OrderFeedback.ExplainEmptyPosition(containerCounter.CurrentLine)
            : OrderFeedback.Explain(result);
        feedbackText.text = OrderFeedback.Title() + "\n" + reason;
        feedbackText.color = Color.red;
        if (instructionText != null)
        {
            instructionText.text = initialInstruction;
        }

        // #4: a wrong order after a correct one must not leave the panel visible.
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(false);
        }
    }

    private void ShowCard()
    {
        if (resultCard != null)
        {
            resultCard.SetActive(true);
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
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(false);
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
}