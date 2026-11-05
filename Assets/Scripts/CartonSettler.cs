using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #108, docs/decisions/037 (2): a carton let go inside the order container without snapping into a place lies flat,
/// lid up, on the floor or on whatever lies below it, instead of hanging tilted where the hand opened (the packages are
/// kinematic and have no gravity). It never moves into a place by itself (the EKZ check must still be able to fail),
/// never while it is held, while the station stock moves or once the order container has been shipped, and never out
/// of the container's trigger, so the counter sees no exit/enter. Bags (no SnapInteractor) are left alone.
/// </summary>
public class CartonSettler : MonoBehaviour
{
    // Time the SnapInteractor gets to take a released carton into a place before we look.
    private const float SnapWait = 0.3f;
    // Clearance kept between the carton and the container walls, and the skin used for the downward cast.
    private const float WallMargin = 0.004f;
    private const float CastSkin = 0.002f;

    [Tooltip("The order container's trigger (the counter's zone).")]
    [SerializeField] private BoxCollider containerZone;

    [Tooltip("Snap places of the container; their snap pose height is the floor a carton stands on.")]
    [SerializeField] private SnapInteractable[] places;

    [Tooltip("Optional. Storage containers on the lanes (IStationStock); nothing moves while they ride.")]
    [SerializeField] private MonoBehaviour stationStock;

    [Tooltip("Optional. Order container conveyor; nothing moves once it has shipped.")]
    [SerializeField] private OrderBoxConveyor conveyor;

    private readonly Dictionary<Grabbable, Coroutine> pending = new();
    private readonly List<(Grabbable grabbable, System.Action<PointerEvent> handler)> subscriptions = new();
    private float floorY;

    // For MCP tests: how many cartons were laid flat.
    public int SettledCount { get; private set; }

    private void Start()
    {
        if (RequiredReference.IsMissing(containerZone, nameof(containerZone), this))
            return;

        // A loose carton stands as high as a snapped one: the snap pose (the SDK holds snapped cartons there, about
        // 2 cm above the visible floor where EkzPlacementCheck draws the outlines).
        floorY = containerZone.bounds.min.y;
        if (places != null && places.Length > 0 && places[0] != null)
        {
            floorY = places[0].transform.position.y;
        }

        foreach (Grabbable grabbable in FindObjectsByType<Grabbable>(FindObjectsInactive.Include))
        {
            if (grabbable.GetComponentInParent<PackageData>(true) == null
                || grabbable.GetComponentInChildren<SnapInteractor>(true) == null)
                continue;

            Grabbable captured = grabbable;
            System.Action<PointerEvent> handler = evt => HandlePointerEvent(captured, evt);
            grabbable.WhenPointerEventRaised += handler;
            subscriptions.Add((grabbable, handler));
        }
    }

    private void OnDestroy()
    {
        foreach ((Grabbable grabbable, System.Action<PointerEvent> handler) in subscriptions)
        {
            if (grabbable != null)
            {
                grabbable.WhenPointerEventRaised -= handler;
            }
        }
    }

