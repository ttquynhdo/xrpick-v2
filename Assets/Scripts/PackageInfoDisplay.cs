using TMPro;
using UnityEngine;

public class PackageInfoDisplay : MonoBehaviour
{
    [Header("Panel Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text ruleText;

    private void Start()
    {
        ShowDefaultInfo();
    }

    public void ShowPackageInfo(
        string packageTitle,
        string packageDescription,
        string packageRule)
    {
        titleText.text = packageTitle;
        descriptionText.text = packageDescription;
        ruleText.text = packageRule;
    }

    public void ShowDefaultInfo()
    {
        titleText.text = "";
        descriptionText.text = "Look below to read the instructions on the table.\n" +
        "When you complete the task, select the next scenario option in the Menu.";
        ruleText.text = "";
    }
}