using System.Text;
using TMPro;
using UnityEngine;

public class MixedPickingValidator : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get. #14 rewrites the feedback.
    private const string CorrectTitle = "Correct!";                                                  // mixed.feedback.correct
    private const string PlacedLineFormat = "{0}: {1}";                                             // mixed.feedback.placed_line
    private const string CheckAgain = "Check the order again.";                                     // mixed.feedback.check_again
    private const string CountLineFormat = "{0}: {1} / {2}";                                        // mixed.feedback.count_line
    private const string WrongMaterialLine =
        "The container holds material that does not belong to this order.";                         // mixed.feedback.wrong_package
    private const string SuccessInstruction =
        "You have successfully completed the task.\nSelect the next scenario option in the MENU.";  // mixed.instruction.success

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

        // #6: one rule decides right/wrong (docs/decisions/005, 006); numbers come from the order lines.
        OrderResult result = container.CheckOrder();
        var text = new StringBuilder();

        if (result.Correct)
        {
            text.Append(CorrectTitle);
            foreach (OrderLineResult line in result.Lines)
            {
                text.Append('\n').AppendFormat(PlacedLineFormat, line.Line.materialName, line.PlacedFull);
            }

            feedbackText.text = text.ToString();
            feedbackText.color = Color.green;
            instructionText.text = SuccessInstruction;
            return;
        }

        text.Append(CheckAgain);
        if (result.Foreign.Count > 0)
        {
            text.Append('\n').Append(WrongMaterialLine);
        }
        foreach (OrderLineResult line in result.Lines)
        {
            text.Append('\n').AppendFormat(CountLineFormat, line.Line.materialName, line.PlacedFull, line.Line.RequiredFull);
        }

        instructionText.text = string.Empty;
        feedbackText.text = text.ToString();
        feedbackText.color = Color.red;
    }
}
