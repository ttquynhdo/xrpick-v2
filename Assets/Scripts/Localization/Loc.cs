using System.Globalization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

/// <summary>
/// The one entry point for trainee texts built in code (decision 002, #11). Scene texts use
/// LocalizeStringEvent on the same table. Placeholders are named, e.g. {n}, and filled here,
/// so the table needs no Smart String syntax.
/// </summary>
public static class Loc
{
    public const string German = "DE";
    public const string English = "EN";

    private const string TableName = "UI";

    // Also read by the Localization package's PlayerPrefLocaleSelector at startup
    // (Assets/Localization/Localization Settings.asset): keep both the same.
    private const string PreferenceKey = "xrpick.locale";
    private const string GermanCode = "de";
    private const string EnglishCode = "en";

    /// <summary>"DE" or "EN": the saved choice, German when nothing (or something unknown) is saved.</summary>
    public static string CurrentLanguage =>
        PlayerPrefs.GetString(PreferenceKey, GermanCode) == EnglishCode ? English : German;

    /// <summary>The text of <paramref name="key"/> in the current language; the key itself when it is missing.</summary>
    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("Loc: empty key.");
            return string.Empty;
        }

        // A missing key raises TranslationNotFound (logged below) and returns the key, because the
        // settings' "no translation found" message is "{key}".
        return LocalizationSettings.StringDatabase.GetLocalizedString(TableName, key);
    }

    /// <summary>Same, with named placeholders: Get("concept.quiz.progress", ("n", 2), ("total", 4)).</summary>
    public static string Get(string key, params (string name, object value)[] args)
    {
        string text = Get(key);
        foreach ((string name, object value) in args)
        {
            string placeholder = "{" + name + "}";
            if (!text.Contains(placeholder))
            {
                Debug.LogError($"Loc: '{key}' has no placeholder {placeholder} in {CurrentLanguage}.");
                continue;
            }

            text = text.Replace(placeholder, System.Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        return text;
    }

    /// <summary>
    /// Saves and selects the language ("DE" / "EN"). Scene texts follow at once; texts written
    /// by code don't, so the caller reloads the scene (decision 002).
    /// </summary>
    public static void SetLanguage(string language)
    {
        string code = language == English ? EnglishCode : GermanCode;
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(code));
        if (locale == null)
        {
            // Change nothing, so the saved choice, the log header and the screen stay in step.
            Debug.LogError($"Loc: locale '{code}' is not in the Localization Settings.");
            return;
        }

        PlayerPrefs.SetString(PreferenceKey, code);
        PlayerPrefs.Save();
        ResearchLogger.SetLanguage(CurrentLanguage);
        LocalizationSettings.SelectedLocale = locale;
    }

    // Before the logger's AfterSceneLoad auto-start, so the first header already has the language.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        ResearchLogger.SetLanguage(CurrentLanguage);

        // Remove first: with domain reload off in the Editor the handler would be added twice.
        LocalizationSettings.StringDatabase.TranslationNotFound -= OnTranslationNotFound;
        LocalizationSettings.StringDatabase.TranslationNotFound += OnTranslationNotFound;
    }

    private static void OnTranslationNotFound(string key, long keyId, TableReference tableReference,
        StringTable table, Locale locale, string noTranslationFoundMessage)
    {
        string name = string.IsNullOrEmpty(key) ? $"id {keyId}" : $"'{key}'";
        Debug.LogError($"Loc: missing text for key {name} in table {tableReference.TableCollectionName} ({locale?.Identifier.Code}).");
    }
}
