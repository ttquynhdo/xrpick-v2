using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Decides which error messages reach the log: identical messages inside a time window are
/// folded into a count, and a session-wide cap limits the number of written lines.
/// </summary>
public sealed class ErrorFolder
{
    // A path segment may contain spaces ("Anna Maria"), but never swallows a file name:
    // a ".ext" followed by ':', whitespace or the end of the text stops the segment.
    private const string FileEnd = @"(?!\.\w+(?:[:\s]|$))";

    // Only rooted paths are stripped (drive letter, \\server, /Users, /home, leading /),
    // so "and/or", "Soll/Ist", "3/5" and URLs keep their meaning.
    private static readonly Regex DirectoryPrefix = new Regex(
        @"(?<![A-Za-z])[A-Za-z]:[\\/](?:(?:" + FileEnd + @"[^\\/:*?""<>|\r\n])+[\\/])*"
        + @"|\\\\(?:(?:" + FileEnd + @"[^\\/:*?""<>|\r\n])+[\\/])+"
        + @"|(?<![^\s""'(=])/(?:Users|home)/(?:(?:" + FileEnd + @"[^/\r\n])+/)+"
        + @"|(?<![^\s""'(=])/(?:[^/\s]+/)+",
        RegexOptions.Compiled);

    private sealed class Entry
    {
        public double LastWrittenAt;
        public int Folded;
    }

    private readonly double windowSeconds;
    private readonly int cap;
    private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
    private int written;
    private bool capReported;

    public ErrorFolder(double windowSeconds = 5, int cap = 200)
    {
        this.windowSeconds = windowSeconds;
        this.cap = cap;
    }

    public ErrorDecision Decide(string message, double t)
    {
        message = message ?? string.Empty;

        if (written >= cap)
        {
            bool first = !capReported;
            capReported = true;
            return new ErrorDecision(false, 0, first);
        }

        if (entries.TryGetValue(message, out Entry entry) && t - entry.LastWrittenAt < windowSeconds)
        {
            entry.Folded++;
            return new ErrorDecision(false, 0, false);
        }

        int folded = entry != null ? entry.Folded : 0;
        if (entry == null)
        {
            entry = new Entry();
            entries[message] = entry;
        }

        entry.LastWrittenAt = t;
        entry.Folded = 0;
        written++;
        return new ErrorDecision(true, folded, false);
    }

    /// <summary>
    /// Returns every message with swallowed repeats not yet written, with its count, and
    /// resets those counts. The count is also the line's n_errors (no new error comes with it).
    /// Works after the cap was reached.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, int>> DrainPending()
    {
        var pending = new List<KeyValuePair<string, int>>();
        foreach (KeyValuePair<string, Entry> pair in entries)
        {
            if (pair.Value.Folded > 0)
            {
                pending.Add(new KeyValuePair<string, int>(pair.Key, pair.Value.Folded));
                pair.Value.Folded = 0;
            }
        }

        return pending;
    }

    /// <summary>Cuts file paths (Windows or POSIX) down to the file name.</summary>
    public static string StripPaths(string text)
    {
        return string.IsNullOrEmpty(text) ? text : DirectoryPrefix.Replace(text, string.Empty);
    }
}
