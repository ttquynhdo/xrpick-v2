using UnityEngine;

public class PackageInfoOnGrab : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private PackageInfoDisplay packageInfoDisplay;
    [SerializeField] private SampleFormulaDisplay sampleFormulaDisplay;

    [Header("Instruction Panel Information")]
    [SerializeField] private string packageTitle;

    [TextArea(2, 5)]
    [SerializeField] private string packageDescription;

    [TextArea(2, 4)]
    [SerializeField] private string packageRule;

    [Header("Sample Formula Information")]
    [SerializeField] private string formulaPackageType;
    [SerializeField] private string formulaMaterial;
    [SerializeField] private int formulaOrderQuantity;
    [SerializeField] private int formulaPackageSize;

    public void ShowInfo()
    {
        // Update Instruction Panel
        if (packageInfoDisplay != null)
        {
            packageInfoDisplay.ShowPackageInfo(
                packageTitle,
                packageDescription,
                packageRule);
        }

        // Update Sample Formula Panel
        if (sampleFormulaDisplay != null)
        {
            sampleFormulaDisplay.ShowFormula(
                formulaPackageType,
                formulaMaterial,
                formulaOrderQuantity,
                formulaPackageSize);
        }
    }

    public void ShowDefaultInfo()
    {
        if (packageInfoDisplay != null)
        {
            packageInfoDisplay.ShowDefaultInfo();
        }

        if (sampleFormulaDisplay != null)
        {
            sampleFormulaDisplay.ShowDefaultFormula();
        }
    }
}