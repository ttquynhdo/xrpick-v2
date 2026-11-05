using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #21/#71, docs/decisions/012 and 019: one storage container per position, all worked at the same place (the work
/// spot left of the middle lane). After a correct position the old container rides away with the packages still
/// in it (sideways onto the middle lane, then back under the rear bench to the corner behind the monitor, where it
/// is switched off) and the next one comes out of the hood on
/// the left lane and turns into the work spot. After the last position the old container only leaves. Packages in
/// the order container, in the trainee's hand or lying outside the storage container stay where they are.
/// </summary>
public class PositionStockSwitcher : MonoBehaviour, IStationStock
{
    [Serializable]
    private class StockSet
    {
        [Tooltip("Storage box of this position.")]
        public GameObject box;

        [Tooltip("Packages lying in this box (they are not children of it).")]
        public List<PackageData> packages = new();
    }

    // #19: any container worked one position at a time (MixedPickingContainer, PiecesContainerCounter; #72: CalcForWMS
    // with a single position, whose storage container only leaves after the order).
    [Tooltip("Component implementing IPositionedOrder (MixedPickingContainer, PiecesContainerCounter, CalcForWMS).")]
    [SerializeField] private MonoBehaviour container;

    [Tooltip("One set per order line, in the same order as the container's Order Lines.")]
    [SerializeField] private List<StockSet> sets = new();

    // #19, decision 016: in ComplexScene the first box appears only after the division question is answered.
    [Tooltip("Hide the first set at start; RevealFirstSet brings it in on the left lane.")]
    [SerializeField] private bool hideFirstUntilRevealed;

    // #71, decision 019: after the last position the last storage container leaves too.
    [Tooltip("Validator of this scene (IOrderCompletion); when the whole order is done the last box leaves.")]
    [SerializeField] private MonoBehaviour validator;

    [Header("Lanes (offsets from the work spot, metres)")]
    [Tooltip("Where a new box appears (hidden in the hood of the left lane), then each stop; the last one must be (0, 0, 0).")]
    [SerializeField] private Vector3[] arrivalPath =
    {
        new(-0.40f, 0f, 0.90f),
        new(-0.40f, 0f, 0f),
        Vector3.zero,
    };

    // Onto the middle lane, then back to its corner at z 3.0, where the WMS monitor hides it from the standing spot.
    [Tooltip("Stops of a leaving box, in order; it is switched off at the last one.")]
    [SerializeField] private Vector3[] departurePath =
    {
        new(0.40f, 0f, 0f),
        new(0.40f, 0f, 2.288f),
    };

    [Tooltip("Average speed on each leg in m/s.")]
    [SerializeField] private float speed = 0.6f;

    [Tooltip("Wait at each corner in seconds.")]
    [SerializeField] private float cornerPause = 0.3f;

    // #99, decision 033: time to read the result card and look at the station before a box moves after a confirm.
    [Tooltip("Seconds between a correct confirm and the first movement, so the result card is read first (decision 033: 4 s).")]
    [SerializeField] private float startDelay = 4f;

    // #99: the first box of ComplexScene comes in after the division question, not after a confirm.
    [Tooltip("Seconds before the first box rides in after RevealFirstSet.")]
    [SerializeField] private float revealDelay = 0.6f;

    [Tooltip("A leaving box has left the station once it is this far from the work spot (metres).")]
    [SerializeField] private float clearDistance = 0.7f;

    private IPositionedOrder order;
    private IOrderCompletion completion;
    private bool firstRevealed;
    private Vector3[] workSpots;

    private readonly List<Ride> rides = new();
    private int blockingRides;

    public bool IsMoving => blockingRides > 0;

    public event Action Settled;

    private void Awake()
    {
        order = container as IPositionedOrder;
        completion = validator as IOrderCompletion;
        firstRevealed = !hideFirstUntilRevealed;

        workSpots = new Vector3[sets.Count];
        for (int i = 0; i < sets.Count; i++)
        {
            workSpots[i] = sets[i].box != null ? sets[i].box.transform.position : transform.position;
            SetVisible(sets[i], i == 0 && firstRevealed);
        }
    }

    private void OnEnable()
    {
        if (order != null)
        {
            order.LineAdvanced += OnLineAdvanced;
        }
        else
        {
            Debug.LogError("PositionStockSwitcher: container is not assigned or is not an IPositionedOrder.", this);
        }

        if (completion != null)
        {
            completion.OrderCompleted += OnOrderCompleted;
        }
        else if (validator != null)
        {
            Debug.LogError("PositionStockSwitcher: validator does not implement IOrderCompletion.", this);
        }
    }

