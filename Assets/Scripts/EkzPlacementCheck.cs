using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #41, docs/decisions/003: EKZ 3 in BasicPickingScene (#94, decision 028: also Mixed position 1). Each carton in the order container must sit in its
/// own snap place (one by one, aligned). Tracks which cartons are snapped from the SnapInteractable events;
/// the validator asks only when the trainee confirms. Quantity counting never reads this.
/// #108, decision 037 (1, 3): each place gets a thin outline on the container floor, inside the carton footprint, so a
/// filled place no longer looks empty. Basic shows them from the start (showPlacesAtStart); Mixed position 1 only after
/// its first placement error (ShowPlaces). After a placement error every loose carton gets a yellow warning mark
/// (MarkLoose) until the validator clears its result (ClearMarks).
/// </summary>
public class EkzPlacementCheck : MonoBehaviour
{
    [Tooltip("The snap places inside the order container, one per carton of the order.")]
    [SerializeField] private SnapInteractable[] places;

    [Tooltip("Optional: played when a carton snaps into a place (the signal must not be colour only).")]
    [SerializeField] private AudioSource snapSound;

    [Header("Place outlines (#108)")]
    [Tooltip("Basic: outlines visible from the start. Mixed: hidden until the first placement error (ShowPlaces).")]
    [SerializeField] private bool showPlacesAtStart = true;

    [Tooltip("Material of the outline strokes.")]
    [SerializeField] private Material outlineMaterial;

    [Tooltip("Outer size of an outline (x, z) in metres; a little smaller than the carton footprint so a carton covers it.")]
    [SerializeField] private Vector2 outlineSize = new(0.166f, 0.111f);

    [SerializeField] private float outlineStroke = 0.008f;

    [Tooltip("How far below the snap pose the container floor lies (the snap pose holds the carton slightly above it).")]
    [SerializeField] private float outlineDrop = 0.022f;

    [Header("Loose carton mark (#108)")]
    [Tooltip("Meta warning icon shown above each loose carton after a placement error.")]
    [SerializeField] private Sprite warningSprite;

    [SerializeField] private Color warningColor = new(1f, 0.82f, 0.1f, 1f);

    [Tooltip("Width of the mark in metres.")]
    [SerializeField] private float markSize = 0.05f;

    private const float OutlineHeight = 0.002f;
    private const float OutlineLift = 0.001f;
    private const float MarkLift = 0.045f;

    private readonly List<GameObject> outlines = new();
    private readonly List<(GameObject mark, PackageData package)> marks = new();

    // #94, decision 028: Empf Kz 003 (EKZ 3) is the only class rated by placement.
    private const string OrderedEmpfKz = "003";

    private readonly HashSet<PackageData> snapped = new();

    /// <summary>True when the order line asks for every carton in its own place (Empf Kz 003).</summary>
    public static bool NeedsPlaces(OrderLine line) => line != null && line.empfKz == OrderedEmpfKz;

    // Raised when a carton snaps into or leaves a place, so an old result can be cleared (#4).
    public event Action PlacementChanged;

    private void OnEnable()
    {
        if (places == null || places.Length == 0)
        {
            Debug.LogError("EkzPlacementCheck: places is not assigned.", this);
            return;
        }

        foreach (SnapInteractable place in places)
        {
            if (place == null)
                continue;

            place.WhenSelectingInteractorViewAdded += HandleSnapped;
            place.WhenSelectingInteractorViewRemoved += HandleUnsnapped;

            // Cartons already in a place when this component is enabled (no event for them).
            foreach (IInteractorView interactor in place.SelectingInteractorViews)
            {
                PackageData package = PackageOf(interactor);

                if (package != null)
                {
                    snapped.Add(package);
                }
            }
        }
    }

    private void Start()
    {
        ShowPlaces(showPlacesAtStart);
    }

    public bool PlacesShown { get; private set; }

    /// <summary>#108: shows or hides the outlines of all places (built on first use, children of the places).</summary>
    public void ShowPlaces(bool show)
    {
        if (show && outlines.Count == 0)
        {
            BuildOutlines();
        }

        PlacesShown = show;
        foreach (GameObject outline in outlines)
        {
            if (outline != null)
            {
                outline.SetActive(show);
            }
        }
    }

    /// <summary>The active packages of the list that do not sit in a snap place.</summary>
    public List<PackageData> LoosePackages(IEnumerable<PackageData> packages)
    {
        var loose = new List<PackageData>();
        foreach (PackageData package in packages)
        {
            if (package != null && package.isActiveAndEnabled && !snapped.Contains(package))
            {
                loose.Add(package);
            }
        }

        return loose;
    }

    /// <summary>#108: a yellow warning mark above every loose carton; replaces earlier marks.</summary>
    public void MarkLoose(IEnumerable<PackageData> packages)
    {
        ClearMarks();
        if (warningSprite == null)
        {
            Debug.LogError("EkzPlacementCheck: warningSprite is not assigned; loose cartons get no mark.", this);
            return;
        }

        foreach (PackageData package in LoosePackages(packages))
        {
            marks.Add((BuildMark(package), package));
        }
    }

    public void ClearMarks()
    {
        foreach ((GameObject mark, PackageData _) in marks)
        {
            if (mark != null)
            {
                Destroy(mark);
            }
        }
        marks.Clear();
    }

    public int MarkCount => marks.Count;

