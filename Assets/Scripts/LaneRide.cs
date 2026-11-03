using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #47/#71, docs/decisions/014 and 019: moves a container and the packages riding in it along the conveyor:
/// straight legs with ease in/out, no rotation. Container and riders move by the same step before each physics
/// step, so nothing leaves or enters a trigger on the way.
/// </summary>
public static class LaneRide
{
    // Rides the given world offsets from the start position in order and waits cornerPause between legs.
    public static IEnumerator Path(Transform container, IReadOnlyList<Transform> riders, IReadOnlyList<Vector3> path,
        float speed, float cornerPause)
    {
        Vector3 previous = Vector3.zero;

        for (int i = 0; i < path.Count; i++)
        {
            if (i > 0 && cornerPause > 0f)
            {
                yield return new WaitForSeconds(cornerPause);
            }

            yield return Leg(container, riders, path[i] - previous, speed);
            previous = path[i];
        }
    }

    public static IEnumerator Leg(Transform container, IReadOnlyList<Transform> riders, Vector3 travel, float speed)
    {
        float duration = speed > 0f ? travel.magnitude / speed : 0f;
        float elapsed = 0f;
        float done = 0f;

        while (done < 1f)
        {
            yield return new WaitForFixedUpdate();

            elapsed += Time.fixedDeltaTime;
            float next = duration > 0f ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration)) : 1f;
            Move(container, riders, travel * (next - done));
            done = next;
        }
    }

    // Moves container and riders at once, e.g. to finish a ride when its owner is disabled.
    public static void Move(Transform container, IReadOnlyList<Transform> riders, Vector3 step)
    {
        if (container != null)
        {
            container.position += step;
        }

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

    // A package in the trainee's hand stays in the hand (a snap place is not a hand).
    public static bool IsHeld(Transform rider)
    {
        foreach (MonoBehaviour behaviour in rider.GetComponentsInChildren<MonoBehaviour>())
        {
            if (behaviour is IInteractable interactable && interactable.State == InteractableState.Select)
                return true;
        }

        return false;
    }
}
