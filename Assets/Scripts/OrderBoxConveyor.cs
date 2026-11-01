using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #47, docs/decisions/011 (K1): when the validator reports the whole order correct, the order container
/// slides back along the roller conveyor (Abschieben) with the packages inside it. Sits on the order container,
/// whose trigger is the counter's trigger, so packages and trigger move together and nothing is counted again.
/// </summary>
public class OrderBoxConveyor : MonoBehaviour
{
    [Tooltip("Validator of this scene; must implement IOrderCompletion.")]
    [SerializeField] private MonoBehaviour validator;

    [Tooltip("Trigger of the order container; packages overlapping it ride along.")]
    [SerializeField] private BoxCollider containerTrigger;

    [Tooltip("World offset of the whole slide (metres).")]
    [SerializeField] private Vector3 travel = new(0f, 0f, 0.9f);

    [Tooltip("Duration of the slide in seconds (decision 011: about 2 s).")]
    [SerializeField] private float duration = 2f;

    [Tooltip("Optional bodies that must move with the container, e.g. the EKZ snap places of #41.")]
    [SerializeField] private Rigidbody[] carriedWithContainer;

    private IOrderCompletion completion;
    private Coroutine slide;

    private void OnEnable()
    {
        completion = validator as IOrderCompletion;

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

            if (rider == transform || rider.IsChildOf(transform) || riders.Contains(rider) || IsHeld(rider))
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

    // A package in the trainee's hand at confirm time stays in the hand (a snap place is not a hand).
    private static bool IsHeld(Transform rider)
    {
        foreach (MonoBehaviour behaviour in rider.GetComponentsInChildren<MonoBehaviour>())
        {
            if (behaviour is IInteractable interactable && interactable.State == InteractableState.Select)
                return true;
        }

        return false;
    }

    private IEnumerator Slide(List<Transform> riders)
    {
        float elapsed = 0f;
        float done = 0f;

        while (done < 1f)
        {
            yield return new WaitForFixedUpdate();

            elapsed += Time.fixedDeltaTime;
            float next = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            Vector3 step = travel * (next - done);
            done = next;

            // Container and riders move by the same step before the next physics step: no trigger exit.
            transform.position += step;
            foreach (Transform rider in riders)
            {
                if (rider == null)
                    continue;

                Vector3 target = rider.position + step;
                rider.position = target;
                if (rider.TryGetComponent(out Rigidbody body))
                {
                    body.position = target;
                }
            }
        }

        slide = null;
    }
}
