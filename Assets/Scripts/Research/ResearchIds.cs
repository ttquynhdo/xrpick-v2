using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Participant ID rules for the research logger: "P" + 3 digits.
/// P000 and P900-P999 are test IDs and are kept apart from real data.
/// P800-P899 are pilot IDs (decision 023): they run like real participants but log to their own
/// folder, so the pilot is analysed apart from the study. Every ID keeps the language chosen on the
/// keypad (#184).
/// </summary>
public static class ResearchIds
{
    public const string PracticeFolder = "practice";
    public const string PilotFolder = "pilot";
    private static readonly Regex IdPattern = new Regex(@"^P[0-9]{3}$", RegexOptions.CultureInvariant);

    public static bool IsValid(string id)
    {
        return id != null && IdPattern.IsMatch(id);
    }

    public static bool IsTestId(string id)
    {
        if (!IsValid(id))
        {
            return false;
        }

        // Compare as text so the rule stays visible: P000 and P900..P999.
        return id == "P000" || string.CompareOrdinal(id, "P900") >= 0;
    }

    public static bool IsPilotId(string id)
    {
        if (!IsValid(id))
        {
            return false;
        }

        return string.CompareOrdinal(id, "P800") >= 0 && string.CompareOrdinal(id, "P899") <= 0;
    }

    /// <summary>
    /// Test IDs go to "practice", pilot IDs to "pilot", so neither mixes with the study's logs.
    /// </summary>
    public static string FolderFor(string researchRoot, string id)
    {
        if (IsTestId(id))
        {
            return Path.Combine(researchRoot, PracticeFolder);
        }

        return IsPilotId(id) ? Path.Combine(researchRoot, PilotFolder) : researchRoot;
    }
}
