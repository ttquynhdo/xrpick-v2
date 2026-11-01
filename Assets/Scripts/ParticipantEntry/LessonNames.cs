public static class LessonNames
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    // \u00A0 (no-break space) after the number keeps "4." on the same line as the name.
    private const string Welcome = "1.\u00A0Einführung";                        // lesson.welcome
    private const string Concept = "2.\u00A0Grundlagen";                        // lesson.concept
    private const string Basic = "3.\u00A0Kommissionieren nach EKZ 3";          // lesson.basic
    private const string Mixed = "4.\u00A0Mehrere Positionen";                  // lesson.mixed
    private const string Complex = "5.\u00A0Volle Kartons und loses Material";    // lesson.complex

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
