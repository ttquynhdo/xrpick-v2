using TMPro;
using UnityEngine;

public class MixedPickingValidator : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    // #25: German (Sie) like the cards; English stays in the csv for the developer mode.
    // {0} = next lesson name (menu label).
    private const string CorrectFormat =
        "Richtig! Alle Materialien und Mengen stimmen mit dem Auftrag überein.\nWählen Sie jetzt im Menü „{0}“."; // mixed.feedback.correct

    // #21, decision 012: {0} = finished position, {1} = next position.
    private const string PositionDoneFormat =
        "Richtig! Position {0} ist erledigt.\nLesen Sie im LVS die Angaben von Position {1} ab."; // mixed.feedback.position_done

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
            ShowResult(string.Format(PositionDoneFormat, finished, container.CurrentPosition), Color.green);
            return;
        }

        if (result.Correct)
        {
            if (!LessonNames.TryGetDisplayName(NextScene, out string nextLesson))
            {
                Debug.LogError($"MixedPickingValidator: no lesson name for {NextScene}.", this);
            }
            ShowResult(string.Format(CorrectFormat, nextLesson), Color.green);
            return;
        }

        bool emptyLaterPosition = container.CurrentPosition > 1 && result.Errors[0].Code == OrderErrorCode.Empty;
        string reason = emptyLaterPosition
            ? OrderFeedback.ExplainEmptyPosition(container.CurrentLine, FeedbackLanguage.German)
            : OrderFeedback.Explain(result, FeedbackLanguage.German);

        ShowResult(OrderFeedback.Title(FeedbackLanguage.German) + "\n" + reason, Color.red);
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
