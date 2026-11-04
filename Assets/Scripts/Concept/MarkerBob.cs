using UnityEngine;

/// <summary>
/// #90, docs/decisions/026 (2): a marker arrow bobs gently up and back while it is shown, so the eye finds it.
/// It only rises above its scene position (the tip never sinks into the object below) and returns there when hidden.
/// </summary>
public class MarkerBob : MonoBehaviour
{
    [Tooltip("Highest rise above the scene position, in metres.")]
    [SerializeField] private float height = 0.015f;

    [Tooltip("Seconds for one up-and-down.")]
    [SerializeField] private float period = 1.2f;

    private Vector3 rest;
    private float startTime;

    private void OnEnable()
    {
        rest = transform.localPosition;
        startTime = Time.time;
    }

    private void OnDisable()
    {
        transform.localPosition = rest;
    }

    private void Update()
    {
        float phase = (Time.time - startTime) / Mathf.Max(period, 0.01f) * 2f * Mathf.PI;
        float rise = height * 0.5f * (1f - Mathf.Cos(phase));
        transform.localPosition = rest + Vector3.up * rise;
    }
}