    private void LateUpdate()
    {
        if (marks.Count == 0)
            return;

        // Each mark stays above its carton (it may be lifted or laid flat again) and faces the eyes.
        Camera eyes = Camera.main;
        foreach ((GameObject mark, PackageData package) in marks)
        {
            if (mark == null || package == null)
                continue;

            mark.transform.position = MarkPosition(package);
            if (eyes != null)
            {
                mark.transform.rotation = Quaternion.LookRotation(mark.transform.position - eyes.transform.position, Vector3.up);
            }
        }
    }

    private void BuildOutlines()
    {
        if (outlineMaterial == null)
        {
            Debug.LogError("EkzPlacementCheck: outlineMaterial is not assigned; no place outlines.", this);
            return;
        }

        foreach (SnapInteractable place in places)
        {
            if (place == null)
                continue;

            var root = new GameObject("Place Outline");
            root.transform.SetParent(place.transform, false);
            // The snap pose is the carton's bottom centre, a little above the floor; the outline lies on the floor.
            root.transform.SetPositionAndRotation(place.transform.position + Vector3.up * (OutlineLift - outlineDrop),
                Quaternion.Euler(0f, place.transform.eulerAngles.y, 0f));

            float halfX = outlineSize.x / 2f, halfZ = outlineSize.y / 2f, halfStroke = outlineStroke / 2f;
            AddStroke(root.transform, new Vector3(0f, 0f, halfZ - halfStroke), new Vector3(outlineSize.x, OutlineHeight, outlineStroke));
            AddStroke(root.transform, new Vector3(0f, 0f, -halfZ + halfStroke), new Vector3(outlineSize.x, OutlineHeight, outlineStroke));
            AddStroke(root.transform, new Vector3(halfX - halfStroke, 0f, 0f), new Vector3(outlineStroke, OutlineHeight, outlineSize.y));
            AddStroke(root.transform, new Vector3(-halfX + halfStroke, 0f, 0f), new Vector3(outlineStroke, OutlineHeight, outlineSize.y));
            outlines.Add(root);
        }
    }

    private void AddStroke(Transform parent, Vector3 localPosition, Vector3 worldSize)
    {
        GameObject stroke = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stroke.name = "Stroke";
        Destroy(stroke.GetComponent<Collider>());
        stroke.transform.SetParent(parent, false);
        // The place may be scaled; the stroke keeps its size in metres.
        Vector3 parentScale = parent.lossyScale;
        stroke.transform.localPosition = new Vector3(localPosition.x / parentScale.x, localPosition.y / parentScale.y, localPosition.z / parentScale.z);
        stroke.transform.localScale = new Vector3(worldSize.x / parentScale.x, worldSize.y / parentScale.y, worldSize.z / parentScale.z);
        var meshRenderer = stroke.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = outlineMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private GameObject BuildMark(PackageData package)
    {
        var mark = new GameObject("Loose Carton Mark", typeof(RectTransform), typeof(Canvas));
        var rect = (RectTransform)mark.transform;
        rect.sizeDelta = new Vector2(100f, 100f);
        rect.localScale = Vector3.one * (markSize / 100f);
        mark.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(mark.transform, false);
        var iconRect = (RectTransform)icon.transform;
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
        var image = icon.GetComponent<Image>();
        image.sprite = warningSprite;
        image.color = warningColor;
        image.preserveAspect = true;
        image.raycastTarget = false;

        mark.transform.position = MarkPosition(package);
        return mark;
    }

    // Above the carton's top, so it never covers the label of a neighbour lying lower.
    private static Vector3 MarkPosition(PackageData package)
    {
        Bounds bounds = BoundsOf(package);
        return new Vector3(bounds.center.x, bounds.max.y + MarkLift, bounds.center.z);
    }

    private static Bounds BoundsOf(PackageData package)
    {
        var bounds = new Bounds(package.transform.position, Vector3.zero);
        bool first = true;
        foreach (MeshRenderer meshRenderer in package.GetComponentsInChildren<MeshRenderer>())
        {
            if (!meshRenderer.enabled)
                continue;

            if (first)
            {
                bounds = meshRenderer.bounds;
                first = false;
            }
            else
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }
        }

        return bounds;
    }

    private void OnDisable()
    {
        if (places != null)
        {
            foreach (SnapInteractable place in places)
            {
                if (place == null)
                    continue;

                place.WhenSelectingInteractorViewAdded -= HandleSnapped;
                place.WhenSelectingInteractorViewRemoved -= HandleUnsnapped;
            }
        }

        snapped.Clear();
        ClearMarks();
    }

    // True when every active package sits in a snap place. Destroyed or disabled cartons are not in the
    // container any more and are skipped (CalcForWMS gets no OnTriggerExit for them).
    public bool AllPlaced(IEnumerable<PackageData> packages)
    {
        foreach (PackageData package in packages)
        {
            if (package == null || !package.isActiveAndEnabled)
                continue;

            if (!snapped.Contains(package))
                return false;
        }

        return true;
    }

    private void HandleSnapped(IInteractorView interactor)
    {
        PackageData package = PackageOf(interactor);

        if (package == null || !snapped.Add(package))
            return;

        if (snapSound != null)
        {
            snapSound.Play();
        }

        PlacementChanged?.Invoke();
    }

    private void HandleUnsnapped(IInteractorView interactor)
    {
        PackageData package = PackageOf(interactor);

        if (package != null && snapped.Remove(package))
        {
            PlacementChanged?.Invoke();
        }
    }

    // The SnapInteractor sits on a child of the carton that holds PackageData.
    private static PackageData PackageOf(IInteractorView interactor)
    {
        return interactor is Component component ? component.GetComponentInParent<PackageData>() : null;
    }
}
