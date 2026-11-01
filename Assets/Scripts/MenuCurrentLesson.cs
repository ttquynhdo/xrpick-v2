using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// #35, docs/decisions/011: marks the lesson of the open scene in the Menu with bold text and a "» " mark,
/// so it is not shown by colour only. Matches the item labels against LessonNames.
/// </summary>
public class MenuCurrentLesson : MonoBehaviour
{
    private const string Mark = "» ";

    private void Start()
    {
        if (!LessonNames.TryGetDisplayName(SceneManager.GetActiveScene().name, out string current))
            return;

        foreach (TMP_Text label in GetComponentsInChildren<TMP_Text>(true))
        {
            if (Normalize(label.text) != Normalize(current))
                continue;

            label.text = Mark + label.text;
            label.fontStyle |= FontStyles.Bold;
            return;
        }

        Debug.LogWarning($"MenuCurrentLesson: no menu item named \"{current}\".", this);
    }

    // The labels may use a normal or a no-break space after the number.
    private static string Normalize(string text) => text.Replace(' ', ' ').Trim();
}
