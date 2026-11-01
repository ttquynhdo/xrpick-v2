using TMPro;
using UnityEngine;

public class MixedPickingValidator : MonoBehaviour
{
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

        int gga = container.GGACount;
        int ggb = container.GGBCount;
        int vpe = container.VPECount;

        if (gga == 2 && vpe == 0 && ggb == 2)
        {
            feedbackText.text =
                "Correct!\n2 cartons KABELVERBINDER A and 2 bags SCHRAUBE B selected.";
               
            feedbackText.color = Color.green;

            instructionText.text = "You have successfully completed the task.\n" +
                "Select the next scenario option in the MENU.";

            return;
        }

        if (gga < 2 || ggb < 2)
        {
            instructionText.text = string.Empty;
            feedbackText.text =
                    $"Check the order again.\n" +
                    $"KABELVERBINDER A: {gga} / 2\n" +
                    $"SCHRAUBE B: {ggb} / 2\n" +
                    $"RING D: {vpe} / 0";
            feedbackText.color = Color.red;

        }
        else
        {
            instructionText.text = string.Empty;
            feedbackText.text =
                   $"Check the order again.\n" +
                    $"KABELVERBINDER A: {gga} / 2\n" +
                    $"SCHRAUBE B: {ggb} / 2\n" +
                    $"RING D: {vpe} / 0";
            feedbackText.color = Color.red;
        }
    }
}