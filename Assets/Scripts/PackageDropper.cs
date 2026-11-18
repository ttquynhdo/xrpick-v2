using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// #153 (root item #157): packages are kinematic and have no gravity, so a package let go in the air used to hang
/// there. Shortly after the last hand lets go, this drops it straight down, still kinematic, in a short fall: into the
/// container below it (onto its floor or the packages in it), onto the desk or whatever else stands below, or, when
/// only the floor is below (let go beyond the desk), back to where it was grabbed (up, across above the containers,
/// down). Known limit: a carton taken out of a snap place and let go beyond the desk goes back onto that place without
/// snapping, so it counts as loose until it is placed again. Created at runtime once per loaded
/// scene like PackageGrabLogger (no component in any scene). One owner per release: a snapped carton is left to its
/// place, a carton over a Basic/Mixed order container to CartonSettler. Never touches a held package, one whose
/// colliders are off (riding on a lane) or one another script moves meanwhile. Writes no log: the counters and
/// ConceptBoxHints see the package arrive like any other move.
/// </summary>
public class PackageDropper : MonoBehaviour
{
    // Time a SnapInteractor gets to take a released carton into a place first (same as CartonSettler).
    private const float SnapWait = 0.3f;
    // Containers are found by this name prefix (PlasticBox_Storage, _Storage_2, _Storage_Next, PlasticBox_Order).
    private const string ContainerPrefix = "PlasticBox_";
    // Inner floor of a container above its collider's bottom, and its wall thickness plus clearance (the collider
    // spans the outer walls; cartons in the scenes sit about 2 cm inside them).
    private const float ContainerFloor = 0.011f;
    private const float WallMargin = 0.015f;
    // A surface lower than this is the room floor: the package goes back to where it was grabbed.
    private const float MinSurfaceY = 0.3f;
    private const float CastSkin = 0.002f;
    private const float CastDepth = 3f;
    private const float Gravity = 9.81f;
    private const float MaxFallSeconds = 0.4f;
    private const float ReturnSeconds = 0.5f;
    // A return flies up to at least this height first, so it passes above the containers' and places' triggers
    // (the snap places reach y 1.12) instead of through them, which would log extra container events.
    private const float ReturnClearY = 1.2f;
    // Pose difference that means another script moved the package during the fall.
    private const float MovedTolerance = 0.0005f;

    private static PackageDropper current;

    private readonly List<(Grabbable grabbable, System.Action<PointerEvent> handler)> subscriptions = new();
    private readonly Dictionary<Grabbable, Coroutine> running = new();
    private readonly Dictionary<Grabbable, Pose> grabbedFrom = new();
    private CartonSettler[] settlers;
    private int ignoredLayers;

    // For MCP tests: how many releases ended on a surface, in a container or back where the package was grabbed.
    public int DroppedCount { get; private set; }
    public int ReturnedCount { get; private set; }
    public static PackageDropper Current => current;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        CreateFor(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            CreateFor(scene);
    }

    private static void CreateFor(Scene scene)
    {
        if (current != null && current.gameObject.scene == scene)
            return;

        var host = new GameObject(nameof(PackageDropper));
        SceneManager.MoveGameObjectToScene(host, scene);
        current = host.AddComponent<PackageDropper>();
    }

    private void Awake()
    {
        // Poke surfaces (Menu, red button, end control) and the player capsule lie on these layers.
        ignoredLayers = LayerMask.GetMask("Ignore Raycast", "Water", "UI");

        foreach (Grabbable grabbable in FindObjectsByType<Grabbable>(FindObjectsInactive.Include))
        {
            if (grabbable.GetComponentInParent<PackageData>(true) == null)
                continue;

            Grabbable captured = grabbable;
            System.Action<PointerEvent> handler = evt => HandlePointerEvent(captured, evt);
            grabbable.WhenPointerEventRaised += handler;
            subscriptions.Add((grabbable, handler));
        }
    }

    private void Start()
    {
        settlers = FindObjectsByType<CartonSettler>(FindObjectsInactive.Include);
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

        if (current == this)
        {
            current = null;
        }
    }

