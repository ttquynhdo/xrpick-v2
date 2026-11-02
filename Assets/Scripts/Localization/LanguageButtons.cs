using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// #11: the Deutsch / English row on the WelcomeScene keypad. It is shown only with the keypad,
// i.e. before a session starts, so the language stays fixed for a whole session.
public class LanguageButtons : MonoBehaviour
{
    // Language names stay in their own language in both locales (no table key).
    private const string GermanLabel = "Deutsch";
    private const string EnglishLabel = "English";
    private const string CurrentMark = "» ";

    [SerializeField] private TMP_Text germanText;
    [SerializeField] private TMP_Text englishText;

    private void Start()
    {
        if (RequiredReference.IsMissing(germanText, nameof(germanText), this)
            | RequiredReference.IsMissing(englishText, nameof(englishText), this))
        {
            return;
        }

        MarkCurrent(germanText, GermanLabel, Loc.CurrentLanguage == Loc.German);
        MarkCurrent(englishText, EnglishLabel, Loc.CurrentLanguage == Loc.English);
    }

    // Called by the buttons' onClick (Inspector).
    public void SelectGerman()
    {
        Select(Loc.German);
    }

    public void SelectEnglish()
    {
        Select(Loc.English);
    }

    private static void Select(string language)
    {
        if (Loc.CurrentLanguage == language)
        {
            return;
        }

        Loc.SetLanguage(language);
        // Reload so texts written by code are rebuilt in the new language too (decision 002).
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void MarkCurrent(TMP_Text label, string name, bool current)
    {
        label.text = current ? CurrentMark + name : name;
        label.fontStyle = current ? FontStyles.Bold : FontStyles.Normal;
    }
}
