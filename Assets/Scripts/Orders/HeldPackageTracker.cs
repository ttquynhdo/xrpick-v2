using System.Collections.Generic;

/// <summary>
/// #90, docs/decisions/026 (2): which packages the trainee holds right now. A package is held from its first Select
/// until its last pointer lets go (two hands on one package count once). Pure C#, covered by HeldPackageTrackerTests.
/// </summary>
public sealed class HeldPackageTracker
{
    private readonly List<int> held = new();
    private readonly Dictionary<int, HashSet<int>> pointers = new();
    private readonly Dictionary<int, string> materials = new();

    // Package ids in grab order.
    public IReadOnlyList<int> Held => held;

    // Returns true when the package was not held before.
    public bool Select(int packageId, int pointerId, string materialKey)
    {
        if (!pointers.TryGetValue(packageId, out HashSet<int> ids))
        {
            ids = new HashSet<int>();
            pointers[packageId] = ids;
        }

        ids.Add(pointerId);
        if (held.Contains(packageId))
            return false;

        held.Add(packageId);
        materials[packageId] = materialKey;
        return true;
    }

    // Unselect or Cancel. Returns true when the package is no longer held.
    public bool Release(int packageId, int pointerId)
    {
        if (!pointers.TryGetValue(packageId, out HashSet<int> ids) || !ids.Remove(pointerId) || ids.Count > 0)
            return false;

        pointers.Remove(packageId);
        materials.Remove(packageId);
        return held.Remove(packageId);
    }

    public bool IsMaterialHeld(string materialKey)
    {
        foreach (string key in materials.Values)
        {
            if (key == materialKey)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Side (-1 left, +1 right, as seen from the head) of each package's label tag, from each package's sideways
    /// position in head space. One package: the side it is on (right when centred). Several: the leftmost gets -1,
    /// all others +1, so two tags never sit between two packages held side by side.
    /// </summary>
    public static int[] TagSides(IReadOnlyList<float> sidewaysPositions)
    {
        var sides = new int[sidewaysPositions.Count];
        if (sides.Length == 1)
        {
            sides[0] = sidewaysPositions[0] < 0f ? -1 : 1;
            return sides;
        }

        int leftmost = 0;
        for (int i = 0; i < sides.Length; i++)
        {
            sides[i] = 1;
            if (sidewaysPositions[i] < sidewaysPositions[leftmost])
            {
                leftmost = i;
            }
        }

        if (sides.Length > 0)
        {
            sides[leftmost] = -1;
        }

        return sides;
    }
}
