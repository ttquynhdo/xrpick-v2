using TMPro;
using UnityEngine;

/// <summary>
/// #45: the label board in ConceptScene. Shows the grabbed package's label explained, or a prompt when nothing
/// is held, so the board is never empty. Quiz feedback has its own card (#32).
/// </summary>
public class PackageInfoDisplay : MonoBehaviour
{
    // Loc table UI (#54).
    private const string DefaultTitleKey = "concept.label.default_title";
    private const string DefaultDescriptionKey = "concept.label.default_body";

    [Header("Panel Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    private void Start()
    {
        ShowDefaultInfo();
    }

    public void ShowPackageInfo(string title, string description)
    {
        SetText(titleText, title);
        SetText(descriptionText, description);
    }

    public void ShowDefaultInfo()
    {
        SetText(titleText, Loc.Get(DefaultTitleKey));
        SetText(descriptionText, Loc.Get(DefaultDescriptionKey));
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
