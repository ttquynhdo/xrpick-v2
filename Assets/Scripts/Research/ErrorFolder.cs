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

    private readonly double _windowSeconds;
    private readonly int _cap;
    private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
    private int _written;
    private bool _capReported;

    public ErrorFolder(double windowSeconds = 5, int cap = 200)
    {
        _windowSeconds = windowSeconds;
        _cap = cap;
    }

    public ErrorDecision Decide(string message, double t)
    {
        message = message ?? string.Empty;

        if (_written >= _cap)
        {
            bool first = !_capReported;
            _capReported = true;
            return new ErrorDecision(false, 0, first);
        }

        if (_entries.TryGetValue(message, out Entry entry) && t - entry.LastWrittenAt < _windowSeconds)
        {
            entry.Folded++;
            return new ErrorDecision(false, 0, false);
        }

        int folded = entry != null ? entry.Folded : 0;
        if (entry == null)
        {
            entry = new Entry();
            _entries[message] = entry;
        }

        entry.LastWrittenAt = t;
        entry.Folded = 0;
        _written++;
        return new ErrorDecision(true, folded, false);
    }

    /// <summary>
    /// Returns every message with swallowed repeats not yet written, with its count, and
    /// resets those counts. Works after the cap was reached.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, int>> DrainPending()
    {
        var pending = new List<KeyValuePair<string, int>>();
        foreach (KeyValuePair<string, Entry> pair in _entries)
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
