using System.Text;
using TMPro;
using UnityEngine;

public class PiecesOrderValidator : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get. #14 rewrites the feedback.
    private const string WrongPackageLine =
        "The container holds material that does not belong to this order."; // complex.feedback.wrong_package
    private const string CorrectFormat = "Correct!\n{0} packages and {1} pieces have been placed.\n"; // complex.feedback.correct
    private const string CheckAgain = "Check the order again.";                                      // complex.feedback.check_again
    private const string PackagesLineFormat = "Packages: {0} / {1}";                                 // complex.feedback.packages_line
    private const string LooseLineFormat = "Loose pieces: {0} / {1}";                                // complex.feedback.loose_line

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

        // #6: one rule decides right/wrong (docs/decisions/005, 006); numbers come from the order lines.
        OrderResult result = containerCounter.CheckOrder();
        int placedLoose = 0;
        int requiredFull = 0;
        int requiredLoose = 0;

        foreach (OrderLineResult line in result.Lines)
        {
            placedLoose += line.PlacedLoose ?? 0;
            requiredFull += line.Line.RequiredFull;
            requiredLoose += line.Line.RequiredLoose;
        }

        if (result.Correct)
        {
            feedbackText.text = string.Format(CorrectFormat, containerCounter.CartonCount, placedLoose);
            feedbackText.color = Color.green;
            congratulationsPanel.SetActive(true);
            instructionText.text = string.Empty;
            return;
        }

        var text = new StringBuilder(CheckAgain);
        if (result.Foreign.Count > 0)
        {
            text.Append('\n').Append(WrongPackageLine);
        }
        text.Append('\n').AppendFormat(PackagesLineFormat, containerCounter.CartonCount, requiredFull);
        text.Append('\n').AppendFormat(LooseLineFormat, placedLoose, requiredLoose);

        feedbackText.text = text.ToString();
        feedbackText.color = Color.red;
        instructionText.text = string.Empty;

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