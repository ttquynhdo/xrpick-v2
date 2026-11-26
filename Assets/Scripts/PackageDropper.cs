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
/// #168: in a container it follows DropPlacementRule (on a thin package or one carrying half its base, else pushed
/// aside by the overlap; top never above the rim; footprint from the package's real turn, not its world box). A
/// package lifted more than 1 cm lets the packages lying on it fall the same way (layer by layer). Rising or sideways
/// moves take MinMoveSeconds instead of jumping.
/// #183: FixedDropBlocks (ComplexScene's raised trays) count as bodies below, so a package let go over one lies on it.
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
    // #168: a move that rises or goes sideways takes at least this long (no jump).
    private const float MinMoveSeconds = 0.3f;
    private const float SidewaysMove = 0.01f;
    // #168: a held package lifted this far lets the packages lying on it fall; checked this often while held.
    private const float LiftDistance = 0.01f;
    private const float LiftCheckSeconds = 0.1f;
    // A package lies on another when its bottom is this close to the other's top and over part of it.
    private const float RestingGap = 0.015f;
    private const float RestingShare = 0.05f;

    private static PackageDropper current;

    private readonly List<(Grabbable grabbable, System.Action<PointerEvent> handler)> subscriptions = new();
    private readonly Dictionary<Grabbable, Coroutine> running = new();
    private readonly Dictionary<Grabbable, Pose> grabbedFrom = new();
    private readonly Dictionary<Grabbable, Coroutine> lifts = new();
    // Where packages on their way down will lie: others are planned around that, not around where they hang now.
    private readonly Dictionary<Transform, Pose> landings = new();
    // Root of every package (its Grabbable, or the PackageData object when it has none).
    private readonly List<Transform> packages = new();
    private CartonSettler[] settlers;
    private FixedDropBlock[] blocks = System.Array.Empty<FixedDropBlock>();
    private int ignoredLayers;

    // For MCP tests: how many releases ended on a surface, in a container or back where the package was grabbed.
    public int DroppedCount { get; private set; }
    public int ReturnedCount { get; private set; }
    // For MCP tests: packages that fell because the package below them was lifted, and the last container outcome.
    public int FellAfterLiftCount { get; private set; }
    public DropOutcome LastOutcome { get; private set; }
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

        foreach (PackageData data in FindObjectsByType<PackageData>(FindObjectsInactive.Include))
        {
            Grabbable owner = data.GetComponentInParent<Grabbable>(true);
            Transform root = owner != null ? owner.transform : data.transform;
            if (!packages.Contains(root))
            {
                packages.Add(root);
            }
        }

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
        blocks = FindObjectsByType<FixedDropBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
            if (!lifts.ContainsKey(grabbable) && isActiveAndEnabled)
            {
                lifts[grabbable] = StartCoroutine(WatchLift(grabbable));
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
        landings.Remove(grabbable.transform);
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
            landings[grabbable.transform] = target;
            yield return Animate(grabbable.transform, target, returning);
            landings.Remove(grabbable.transform);
        }

        grabbedFrom.Remove(grabbable);
        running.Remove(grabbable);
    }

    /// <summary>Decides where a released package goes; false when it stays where it is. Public for MCP tests.</summary>
    public bool TryPlan(Grabbable grabbable, Pose grabbedPose, bool haveGrabbedPose, out Pose target, out bool returning)
    {
        return TryPlan(grabbable, grabbedPose, haveGrabbedPose, null, out target, out returning);
    }

    // ignore: packages about to fall themselves (lying on a lifted one), so nothing lands on them.
    private bool TryPlan(Grabbable grabbable, Pose grabbedPose, bool haveGrabbedPose, ICollection<Transform> ignore,
        out Pose target, out bool returning)
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
            // #168: DropPlacementRule instead of the highest package below any part of it.
            Bounds box = container.bounds;
            var space = new DropSpace(box.min.x + WallMargin, box.max.x - WallMargin, box.min.z + WallMargin,
                box.max.z - WallMargin, box.min.y + ContainerFloor, box.max.y);
            if (!PlanInSpace(package, package.position, rotation, space, ignore, out Vector3 placed))
                return false;

            target = new Pose(placed, rotation);
            return true;
        }
        else
        {
            // Start one body height up, so a package pushed a little into the desk still lands on top of it.
            float startY = centre.y + half.y;
            float surfaceY = float.NegativeInfinity;
            foreach (RaycastHit hit in Cast(new Vector3(centre.x, startY, centre.z), half, CastDepth))
            {
                if (IsOwn(hit.collider, package) || !IsSurface(hit.collider) || IsIgnored(hit.collider, ignore))
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

    /// <summary>
    /// #168: where the package lies in the container when its root stands at position/rotation and is let down there
    /// (DropPlacementRule). Also used by CartonSettler with the order container's own floor and walls.
    /// </summary>
    public bool PlanInSpace(Transform package, Vector3 position, Quaternion rotation, DropSpace space,
        ICollection<Transform> ignore, out Vector3 placed)
    {
        placed = position;
        if (!TryMeasure(package, position, rotation, out DropBody body))
            return false;

        var others = new List<DropBody>();
        foreach (Transform other in packages)
        {
            if (other == package || other == null || !other.gameObject.activeInHierarchy
                || (ignore != null && ignore.Contains(other)) || IsHeld(other))
                continue;

            // A package on its way down counts where it will lie, one still waiting to drop not at all.
            if (!landings.TryGetValue(other, out Pose pose))
            {
                pose = new Pose(other.position, other.rotation);
                Grabbable otherGrabbable = other.GetComponent<Grabbable>();
                if (otherGrabbable != null && running.ContainsKey(otherGrabbable))
                    continue;
            }
            if (!TryMeasure(other, pose.position, pose.rotation, out DropBody otherBody))
                continue;

            if (LiesIn(otherBody, space))
            {
                others.Add(otherBody);
            }
        }

        // #183: fixed blocks (raised trays) in this container.
        foreach (FixedDropBlock block in blocks)
        {
            if (block != null && block.isActiveAndEnabled && TryMeasureBlock(block, out DropBody blockBody)
                && LiesIn(blockBody, space))
            {
                others.Add(blockBody);
            }
        }

        DropPlan plan = DropPlacementRule.Plan(body, space, others);
        LastOutcome = plan.Outcome;
        placed = position + new Vector3((float)(plan.X - body.X), (float)(plan.Bottom - body.Bottom), (float)(plan.Z - body.Z));
        return true;
    }

    /// <summary>#168: lays a package down to target in a short fall (CartonSettler hands its cartons over here).</summary>
    public void MoveDown(Grabbable grabbable, Pose target)
    {
        if (grabbable == null || !isActiveAndEnabled)
            return;

        Stop(grabbable);
        // A release still waiting to drop is replaced by this move, so its grab pose must not outlive it.
        grabbedFrom.Remove(grabbable);
        running[grabbable] = StartCoroutine(MoveThenForget(grabbable, target));
    }

    private IEnumerator MoveThenForget(Grabbable grabbable, Pose target)
    {
        landings[grabbable.transform] = target;
        yield return Animate(grabbable.transform, target, false);
        landings.Remove(grabbable.transform);
        running.Remove(grabbable);
    }

    // #168 rule 6: while the package is held, the first time it is lifted more than LiftDistance the packages lying on
    // it (and on those) fall. Polled a few times a second, only while held.
    private IEnumerator WatchLift(Grabbable grabbable)
    {
        Transform package = grabbable.transform;
        Vector3 start = package.position;
        List<Transform> resting = RestingOn(package);
        var wait = new WaitForSeconds(LiftCheckSeconds);
        yield return null;

        while (resting.Count > 0 && CartonSettler.HeldByHand(grabbable) > 0)
        {
            // Lifted, not only touched or slid sideways.
            if (package.position.y - start.y > LiftDistance)
            {
                DropResting(resting);
                break;
            }
            yield return wait;
        }
        lifts.Remove(grabbable);
    }

    // Everything lying on the package, directly or on something lying on it.
    private List<Transform> RestingOn(Transform package)
    {
        // Each package measured once for the whole search (this runs in the frame of the grab).
        var bodies = new Dictionary<Transform, DropBody>();
        foreach (Transform other in packages)
        {
            if (other != null && other.gameObject.activeInHierarchy
                && TryMeasure(other, other.position, other.rotation, out DropBody measured))
            {
                bodies[other] = measured;
            }
        }

        var resting = new List<Transform>();
        var queue = new Queue<Transform>();
        queue.Enqueue(package);
        while (queue.Count > 0)
        {
            Transform below = queue.Dequeue();
            if (!bodies.TryGetValue(below, out DropBody belowBody))
                continue;

            foreach (KeyValuePair<Transform, DropBody> entry in bodies)
            {
                Transform other = entry.Key;
                DropBody body = entry.Value;
                if (other == package || resting.Contains(other))
                    continue;

                if (body.Bottom < belowBody.Top - RestingGap || body.Bottom > belowBody.Top + RestingGap
                    || DropPlacementRule.Support(body, new[] { belowBody }) < RestingShare)
                    continue;

                resting.Add(other);
                queue.Enqueue(other);
            }
        }
        return resting;
    }

    private void DropResting(List<Transform> resting)
    {
        // Lowest first; each is planned with those above it ignored and those below already where they will lie.
        resting.Sort((a, b) => BottomOf(a).CompareTo(BottomOf(b)));
        var waiting = new HashSet<Transform>(resting);
        var moves = new List<(Grabbable grabbable, Pose from, Pose to)>();

        foreach (Transform package in resting)
        {
            waiting.Remove(package);
            Grabbable grabbable = package.GetComponent<Grabbable>();
            if (grabbable == null || IsHeld(package) || !AnyEnabled(OwnColliders(package)))
                continue;

            SnapInteractor snap = grabbable.GetComponentInChildren<SnapInteractor>();
            if (snap != null && snap.HasSelectedInteractable)
                continue;

            // The order container's settler lays its cartons; if it cannot now, the plan below decides.
            CartonSettler settler = SettlerCovering(grabbable);
            if (settler != null && settler.TrySettle(grabbable, waiting))
            {
                FellAfterLiftCount++;
                continue;
            }

            if (!TryPlan(grabbable, default, false, waiting, out Pose target, out bool returning) || returning)
                continue;

            moves.Add((grabbable, new Pose(package.position, package.rotation), target));
            package.SetPositionAndRotation(target.position, target.rotation);
            Physics.SyncTransforms();
        }

        // Back to where they are now (no trigger saw the planning moves), then down in a short fall each.
        foreach ((Grabbable grabbable, Pose from, Pose _) in moves)
        {
            grabbable.transform.SetPositionAndRotation(from.position, from.rotation);
        }
        Physics.SyncTransforms();
        foreach ((Grabbable grabbable, Pose _, Pose to) in moves)
        {
            FellAfterLiftCount++;
            MoveDown(grabbable, to);
        }
    }

    private float BottomOf(Transform package)
    {
        return TryMeasure(package, package.position, package.rotation, out DropBody body) ? (float)body.Bottom : package.position.y;
    }

    private CartonSettler SettlerCovering(Grabbable grabbable)
    {
        if (settlers == null)
            return null;

        foreach (CartonSettler settler in settlers)
        {
            if (settler != null && settler.isActiveAndEnabled && settler.Covers(grabbable))
                return settler;
        }
        return null;
    }

    // Held by a hand (a carton sitting in a snap place counts as lying, not held).
    private static bool IsHeld(Transform package)
    {
        Grabbable grabbable = package.GetComponent<Grabbable>();
        return grabbable != null && CartonSettler.HeldByHand(grabbable) > 0;
    }

    private static bool IsIgnored(Collider collider, ICollection<Transform> ignore)
    {
        if (ignore == null)
            return false;

        foreach (Transform package in ignore)
        {
            if (package != null && collider.transform.IsChildOf(package))
                return true;
        }
        return false;
    }

    // #168 (N5): the package's outline in its own turn (yaw), from its colliders' local boxes as if its root stood at
    // position/rotation, not its world-axis box; false when it has no enabled collider.
    private static bool TryMeasure(Transform root, Vector3 position, Quaternion rotation, out DropBody body)
    {
        body = default;
        Quaternion yaw = YawOf(rotation);
        Quaternion toYaw = Quaternion.Inverse(yaw);
        Quaternion turn = rotation * Quaternion.Inverse(root.rotation);
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        bool any = false;

        foreach (Collider collider in root.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled || !TryLocalBox(collider, out Vector3 centre, out Vector3 size))
                continue;

            for (int corner = 0; corner < 8; corner++)
            {
                var sign = new Vector3((corner & 1) == 0 ? -0.5f : 0.5f, (corner & 2) == 0 ? -0.5f : 0.5f,
                    (corner & 4) == 0 ? -0.5f : 0.5f);
                Vector3 world = collider.transform.TransformPoint(centre + Vector3.Scale(size, sign));
                // Yaw frame: x/z along the package's own sides, y stays the world height.
                Vector3 local = toYaw * (position + turn * (world - root.position));
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
                any = true;
            }
        }

        if (!any)
            return false;

        Vector3 middle = yaw * new Vector3((min.x + max.x) / 2f, 0f, (min.z + max.z) / 2f);
        body = new DropBody(middle.x, middle.z, (max.x - min.x) / 2f, (max.z - min.z) / 2f,
            yaw.eulerAngles.y * Mathf.Deg2Rad, min.y, max.y);
        return true;
    }

    // Only what lies in this container (not what hangs above its rim).
    private static bool LiesIn(DropBody body, DropSpace space)
    {
        return body.X >= space.MinX - 0.02 && body.X <= space.MaxX + 0.02 && body.Z >= space.MinZ - 0.02
               && body.Z <= space.MaxZ + 0.02 && body.Bottom <= space.Rim && body.Top >= space.Floor - 0.02;
    }

    // #183: a fixed block's box measured like a package (footprint in its own yaw, world heights).
    private static bool TryMeasureBlock(FixedDropBlock block, out DropBody body)
    {
        Transform t = block.transform;
        Quaternion yaw = YawOf(t.rotation);
        Quaternion toYaw = Quaternion.Inverse(yaw);
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        for (int corner = 0; corner < 8; corner++)
        {
            var sign = new Vector3((corner & 1) == 0 ? -0.5f : 0.5f, (corner & 2) == 0 ? -0.5f : 0.5f,
                (corner & 4) == 0 ? -0.5f : 0.5f);
            Vector3 local = toYaw * t.TransformPoint(block.Centre + Vector3.Scale(block.Size, sign));
            min = Vector3.Min(min, local);
            max = Vector3.Max(max, local);
        }

        Vector3 middle = yaw * new Vector3((min.x + max.x) / 2f, 0f, (min.z + max.z) / 2f);
        body = new DropBody(middle.x, middle.z, (max.x - min.x) / 2f, (max.z - min.z) / 2f,
            yaw.eulerAngles.y * Mathf.Deg2Rad, min.y, max.y);
        return block.Size.x > 0f && block.Size.y > 0f && block.Size.z > 0f;
    }

    private static bool TryLocalBox(Collider collider, out Vector3 centre, out Vector3 size)
    {
        switch (collider)
        {
            case BoxCollider box:
                centre = box.center;
                size = box.size;
                return true;
            case MeshCollider mesh when mesh.sharedMesh != null:
                centre = mesh.sharedMesh.bounds.center;
                size = mesh.sharedMesh.bounds.size;
                return true;
            case SphereCollider sphere:
                centre = sphere.center;
                size = Vector3.one * sphere.radius * 2f;
                return true;
            case CapsuleCollider capsule:
                centre = capsule.center;
                size = Vector3.one * capsule.radius * 2f;
                size[capsule.direction] = Mathf.Max(capsule.height, capsule.radius * 2f);
                return true;
            default:
                centre = Vector3.zero;
                size = Vector3.zero;
                return false;
        }
    }

    private IEnumerator Animate(Transform package, Pose target, bool returning)
    {
        Vector3 from = package.position;
        Quaternion fromRotation = package.rotation;
        float height = Mathf.Max(0f, from.y - target.position.y);
        bool rising = target.position.y > from.y + MovedTolerance;
        Vector3 sideways = target.position - from;
        sideways.y = 0f;
        float duration = returning ? ReturnSeconds : Mathf.Min(MaxFallSeconds, Mathf.Sqrt(2f * height / Gravity));
        if (!returning && (rising || sideways.magnitude > SidewaysMove))
        {
            // #168 (N3): pushed aside or onto a higher package: a short move, never a jump.
            duration = Mathf.Max(duration, MinMoveSeconds);
        }
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
                position.y = Mathf.Lerp(from.y, target.position.y, rising ? Mathf.SmoothStep(0f, 1f, k) : k * k);
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
