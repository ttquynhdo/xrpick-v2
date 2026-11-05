using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// #81, docs/decisions/019 and 025: at the tour step "material_flow" the ConceptScene station shows the way of the
/// containers on the real three-lane conveyor instead of describing it. After startDelay the storage container rides
/// onto the middle lane and away behind the monitor while the next storage container comes out of the hood into the
/// work spot; once it has stopped, the order container rides away over the middle lane too. Containers are switched off
/// only at the end of their ride, out of sight. Leaving the step, "Von vorn" or disabling puts everything back. Same
/// paths as PositionStockSwitcher/OrderBoxConveyor (LaneRide), but no station_move events.
/// #89: the tour bar stands between the trainee's eyes and the start of the middle lane, so the objects in
/// hideWhileRiding are hidden from the first ride until the order container has left, then shown again.
/// #100, decision 030 item 5: the containers only ride once no hint is shown (ConceptBoxHints); until then the bar stays.
/// #92, decision 032: the sample cartons are grabbable and start in the storage container. If the order container
/// holds fewer cartons than the order needs, the missing ones are moved over one by one before anything rides, so Ist
/// reaches Soll. The cartons lying in each container ride with it (colliders and interactables off, Ist frozen on the
/// sample WMS); any reset puts all of them back to their scene-start pose, grabbable. While the demo moves cartons,
/// ConceptBoxHints logs no container_* events.
/// </summary>
public class LaneFlowDemo : MonoBehaviour
{
    // About half a carton's width: a slot closer than this to a carton in the order container is taken.
    private const float SlotTakenDistance = 0.07f;

    [SerializeField] private WmsFieldTour tour;

    [Tooltip("Tour step id that runs the demo.")]
    [SerializeField] private string stepId = "material_flow";

    [Header("Containers")]
    [SerializeField] private Transform storageBox;

    [Tooltip("Next storage container, inactive at its place in the work spot until the demo runs.")]
    [SerializeField] private Transform nextStorageBox;

    [Tooltip("Optional: static riders in the next storage container. #92: empty in ConceptScene, so nothing there looks grabbable without being so.")]
    [SerializeField] private Transform nextStorageStock;

    [SerializeField] private Transform orderBox;

    [Tooltip("Hints on wrong or extra packages; the demo waits until none is shown. Also tells which cartons lie where.")]
    [SerializeField] private ConceptBoxHints boxHints;

    [Header("Sample cartons (#92)")]
    [Tooltip("The grabbable sample cartons; all go back to their scene-start pose on reset.")]
    [SerializeField] private Transform[] sampleCartons;

    [Tooltip("Sample WMS order: how many cartons the order needs; Ist is frozen while the cartons ride.")]
    [SerializeField] private SampleWmsOrder order;

    [Tooltip("Places for cartons moved into the order container, as offsets from its position (same grid as the storage container).")]
    [SerializeField] private Vector3[] orderSlots =
    {
        new(-0.091f, 0.012f, -0.125f), new(-0.091f, 0.012f, 0f), new(-0.091f, 0.012f, 0.125f),
        new(0.091f, 0.012f, -0.125f), new(0.091f, 0.012f, 0f), new(0.091f, 0.012f, 0.125f)
    };

    [Tooltip("Seconds between two cartons moved over.")]
    [SerializeField] private float transferInterval = 0.6f;

    [Header("Paths (world offsets, same as Basic/Mixed/Complex)")]
    [Tooltip("Offsets of the next storage container from its work spot; the first is the start in the hood, the last must be zero.")]
    [SerializeField] private Vector3[] arrivalPath = { new(-0.40f, 0f, 0.90f), new(-0.40f, 0f, 0f), Vector3.zero };

    [Tooltip("Offsets from the work spot: onto the middle lane, then back behind the monitor.")]
    [SerializeField] private Vector3[] departurePath = { new(0.40f, 0f, 0f), new(0.40f, 0f, 2.29f) };

    [Tooltip("Offsets of the order container: onto the middle lane, then back behind the monitor.")]
    [SerializeField] private Vector3[] orderPath = { new(-0.40f, 0f, 0f), new(-0.40f, 0f, 2.29f) };

    [Header("View (#89)")]
    [Tooltip("Hidden while the containers ride (they would block the middle lane); only those active at the start are shown again.")]
    [SerializeField] private GameObject[] hideWhileRiding;

    [Tooltip("Seconds to read the step on the bar before the containers ride.")]
    [SerializeField] private float startDelay = 4f;
    [SerializeField] private float storageSpeed = 0.6f;
    [SerializeField] private float orderSpeed = 0.45f;
    [SerializeField] private float cornerPause = 0.3f;

    private readonly List<(Transform transform, Vector3 position, bool active, bool keepActive)> start = new();
    private readonly List<(Transform carton, Vector3 position, Quaternion rotation)> cartonStart = new();
    private readonly List<GameObject> hidden = new();
    private readonly List<CartonRider> riders = new();
    private bool changed;

