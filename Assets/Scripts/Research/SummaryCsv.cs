using System;
using System.Globalization;
using System.Text;

/// <summary>Formats VisitSummary rows as CSV: comma separated, dot decimal, empty = not measured.</summary>
public static class SummaryCsv
{
    public const string Header =
        "participant_id,session_id,build,schema_version,scene,scene_visit,enter_utc,duration_s,paused_s," +
        "confirm_count,first_attempt_correct,attempts_to_correct,time_to_first_confirm_s,time_to_correct_s," +
        "completed,error_codes_all,grabs,drops_floor,container_enters,container_exits,dwell_wms_s," +
        "dwell_instructions_s,pauses,hmd_removed_count,fps_avg,dropped_frames,unity_errors";

    private const int ColumnCount = 27;

    public static string FormatRow(VisitSummary v)
    {
        // Index = position in Header; untouched cells stay empty on purpose.
        var cells = new string[ColumnCount];
        cells[0] = v.ParticipantId;
        cells[1] = v.SessionId;
        cells[2] = v.Build;
        cells[3] = Number(v.SchemaVersion);
        cells[4] = v.Scene;
        cells[5] = Number(v.SceneVisit);
        cells[6] = v.EnterUtc;
        cells[7] = Number(v.DurationS);
        cells[8] = Number(v.PausedS);
        cells[22] = Number(v.Pauses);
        cells[23] = Number(v.HmdRemovedCount);
        cells[26] = Number(v.UnityErrors);

        var sb = new StringBuilder();
        for (int i = 0; i < cells.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(Quote(cells[i]));
        }

        return sb.ToString();
    }

    private static string Number(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    // NaN/Infinity would be an invalid value in R; an empty cell means "not measured".
    private static string Number(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return string.Empty;
        }

        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string Quote(string cell)
    {
        if (string.IsNullOrEmpty(cell))
        {
            return string.Empty;
        }

        if (cell.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
        {
            return cell;
        }

        return "\"" + cell.Replace("\"", "\"\"") + "\"";
    }
}
