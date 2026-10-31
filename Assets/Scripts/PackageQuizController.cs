using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackageQuizController : MonoBehaviour
{
    [Header("Answer Toggles")]
    [SerializeField] private Toggle ggToggle;
    [SerializeField] private Toggle vpeToggle;
    [SerializeField] private Toggle bothToggle;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    private void Start()
    {
        // Không hiển thị feedback khi scene bắt đầu
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    // Gán function này vào button đỏ
    public void CheckAnswer()
    {
        if (feedbackText == null)
        {
            Debug.LogWarning("Feedback Text has not been assigned.");
            return;
        }

        // Chưa chọn đáp án
        if (!ggToggle.isOn && !vpeToggle.isOn && !bothToggle.isOn)
        {
            feedbackText.text =
                "<color=#F2C94C><b>Please select an answer first.</b></color>";
            return;
        }

        // Đáp án đúng là VPE
        if (vpeToggle.isOn)
        {
            feedbackText.text =
                "<color=#27AE60><b>Correct!</b></color>\n" +
                "VPE packages can be opened when partial quantities are required.\n\n" +
                 "You have successfully completed the task\n" +
                 "<color=#F2C94C><b>Select the next scenario option in the Menu to continue.</b></color>"
                ;
        }
        else
        {
            feedbackText.text =
                "<color=#EB5757><b>Not quite. Try again.</b></color>\n" +
                "GG packages must remain closed. VPE packages can be opened.";
        }
    }
}