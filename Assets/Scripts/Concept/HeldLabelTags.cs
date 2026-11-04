using System;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

/// <summary>
/// #90, docs/decisions/026 (2): while the trainee holds a package, a small tag next to it explains its label
/// (Mat.-Nr and VPE from its PackageData). The tag sits beside the package as seen from the head, so it never covers
/// the label, and faces the eyes; two packages get two tags, one on the outer side of each. Releasing hides the tag.
/// The tags only read the packages' transforms. Replaces the label board Info Panel (inactive since #89).
/// </summary>
public class HeldLabelTags : MonoBehaviour
{
    // Loc table UI (#54).
    private const string MatNrKey = "concept.label.tag.matnr";
    private const string VpeKey = "concept.label.tag.vpe";

    [Tooltip("Inactive world-space canvas with one TMP text; cloned once per held package.")]
    [SerializeField] private RectTransform tagTemplate;

    [Tooltip("Eyes; empty = Camera.main.")]
    [SerializeField] private Transform head;

    [Tooltip("Gap between the package's outline and the tag, in metres.")]
    [SerializeField] private float gap = 0.02f;

    private readonly List<(Grabbable grabbable, Action<PointerEvent> handler)> packageGrabs = new();
    private readonly Dictionary<int, HeldPackage> packages = new();
    private readonly List<RectTransform> tags = new();
    private readonly HeldPackageTracker tracker = new();

    // Raised when a package starts or stops being held.
    public event Action HeldChanged;

    public bool IsMaterialHeld(string materialKey) => tracker.IsMaterialHeld(materialKey);

    private void Awake()
    {
        foreach (Grabbable grabbable in FindObjectsByType<Grabbable>(FindObjectsInactive.Include))
        {
            PackageData package = grabbable.GetComponentInParent<PackageData>(true);
            if (package == null || string.IsNullOrEmpty(package.MaterialKey))
                continue;

            var held = new HeldPackage(packages.Count, grabbable, package);
            packages[held.Id] = held;
            packageGrabs.Add((grabbable, evt => HandlePointerEvent(evt, held)));
        }
    }

    private void OnEnable()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in packageGrabs)
        {
            grabbable.WhenPointerEventRaised += handler;
        }
    }

    private void OnDisable()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in packageGrabs)
        {
            if (grabbable != null)
            {
                grabbable.WhenPointerEventRaised -= handler;
            }
        }
    }

    private void Start()
    {
        if (RequiredReference.IsMissing(tagTemplate, nameof(tagTemplate), this))
            return;

        tagTemplate.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        IReadOnlyList<int> held = tracker.Held;
        Transform eyes = head != null ? head : Camera.main != null ? Camera.main.transform : null;
        if (tagTemplate == null || eyes == null)
            return;

        EnsureTagCount(held.Count);
        if (held.Count == 0)
            return;

        var sideways = new float[held.Count];
        var bounds = new Bounds[held.Count];
        for (int i = 0; i < held.Count; i++)
        {
            bounds[i] = packages[held[i]].Bounds();
            sideways[i] = eyes.InverseTransformPoint(bounds[i].center).x;
        }

        int[] sides = HeldPackageTracker.TagSides(sideways);
        for (int i = 0; i < held.Count; i++)
        {
            PlaceTag(tags[i], packages[held[i]], bounds[i], sides[i], eyes.position);
        }
    }

    private void HandlePointerEvent(PointerEvent evt, HeldPackage package)
    {
        int packageId = package.Id;
        bool changed = evt.Type switch
        {
            PointerEventType.Select => tracker.Select(packageId, evt.Identifier, package.Data.MaterialKey),
            PointerEventType.Unselect or PointerEventType.Cancel => tracker.Release(packageId, evt.Identifier),
            _ => false
        };

        if (changed)
        {
            HeldChanged?.Invoke();
        }
    }

    private void EnsureTagCount(int count)
    {
        while (tags.Count < count)
        {
            RectTransform tag = Instantiate(tagTemplate, tagTemplate.parent);
            tag.name = $"{tagTemplate.name} {tags.Count + 1}";
            tags.Add(tag);
        }

        for (int i = 0; i < tags.Count; i++)
        {
            bool shown = i < count;
            if (tags[i].gameObject.activeSelf != shown)
            {
                tags[i].gameObject.SetActive(shown);
            }
        }
    }

    private void PlaceTag(RectTransform tag, HeldPackage package, Bounds bounds, int side, Vector3 eyes)
    {
        Vector3 toPackage = bounds.center - eyes;
        Vector3 right = Vector3.Cross(Vector3.up, toPackage).normalized;
        if (right == Vector3.zero)
        {
            right = Vector3.right;
        }

        // Half the package's width as seen from the head, plus the gap and half the tag.
        Vector3 e = bounds.extents;
        float packageHalf = Mathf.Abs(e.x * right.x) + Mathf.Abs(e.y * right.y) + Mathf.Abs(e.z * right.z);
        float tagHalf = tag.rect.width * tag.lossyScale.x * 0.5f;

        Vector3 position = bounds.center + right * (side * (packageHalf + gap + tagHalf));
        tag.SetPositionAndRotation(position, Quaternion.LookRotation(position - eyes, Vector3.up));

        if (package.ShownOn != tag)
        {
            package.ShownOn = tag;
            tag.GetComponentInChildren<TMP_Text>(true).text =
                Loc.Get(MatNrKey, ("matnr", package.Data.MaterialNumber)) + "\n" +
                Loc.Get(VpeKey, ("vpe", package.Data.Vpe));
            ForgetOthers(package, tag);
        }
    }

    // A tag shows one package; another package that showed on it must rewrite the text next time.
    private void ForgetOthers(HeldPackage owner, RectTransform tag)
    {
        foreach (HeldPackage other in packages.Values)
        {
            if (other != owner && other.ShownOn == tag)
            {
                other.ShownOn = null;
            }
        }
    }

    private sealed class HeldPackage
    {
        private readonly Renderer[] renderers;

        public HeldPackage(int id, Grabbable grabbable, PackageData data)
        {
            Id = id;
            Grabbable = grabbable;
            Data = data;

            // Only this package's own renderers: a nested grabbable (a ring in an opened carton) has its own tag.
            var own = new List<Renderer>();
            foreach (Renderer renderer in grabbable.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponentInParent<Grabbable>(true) == grabbable)
                {
                    own.Add(renderer);
                }
            }
            renderers = own.ToArray();
        }

        public int Id { get; }

        public Grabbable Grabbable { get; }

        public PackageData Data { get; }

        public RectTransform ShownOn { get; set; }

        public Bounds Bounds()
        {
            bool found = false;
            var bounds = new Bounds(Grabbable.transform.position, Vector3.zero);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;

                if (found)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    bounds = renderer.bounds;
                    found = true;
                }
            }

            return bounds;
        }
    }
}
