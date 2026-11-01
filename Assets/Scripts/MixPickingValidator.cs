using TMPro;
using UnityEngine;

public class MixedPickingValidator : MonoBehaviour
{
    // Key in docs/i18n/strings-todo.csv until #11/#12 move it to Loc.Get. {0} = next lesson name (menu label).
    // #25: German (Sie) like the cards; English stays in the csv for the developer mode.
    private const string CorrectFormat =
        "Richtig! Alle Materialien und Mengen stimmen mit dem Auftrag überein.\nWählen Sie jetzt im Menü „{0}“."; // mixed.feedback.correct

    private const string NextScene = "ComplexScene";

    [SerializeField] private MixedPickingContainer container;
    [SerializeField] private TMP_Text feedbackText;
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
    }

    // #4: an old result must not stay visible once the container content changes.
    private void ClearResult()
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
        if (instructionText != null)
        {
            instructionText.text = initialInstruction;
        }
    }

    public void ConfirmOrder()
    {
        // Non-short-circuit | so every missing reference is logged at once.
        bool missing =
            RequiredReference.IsMissing(container, nameof(container), this) |
            RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this) |
            RequiredReference.IsMissing(instructionText, nameof(instructionText), this);

        if (missing)
            return;

        // #6: one rule decides right/wrong (docs/decisions/005, 006).
        OrderResult result = container.CheckOrder();

        // #14: the instruction stays visible in both cases; the feedback never prints target numbers.
        if (result.Correct)
        {
            if (!LessonNames.TryGetDisplayName(NextScene, out string nextLesson))
            {
                Debug.LogError($"MixedPickingValidator: no lesson name for {NextScene}.", this);
            }
            feedbackText.text = string.Format(CorrectFormat, nextLesson);
            feedbackText.color = Color.green;
            return;
        }

        feedbackText.text =
            OrderFeedback.Title(FeedbackLanguage.German) + "\n" + OrderFeedback.Explain(result, FeedbackLanguage.German);
        feedbackText.color = Color.red;
    }
}
