using System;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

/// <summary>
/// #100, docs/decisions/030: the ConceptScene station reacts to what the trainee puts into its two containers. Ist on
/// the sample WMS counts the sample material lying in the order container; a package of another material in either
/// container, or one that brings Ist above Soll, gets a yellow tag beside it saying what is wrong and to take it out.
/// One tag at a time, on the container's front wall right in front of the package, never a target number. The rule is ConceptBoxHintRule; this script only finds the packages.
/// Packages are kinematic and keep where they are let go, so the containers are read with Physics.OverlapBox over
/// their inside (zone colliders stay disabled) instead of trigger events; a package switched off or riding with its
/// colliders off is simply no longer found.
/// </summary>
public class ConceptBoxHints : MonoBehaviour
{
    // Loc table UI (#54); the order texts are the ones of the practice scenes' feedback (decision 030 item 2).
    private const string OrderWrongMaterialKey = "order.feedback.wrong_material";
    private const string OrderTooManyKey = "order.feedback.too_many";
    private const string StorageWrongMaterialKey = "concept.hint.storage_wrong_material";

    [SerializeField] private SampleWmsOrder order;

    [Tooltip("For tour_step_id in the log.")]
    [SerializeField] private WmsFieldTour tour;

    [Header("Containers (disabled box colliders spanning each container's inside)")]
    [SerializeField] private BoxCollider orderZone;
    [SerializeField] private BoxCollider storageZone;

    [Header("Tag")]
    [Tooltip("Inactive world-space canvas with one TMP text; shown beside the package with the hint.")]
    [SerializeField] private RectTransform tagTemplate;

    [Tooltip("Eyes; empty = Camera.main.")]
    [SerializeField] private Transform head;

    [Tooltip("Gap between the container's front wall and the tag, in metres.")]
    [SerializeField] private float gap = 0.02f;

    [Tooltip("Height of the containers' front wall above the zone's bottom, in metres; the tag's top edge is level with it.")]
    [SerializeField] private float frontWallHeight = 0.19f;

    private readonly Collider[] overlaps = new Collider[128];
    private readonly List<Package> packages = new();
    private readonly Dictionary<PackageData, Package> byData = new();
    private readonly Dictionary<Grabbable, HashSet<int>> grabPointers = new();
    private readonly List<(Grabbable grabbable, Action<PointerEvent> handler)> grabHandlers = new();
    private readonly List<LyingPackage> lying = new();
    private readonly HashSet<Package> found = new();

    private ConceptBoxHintRule rule;
    private ConceptHint? shown;
    private TMP_Text tagText;

    /// <summary>No package of another material lies in either container, so the lane demo may run.</summary>
    public bool NoWrongMaterial => rule == null || rule.NoWrongMaterial;

    /// <summary>No hint is shown and none is waiting to settle.</summary>
    public bool IsClear => NoWrongMaterial && (rule == null || !rule.Current.HasValue);

    /// <summary>
    /// The sample-material packages lying in the storage container (they ride with it), each with the pose it had
    /// when the scene started.
    /// </summary>
    public List<(Transform root, Vector3 startPosition, Quaternion startRotation)> SampleInStorage()
    {
        var riders = new List<(Transform, Vector3, Quaternion)>();
        if (rule == null)
            return riders;

        foreach (int id in rule.SettledSampleIn(ConceptBox.Storage))
        {
            Package package = packages[id];
            riders.Add((package.Root, package.StartPosition, package.StartRotation));
        }

        return riders;
    }

    private void Awake()
    {
        foreach (PackageData data in FindObjectsByType<PackageData>(FindObjectsInactive.Include))
        {
            Grabbable own = data.GetComponentInParent<Grabbable>(true);
            if (own == null || string.IsNullOrEmpty(data.MaterialKey))
                continue;

            var package = new Package(packages.Count, data, own);
            packages.Add(package);
            byData[data] = package;

            foreach (Grabbable grabbable in package.Holders)
            {
                if (grabPointers.ContainsKey(grabbable))
                    continue;

                grabPointers[grabbable] = new HashSet<int>();
                Grabbable target = grabbable;
                grabHandlers.Add((grabbable, evt => HandlePointerEvent(target, evt)));
            }
        }
    }