    private void Awake()
    {
        Remember(storageBox);
        Remember(nextStorageBox);
        Remember(orderBox);
        foreach (Transform rider in Riders(nextStorageStock))
        {
            Remember(rider, keepActive: true);
        }
        Remember(nextStorageStock);

        if (sampleCartons == null)
            return;

        foreach (Transform carton in sampleCartons)
        {
            if (carton != null)
            {
                cartonStart.Add((carton, carton.position, carton.rotation));
            }
        }
    }

    private void OnEnable()
    {
        if (tour != null)
        {
            tour.StepShown += HandleStepShown;
        }
    }

    private void OnDisable()
    {
        if (tour != null)
        {
            tour.StepShown -= HandleStepShown;
        }

        ResetStation();
    }

    private void Start()
    {
        bool missing = RequiredReference.IsMissing(tour, nameof(tour), this)
                       | RequiredReference.IsMissing(storageBox, nameof(storageBox), this)
                       | RequiredReference.IsMissing(nextStorageBox, nameof(nextStorageBox), this)
                       | RequiredReference.IsMissing(orderBox, nameof(orderBox), this)
                       | RequiredReference.IsMissing(boxHints, nameof(boxHints), this)
                       | RequiredReference.IsMissing(order, nameof(order), this);

        if (!missing && (arrivalPath.Length == 0 || arrivalPath[^1] != Vector3.zero))
        {
            Debug.LogError("LaneFlowDemo: arrivalPath must end at (0, 0, 0).", this);
        }
        if (sampleCartons == null || sampleCartons.Length == 0)
        {
            Debug.LogError("LaneFlowDemo: sampleCartons is empty.", this);
        }
    }

    private void HandleStepShown(string shownStepId)
    {
        ResetStation();

        if (shownStepId == stepId)
        {
            StartCoroutine(Run());
        }
    }

    private IEnumerator Run()
    {
        changed = true;
        yield return new WaitForSeconds(startDelay);

        while (boxHints != null && !boxHints.IsClear)
        {
            yield return null;
        }

        if (boxHints != null)
        {
            boxHints.LogSuspended = true;
        }
        HideViewBlockers();

        yield return FillOrder();

        List<Transform> storageRiders = TakeRiders(ConceptBox.Storage);

        // The next storage container starts in the hood.
        List<Transform> nextRiders = Riders(nextStorageStock);
        LaneRide.Move(nextStorageBox, nextRiders, arrivalPath[0]);
        SetActive(nextStorageBox, true);
        SetActive(nextStorageStock, true);

        var arrival = new List<Vector3>();
        for (int i = 1; i < arrivalPath.Length; i++)
        {
            arrival.Add(arrivalPath[i] - arrivalPath[0]);
        }

        bool arrived = false;
        StartCoroutine(RideAway(storageBox, storageRiders, departurePath, storageSpeed));
        StartCoroutine(RideThen(nextStorageBox, nextRiders, arrival, storageSpeed, () => arrived = true));

        while (!arrived)
        {
            yield return null;
        }

        // Ist stays at what the order container holds while its cartons ride with their colliders off.
        List<Transform> orderRiders = TakeRiders(ConceptBox.Order);
        if (order != null)
        {
            order.SetShownPieces(orderRiders.Count * order.CurrentLine.vpe);
        }
        yield return RideAway(orderBox, orderRiders, orderPath, orderSpeed);

        ShowViewBlockers();
    }

    // The cartons the order still needs come over from the storage container (or wherever they lie) one by one.
    private IEnumerator FillOrder()
    {
        if (boxHints == null || order == null || sampleCartons == null)
            yield break;

        int vpe = Mathf.Max(order.CurrentLine.vpe, 1);
        List<Transform> inOrder = boxHints.SampleIn(ConceptBox.Order);
        int missing = order.CurrentLine.soll / vpe - inOrder.Count;
        if (missing <= 0)
            yield break;

        var candidates = new List<Transform>(boxHints.SampleIn(ConceptBox.Storage));
        foreach (Transform carton in sampleCartons)
        {
            if (carton != null && !candidates.Contains(carton) && !inOrder.Contains(carton) && !boxHints.IsHeld(carton))
            {
                candidates.Add(carton);
            }
        }

        var taken = new List<Vector3>();
        foreach (Transform carton in inOrder)
        {
            taken.Add(carton.position);
        }

        for (int i = 0; i < missing && i < candidates.Count; i++)
        {
            if (!FreeSlot(taken, out Vector3 slot))
                break;

            if (i > 0)
            {
                yield return new WaitForSeconds(transferInterval);
            }

            Transform carton = candidates[i];
            int index = cartonStart.FindIndex(c => c.carton == carton);
            Quaternion rotation = index >= 0 ? cartonStart[index].rotation : carton.rotation;
            carton.gameObject.SetActive(true);
            carton.SetPositionAndRotation(slot, rotation);
            taken.Add(slot);
        }

        // Let ConceptBoxHints settle the cartons, so Ist reaches Soll before anything rides.
        yield return new WaitForSeconds((float)ConceptBoxHintRule.SettleSeconds + 0.3f);
    }

