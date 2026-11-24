using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #108, docs/decisions/037 (2): a carton let go inside the order container without snapping into a place lies flat,
/// lid up, on the floor or on whatever lies below it, instead of hanging tilted where the hand opened (the packages are
/// kinematic and have no gravity). It never moves into a place by itself (the EKZ check must still be able to fail),
/// never while it is held, while the station stock moves or once the order container has been shipped, and never
/// sideways out of the container. #157: a carton let go above the rim (up to AboveRim) is laid into the container too,
/// so nothing hangs over it; a carton let go over a free place snaps into it instead (the places' trigger zones reach
/// above the rim). Bags (no SnapInteractor) are left alone. #153: everything else let go in the air is PackageDropper's
/// (Covers tells it which cartons are this settler's). #168: where it lies follows DropPlacementRule (via
/// PackageDropper.PlanInSpace: on top only with half its base carried, else pushed aside; never above the rim) and it
/// gets there in PackageDropper's short fall instead of a jump.
/// </summary>
public class CartonSettler : MonoBehaviour
{
    // Time the SnapInteractor gets to take a released carton into a place before we look.
    private const float SnapWait = 0.3f;
    // Clearance kept between the carton and the container walls.
    private const float WallMargin = 0.004f;
    // #157: how far above the container's rim a released carton is still laid into the container.
    private const float AboveRim = 0.25f;

    [Tooltip("The order container's trigger (the counter's zone).")]
    [SerializeField] private BoxCollider containerZone;

    [Tooltip("Snap places of the container; their snap pose height is the floor a carton stands on.")]
    [SerializeField] private SnapInteractable[] places;

    [Tooltip("Optional. Storage containers on the lanes (IStationStock); nothing moves while they ride.")]
    [SerializeField] private MonoBehaviour stationStock;

    [Tooltip("Optional. Order container conveyor; nothing moves once it has shipped.")]
    [SerializeField] private OrderBoxConveyor conveyor;

    private readonly Dictionary<Grabbable, Coroutine> pending = new();
    private readonly HashSet<Grabbable> cartons = new();
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
            cartons.Add(grabbable);
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
        return TrySettle(grabbable, null);
    }

    /// <summary>
    /// #168: ignore = packages about to fall themselves (PackageDropper, when the package below them is lifted).
    /// The carton lies where DropPlacementRule puts it and gets there in PackageDropper's short fall.
    /// </summary>
    public bool TrySettle(Grabbable grabbable, ICollection<Transform> ignore)
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
        if (!InReach(body, zone))
            return false;

        // Lid up, keeping the direction the carton points to.
        Vector3 forward = Vector3.ProjectOnPlane(carton.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(carton.up, Vector3.up);
        }
        Quaternion upright = Quaternion.LookRotation(forward.normalized, Vector3.up);

        PackageDropper dropper = PackageDropper.Current;
        if (dropper == null)
        {
            Debug.LogError("CartonSettler: no PackageDropper in this scene, carton left where it was.", this);
            return false;
        }

        // Upright about the collider centre, so the carton stays where the hand let it go; then DropPlacementRule
        // inside the walls, from the snap places' floor up to the rim.
        Vector3 bodyCentre = body.bounds.center;
        Vector3 uprightPivot = bodyCentre + upright * Quaternion.Inverse(carton.rotation) * (carton.position - bodyCentre);
        var space = new DropSpace(zone.min.x + WallMargin, zone.max.x - WallMargin, zone.min.z + WallMargin,
            zone.max.z - WallMargin, floorY, zone.max.y);
        if (!dropper.PlanInSpace(carton, uprightPivot, upright, space, ignore, out Vector3 placed))
            return false;

        dropper.MoveDown(grabbable, new Pose(placed, upright));
        SettledCount++;
        return true;
    }

    /// <summary>
    /// #153: true when this settler lays the released package down (a carton of this scene with a SnapInteractor,
    /// over the order container up to AboveRim, while it may settle), so PackageDropper leaves it alone.
    /// </summary>
    public bool Covers(Grabbable grabbable)
    {
        if (grabbable == null || containerZone == null || !cartons.Contains(grabbable))
            return false;
        // TrySettle does nothing then, so the carton must not be left hanging: PackageDropper takes it.
        if (stationStock is IStationStock stock && stock.IsMoving)
            return false;
        if (conveyor != null && conveyor.IsShipped)
            return false;

        BoxCollider body = grabbable.GetComponent<BoxCollider>();
        if (body == null)
            return false;

        Physics.SyncTransforms();
        return InReach(body, containerZone.bounds);
    }

    // #157: a hand often lets go above the rim; a carton over the container still belongs in it.
    private static bool InReach(BoxCollider body, Bounds zone)
    {
        Vector3 bodyCentre = body.bounds.center;
        return bodyCentre.x >= zone.min.x && bodyCentre.x <= zone.max.x
            && bodyCentre.z >= zone.min.z && bodyCentre.z <= zone.max.z
            && bodyCentre.y >= zone.min.y && bodyCentre.y <= zone.max.y + AboveRim;
    }

    // Hands only: the SnapInteractor also selects the carton while it sits in a place.
    internal static int HeldByHand(Grabbable grabbable)
    {
        SnapInteractor snap = grabbable.GetComponentInChildren<SnapInteractor>();
        int snapSelects = snap != null && snap.HasSelectedInteractable ? 1 : 0;
        return Mathf.Max(0, grabbable.SelectingPointsCount - snapSelects);
    }
}
