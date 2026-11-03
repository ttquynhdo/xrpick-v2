using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// #35, docs/decisions/011: marks the lesson of the open scene in the Menu with bold text and a "» " mark,
/// so it is not shown by colour only. #54: also writes the menu title and every lesson name in the current
/// language. Each item is found by the SceneMenuController method its toggle calls, not by its text.
/// </summary>
public class MenuCurrentLesson : MonoBehaviour
{
    private const string Mark = "» ";
    private const string TitleKey = "menu.title";
    private const string ItemLabelName = "Title";

    private static readonly Dictionary<string, string> SceneByMethod = new()
    {
        { nameof(SceneMenuController.OpenWelcome), "WelcomeScene" },
        { nameof(SceneMenuController.OpenConcept), "ConceptScene" },
        { nameof(SceneMenuController.OpenBasicPicking), "BasicPickingScene" },
        { nameof(SceneMenuController.OpenMixedPicking), "MixedPickingScene" },
        { nameof(SceneMenuController.OpenComplex), "ComplexScene" },
    };

    [Tooltip("The heading above the lesson items.")]
    [SerializeField] private TMP_Text menuTitle;

    private void Start()
    {
        if (!RequiredReference.IsMissing(menuTitle, nameof(menuTitle), this))
            menuTitle.text = Loc.Get(TitleKey);

        string activeScene = SceneManager.GetActiveScene().name;
        bool marked = false;

        foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true))
        {
            if (!TryGetScene(toggle, out string scene) || !LessonNames.TryGetDisplayName(scene, out string lesson))
                continue;

            TMP_Text label = LabelOf(toggle);
            if (label == null)
            {
                Debug.LogWarning($"MenuCurrentLesson: menu item '{toggle.name}' has no TMP named {ItemLabelName}.", this);
                continue;
            }

            bool current = scene == activeScene;
            label.text = current ? Mark + lesson : lesson;
            if (current)
            {
                label.fontStyle |= FontStyles.Bold;
                marked = true;
            }
        }

        if (!marked)
            Debug.LogWarning($"MenuCurrentLesson: no menu item opens {activeScene}.", this);
    }

    private static bool TryGetScene(Toggle toggle, out string scene)
    {
        for (int i = 0; i < toggle.onValueChanged.GetPersistentEventCount(); i++)
        {
            if (SceneByMethod.TryGetValue(toggle.onValueChanged.GetPersistentMethodName(i), out scene))
                return true;
        }

        scene = null;
        return false;
    }

    private static TMP_Text LabelOf(Toggle toggle)
    {
        foreach (TMP_Text text in toggle.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == ItemLabelName)
                return text;
        }

        return null;
    }
}
