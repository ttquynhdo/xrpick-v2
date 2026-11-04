using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// File layout of one research session: appends event lines and summary rows, and
/// recovers sessions that ended without session_end. IO failures never throw; they
/// turn the affected file off for the rest of the session.
/// </summary>
public sealed class SessionFiles
{
    private const string EventsSuffix = "_events.jsonl";

    private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

    private readonly string folder;
    private bool eventsFailed;
    private bool summaryFailed;

    public string EventsPath { get; }
    public string SummaryPath { get; }

    public SessionFiles(string researchRoot, string participantId, string sessionId)
    {
        folder = ResearchIds.FolderFor(researchRoot, participantId);
        EventsPath = Path.Combine(folder, participantId + "_" + sessionId + EventsSuffix);
        SummaryPath = Path.Combine(folder, participantId + "_" + sessionId + "_summary.csv");
    }

    /// <summary>Builds one event line with the common fields in the agreed order.</summary>
    public static string EventLine(double? t, string utc, string participantId, string sessionId, string scene,
        int? sceneVisit, string eventName, JsonFields data)
    {
        double? rounded = t.HasValue ? Math.Round(t.Value, 3) : (double?)null;
        return new JsonFields()
            .Add("t", rounded)
            .Add("utc", utc)
            .Add("participant_id", participantId)
            .Add("session_id", sessionId)
            .Add("scene", scene)
            .Add("scene_visit", sceneVisit)
            .Add("event", eventName)
            .AddRaw("data", data != null ? data.ToJson() : "{}")
            .ToJson();
    }

    /// <summary>
    /// Appends one event line. With <paramref name="forceToDisk"/> the OS cache is flushed to the
    /// storage device too, for the lines written right before the headset may sleep or power off.
    /// </summary>
    public bool AppendEvent(string jsonLine, bool forceToDisk = false)
    {
        if (eventsFailed)
        {
            return false;
        }

        if (!TryAppend(EventsPath, jsonLine + "\n", forceToDisk))
        {
            eventsFailed = true;
            return false;
        }

        return true;
    }

    public bool AppendSummary(string csvRow)
    {
        if (summaryFailed)
        {
            return false;
        }

        // The header goes in front of the first row of a new file.
        string text = File.Exists(SummaryPath) ? csvRow + "\n" : SummaryCsv.Header + "\n" + csvRow + "\n";
        if (!TryAppend(SummaryPath, text, false))
        {
            summaryFailed = true;
            return false;
        }

        return true;
    }