    private bool FreeSlot(List<Vector3> taken, out Vector3 slot)
    {
        foreach (Vector3 offset in orderSlots)
        {
            Vector3 candidate = orderBox.position + offset;
            bool free = true;
            foreach (Vector3 position in taken)
            {
                var distance = new Vector2(position.x - candidate.x, position.z - candidate.z);
                if (distance.magnitude < SlotTakenDistance)
                {
                    free = false;
                    break;
                }
            }

            if (free)
            {
                slot = candidate;
                return true;
            }
        }

        slot = default;
        return false;
    }

    // The cartons lying in a container ride with it: not grabbable, not seen by the containers.
    private List<Transform> TakeRiders(ConceptBox box)
    {
        var list = new List<Transform>();
        if (boxHints == null)
            return list;

        foreach (Transform carton in boxHints.SampleIn(box))
        {
            riders.Add(new CartonRider(carton));
            list.Add(carton);
        }

        return list;
    }

    private void HideViewBlockers()
    {
        if (hideWhileRiding == null)
            return;

        foreach (GameObject blocker in hideWhileRiding)
        {
            if (blocker != null && blocker.activeSelf)
            {
                hidden.Add(blocker);
                blocker.SetActive(false);
            }
        }
    }

    private void ShowViewBlockers()
    {
        foreach (GameObject blocker in hidden)
        {
            if (blocker != null)
            {
                blocker.SetActive(true);
            }
        }

        hidden.Clear();
    }

    private IEnumerator RideAway(Transform box, List<Transform> boxRiders, IReadOnlyList<Vector3> path, float speed)
    {
        yield return LaneRide.Path(box, boxRiders, path, speed, cornerPause);

        // Out of sight behind the monitor: switch off. The riders, not their parent, which may hold a script.
        SetActive(box, false);
        foreach (Transform rider in boxRiders)
        {
            SetActive(rider, false);
        }
    }

    private IEnumerator RideThen(Transform box, List<Transform> boxRiders, IReadOnlyList<Vector3> path, float speed,
        System.Action done)
    {
        yield return LaneRide.Path(box, boxRiders, path, speed, cornerPause);
        done();
    }

    // Everything back to where it was in the scene: the next storage container hidden again, every sample carton in
    // the storage container and grabbable, Ist 0.
    private void ResetStation()
    {
        StopAllCoroutines();
        ShowViewBlockers();
        foreach (CartonRider rider in riders)
        {
            rider.Restore();
        }
        riders.Clear();

        if (!changed)
            return;

        changed = false;
        foreach ((Transform target, Vector3 position, bool active, bool keepActive) in start)
        {
            if (target == null)
                continue;

            target.position = position;
            if (!keepActive)
            {
                target.gameObject.SetActive(active);
            }
        }

        // A carton in the trainee's hand stays there; it is put back wherever it is let go.
        foreach ((Transform carton, Vector3 position, Quaternion rotation) in cartonStart)
        {
            if (carton != null && (boxHints == null || !boxHints.IsHeld(carton)))
            {
                carton.gameObject.SetActive(true);
                carton.SetPositionAndRotation(position, rotation);
            }
        }

        if (order != null)
        {
            order.SetShownPieces(0);
        }
        if (boxHints != null)
        {
            boxHints.LogSuspended = false;
        }
    }

    // keepActive: only the position comes back on reset (riders whose visibility another script owns).
    private void Remember(Transform target, bool keepActive = false)
    {
        if (target != null)
        {
            start.Add((target, target.position, target.gameObject.activeSelf, keepActive));
        }
    }

    private static List<Transform> Riders(Transform stock)
    {
        var list = new List<Transform>();
        if (stock == null)
            return list;

        foreach (Transform child in stock)
        {
            list.Add(child);
        }

        return list;
    }

    // A grabbable carton riding with a container: colliders and interactables off until the reset.
    private sealed class CartonRider
    {
        private readonly Transform transform;
        private readonly List<Collider> colliders = new();
        private readonly List<Behaviour> interactables = new();

        public CartonRider(Transform transform)
        {
            this.transform = transform;

            foreach (Collider collider in transform.GetComponentsInChildren<Collider>())
            {
                if (collider.enabled)
                {
                    collider.enabled = false;
                    colliders.Add(collider);
                }
            }
            foreach (Behaviour behaviour in transform.GetComponentsInChildren<Behaviour>())
            {
                if (behaviour is Oculus.Interaction.IInteractable && behaviour.enabled)
                {
                    behaviour.enabled = false;
                    interactables.Add(behaviour);
                }
            }
        }

        public void Restore()
        {
            if (transform == null)
                return;

            transform.gameObject.SetActive(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
            foreach (Behaviour behaviour in interactables)
            {
                behaviour.enabled = true;
            }
        }
    }

    private static void SetActive(Transform target, bool active)
    {
        if (target != null)
        {
            target.gameObject.SetActive(active);
        }
    }
}
