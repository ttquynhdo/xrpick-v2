using TMPro;
using UnityEngine;

public class MixedPickingValidator : MonoBehaviour
{
    [SerializeField] private MixedPickingContainer container;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text instructionText;
    private void Start()
    {

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
    }

    public void ConfirmOrder()
    {
        
        if (container == null)
        {
            Debug.LogError("MixedPickingContainer is not assigned.");
            return;
        }

        int gga = container.GGACount;
        int ggb = container.GGBCount;
        int vpe = container.VPECount;

        if (gga == 2 && vpe == 0 && ggb == 2)
        {
            feedbackText.text =
                "Correct!\n2 GG Connector A packages and 2 GG Screw packages selected.";
               
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
                    $"GG Connector A: {gga} / 2\n" +
                    $"GG Screw B: {ggb} / 2\n" +
                    $"VPE: {vpe} / 0";
            feedbackText.color = Color.red;
            
        }
        else
        {
            instructionText.text = string.Empty;
            feedbackText.text =
                   $"Check the order again.\n" +
                    $"GG Connector A: {gga} / 2\n" +
                    $"GG Screw B: {ggb} / 2\n" +
                    $"VPE: {vpe} / 0";
            feedbackText.color = Color.red;
        }
    }
}