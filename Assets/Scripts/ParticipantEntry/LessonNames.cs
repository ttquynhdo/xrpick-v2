public static class LessonNames
{
    // Loc table UI (#54). The entries keep a no-break space after the number, so "4." stays on the
    // same line as the name.
    private const string Welcome = "lesson.welcome";
    private const string Concept = "lesson.concept";
    private const string Basic = "lesson.basic";
    private const string Mixed = "lesson.mixed";
    private const string Complex = "lesson.complex";

    // Unknown scene names return false (no resume point).
    public static bool TryGetDisplayName(string sceneName, out string displayName)
    {
        string key = KeyOf(sceneName);
        displayName = key != null ? Loc.Get(key) : null;
        return key != null;
    }

    private static string KeyOf(string sceneName)
    {
        switch (sceneName)
        {
            case "WelcomeScene": return Welcome;
            case "ConceptScene": return Concept;
            case "BasicPickingScene": return Basic;
            case "MixedPickingScene": return Mixed;
            case "ComplexScene": return Complex;
            default: return null;
        }
    }
}
