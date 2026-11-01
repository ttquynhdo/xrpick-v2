using TMPro;
using UnityEngine;

public class PiecesOrderValidator : MonoBehaviour
{
    // Key in docs/i18n/strings-todo.csv until #11/#12 move it to Loc.Get. Last lesson: the Congratulations panel
    // tells the trainee what to do next.
    private const string CorrectText = "Correct! The full cartons and the loose material match the order."; // complex.feedback.correct

    [Header("References")]
    [SerializeField] private PiecesContainerCounter containerCounter;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject congratulationsPanel;
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
    }

    public void ConfirmOrder()
    {
        // Non-short-circuit | so every missing reference is logged at once.
        bool missing =
            RequiredReference.IsMissing(containerCounter, nameof(containerCounter), this) |
            RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this) |
            RequiredReference.IsMissing(congratulationsPanel, nameof(congratulationsPanel), this) |
            RequiredReference.IsMissing(instructionText, nameof(instructionText), this);

        if (missing)
            return;

        // #6: one rule decides right/wrong (docs/decisions/005, 006).
        OrderResult result = containerCounter.CheckOrder();

        if (result.Correct)
        {
            feedbackText.text = CorrectText;
            feedbackText.color = Color.green;
            congratulationsPanel.SetActive(true);
            instructionText.text = string.Empty;
            return;
        }

        // #14: the instruction stays visible; the feedback names the first error, never the target numbers.
        feedbackText.text =
            OrderFeedback.Title(FeedbackLanguage.English) + "\n" + OrderFeedback.Explain(result, FeedbackLanguage.English);
        feedbackText.color = Color.red;
        instructionText.text = initialInstruction;

        // #4: a wrong order after a correct one must not leave the panel visible.
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(false);
        }
    }

    // #4: an old result must not stay visible once the container content changes.
    private void ClearResult()
    {
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
    }
}