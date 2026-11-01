public static class LessonNames
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string Welcome = "1. Einführung";                        // lesson.welcome
    private const string Concept = "2. Grundlagen";                        // lesson.concept
    private const string Basic = "3. Einzelpick nach EKZ 3";               // lesson.basic
    private const string Mixed = "4. Mehrere Positionen";                  // lesson.mixed
    private const string Complex = "5. Volle Kartons + Einzelteile";       // lesson.complex

    // Unknown scene names return false (no resume point).
    public static bool TryGetDisplayName(string sceneName, out string displayName)
    {
        switch (sceneName)
        {
            case "WelcomeScene": displayName = Welcome; return true;
            case "ConceptScene": displayName = Concept; return true;
            case "BasicPickingScene": displayName = Basic; return true;
            case "MixedPickingScene": displayName = Mixed; return true;
            case "ComplexScene": displayName = Complex; return true;
            default:
                displayName = null;
                return false;
        }
    }
}