    private void OnDisable()
    {
        if (order != null)
        {
            order.LineAdvanced -= OnLineAdvanced;
        }
        if (completion != null)
        {
            completion.OrderCompleted -= OnOrderCompleted;
        }

        // A disabled object stops its coroutines: put every box where its ride ends and give its packages back.
        StopAllCoroutines();
        foreach (Ride ride in rides)
        {
            ride.Finish();
        }
        rides.Clear();

        // Listeners waiting for the box must not keep showing the old position.
        bool wasMoving = blockingRides > 0;
        blockingRides = 0;
        if (wasMoving)
        {
            Settled?.Invoke();
        }
    }

    private void Start()
    {
        if (order != null && sets.Count != order.OrderLines.Count)
        {
            Debug.LogError($"PositionStockSwitcher: {sets.Count} stock sets but {order.OrderLines.Count} order lines.", this);
        }
        if (arrivalPath.Length == 0 || arrivalPath[^1] != Vector3.zero || departurePath.Length == 0 || speed <= 0f)
        {
            Debug.LogError("PositionStockSwitcher: arrivalPath must end at (0, 0, 0), departurePath must not be empty and speed must be positive.", this);
        }
    }

    // #19: brings the first box in once (after the division question). Does nothing when it is already shown.
    public void RevealFirstSet()
    {
        if (firstRevealed || sets.Count == 0)
            return;

        firstRevealed = true;
        StartArrival(0, revealDelay);
    }

    private void OnLineAdvanced(int position)
    {
        int next = position - 1;

        if (next <= 0 || next >= sets.Count)
            return;

        // Activate the next box now (hidden in the hood): the container computes Rest right after this event.
        StartDeparture(next - 1);
        StartArrival(next, startDelay);
    }

    private void OnOrderCompleted()
    {
        if (order != null && order.CurrentPosition - 1 < sets.Count)
        {
            StartDeparture(order.CurrentPosition - 1);
        }
    }

    private void StartArrival(int index, float delay)
    {
        StockSet set = sets[index];
        var riders = new List<Transform>();

        foreach (PackageData package in set.packages)
        {
            if (package != null)
            {
                riders.Add(package.transform);
            }
        }

        // Start at the work spot (where the scene keeps every set), jump into the hood, then ride to the spot.
        SetVisible(set, true);
        Vector3 offset = arrivalPath.Length > 0 ? arrivalPath[0] : Vector3.zero;
        Transform box = set.box != null ? set.box.transform : null;
        LaneRide.Move(box, riders, workSpots[index] + offset - (box != null ? box.position : workSpots[index]));

        var path = new List<Vector3>();
        for (int i = 1; i < arrivalPath.Length; i++)
        {
            path.Add(arrivalPath[i] - offset);
        }

        var ride = new Ride(box, riders, path, hideAtEnd: false);
        StartRide(ride, () => ride.Done, OutcomeEvents.MoveStorageIn, index + 1, delay);
    }

    private void StartDeparture(int index)
    {
        StockSet set = sets[index];

        if (set.box == null || !set.box.activeInHierarchy)
            return;

        Transform box = set.box.transform;
        Bounds footprint = BoxBounds(set.box);
        var riders = new List<Transform>();

        foreach (PackageData package in set.packages)
        {
            if (package == null || !package.gameObject.activeInHierarchy || order.Contains(package)
                || LaneRide.IsHeld(package.transform) || !IsInside(package, footprint))
                continue;

            riders.Add(package.transform);
        }

        Vector3 start = box.position;
        var ride = new Ride(box, riders, departurePath, hideAtEnd: true);
        StartRide(ride, () => ride.Done || (box.position - start).magnitude >= clearDistance,
            OutcomeEvents.MoveStorageOut, index + 1, startDelay);
    }

    // what and position go into station_move_start / _arrived (#28, decision 021): the position just finished
    // for a box leaving, the new one for a box arriving. The station counts as moving from here on, also during
    // the delay (#99): the red button is ignored and the riders can't be grabbed while the trainee reads.
    private void StartRide(Ride ride, Func<bool> clearOfStation, string what, int position, float delay)
    {
        rides.Add(ride);
        blockingRides++;
        ride.Freeze();
        StartCoroutine(RunRide(ride, what, position, delay));
        StartCoroutine(WaitUntilClear(clearOfStation));
    }

