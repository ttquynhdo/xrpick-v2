using UnityEngine;

/// <summary>
/// #90/#100: puts a small world-space tag beside a package as seen from the head, so it never covers the package,
/// and turns it to face the eyes. Shared by HeldLabelTags and ConceptBoxHints.
/// </summary>
public static class TagPlacement
{
    /// <param name="side">-1 left, +1 right of the package, as seen from the head.</param>
    /// <param name="gap">Metres between the package's outline and the tag.</param>
    public static void Beside(RectTransform tag, Bounds bounds, int side, Vector3 eyes, float gap)
    {
        Vector3 toPackage = bounds.center - eyes;
        Vector3 right = Vector3.Cross(Vector3.up, toPackage).normalized;
        if (right == Vector3.zero)
        {
            right = Vector3.right;
        }

        // Half the package's width as seen from the head, plus the gap and half the tag.
        Vector3 e = bounds.extents;
        float packageHalf = Mathf.Abs(e.x * right.x) + Mathf.Abs(e.y * right.y) + Mathf.Abs(e.z * right.z);
        float tagHalf = tag.rect.width * tag.lossyScale.x * 0.5f;

        Vector3 position = bounds.center + right * (side * (packageHalf + gap + tagHalf));
        tag.SetPositionAndRotation(position, Quaternion.LookRotation(position - eyes, Vector3.up));
    }

    /// <summary>Bounds of the enabled renderers of a package, or a point at its position when none is shown.</summary>
    public static Bounds Of(Renderer[] renderers, Vector3 fallback)
    {
        bool found = false;
        var bounds = new Bounds(fallback, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            if (found)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            else
            {
                bounds = renderer.bounds;
                found = true;
            }
        }

        return bounds;
    }
}
