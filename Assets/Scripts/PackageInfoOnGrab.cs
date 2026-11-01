using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// #45, docs/decisions/007 (5): on grab, the label board explains this package's Mat.-Nr and VPE (from its
/// PackageData); on release the package info disappears (D3). No division result is shown.
/// </summary>
public class PackageInfoOnGrab : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string MatNrFormat = "Mat.-Nr: {0} – die Materialnummer. Dieselbe Nummer steht im LVS.";     // concept.label.matnr
    private const string VpeFormat = "VPE: {0} ST – so viele Stück sind in dieser Verpackung.";                 // concept.label.vpe

    [Header("Panels")]
    [SerializeField] private PackageInfoDisplay packageInfoDisplay;

    [Header("Package")]
    [SerializeField] private PackageData package;

    [Tooltip("Material name as on the label (Bez.), e.g. KABELVERBINDER A.")]
    [FormerlySerializedAs("packageTitle")]
    [SerializeField] private string materialName;

    public void ShowInfo()
    {
        if (RequiredReference.IsMissing(packageInfoDisplay, nameof(packageInfoDisplay), this)
            | RequiredReference.IsMissing(package, nameof(package), this))
            return;

        string description =
            string.Format(MatNrFormat, package.MaterialNumber) + "\n\n" +
            string.Format(VpeFormat, package.Vpe);

        packageInfoDisplay.ShowPackageInfo(materialName, description);
    }

    public void ShowDefaultInfo()
    {
        if (packageInfoDisplay != null)
        {
            packageInfoDisplay.ShowDefaultInfo();
        }
    }
}
