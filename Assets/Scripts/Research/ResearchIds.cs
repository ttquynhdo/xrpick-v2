using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Participant ID rules for the research logger: "P" + 3 digits.
/// P000 and P900-P999 are test IDs and are kept apart from real data.
/// </summary>
public static class ResearchIds
{
    private const string PracticeFolder = "practice";
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

    /// <summary>Test IDs go to a "practice" subfolder so they never mix with real logs.</summary>
    public static string FolderFor(string researchRoot, string id)
    {
        return IsTestId(id) ? Path.Combine(researchRoot, PracticeFolder) : researchRoot;
    }
}