    private void HandlePointerEvent(Grabbable grabbable, PointerEvent evt)
    {
        // A SnapInteractor selecting the carton means it sits in a place, not a hand.
        if (evt.Data is SnapInteractor)
            return;

        if (evt.Type == PointerEventType.Select)
        {
            // A grab during a fall keeps the pose of the first grab, so a later return does not end in the air.
            Stop(grabbable);
            if (!grabbedFrom.ContainsKey(grabbable))
            {
                grabbedFrom[grabbable] = new Pose(grabbable.transform.position, grabbable.transform.rotation);
            }
            return;
        }

        if ((evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            && CartonSettler.HeldByHand(grabbable) == 0 && isActiveAndEnabled)
        {
            Stop(grabbable);
            running[grabbable] = StartCoroutine(DropAfterRelease(grabbable));
        }
    }

    private void Stop(Grabbable grabbable)
    {
        if (running.TryGetValue(grabbable, out Coroutine routine))
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
            running.Remove(grabbable);
        }
    }

    private IEnumerator DropAfterRelease(Grabbable grabbable)
    {
        yield return new WaitForSeconds(SnapWait);

        bool haveStart = grabbedFrom.TryGetValue(grabbable, out Pose start);

        if (TryPlan(grabbable, start, haveStart, out Pose target, out bool returning))
        {
            yield return Animate(grabbable.transform, target, returning);
        }

        grabbedFrom.Remove(grabbable);
        running.Remove(grabbable);
    }

    /// <summary>Decides where a released package goes; false when it stays where it is. Public for MCP tests.</summary>
    public bool TryPlan(Grabbable grabbable, Pose grabbedPose, bool haveGrabbedPose, out Pose target, out bool returning)
    {
        target = default;
        returning = false;

        if (grabbable == null || !grabbable.isActiveAndEnabled || CartonSettler.HeldByHand(grabbable) > 0)
            return false;

        SnapInteractor snap = grabbable.GetComponentInChildren<SnapInteractor>();
        if (snap != null && snap.HasSelectedInteractable)
            return false;

        if (settlers != null)
        {
            foreach (CartonSettler settler in settlers)
            {
                if (settler != null && settler.isActiveAndEnabled && settler.Covers(grabbable))
                    return false;
            }
        }

        Transform package = grabbable.transform;
        List<Collider> own = OwnColliders(package);
        if (own.Count == 0)
            return false;

        // Lid up as when it was grabbed (cartons lid up, rings and bags as they lay), turned as the hand left it.
        Quaternion rotation = package.rotation;
        PackageData data = package.GetComponentInParent<PackageData>();
        if (data != null && data.packageType == PackageType.LooseItemDisplay)
        {
            // Loose pieces stand on edge in their opened carton; let go anywhere else they lie flat.
            rotation = LieFlat(package);
        }
        else if (haveGrabbedPose)
        {
            rotation = YawOf(package.rotation) * Quaternion.Inverse(YawOf(grabbedPose.rotation)) * grabbedPose.rotation;
        }

        Quaternion oldRotation = package.rotation;
        package.rotation = rotation;
        Physics.SyncTransforms();
        Bounds body = BoundsOf(own);
        package.rotation = oldRotation;
        Physics.SyncTransforms();
        Vector3 pivotOffset = package.position - body.center;

        Collider container = ContainerBelow(body);
        Vector3 centre = body.center;
        Vector3 half = body.extents;
        float landingY;

        if (container != null)
        {
            Bounds box = container.bounds;
            centre.x = Mathf.Clamp(centre.x, box.min.x + half.x + WallMargin, box.max.x - half.x - WallMargin);
            centre.z = Mathf.Clamp(centre.z, box.min.z + half.z + WallMargin, box.max.z - half.z - WallMargin);
            // From above the rim onto the floor or the packages lying in it.
            float startY = Mathf.Max(centre.y, box.max.y + half.y + 0.02f);
            landingY = box.min.y + ContainerFloor + half.y;
            foreach (RaycastHit hit in Cast(new Vector3(centre.x, startY, centre.z), half, startY - landingY))
            {
                if (IsOwn(hit.collider, package) || !IsOtherPackage(hit.collider))
                    continue;

                // The cast box is CastSkin smaller than the body on every side.
                landingY = Mathf.Max(landingY, startY - hit.distance + CastSkin);
            }
        }
        else
        {
            // Start one body height up, so a package pushed a little into the desk still lands on top of it.
            float startY = centre.y + half.y;
            float surfaceY = float.NegativeInfinity;
            foreach (RaycastHit hit in Cast(new Vector3(centre.x, startY, centre.z), half, CastDepth))
            {
                if (IsOwn(hit.collider, package) || !IsSurface(hit.collider))
                    continue;

                surfaceY = Mathf.Max(surfaceY, startY - hit.distance - half.y + CastSkin);
            }

            if (float.IsNegativeInfinity(surfaceY) || surfaceY < MinSurfaceY)
            {
                if (!haveGrabbedPose)
                    return false;

                target = grabbedPose;
                returning = true;
                return true;
            }

            landingY = surfaceY + half.y;
        }

        var landing = new Vector3(centre.x, landingY, centre.z);
        target = new Pose(landing + pivotOffset, rotation);
        return true;
    }

