using TMPro;
using UnityEngine;

/// <summary>
/// #45: the label board in ConceptScene. Shows the grabbed package's label explained, or a prompt when nothing
/// is held, so the board is never empty. Quiz feedback has its own card (#32).
/// </summary>
public class PackageInfoDisplay : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string DefaultTitle = "Etikett lesen";                                             // concept.label.default_title
    private const string DefaultDescription = "Nehmen Sie eine Verpackung in die Hand. Hier sehen Sie, was auf dem Etikett steht.";  // concept.label.default_body

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
        SetText(titleText, DefaultTitle);
        SetText(descriptionText, DefaultDescription);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
