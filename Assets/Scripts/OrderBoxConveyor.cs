using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #47/#50, docs/decisions/011 and 014: when the validator reports the whole order correct, the order container
/// rides away along the order lane with the packages inside it: straight legs, a short stop at each right-angle
/// transfer, no rotation. Sits on the order container, whose trigger is the counter's trigger, so packages and
/// trigger move together and nothing is counted again. #71, decision 019: in Mixed/Complex it first waits until the
/// last storage container has left the station, then slides sideways onto the middle lane.
/// </summary>
public class OrderBoxConveyor : MonoBehaviour
{
    [Tooltip("Validator of this scene; must implement IOrderCompletion.")]
    [SerializeField] private MonoBehaviour validator;

    [Tooltip("Trigger of the order container; packages overlapping it ride along.")]
    [SerializeField] private BoxCollider containerTrigger;

    // Defaults follow the order lane of WarehouseConveyorNetwork from the station at (0.33, 0.71):
    // corner at z 3.0, corner at x 5.0, stop behind the empty station's container (z 4.64) at x 5.
    [Tooltip("Stops along the order lane as world offsets from the start position (metres), in order. Each leg is straight; the container waits at every stop but the last.")]
    [SerializeField] private Vector3[] path =
    {
        new(0f, 0f, 2.288f),
        new(4.671f, 0f, 2.288f),
        new(4.671f, 0f, 3.678f),
    };

    [Tooltip("Average speed on each leg in m/s (decision 014: as the old slide, 0.9 m in 2 s).")]
    [SerializeField] private float speed = 0.45f;

    [Tooltip("Wait at each right-angle transfer in seconds.")]
    [SerializeField] private float cornerPause = 1f;

    [Tooltip("Optional bodies that must move with the container, e.g. the EKZ snap places of #41.")]
    [SerializeField] private Rigidbody[] carriedWithContainer;

    // #71, decision 019: the storage container leaves the station first.
    [Tooltip("Optional. Storage containers on the station lanes (IStationStock); the order container waits while they move.")]
    [SerializeField] private MonoBehaviour stationStock;

    private IOrderCompletion completion;
    private IStationStock stock;
    private Coroutine slide;

    private void OnEnable()
    {
        completion = validator as IOrderCompletion;
        stock = stationStock as IStationStock;

        if (stationStock != null && stock == null)
        {
            Debug.LogError("OrderBoxConveyor: stationStock does not implement IStationStock.", this);
        }

        if (completion == null)
        {
            Debug.LogError("OrderBoxConveyor: validator is not assigned or does not implement IOrderCompletion.", this);
            return;
        }
        if (containerTrigger == null)
        {
            Debug.LogError("OrderBoxConveyor: containerTrigger is not assigned.", this);
            return;
        }
        if (path == null || path.Length == 0 || speed <= 0f)
        {
            Debug.LogError("OrderBoxConveyor: path is empty or speed is not positive.", this);
            return;
        }

        completion.OrderCompleted += StartSlide;
    }

    private void OnDisable()
    {
        if (completion != null)
        {
            completion.OrderCompleted -= StartSlide;
        }
        if (slide != null)
        {
            StopCoroutine(slide);
            slide = null;
        }
    }

    private void StartSlide()
    {
        if (slide != null)
            return;

        slide = StartCoroutine(Slide(CollectRiders()));
    }

    // Packages inside the trigger ride along and can no longer be grabbed: the order is shipped.
    private List<Transform> CollectRiders()
    {
        var riders = new List<Transform>();
        Bounds bounds = containerTrigger.bounds;

        // Collide: the loose pieces of ComplexScene only have trigger colliders.
        foreach (Collider other in Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide))
        {
            // Only packages ride along: the desk and other static colliders also touch the trigger.
            PackageData package = other.GetComponentInParent<PackageData>();

            if (package == null)
                continue;

            Transform rider = other.attachedRigidbody != null ? other.attachedRigidbody.transform : package.transform;

            if (rider == transform || rider.IsChildOf(transform) || riders.Contains(rider) || LaneRide.IsHeld(rider))
                continue;

            riders.Add(rider);

            foreach (MonoBehaviour behaviour in rider.GetComponentsInChildren<MonoBehaviour>())
            {
                if (behaviour is IInteractable)
                {
                    behaviour.enabled = false;
                }
            }
        }

        if (carriedWithContainer != null)
        {
            foreach (Rigidbody body in carriedWithContainer)
            {
                if (body != null && !riders.Contains(body.transform))
                {
                    riders.Add(body.transform);
                }
            }
        }

        return riders;
    }

    private IEnumerator Slide(List<Transform> riders)
    {
        if (stock != null)
        {
            // The storage container starts leaving in the same OrderCompleted event; let it register first.
            yield return null;
            while (stock.IsMoving)
            {
                yield return null;
            }
        }

        yield return LaneRide.Path(transform, riders, path, speed, cornerPause);
        slide = null;
    }
}
