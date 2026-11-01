using TMPro;
using UnityEngine;

public class PiecesOrderValidator : MonoBehaviour
{
    // Key in docs/i18n/strings-todo.csv until #11/#12 move it to Loc.Get.
    private const string WrongPackageLine =
        "The container holds material that does not belong to this order."; // complex.feedback.wrong_package

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
        if (containerCounter == null)
        {
            Debug.LogError("PiecesContainerCounter has not been assigned.");
            return;
        }

        int GGACount = containerCounter.GGACount;
        int GGBCount = containerCounter.GGBCount;
        int looseItemCount = containerCounter.LooseItemCount;

        if (containerCounter.IsCorrect())
        {
            feedbackText.text =
                    $"Correct!\n" +
                     $"{GGBCount} packages and "+
                    $"{looseItemCount} pieces have been placed.\n";

            feedbackText.color = Color.green;
            congratulationsPanel.SetActive(true);
            instructionText.text = string.Empty;
            return;
        }

        string wrongPackage = containerCounter.OtherCount > 0 ? WrongPackageLine + "\n" : string.Empty;

        feedbackText.text =
            $"Check the order again.\n" +
            wrongPackage +
            $"Packages: {GGBCount + GGACount} / 2\n" +
            $"Loose pieces: {looseItemCount} / 5";

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