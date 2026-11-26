using UnityEngine;

/// <summary>
/// #183 (decision 043 item 6): a fixed solid block in a container that packages let go over it lie on, like the raised
/// tray („Modul“) in ComplexScene's storage containers. It is not a package: no collider, not grabbable, not counted,
/// never moved or logged. PackageDropper adds its box (local to this transform) to the bodies below when it plans where a
/// released package lies (DropPlacementRule), so a ring let go over the tray lies on its surface, not inside it.
/// </summary>
public class FixedDropBlock : MonoBehaviour
{
    [Tooltip("Centre of the solid part, in this object's local space (the tray: its base block and floor).")]
    [SerializeField] private Vector3 centre;

    [Tooltip("Size of the solid part, in this object's local space; its top is where packages come to rest.")]
    [SerializeField] private Vector3 size = new(0.17f, 0.1f, 0.15f);

    public Vector3 Centre => centre;

    public Vector3 Size => size;

    /// <summary>Editor tools set the box when they build the block.</summary>
    public void SetBox(Vector3 newCentre, Vector3 newSize)
    {
        centre = newCentre;
        size = newSize;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.6f);
        Gizmos.DrawWireCube(centre, size);
    }
}