    private IEnumerator RunRide(Ride ride, string what, int position, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        ResearchLogger.LogStationMove(false, what, position);
        yield return LaneRide.Path(ride.Box, ride.Riders, ride.Path, speed, cornerPause);
        ResearchLogger.LogStationMove(true, what, position);

        ride.Finish();
        rides.Remove(ride);
    }

    private IEnumerator WaitUntilClear(Func<bool> clearOfStation)
    {
        while (!clearOfStation())
        {
            yield return null;
        }

        blockingRides--;
        if (blockingRides == 0)
        {
            Settled?.Invoke();
        }
    }

    // A package that the trainee put on the desk or the floor is not in the storage box any more.
    private static bool IsInside(PackageData package, Bounds footprint)
    {
        Bounds bounds = BoxBounds(package.gameObject);
        Vector3 centre = bounds.size == Vector3.zero ? package.transform.position : bounds.center;

        return centre.x >= footprint.min.x && centre.x <= footprint.max.x
            && centre.z >= footprint.min.z && centre.z <= footprint.max.z
            && centre.y >= footprint.min.y - 0.05f && centre.y <= footprint.max.y + 0.30f;
    }

    private static Bounds BoxBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(root.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        bounds.Expand(new Vector3(0.04f, 0f, 0.04f));
        return bounds;
    }

    private static void SetVisible(StockSet set, bool visible)
    {
        if (set.box != null)
        {
            set.box.SetActive(visible);
        }

        foreach (PackageData package in set.packages)
        {
            if (package != null)
            {
                package.gameObject.SetActive(visible);
            }
        }
    }

    // One box on its way and the packages riding in it. While riding, packages can't be grabbed and have no
    // colliders (nothing is counted on the way, nothing bumps into the rear bench).
    private sealed class Ride
    {
        private readonly bool hideAtEnd;
        private readonly Vector3 end;
        private readonly List<Behaviour> switchedOff = new();
        private readonly List<Collider> collidersOff = new();
        private readonly Dictionary<Rigidbody, bool> wasKinematic = new();

        public Ride(Transform box, List<Transform> riders, IReadOnlyList<Vector3> path, bool hideAtEnd)
        {
            Box = box;
            Riders = riders;
            Path = path;
            this.hideAtEnd = hideAtEnd;
            end = (box != null ? box.position : Vector3.zero) + (path.Count > 0 ? path[^1] : Vector3.zero);
        }

        public Transform Box { get; }
        public List<Transform> Riders { get; }
        public IReadOnlyList<Vector3> Path { get; }
        public bool Done { get; private set; }

        public void Freeze()
        {
            foreach (Transform rider in Riders)
            {
                foreach (MonoBehaviour behaviour in rider.GetComponentsInChildren<MonoBehaviour>())
                {
                    if (behaviour is IInteractable && behaviour.enabled)
                    {
                        behaviour.enabled = false;
                        switchedOff.Add(behaviour);
                    }
                }
                foreach (Collider collider in rider.GetComponentsInChildren<Collider>())
                {
                    if (collider.enabled)
                    {
                        collider.enabled = false;
                        collidersOff.Add(collider);
                    }
                }
                foreach (Rigidbody body in rider.GetComponentsInChildren<Rigidbody>())
                {
                    if (!wasKinematic.ContainsKey(body))
                    {
                        wasKinematic[body] = body.isKinematic;
                        body.isKinematic = true;
                    }
                }
            }
        }

        // Puts the box at the end of its ride (also when the ride was cut short) and gives the packages back.
        public void Finish()
        {
            if (Done)
                return;

            Done = true;

            if (Box != null)
            {
                LaneRide.Move(Box, Riders, end - Box.position);
            }

            foreach (Behaviour behaviour in switchedOff)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = true;
                }
            }
            foreach (Collider collider in collidersOff)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
            foreach (KeyValuePair<Rigidbody, bool> entry in wasKinematic)
            {
                if (entry.Key != null)
                {
                    entry.Key.isKinematic = entry.Value;
                }
            }

            if (!hideAtEnd)
                return;

            if (Box != null)
            {
                Box.gameObject.SetActive(false);
            }
            foreach (Transform rider in Riders)
            {
                if (rider != null)
                {
                    rider.gameObject.SetActive(false);
                }
            }
        }
    }
}
