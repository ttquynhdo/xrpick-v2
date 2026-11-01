using System;
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
/// research log header can tell builds apart (#26). "git describe --always --dirty" adds
/// "-dirty" when the build has uncommitted changes. Play mode logs "editor" instead.
/// The file is git-ignored.
/// </summary>
public sealed class ResearchBuildInfoWriter : IPreprocessBuildWithReport
{
    private const string AssetPath = "Assets/Resources/ResearchBuildInfo.txt";
    private const string Unknown = "unknown";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        string hash = ReadGitHash();
        Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
        File.WriteAllText(AssetPath, hash, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
    }

    private static string ReadGitHash()
    {
        try
        {
            var info = new ProcessStartInfo("git", "describe --always --dirty")
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                if (process.ExitCode == 0 && output.Length > 0)
                {
                    return output;
                }
            }
        }
        catch (Win32Exception e)
        {
            Debug.LogWarning("ResearchBuildInfoWriter: git not found (" + e.Message + ").");
            return Unknown;
        }
        catch (InvalidOperationException e)
        {
            Debug.LogWarning("ResearchBuildInfoWriter: git failed (" + e.Message + ").");
            return Unknown;
        }

        Debug.LogWarning("ResearchBuildInfoWriter: git describe returned no hash; build is logged as unknown.");
        return Unknown;
    }
}