    private IEnumerator Animate(Transform package, Pose target, bool returning)
    {
        Vector3 from = package.position;
        Quaternion fromRotation = package.rotation;
        float height = Mathf.Max(0f, from.y - target.position.y);
        float duration = returning ? ReturnSeconds : Mathf.Min(MaxFallSeconds, Mathf.Sqrt(2f * height / Gravity));
        float clearY = Mathf.Max(ReturnClearY, from.y, target.position.y);
        List<Collider> colliders = OwnColliders(package);
        Vector3 written = from;
        Quaternion writtenRotation = fromRotation;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            // Another script (a ride, the Concept demo, a snap) moved it: that one owns the package now.
            if (MovedSince(package, written, writtenRotation) || !AnyEnabled(colliders))
                yield break;

            float k = t / duration;
            Vector3 position;
            float turn;
            if (returning)
            {
                // Up, across, down (a third of the time each).
                position = ReturnPath(from, target.position, clearY, k);
                turn = Mathf.SmoothStep(0f, 1f, k);
            }
            else
            {
                // Falling: horizontal and turn ease out, height accelerates like free fall.
                turn = 1f - (1f - k) * (1f - k);
                position = Vector3.Lerp(from, target.position, turn);
                position.y = Mathf.Lerp(from.y, target.position.y, k * k);
            }
            package.SetPositionAndRotation(position, Quaternion.Slerp(fromRotation, target.rotation, turn));
            written = package.position;
            writtenRotation = package.rotation;
            yield return null;
        }

        if (MovedSince(package, written, writtenRotation) || !AnyEnabled(colliders))
            yield break;

