using System;
using TMPro;
using UnityEngine;

public class PiecesOrderValidator : MonoBehaviour, IOrderCompletion
{
    // #47: raised once when the last position is correct (#19); later confirms and content changes are ignored.
    public bool IsCompleted { get; private set; }

    public event Action OrderCompleted;

    // Key in docs/i18n/strings-todo.csv until #11/#12 move it to Loc.Get. Last lesson: the Congratulations panel
    // tells the trainee what to do next. #25: German (Sie) like the cards.
    private const string CorrectText = "Richtig! Volle Kartons und loses Material stimmen mit dem Auftrag überein."; // complex.feedback.correct

    // #19, decision 016: {0} = finished position, {1} = next position. The middle line is the Anbruchmenge
    // reminder (Detail Description 5.12); it is not rated.
    private const string PositionDoneFormat =
        "Richtig! Position {0} ist erledigt.\nDie Restmenge im angebrochenen Karton bleibt im Modul im Lagerbehälter.\nLesen Sie im LVS die Angaben von Position {1} ab."; // complex.feedback.position_done

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

        // #6: one rule decides right/wrong (docs/decisions/005, 006); #19: only the current position.
        OrderResult result = containerCounter.CheckOrder();

        if (result.Correct && !containerCounter.IsLastPosition)
        {
            int finished = containerCounter.CurrentPosition;
            // Advancing raises ContentsChanged, which clears the card: write the result after it.
            containerCounter.AdvanceToNextLine();
            ShowCard();
            feedbackText.text = string.Format(PositionDoneFormat, finished, containerCounter.CurrentPosition);
            feedbackText.color = Color.green;
            return;
        }

        ShowCard();

        if (result.Correct)
        {
            feedbackText.text = CorrectText;
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
            ? OrderFeedback.ExplainEmptyPosition(containerCounter.CurrentLine, FeedbackLanguage.German)
            : OrderFeedback.Explain(result, FeedbackLanguage.German);
        feedbackText.text = OrderFeedback.Title(FeedbackLanguage.German) + "\n" + reason;
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