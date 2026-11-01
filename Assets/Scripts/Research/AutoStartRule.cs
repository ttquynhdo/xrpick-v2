/// <summary>
/// Decides whether the logger starts the default session by itself. Pure C#, so it is testable.
/// </summary>
public static class AutoStartRule
{
    /// <summary>
    /// The first build scene (WelcomeScene) hosts the participant keypad, which starts the session
    /// itself; every other start scene (Editor shortcut) still auto-starts. A scene outside the build
    /// reports index -1 and auto-starts too.
    /// </summary>
    public static bool ShouldAutoStart(bool autoStartSetting, int activeSceneBuildIndex)
    {
        return autoStartSetting && activeSceneBuildIndex != 0;
    }
}
