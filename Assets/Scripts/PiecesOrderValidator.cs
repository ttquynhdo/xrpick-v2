using TMPro;
using UnityEngine;

public class PiecesOrderValidator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PiecesContainerCounter containerCounter;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject congratulationsPanel;
    [SerializeField] private TMP_Text instructionText;

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
        int totalPieces = containerCounter.TotalPieces;

        if (GGBCount == 2 && GGACount ==0 && looseItemCount == 5)
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

        feedbackText.text =
            $"Check the order again.\n" +
            $"Packages: {GGBCount + GGACount} / 2\n" +
            $"Loose pieces: {looseItemCount} / 5";

        feedbackText.color = Color.red;
        instructionText.text = string.Empty;
    }
}