using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

/// <summary>
/// Writes the git hash into Resources/ResearchBuildInfo.txt before each player build, so the
/// research log header can tell builds apart (#26): the 7-digit short hash, plus "-dirty" when
/// the tree has uncommitted changes (modified, staged or untracked files that are not ignored).
/// Play mode logs "editor" instead; "unknown" when git fails. The file is git-ignored.
/// #162: a dirty tree stops every build that is not a Development Build, so a study build always
/// comes from a clean commit; a Development Build goes on with a warning that lists the files.
/// </summary>
public sealed class ResearchBuildInfoWriter : IPreprocessBuildWithReport
{
    private const string AssetPath = "Assets/Resources/ResearchBuildInfo.txt";
    private const string Unknown = "unknown";
    private const string DirtySuffix = "-dirty";

    // First of all preprocess callbacks: other packages (performance tests, Localization, Meta XR, URP) write
    // files into the project during their own preprocess and remove them only after a successful build. Run
    // after them, the check would count their files and block every build; a blocked build would also leave
    // their files behind, because no postprocess runs after a failed build.
    public int callbackOrder => int.MinValue;

    public void OnPreprocessBuild(BuildReport report)
    {
        bool development = (report.summary.options & BuildOptions.Development) != 0;
        Prepare(development);
    }

    /// <summary>
    /// Checks the git tree, writes the build string and returns it. Throws BuildFailedException
    /// when the tree is dirty and the build is not a Development Build (#162). Public for MCP tests.
    /// </summary>
    public static string Prepare(bool development)
    {
        string hash = RunGit("rev-parse --short=7 HEAD")?.Trim();
        List<string> changes = ReadChanges();

        string build;
        if (hash == null || changes == null)
        {
            Debug.LogWarning("ResearchBuildInfoWriter: git failed; the build is logged as unknown.");
            build = Unknown;
        }
        else if (changes.Count == 0)
        {
            build = hash;
        }
        else
        {
            string list = string.Join("\n", changes);
            if (!development)
            {
                throw new BuildFailedException(
                    "ResearchBuildInfoWriter: build stopped, " + changes.Count + " uncommitted change(s). " +
                    "A study build must come from a clean commit, so the build hash in the research log names " +
                    "the exact app. Commit (or discard) these files and build again, or tick Development Build " +
                    "for a test build:\n" + list);
            }

            Debug.LogWarning("ResearchBuildInfoWriter: Development Build from a dirty tree (" + changes.Count +
                             " uncommitted change(s)); the research log will show build " + hash + DirtySuffix +
                             ". Never use this build for study or pilot sessions:\n" + list);
            build = hash + DirtySuffix;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
        File.WriteAllText(AssetPath, build, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
        return build;
    }

    // One line per changed path ("git status --porcelain": " M file", "?? file"); null when git fails.
    private static List<string> ReadChanges()
    {
        string output = RunGit("-c core.quotepath=false status --porcelain --untracked-files=all");
        if (output == null)
        {
            return null;
        }

        var lines = new List<string>();
        foreach (string line in output.Split('\n'))
        {
            string trimmed = line.TrimEnd('\r');
            if (trimmed.Length > 0)
            {
                lines.Add(trimmed);
            }
        }

        return lines;
    }

    // Standard output of a successful git call, untrimmed (porcelain lines start with a space); null when
    // git is missing or fails.
    private static string RunGit(string arguments)
    {
        try
        {
            var info = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                StandardOutputEncoding = new UTF8Encoding(false),
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode == 0 ? output : null;
            }
        }
        catch (Win32Exception e)
        {
            Debug.LogWarning("ResearchBuildInfoWriter: git not found (" + e.Message + ").");
            return null;
        }
        catch (InvalidOperationException e)
        {
            Debug.LogWarning("ResearchBuildInfoWriter: git failed (" + e.Message + ").");
            return null;
        }
    }
}
