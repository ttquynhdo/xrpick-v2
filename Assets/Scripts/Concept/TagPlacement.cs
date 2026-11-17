using UnityEngine;

/// <summary>
/// #90/#100: puts a small world-space tag beside a package as seen from the head, so it never covers the package,
/// and turns it to face the eyes. Used by ConceptBoxHints (HeldLabelTags was removed in #104).
/// #131 (checklist T8): tags stand upright and turn only about the vertical axis (Upright), once when they are placed
/// (TagFacing), so they never swim with small head movements.
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
        tag.SetPositionAndRotation(position, Upright(position, eyes, tag.rotation));
    }

    /// <summary>An upright rotation whose face points away from the eyes (a canvas is read from behind its forward),
    /// turned only about the vertical axis; the fallback when the eyes are straight above or below.</summary>
    public static Quaternion Upright(Vector3 position, Vector3 eyes, Quaternion fallback)
    {
        Vector3 away = position - eyes;
        away.y = 0f;
        return away.sqrMagnitude < 1e-6f ? fallback : Quaternion.LookRotation(away, Vector3.up);
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

/// <summary>
/// #131: remembers where a tag was turned to the eyes and turns it again only when the tag is placed anew
/// (Reset) or has moved with its package by more than MoveThreshold, never because the head moved.
/// </summary>
public sealed class TagFacing
{
    public const float MoveThreshold = 0.01f;

    private Vector3 facedAt;
    private Quaternion rotation = Quaternion.identity;
    private bool valid;

    public void Reset() => valid = false;

    public Quaternion For(Vector3 position, Vector3 eyes)
    {
        if (!valid || (position - facedAt).sqrMagnitude > MoveThreshold * MoveThreshold)
        {
            rotation = TagPlacement.Upright(position, eyes, rotation);
            facedAt = position;
            valid = true;
        }

        return rotation;
    }
}