        package.SetPositionAndRotation(target.position, target.rotation);
        if (returning)
        {
            ReturnedCount++;
        }
        else
        {
            DroppedCount++;
        }
    }

    private static Vector3 ReturnPath(Vector3 from, Vector3 to, float clearY, float k)
    {
        const float third = 1f / 3f;
        if (k < third)
            return new Vector3(from.x, Mathf.Lerp(from.y, clearY, Mathf.SmoothStep(0f, 1f, k / third)), from.z);
        if (k < 2f * third)
        {
            Vector3 across = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, (k - third) / third));
            return new Vector3(across.x, clearY, across.z);
        }
        return new Vector3(to.x, Mathf.Lerp(clearY, to.y, Mathf.SmoothStep(0f, 1f, (k - 2f * third) / third)), to.z);
    }

    private static bool MovedSince(Transform package, Vector3 position, Quaternion rotation)
    {
        return (package.position - position).sqrMagnitude > MovedTolerance * MovedTolerance
            || Quaternion.Angle(package.rotation, rotation) > 0.5f;
    }

    // Riders of a lane ride have their colliders switched off.
    private static bool AnyEnabled(List<Collider> colliders)
    {
        foreach (Collider collider in colliders)
        {
            if (collider != null && collider.enabled)
                return true;
        }
        return false;
    }

    private RaycastHit[] Cast(Vector3 start, Vector3 half, float distance)
    {
        Vector3 castHalf = Vector3.Max(half - Vector3.one * CastSkin, Vector3.one * CastSkin);
        return Physics.BoxCastAll(start, castHalf, Vector3.down, Quaternion.identity, Mathf.Max(0f, distance),
            ~ignoredLayers, QueryTriggerInteraction.Collide);
    }

    // The container below any part of the package's footprint (its own collider, solid or the counter's trigger); the
    // one under most of it when it spans two. A package let go half over a container's wall goes in, never onto the rim:
    // the container's collider spans the whole box, so a cast would stand the package on top of it.
    private Collider ContainerBelow(Bounds body)
    {
        Collider best = null;
        float bestArea = 0f;
        foreach (Collider collider in Physics.OverlapBox(new Vector3(body.center.x, MinSurfaceY + CastDepth / 2f, body.center.z),
                     new Vector3(body.extents.x, CastDepth / 2f, body.extents.z), Quaternion.identity, ~ignoredLayers,
                     QueryTriggerInteraction.Collide))
        {
            if (!IsContainer(collider) || body.center.y <= collider.bounds.min.y)
                continue;

            Bounds box = collider.bounds;
            float overlapX = Mathf.Min(body.max.x, box.max.x) - Mathf.Max(body.min.x, box.min.x);
            float overlapZ = Mathf.Min(body.max.z, box.max.z) - Mathf.Max(body.min.z, box.min.z);
            float area = Mathf.Max(0f, overlapX) * Mathf.Max(0f, overlapZ);
            if (area > bestArea)
            {
                best = collider;
                bestArea = area;
            }
        }
        return best;
    }

    private static bool IsContainer(Collider collider)
    {
        return collider.name.StartsWith(ContainerPrefix) && collider.GetComponentInParent<PackageData>() == null;
    }

    // Packages of other kinds have trigger bodies (only the front sticker is solid), so their triggers count.
    private static bool IsOtherPackage(Collider collider)
    {
        PackageData package = collider.GetComponentInParent<PackageData>();
        if (package == null)
            return false;

        Grabbable grabbable = package.GetComponentInChildren<Grabbable>();
        return grabbable == null || CartonSettler.HeldByHand(grabbable) == 0;
    }

    private static bool IsSurface(Collider collider)
    {
        if (IsOtherPackage(collider))
            return true;
        if (collider.GetComponentInParent<PackageData>() != null)
            return false;
        return !collider.isTrigger || IsContainer(collider);
    }

    private static bool IsOwn(Collider collider, Transform package)
    {
        return collider.transform.IsChildOf(package);
    }

    private static List<Collider> OwnColliders(Transform package)
    {
        var colliders = new List<Collider>();
        foreach (Collider collider in package.GetComponentsInChildren<Collider>())
        {
            if (collider.enabled && collider.bounds.size != Vector3.zero)
            {
                colliders.Add(collider);
            }
        }
        return colliders;
    }

    private static Bounds BoundsOf(List<Collider> colliders)
    {
        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Count; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }
        return bounds;
    }

    // Turns the package so its thinnest box-collider axis points up, keeping the rest of the hand's turn.
    private static Quaternion LieFlat(Transform package)
    {
        BoxCollider box = package.GetComponentInChildren<BoxCollider>();
        if (box == null)
            return package.rotation;

        Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);
        Vector3 axis = size.x <= size.y && size.x <= size.z ? Vector3.right : size.y <= size.z ? Vector3.up : Vector3.forward;
        Vector3 world = box.transform.rotation * axis;
        if (Vector3.Dot(world, Vector3.up) < 0f)
        {
            world = -world;
        }
        return Quaternion.FromToRotation(world, Vector3.up) * package.rotation;
    }

    private static Quaternion YawOf(Quaternion rotation)
    {
        Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(rotation * Vector3.up, Vector3.up);
        }
        return forward.sqrMagnitude < 0.0001f ? Quaternion.identity : Quaternion.LookRotation(forward.normalized, Vector3.up);
    }
}
