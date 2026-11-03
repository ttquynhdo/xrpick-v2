using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// #45, docs/decisions/007 (5): on grab, the label board explains this package's Mat.-Nr and VPE (from its
/// PackageData); on release the package info disappears (D3). No division result is shown.
/// </summary>
public class PackageInfoOnGrab : MonoBehaviour
{
    // Loc table UI (#54).
    private const string MatNrKey = "concept.label.matnr";
    private const string VpeKey = "concept.label.vpe";

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
            Loc.Get(MatNrKey, ("matnr", package.MaterialNumber)) + "\n\n" +
            Loc.Get(VpeKey, ("vpe", package.Vpe));

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
