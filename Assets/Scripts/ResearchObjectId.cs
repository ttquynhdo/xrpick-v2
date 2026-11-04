using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// #29: object_id of a package in the research log: its hierarchy path in the scene, e.g.
/// „Interactables/Ring (3)“. Stable between runs as long as the scene's GameObject names stay the same
/// (research-logging-spec §7). Taken once per package, so a later reparenting does not change it.
/// </summary>
public static class ResearchObjectId
{
    private static readonly Dictionary<PackageData, string> ids = new();

    public static string Of(PackageData package)
    {
        if (package == null)
            return null;

        if (!ids.TryGetValue(package, out string id))
        {
            id = PathOf(package.transform);
            ids[package] = id;
        }

        return id;
    }

    /// <summary>Drops the ids of an unloaded scene's packages.</summary>
    public static void Clear()
    {
        ids.Clear();
    }

    private static string PathOf(Transform t)
    {
        var sb = new StringBuilder(t.name);
        for (Transform parent = t.parent; parent != null; parent = parent.parent)
        {
            sb.Insert(0, '/').Insert(0, parent.name);
        }

        return sb.ToString();
    }
}
