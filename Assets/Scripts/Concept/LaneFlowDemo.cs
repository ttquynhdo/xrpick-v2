using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// #81, docs/decisions/019 and 025: at the tour step "material_flow" the ConceptScene station shows the way of the
/// containers on the real three-lane conveyor instead of describing it. After startDelay the storage container rides
/// onto the middle lane and away behind the monitor while the next storage container comes out of the hood into the
/// work spot; once it has stopped, the order container rides away over the middle lane too. Containers are switched off
/// only at the end of their ride, out of sight. Leaving the step, "Von vorn" or disabling puts everything back. Same
/// paths as PositionStockSwitcher/OrderBoxConveyor (LaneRide), but no station_move events and no grabbable riders.
/// #89: the tour bar stands between the trainee's eyes and the start of the middle lane, so the objects in
/// hideWhileRiding are hidden from the first ride until the order container has left, then shown again.
/// #91, decision 030: the order container leaves with the counted cartons in it (CartonCountDemo shows them at this
/// step). Riders only get their position back on reset; whether they are shown is up to the script that owns them.
/// </summary>
public class LaneFlowDemo : MonoBehaviour
{
    [SerializeField] private WmsFieldTour tour;

    [Tooltip("Tour step id that runs the demo.")]
    [SerializeField] private string stepId = "material_flow";

    [Header("Containers")]
    [SerializeField] private Transform storageBox;

    [Tooltip("Static cartons riding in the storage container (its children ride).")]
    [SerializeField] private Transform storageStock;

    [Tooltip("Next storage container, inactive at its place in the work spot until the demo runs.")]
    [SerializeField] private Transform nextStorageBox;

    [SerializeField] private Transform nextStorageStock;

    [SerializeField] private Transform orderBox;

    [Tooltip("Static cartons riding in the order container (its children ride; CartonCountDemo shows them).")]
    [SerializeField] private Transform orderStock;

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
    private readonly List<GameObject> hidden = new();
    private bool changed;

    private void Awake()
    {
        Remember(storageBox);
        Remember(nextStorageBox);
        Remember(orderBox);
        foreach (Transform rider in Riders(storageStock))
        {
            Remember(rider, keepActive: true);
        }
        foreach (Transform rider in Riders(nextStorageStock))
        {
            Remember(rider, keepActive: true);
        }
        foreach (Transform rider in Riders(orderStock))
        {
            Remember(rider, keepActive: true);
        }
        Remember(storageStock);
        Remember(nextStorageStock);
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
                       | RequiredReference.IsMissing(orderBox, nameof(orderBox), this);

        if (!missing && (arrivalPath.Length == 0 || arrivalPath[^1] != Vector3.zero))
        {
            Debug.LogError("LaneFlowDemo: arrivalPath must end at (0, 0, 0).", this);
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

        HideViewBlockers();

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
        StartCoroutine(RideAway(storageBox, storageStock, departurePath, storageSpeed));
        StartCoroutine(RideThen(nextStorageBox, nextRiders, arrival, storageSpeed, () => arrived = true));

        while (!arrived)
        {
            yield return null;
        }

        yield return RideAway(orderBox, orderStock, orderPath, orderSpeed);

        ShowViewBlockers();
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

    private IEnumerator RideAway(Transform box, Transform stock, IReadOnlyList<Vector3> path, float speed)
    {
        List<Transform> riders = Riders(stock);
        yield return LaneRide.Path(box, riders, path, speed, cornerPause);

        // Out of sight behind the monitor: switch off. The riders, not their parent, which may hold a script.
        SetActive(box, false);
        foreach (Transform rider in riders)
        {
            SetActive(rider, false);
        }
    }

    private IEnumerator RideThen(Transform box, List<Transform> riders, IReadOnlyList<Vector3> path, float speed, System.Action done)
    {
        yield return LaneRide.Path(box, riders, path, speed, cornerPause);
        done();
    }

    // Everything back to where it was in the scene; the next storage container hidden again.
    private void ResetStation()
    {
        StopAllCoroutines();
        ShowViewBlockers();
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
        var riders = new List<Transform>();
        if (stock == null)
            return riders;

        foreach (Transform child in stock)
        {
            riders.Add(child);
        }

        return riders;
    }

    private static void SetActive(Transform target, bool active)
    {
        if (target != null)
        {
            target.gameObject.SetActive(active);
        }
    }
}