    private bool TryAppend(string path, string text, bool forceToDisk)
    {
        try
        {
            Directory.CreateDirectory(folder);
            byte[] bytes = Utf8NoBom.GetBytes(text);
            using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
                // Flush(true) also flushes the OS buffers (fsync), not only the stream's own.
                stream.Flush(forceToDisk);
            }

            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    /// Adds one crash_recovered line to every events file whose last parseable line is
    /// neither session_end nor crash_recovered. Returns how many files were marked.
    /// </summary>
    public static int MarkCrashedSessions(string researchRoot, string recoveredBySessionId, string utcNow)
    {
        int marked = 0;
        foreach (var dir in new[]
                 {
                     researchRoot,
                     Path.Combine(researchRoot, ResearchIds.PracticeFolder),
                     Path.Combine(researchRoot, ResearchIds.PilotFolder),
                 })
        {
            foreach (var path in ListEventFiles(dir, "*" + EventsSuffix))
            {
                if (TryMarkCrashed(path, recoveredBySessionId, utcNow))
                {
                    marked++;
                }
            }
        }

        return marked;
    }

    private static bool TryMarkCrashed(string path, string recoveredBySessionId, string utcNow)
    {
        try
        {
            string text = File.ReadAllText(path, Utf8NoBom);
            var lines = ParseLines(text.Split('\n'));
            if (lines.Count == 0)
            {
                return false;
            }

            var last = lines[lines.Count - 1];
            if (last.Event == "session_end" || last.Event == "crash_recovered")
            {
                return false;
            }

            double? lastT = null;
            if (double.TryParse(last.TRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
            {
                lastT = parsed;
            }

            var data = new JsonFields()
                .Add("last_event_t", lastT)
                .Add("recovered_by_session_id", recoveredBySessionId);
            string line = EventLine(null, utcNow, last.ParticipantId, last.SessionId, null, null, "crash_recovered", data);

            // A cut-off last line has no newline: start a new line so the marker stays parseable.
            string prefix = text.EndsWith("\n", StringComparison.Ordinal) ? string.Empty : "\n";
            File.AppendAllText(path, prefix + line + "\n", Utf8NoBom);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    /// Finds the participant's newest earlier session (by header utc). Returns true with the scene
    /// of its last scene_enter only when that session has no session_end.
    /// </summary>
    public static bool TryFindResumePoint(string researchRoot, string participantId, string excludeSessionId,
        out string scene)
    {
        scene = null;
        string participantFolder = ResearchIds.FolderFor(researchRoot, participantId);
        string excluded = participantId + "_" + excludeSessionId + EventsSuffix;

        List<ParsedLine> newest = null;
        string newestUtc = null;
        foreach (var path in ListEventFiles(participantFolder, participantId + "_*" + EventsSuffix))
        {
            if (Path.GetFileName(path) == excluded)
            {
                continue;
            }

            List<ParsedLine> lines;
            string utc = null;
            try
            {
                lines = ParseLines(File.ReadAllText(path, Utf8NoBom).Split('\n'));
                foreach (var l in lines)
                {
                    if (l.Event == "header")
                    {
                        utc = l.Utc;
                        break;
                    }
                }

                // Without a readable header utc the file's write time stands in for it.
                if (utc == null)
                {
                    utc = File.GetLastWriteTimeUtc(path).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
                }
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            if (newest == null || string.CompareOrdinal(utc, newestUtc) > 0)
            {
                newest = lines;
                newestUtc = utc;
            }
        }

        if (newest == null)
        {
            return false;
        }

        string lastScene = null;
        foreach (var l in newest)
        {
            if (l.Event == "session_end")
            {
                return false;
            }

            if (l.Event == "scene_enter" && l.Scene != null)
            {
                lastScene = l.Scene;
            }
        }

        scene = lastScene;
        return lastScene != null;
    }

    /// <summary>
    /// True when any earlier events file of the participant (not the excluded session) has a
    /// parseable session_end line.
    /// </summary>
    public static bool HasEndedSession(string researchRoot, string participantId, string excludeSessionId)
    {
        string participantFolder = ResearchIds.FolderFor(researchRoot, participantId);
        string excluded = participantId + "_" + excludeSessionId + EventsSuffix;
        foreach (var path in ListEventFiles(participantFolder, participantId + "_*" + EventsSuffix))
        {
            if (Path.GetFileName(path) == excluded)
            {
                continue;
            }

            try
            {
                foreach (var l in ParseLines(File.ReadAllText(path, Utf8NoBom).Split('\n')))
                {
                    if (l.Event == "session_end")
                    {
                        return true;
                    }
                }
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
        }

        return false;
    }

    private static string[] ListEventFiles(string dir, string pattern)
    {
        try
        {
            return Directory.Exists(dir) ? Directory.GetFiles(dir, pattern) : new string[0];
        }
        catch (IOException)
        {
            return new string[0];
        }
        catch (UnauthorizedAccessException)
        {
            return new string[0];
        }
    }

    // Minimal parsing on purpose: only top-level fields written before "data" are read.
    private struct ParsedLine
    {
        public string Event;
        public string Scene;
        public string Utc;
        public string ParticipantId;
        public string SessionId;
        public string TRaw;
    }

    private static List<ParsedLine> ParseLines(string[] rawLines)
    {
        var result = new List<ParsedLine>();
        foreach (var raw in rawLines)
        {
            string line = raw.Trim();
            // A line cut off by a crash lacks the closing brace and is skipped.
            if (line.Length < 2 || line[0] != '{' || line[line.Length - 1] != '}')
            {
                continue;
            }

            string eventName = StringField(line, "event");
            if (eventName == null)
            {
                continue;
            }

            result.Add(new ParsedLine
            {
                Event = eventName,
                Scene = StringField(line, "scene"),
                Utc = StringField(line, "utc"),
                ParticipantId = StringField(line, "participant_id"),
                SessionId = StringField(line, "session_id"),
                TRaw = RawField(line, "t")
            });
        }

        return result;
    }

    private static string StringField(string line, string key)
    {
        string raw = RawField(line, key);
        if (raw == null || raw.Length < 2 || raw[0] != '"')
        {
            return null;
        }

        int end = 1;
        while (end < raw.Length && raw[end] != '"')
        {
            end += raw[end] == '\\' ? 2 : 1;
        }

        return end < raw.Length ? raw.Substring(1, end - 1) : null;
    }

    // Returns the text after "key": up to the next top-level comma/brace, quoted strings kept whole.
    private static string RawField(string line, string key)
    {
        string needle = "\"" + key + "\":";
        int start = line.IndexOf(needle, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        start += needle.Length;
        if (start >= line.Length)
        {
            return null;
        }

        if (line[start] == '"')
        {
            int end = start + 1;
            while (end < line.Length && line[end] != '"')
            {
                end += line[end] == '\\' ? 2 : 1;
            }

            return end < line.Length ? line.Substring(start, end - start + 1) : null;
        }

        int stop = start;
        while (stop < line.Length && line[stop] != ',' && line[stop] != '}')
        {
            stop++;
        }

        return line.Substring(start, stop - start);
    }
}
