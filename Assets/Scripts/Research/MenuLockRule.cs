/// <summary>
/// #42, research spec section 9: during a research session every learner walks the course once, in
/// build order. Pure C#, so it is testable.
/// </summary>
public static class MenuLockRule
{
    /// <summary>
    /// True when the Menu item for <paramref name="targetBuildIndex"/> may be opened from the scene with
    /// <paramref name="currentBuildIndex"/>. Without a session (null ID) or with a test ID every item is
    /// open; with a research ID only the next lesson is, and only once the current lesson is done (no going
    /// back, no replaying the current one, no leaving a lesson early by a stray touch).
    /// </summary>
    public static bool IsOpen(string participantId, int currentBuildIndex, int targetBuildIndex, bool lessonDone)
    {
        if (participantId == null || ResearchIds.IsTestId(participantId))
            return true;

        return lessonDone && currentBuildIndex >= 0 && targetBuildIndex == currentBuildIndex + 1;
    }
}