    private void OnEnable()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in grabHandlers)
        {
            grabbable.WhenPointerEventRaised += handler;
        }
    }

    private void OnDisable()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in grabHandlers)
        {
            if (grabbable != null)
            {
                grabbable.WhenPointerEventRaised -= handler;
            }
        }

        // No log here: during a scene change it would land in the next visit.
        shown = null;
        if (tagTemplate != null)
        {
            tagTemplate.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        bool missing = RequiredReference.IsMissing(order, nameof(order), this)
                       | RequiredReference.IsMissing(orderZone, nameof(orderZone), this)
                       | RequiredReference.IsMissing(storageZone, nameof(storageZone), this)
                       | RequiredReference.IsMissing(tagTemplate, nameof(tagTemplate), this);
        if (missing)
        {
            enabled = false;
            return;
        }

        rule = new ConceptBoxHintRule(order.CurrentLine.materialKey, order.CurrentLine.soll);
        tagText = tagTemplate.GetComponentInChildren<TMP_Text>(true);
        tagTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (rule == null)
            return;

        lying.Clear();
        Collect(orderZone, ConceptBox.Order);
        Collect(storageZone, ConceptBox.Storage);

        rule.Update(Time.timeAsDouble, lying, order.ShownPieces);
        order.SetPlacedPieces(rule.PlacedPieces);
        ShowHint(rule.Current);
    }

    private void LateUpdate()
    {
        if (!shown.HasValue)
            return;

        Transform eyes = head != null ? head : Camera.main != null ? Camera.main.transform : null;
        if (eyes == null)
            return;

        Package package = packages[shown.Value.PackageId];
        BoxCollider zone = shown.Value.Box == ConceptBox.Order ? orderZone : storageZone;
        PlaceOnFrontWall(zone, package.Bounds().center, eyes.position);
    }

    // On the container's front wall, right in front of the package, facing the eyes. Beside the package the tag would
    // be hidden: from the standing spot the tour bar covers the inner half of both containers and the Menu the space
    // right of the order container, while the front wall is always in view (a package in a container is seen over it).
    private void PlaceOnFrontWall(BoxCollider zone, Vector3 package, Vector3 eyes)
    {
        Transform t = zone.transform;
        Vector3 local = t.InverseTransformPoint(package);
        Vector3 half = zone.size * 0.5f;
        float tagHalf = tagTemplate.rect.width * tagTemplate.lossyScale.x * 0.5f / Mathf.Max(t.lossyScale.x, 1e-4f);
        float tagHeight = tagTemplate.rect.height * tagTemplate.lossyScale.y / Mathf.Max(t.lossyScale.y, 1e-4f);
        float frontGap = gap / Mathf.Max(t.lossyScale.z, 1e-4f);

        local.x = Mathf.Clamp(local.x, zone.center.x - half.x + tagHalf, zone.center.x + half.x - tagHalf);
        local.y = zone.center.y - half.y + frontWallHeight / Mathf.Max(t.lossyScale.y, 1e-4f) - tagHeight * 0.5f;
        local.z = zone.center.z - half.z - frontGap;

        Vector3 position = t.TransformPoint(local);
        tagTemplate.SetPositionAndRotation(position, Quaternion.LookRotation(position - eyes, Vector3.up));
    }

    // Packages inside the zone and not held; a package inside another lying package (a ring in its opened carton)
    // moves with it and is left to its carton.
    private void Collect(BoxCollider zone, ConceptBox box)
    {
        if (zone == null || !zone.gameObject.activeInHierarchy)
            return;

        Transform t = zone.transform;
        Vector3 center = t.TransformPoint(zone.center);
        Vector3 halfExtents = Vector3.Scale(zone.size, t.lossyScale) * 0.5f;
        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, overlaps, t.rotation, ~0,
            QueryTriggerInteraction.Collide);

        found.Clear();
        for (int i = 0; i < count; i++)
        {
            PackageData data = overlaps[i].GetComponentInParent<PackageData>();
            if (data != null && byData.TryGetValue(data, out Package package) && !IsHeld(package))
            {
                found.Add(package);
            }
        }

        foreach (Package package in found)
        {
            if (package.Parent != null && found.Contains(byData.GetValueOrDefault(package.Parent)))
                continue;

            PackageData data = package.Data;
            lying.Add(new LyingPackage(package.Id, box, data.MaterialKey, OrderContents.Form(data.packageType),
                data.Vpe));
        }
    }

    private bool IsHeld(Package package)
    {
        foreach (Grabbable grabbable in package.Holders)
        {
            if (grabPointers[grabbable].Count > 0)
                return true;
        }

        return false;
    }

    private void HandlePointerEvent(Grabbable grabbable, PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                grabPointers[grabbable].Add(evt.Identifier);
                break;
            case PointerEventType.Unselect:
            case PointerEventType.Cancel:
                grabPointers[grabbable].Remove(evt.Identifier);
                break;
        }
    }

    private void ShowHint(ConceptHint? hint)
    {
        if (Same(hint, shown))
            return;

        if (shown.HasValue)
        {
            Log(false, shown.Value);
        }

        shown = hint;
        tagTemplate.gameObject.SetActive(hint.HasValue);
        if (!hint.HasValue)
            return;

        tagText.text = TextOf(hint.Value);
        Log(true, hint.Value);
        LateUpdate();
    }

    private string TextOf(ConceptHint hint)
    {
        if (hint.Box == ConceptBox.Storage)
            return Loc.Get(StorageWrongMaterialKey);

        return hint.Reason == ConceptHintReason.TooMany
            ? Loc.Get(OrderTooManyKey, ("name", order.CurrentLine.materialName))
            : Loc.Get(OrderWrongMaterialKey);
    }

    private void Log(bool isShown, ConceptHint hint)
    {
        PackageData data = packages[hint.PackageId].Data;
        ResearchLogger.LogConceptHint(isShown,
            hint.Box == ConceptBox.Order ? InteractionEvents.ContainerOrder : InteractionEvents.ContainerStorage,
            hint.Reason == ConceptHintReason.TooMany ? InteractionEvents.HintTooMany : InteractionEvents.HintWrongMaterial,
            ResearchObjectId.Of(data), data.MaterialKey, tour != null ? tour.CurrentStepId : null);
    }

    private static bool Same(ConceptHint? a, ConceptHint? b)
    {
        if (a.HasValue != b.HasValue)
            return false;

        return !a.HasValue || (a.Value.PackageId == b.Value.PackageId && a.Value.Box == b.Value.Box
                               && a.Value.Reason == b.Value.Reason);
    }

    private sealed class Package
    {
        private readonly Renderer[] renderers;

        public Package(int id, PackageData data, Grabbable own)
        {
            Id = id;
            Data = data;
            Root = own.transform;
            StartPosition = Root.position;
            StartRotation = Root.rotation;
            Holders = data.GetComponentsInParent<Grabbable>(true);
            Parent = data.transform.parent != null ? data.transform.parent.GetComponentInParent<PackageData>(true) : null;

            // Only this package's own renderers: a nested package (a ring in an opened carton) has its own.
            var list = new List<Renderer>();
            foreach (Renderer renderer in own.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponentInParent<Grabbable>(true) == own)
                {
                    list.Add(renderer);
                }
            }
            renderers = list.ToArray();
        }

        public int Id { get; }

        public PackageData Data { get; }

        // The grabbable object that moves; rides with the storage container.
        public Transform Root { get; }

        public Vector3 StartPosition { get; }

        public Quaternion StartRotation { get; }

        // This package's grabbable and those of the packages around it (holding the carton holds its rings).
        public Grabbable[] Holders { get; }

        public PackageData Parent { get; }

        public Bounds Bounds() => TagPlacement.Of(renderers, Root.position);
    }
}