    private void HandlePointerEvent(Grabbable grabbable, PointerEvent evt)
    {
        // A SnapInteractor selecting the carton means it sits in a place, not a hand letting go.
        if (evt.Data is SnapInteractor)
            return;

        if (evt.Type == PointerEventType.Select && pending.TryGetValue(grabbable, out Coroutine running))
        {
            StopCoroutine(running);
            pending.Remove(grabbable);
            return;
        }

        if ((evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            && HeldByHand(grabbable) == 0 && isActiveAndEnabled)
        {
            if (pending.TryGetValue(grabbable, out Coroutine old))
            {
                StopCoroutine(old);
            }
            pending[grabbable] = StartCoroutine(SettleAfterRelease(grabbable));
        }
    }

    private IEnumerator SettleAfterRelease(Grabbable grabbable)
    {
        yield return new WaitForSeconds(SnapWait);
        pending.Remove(grabbable);
        TrySettle(grabbable);
    }

    /// <summary>Lays the carton flat if it lies loose in the container; returns whether it moved. Public for tests.</summary>
    public bool TrySettle(Grabbable grabbable)
    {
        if (grabbable == null || !grabbable.isActiveAndEnabled || HeldByHand(grabbable) > 0)
            return false;
        if (stationStock is IStationStock stock && stock.IsMoving)
            return false;
        if (conveyor != null && conveyor.IsShipped)
            return false;

        SnapInteractor snap = grabbable.GetComponentInChildren<SnapInteractor>();
        if (snap != null && snap.HasSelectedInteractable)
            return false;

        Transform carton = grabbable.transform;
        BoxCollider body = carton.GetComponent<BoxCollider>();
        if (body == null)
            return false;

        Physics.SyncTransforms();
        Bounds zone = containerZone.bounds;
        if (!zone.Contains(body.bounds.center))
            return false;

        // Lid up, keeping the direction the carton points to.
        Vector3 forward = Vector3.ProjectOnPlane(carton.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(carton.up, Vector3.up);
        }
        Quaternion upright = Quaternion.LookRotation(forward.normalized, Vector3.up);

        Vector3 halfSize = Vector3.Scale(body.size, carton.lossyScale) / 2f;
        Vector3 centreOffset = Vector3.Scale(body.center, carton.lossyScale);

        // Footprint of the upright carton, kept inside the container walls.
        Vector3 worldCentre = body.bounds.center;
        Vector3 extent = AxisExtent(upright, halfSize);
        worldCentre.x = Mathf.Clamp(worldCentre.x, zone.min.x + extent.x + WallMargin, zone.max.x - extent.x - WallMargin);
        worldCentre.z = Mathf.Clamp(worldCentre.z, zone.min.z + extent.z + WallMargin, zone.max.z - extent.z - WallMargin);

        // Cast down from above the container onto the floor or the packages below.
        float startY = zone.max.y + halfSize.y + 0.05f;
        var start = new Vector3(worldCentre.x, startY, worldCentre.z);
        float floorCentreY = floorY + extent.y;
        float landingY = floorCentreY;
        Vector3 castHalf = halfSize - Vector3.one * CastSkin;
        // The carton body is a trigger collider (only the thin front sticker is solid), so triggers count, but only
        // those of packages: the container zone and the snap places are triggers too.
        foreach (RaycastHit hit in Physics.BoxCastAll(start, castHalf, Vector3.down, upright, startY - floorCentreY, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.transform.IsChildOf(carton) || hit.collider.GetComponentInParent<PackageData>() == null)
                continue;

            landingY = Mathf.Max(landingY, startY - hit.distance);
        }

        // The collider centre lands there; the pivot follows from the collider's offset in the new rotation.
        var newCentre = new Vector3(worldCentre.x, landingY, worldCentre.z);
        carton.SetPositionAndRotation(newCentre - upright * centreOffset, upright);
        SettledCount++;
        return true;
    }

    // Hands only: the SnapInteractor also selects the carton while it sits in a place.
    private static int HeldByHand(Grabbable grabbable)
    {
        SnapInteractor snap = grabbable.GetComponentInChildren<SnapInteractor>();
        int snapSelects = snap != null && snap.HasSelectedInteractable ? 1 : 0;
        return Mathf.Max(0, grabbable.SelectingPointsCount - snapSelects);
    }

    // Half size of an oriented box along the world axes.
    private static Vector3 AxisExtent(Quaternion rotation, Vector3 halfSize)
    {
        Vector3 right = rotation * Vector3.right * halfSize.x;
        Vector3 up = rotation * Vector3.up * halfSize.y;
        Vector3 forward = rotation * Vector3.forward * halfSize.z;
        return new Vector3(
            Mathf.Abs(right.x) + Mathf.Abs(up.x) + Mathf.Abs(forward.x),
            Mathf.Abs(right.y) + Mathf.Abs(up.y) + Mathf.Abs(forward.y),
            Mathf.Abs(right.z) + Mathf.Abs(up.z) + Mathf.Abs(forward.z));
    }
}
