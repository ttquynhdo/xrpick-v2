using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// #21, docs/decisions/012: one storage box per position, all at the same place. Only the box of the current
/// position is shown; when the container advances, the old box shrinks away and the next one grows in.
/// Packages already in the order container stay where they are.
/// </summary>
public class PositionStockSwitcher : MonoBehaviour
{
    // Decision 012 asks for a short fade; the box and package materials are opaque, so the switch scales instead.
    private const float TransitionSeconds = 0.35f;
    private const float HiddenScale = 0.01f;

    [Serializable]
    private class StockSet
    {
        [Tooltip("Storage box of this position.")]
        public GameObject box;

        [Tooltip("Packages lying in this box (they are not children of it).")]
        public List<PackageData> packages = new();
    }

    [SerializeField] private MixedPickingContainer container;

    [Tooltip("One set per order line, in the same order as the container's Order Lines.")]
    [SerializeField] private List<StockSet> sets = new();

    // State of a running switch, so OnDisable can finish it.
    private Dictionary<GameObject, Vector3> leavingScales;
    private Dictionary<GameObject, Vector3> comingScales;
    private Dictionary<Rigidbody, bool> wasKinematic;

    private void Awake()
    {
        for (int i = 0; i < sets.Count; i++)
        {
            SetVisible(sets[i], i == 0);
        }
    }

    private void OnEnable()
    {
        if (container != null)
        {
            container.LineAdvanced += OnLineAdvanced;
        }
        else
        {
            Debug.LogError("PositionStockSwitcher: container is not assigned.", this);
        }
    }

    private void OnDisable()
    {
        if (container != null)
        {
            container.LineAdvanced -= OnLineAdvanced;
        }

        // A disabled object stops its coroutines: never leave boxes tiny or packages stuck kinematic.
        FinishSwitch();
    }

    private void Start()
    {
        if (container != null && sets.Count != container.OrderLines.Count)
        {
            Debug.LogError($"PositionStockSwitcher: {sets.Count} stock sets but {container.OrderLines.Count} order lines.", this);
        }
    }

    private void OnLineAdvanced(int position)
    {
        int next = position - 1;

        if (next <= 0 || next >= sets.Count)
            return;

        StopAllCoroutines();
        FinishSwitch();

        List<GameObject> leaving = Members(sets[next - 1], true);
        List<GameObject> coming = Members(sets[next], false);

        // Activate now (tiny, still): the container computes Rest right after this event from the active packages.
        Dictionary<GameObject, Vector3> comingScale = Freeze(coming);
        ApplyScale(comingScale, HiddenScale);
        foreach (GameObject member in coming)
        {
            member.SetActive(true);
        }

        StartCoroutine(Switch(leaving, comingScale));
    }

    // Packages already in the order container belong to the order now, not to the box.
    private List<GameObject> Members(StockSet set, bool skipPicked)
    {
        var members = new List<GameObject>();

        if (set.box != null)
        {
            members.Add(set.box);
        }

        foreach (PackageData package in set.packages)
        {
            if (package == null || (skipPicked && container.Contains(package)))
                continue;

            members.Add(package.gameObject);
        }

        return members;
    }

    private IEnumerator Switch(List<GameObject> leaving, Dictionary<GameObject, Vector3> comingScale)
    {
        leavingScales = Freeze(leaving);
        comingScales = comingScale;
        wasKinematic = SetKinematic(leaving, new List<GameObject>(comingScale.Keys));

        yield return Scale(leavingScales, 1f, HiddenScale);

        foreach (KeyValuePair<GameObject, Vector3> entry in leavingScales)
        {
            entry.Key.SetActive(false);
            entry.Key.transform.localScale = entry.Value;
        }
        leavingScales = null;

        yield return Scale(comingScales, HiddenScale, 1f);

        FinishSwitch();
    }

    // Ends a running switch at once: old box hidden, new box at full size, kinematic flags restored.
    private void FinishSwitch()
    {
        if (leavingScales != null)
        {
            foreach (KeyValuePair<GameObject, Vector3> entry in leavingScales)
            {
                if (entry.Key == null)
                    continue;
                entry.Key.SetActive(false);
                entry.Key.transform.localScale = entry.Value;
            }
            leavingScales = null;
        }

        if (comingScales != null)
        {
            foreach (KeyValuePair<GameObject, Vector3> entry in comingScales)
            {
                if (entry.Key != null)
                {
                    entry.Key.transform.localScale = entry.Value;
                }
            }
            comingScales = null;
        }

        if (wasKinematic != null)
        {
            foreach (KeyValuePair<Rigidbody, bool> entry in wasKinematic)
            {
                if (entry.Key != null)
                {
                    entry.Key.isKinematic = entry.Value;
                }
            }
            wasKinematic = null;
        }
    }

    private static Dictionary<GameObject, Vector3> Freeze(List<GameObject> members)
    {
        var scales = new Dictionary<GameObject, Vector3>();

        foreach (GameObject member in members)
        {
            scales[member] = member.transform.localScale;
        }

        return scales;
    }

    // Moving a scaled object with a dynamic Rigidbody would launch the packages; keep them still until shown.
    private static Dictionary<Rigidbody, bool> SetKinematic(List<GameObject> leaving, List<GameObject> coming)
    {
        var wasKinematic = new Dictionary<Rigidbody, bool>();

        foreach (List<GameObject> group in new[] { leaving, coming })
        {
            foreach (GameObject member in group)
            {
                foreach (Rigidbody body in member.GetComponentsInChildren<Rigidbody>())
                {
                    if (!wasKinematic.ContainsKey(body))
                    {
                        wasKinematic[body] = body.isKinematic;
                        body.isKinematic = true;
                    }
                }
            }
        }

        return wasKinematic;
    }

    private static IEnumerator Scale(Dictionary<GameObject, Vector3> scales, float from, float to)
    {
        for (float t = 0f; t < TransitionSeconds; t += Time.deltaTime)
        {
            float factor = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / TransitionSeconds));
            ApplyScale(scales, factor);
            yield return null;
        }

        ApplyScale(scales, to);
    }

    private static void ApplyScale(Dictionary<GameObject, Vector3> scales, float factor)
    {
        foreach (KeyValuePair<GameObject, Vector3> entry in scales)
        {
            entry.Key.transform.localScale = entry.Value * factor;
        }
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
}
