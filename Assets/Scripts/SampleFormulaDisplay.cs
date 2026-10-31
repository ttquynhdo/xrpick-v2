using TMPro;
using UnityEngine;

public class SampleFormulaDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text formulaContentText;

    private const string DefaultContent =
        "";

    private void Start()
    {
        ShowDefaultFormula();
    }

    public void ShowFormula(
        string packageType,
        string material,
        int orderQuantity,
        int packageSize)
    {
        if (formulaContentText == null)
        {
            Debug.LogWarning("Formula Content Text has not been assigned.");
            return;
        }

        int requiredPackages = orderQuantity / packageSize;

        formulaContentText.text =
            $"<color=#FFD700><b>{packageType} order:</b></color>\n\n" +
            $"Material: <b>{material}</b>\n" +
            $"Order quantity: <b>{orderQuantity} pcs</b>\n" +
            $"Package size: <b>{packageSize} pcs / package</b>\n\n" +
            $"Rule: <b>{orderQuantity} ÷ {packageSize} = " +
            $"{requiredPackages} {packageType} packages</b>";
    }

    public void ShowDefaultFormula()
    {
        if (formulaContentText != null)
        {
            formulaContentText.text = DefaultContent;
        }
    }
}