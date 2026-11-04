using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// #88, decision 029: one size per package kind in every scene and one fixed-size label on each carton lid.
/// Cartons: mesh SM_box_1 at the Basic carton size (Complex's CardboardBox06 cartons get the same mesh),
/// one label on the lid, the front sticker hidden (its collider stays: it is the carton's solid collider).
/// Bags: the Mixed bag 25 % larger, one label inside the bag outline facing the standing spot.
/// Every label uses the same font size with no auto-size. Opened cartons and loose pieces are not touched.
/// Check reports every deviation; StandardizeOpenScene fixes them in place (dry run only reports).
/// Running it again changes nothing new. Re-run after a label text or package size changes.
/// </summary>
public static class PackageStandardizer
{
    private const string CartonMeshName = "SM_box_1";
    private const string OldCartonMeshName = "CardboardBox06";
    private const string BagMeshName = "PlasticBag02";
    private const string CartonPrefabPath = "Assets/GVOZDY/Cardboard Boxes with Tape/Prefabs/box_1_brownTape.prefab";

    // Scale of the SM_box_1 mesh in BasicPickingScene = the standard carton (17.8 x 9.2 x 12.3 cm).
    private static readonly Vector3 CartonMeshScale = new(0.3f, 0.3f, 0.4f);
    // Standard bag = the Mixed bag (mesh scale 0.3) made 25 % larger, so a label with the carton font fits on it.
    private const float BagMeshScale = 0.375f;

    private const float SizeTolerance = 0.002f;
    private const float LidLabelShare = 0.6f;      // label covers 60 % of the lid (user, 2026-11-04)
    private const float TextShare = 0.84f;         // 8 % margin on each side of the label
    private const float TextWorldScale = 0.001f;   // 1 text unit = 1 mm
    // The bag is pear-shaped: its body is about 12.5 cm wide at mid height; lower down it bulges forward.
    private const float BagLabelWidth = 0.10f;
    private const float BagLabelHeight = 0.06f;
    private const float BagLabelHeightShare = 0.45f;
    private const float BagEdgeMargin = 0.005f;
    private const float LabelLift = 0.0015f;

    // The longest lines that must fit: carton names on the lid, the Mat.-Nr line on a bag label.
    private const string LongestCartonLine = "SCHLAUCHSCHELLE B";
    private const string LongestBagLine = "Mat.-Nr: 738190";

    [MenuItem("XRPick/Packages/Check open scene")]
    private static void CheckMenu()
    {
        Debug.Log(Check());
    }

    [MenuItem("XRPick/Packages/Standardize open scene (dry run)")]
    private static void DryRunMenu()
    {
        Debug.Log(StandardizeOpenScene(true));
    }

    [MenuItem("XRPick/Packages/Standardize open scene")]
    private static void StandardizeMenu()
    {
        Debug.Log(StandardizeOpenScene(false));
    }

    public static Vector3 CartonSize => Vector3.Scale(CartonMesh().bounds.size, CartonMeshScale);

    public static Vector3 BagSize(Mesh bagMesh) => bagMesh.bounds.size * BagMeshScale;

    /// <summary>Fixes size and labels of every carton and bag in the open scene. Returns a report.</summary>
    public static string StandardizeOpenScene(bool dryRun)
    {
        var report = new StringBuilder();
        Scene scene = SceneManager.GetActiveScene();
        report.AppendLine($"PackageStandardizer {(dryRun ? "dry run" : "apply")}: {scene.name}");

        Mesh cartonMesh = CartonMesh();
        Material[] cartonMaterials = CartonMaterials();
        float fontSize = LabelFontSize(scene);
        report.AppendLine($"font size {fontSize:F1} (carton size {CartonSize.ToString("F3")})");

        foreach (MeshFilter filter in Cartons(scene))
        {
            report.AppendLine("carton " + PathOf(filter.transform));
            if (dryRun)
                continue;

            if (filter.sharedMesh.name.StartsWith(OldCartonMeshName))
                SwapToCartonMesh(filter, cartonMesh, cartonMaterials);

            FitSize(filter, CartonSize);
            HideFrontSticker(filter.transform);
            PlaceLidLabel(filter, fontSize);
        }

        foreach (MeshFilter filter in Bags(scene))
        {
            report.AppendLine("bag " + PathOf(filter.transform));
            if (dryRun)
                continue;

            FitSize(filter, BagSize(filter.sharedMesh));
            PlaceBagLabel(filter, fontSize);
        }

        if (!dryRun)
            EditorSceneManager.MarkSceneDirty(scene);

        report.AppendLine(Check());
        return report.ToString();
    }

    /// <summary>Lists every carton or bag whose size, label or label text deviates. Empty list = OK.</summary>
    public static string Check()
    {
        Scene scene = SceneManager.GetActiveScene();
        var errors = new List<string>();
        float fontSize = LabelFontSize(scene);
        Vector3 cartonSize = CartonSize;
        int cartons = 0, bags = 0;

        foreach (MeshFilter filter in Cartons(scene))
        {
            cartons++;
            string path = PathOf(filter.transform);
            if (filter.sharedMesh.name != CartonMeshName)
                errors.Add($"{path}: mesh {filter.sharedMesh.name}, not {CartonMeshName}");

            CheckSize(errors, path, filter, cartonSize);
            if (Vector3.Angle(filter.transform.up, Vector3.up) > 1f)
                errors.Add($"{path}: lid not up");

            List<TMP_Text> visible = VisibleLabels(filter.transform);
            if (visible.Count != 1 || visible[0].transform.parent.name != "Label Top")
                errors.Add($"{path}: {visible.Count} visible labels, expected only Label Top");

            CheckLabels(errors, path, filter, visible, fontSize);
        }

        foreach (MeshFilter filter in Bags(scene))
        {
            bags++;
            string path = PathOf(filter.transform);
            CheckSize(errors, path, filter, BagSize(filter.sharedMesh));
            List<TMP_Text> visible = VisibleLabels(filter.transform);
            if (visible.Count != 1)
                errors.Add($"{path}: {visible.Count} visible labels, expected 1");

            CheckLabels(errors, path, filter, visible, fontSize);
        }

        var report = new StringBuilder($"PackageStandardizer check {scene.name}: {cartons} cartons, {bags} bags, {errors.Count} errors");
        foreach (string error in errors)
            report.Append("\n- ").Append(error);

        return report.ToString();
    }

    // Every SM_box_1 in the scene is a carton (grabbable or static); CardboardBox06 only where it is a package.
    private static IEnumerable<MeshFilter> Cartons(Scene scene)
    {
        return AllFilters(scene).Where(f =>
            f.sharedMesh.name == CartonMeshName
            || (f.sharedMesh.name.StartsWith(OldCartonMeshName) && f.GetComponentInParent<PackageData>(true) != null));
    }

    private static IEnumerable<MeshFilter> Bags(Scene scene)
    {
        return AllFilters(scene).Where(f => f.sharedMesh.name == BagMeshName);
    }

    private static IEnumerable<MeshFilter> AllFilters(Scene scene)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<MeshFilter>(true))
            .Where(f => f.sharedMesh != null);
    }

    private static Mesh CartonMesh()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CartonPrefabPath);
        return prefab.GetComponentsInChildren<MeshFilter>(true).First(f => f.sharedMesh.name == CartonMeshName).sharedMesh;
    }

    private static Material[] CartonMaterials()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CartonPrefabPath);
        MeshFilter filter = prefab.GetComponentsInChildren<MeshFilter>(true).First(f => f.sharedMesh.name == CartonMeshName);
        return filter.GetComponent<MeshRenderer>().sharedMaterials;
    }

    private static void SwapToCartonMesh(MeshFilter filter, Mesh mesh, Material[] materials)
    {
        Undo.RecordObject(filter, "Carton mesh");
        filter.sharedMesh = mesh;
        Record(filter);

        var renderer = filter.GetComponent<MeshRenderer>();
        Undo.RecordObject(renderer, "Carton material");
        renderer.sharedMaterials = materials;
        Record(renderer);

        // The old prefab's solid collider sits on the mesh object: fit it to the new mesh.
        foreach (BoxCollider box in filter.GetComponents<BoxCollider>().Where(b => !b.isTrigger))
        {
            Undo.RecordObject(box, "Carton collider");
            box.center = mesh.bounds.center;
            box.size = mesh.bounds.size;
            Record(box);
        }
    }

    // Scales the node that carries the package (the mesh itself, or its parent when the mesh is an unscaled child)
    // so that the mesh has the wanted world size. Pivots sit at the bottom centre, so the package stays on its surface.
    private static void FitSize(MeshFilter filter, Vector3 size)
    {
        Transform mesh = filter.transform;
        Transform scaled = mesh.localScale == Vector3.one && mesh.GetComponent<PackageData>() == null && mesh.parent != null
            ? mesh.parent
            : mesh;

        Vector3 wanted = Divide(size, filter.sharedMesh.bounds.size);
        Vector3 current = Abs(mesh.lossyScale);
        Vector3 factor = Divide(wanted, current);
        if ((factor - Vector3.one).sqrMagnitude < 1e-8f)
            return;

        Undo.RecordObject(scaled, "Package size");
        scaled.localScale = Vector3.Scale(scaled.localScale, factor);
        Record(scaled);
    }

    // The front sticker's renderer and text go; its BoxCollider stays because counters and physics use it.
    private static void HideFrontSticker(Transform carton)
    {
        Transform sticker = carton.Find("Cube");
        if (sticker == null)
            return;

        var renderer = sticker.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.enabled)
        {
            Undo.RecordObject(renderer, "Hide front sticker");
            renderer.enabled = false;
            Record(renderer);
        }

        foreach (TMP_Text text in sticker.GetComponentsInChildren<TMP_Text>(true))
        {
            if (!text.gameObject.activeSelf)
                continue;

            Undo.RecordObject(text.gameObject, "Hide front sticker text");
            text.gameObject.SetActive(false);
            Record(text.gameObject);
        }
    }

    private static void PlaceLidLabel(MeshFilter filter, float fontSize)
    {
        Transform carton = filter.transform;
        Transform label = carton.Find("Label Top");
        if (label == null)
        {
            Debug.LogError($"PackageStandardizer: {PathOf(carton)} has no 'Label Top'.", carton);
            return;
        }

        Bounds bounds = filter.sharedMesh.bounds;
        Vector3 scale = Abs(carton.lossyScale);
        Vector3 worldSize = Vector3.Scale(bounds.size, scale);
        var labelSize = new Vector2(worldSize.x * LidLabelShare, worldSize.z * LidLabelShare);

        Undo.RecordObject(label, "Lid label");
        label.localPosition = new Vector3(bounds.center.x, bounds.max.y + LabelLift / scale.y, bounds.center.z);
        label.localRotation = Quaternion.Euler(90f, 0f, 0f);
        label.localScale = new Vector3(labelSize.x / scale.x, labelSize.y / scale.z, 0.001f / scale.y);
        Record(label);

        SetLabelText(label, labelSize, fontSize);
    }

    // The bag label sits on the bag's front (-z) surface, found by a ray against a temporary mesh collider.
    private static void PlaceBagLabel(MeshFilter filter, float fontSize)
    {
        Transform bag = filter.transform;
        Transform label = bag.Cast<Transform>().FirstOrDefault(c => c.GetComponentInChildren<TMP_Text>(true) != null);
        if (label == null)
        {
            Debug.LogError($"PackageStandardizer: {PathOf(bag)} has no label.", bag);
            return;
        }

        // The label faces the trainee's standing spot (the rig origin), whatever way the bag lies.
        Bounds world = WorldBounds(filter);
        Vector3 toTrainee = Vector3.ProjectOnPlane(-world.center, Vector3.up);
        Vector3 facing = toTrainee.sqrMagnitude > 1e-6f ? toTrainee.normalized : -bag.forward;
        Vector3 side = Vector3.Cross(Vector3.up, facing);
        var height = new Vector3(world.center.x, world.min.y + world.size.y * BagLabelHeightShare, world.center.z);
        float halfWidth = BagLabelWidth * 0.5f + BagEdgeMargin;

        var probe = new GameObject("Bag label probe");
        probe.transform.SetPositionAndRotation(bag.position, bag.rotation);
        probe.transform.localScale = bag.lossyScale;
        var collider = probe.AddComponent<MeshCollider>();
        collider.sharedMesh = filter.sharedMesh;
        Physics.SyncTransforms();

        bool Surface(float offset, out RaycastHit hit)
        {
            Vector3 from = height + side * offset + facing;
            return collider.Raycast(new Ray(from, -facing), out hit, 2f);
        }

        bool centre = Surface(0f, out RaycastHit centreHit);
        bool edges = Surface(-halfWidth, out _) && Surface(halfWidth, out _);
        Object.DestroyImmediate(probe);

        if (!centre || !edges)
        {
            Debug.LogError($"PackageStandardizer: no room for a {BagLabelWidth * 100f:F0} cm label on {PathOf(bag)}.", bag);
            return;
        }

        Undo.RecordObject(label, "Bag label");
        label.position = centreHit.point + centreHit.normal * LabelLift;
        label.rotation = Quaternion.LookRotation(-centreHit.normal, Vector3.up);
        Vector3 parentScale = Abs(bag.lossyScale);
        label.localScale = new Vector3(BagLabelWidth / parentScale.x, BagLabelHeight / parentScale.y, 0.001f / parentScale.z);
        Record(label);

        SetLabelText(label, new Vector2(BagLabelWidth, BagLabelHeight), fontSize);
    }

    private static void SetLabelText(Transform label, Vector2 labelSize, float fontSize)
    {
        var text = label.GetComponentInChildren<TextMeshPro>(true);
        if (text == null)
            return;

        Transform t = text.transform;
        Undo.RecordObject(t, "Label text");
        t.localPosition = new Vector3(0f, 0f, -0.6f);
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        Vector3 inherited = Abs(t.lossyScale);
        t.localScale = new Vector3(TextWorldScale / inherited.x, TextWorldScale / inherited.y, TextWorldScale / inherited.z);
        Record(t);

        Undo.RecordObject(text.rectTransform, "Label text rect");
        text.rectTransform.sizeDelta = labelSize * TextShare / TextWorldScale;
        Record(text.rectTransform);

        Undo.RecordObject(text, "Label text");
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = Vector4.zero;
        Record(text);

        if (!text.gameObject.activeSelf)
        {
            Undo.RecordObject(text.gameObject, "Label text");
            text.gameObject.SetActive(true);
            Record(text.gameObject);
        }
    }

    // One font size for every label: the largest at which the longest carton line fits the lid label and the
    // Mat.-Nr line fits a bag label, three lines high each. Text width grows linearly with the font size.
    private static float LabelFontSize(Scene scene)
    {
        TextMeshPro sample = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TextMeshPro>(true))
            .FirstOrDefault(t => t.transform.parent != null && t.transform.parent.name == "Label Top");
        if (sample == null)
            return 0f;

        Vector3 carton = CartonSize;
        float cartonWidth = carton.x * LidLabelShare * TextShare / TextWorldScale;
        float cartonHeight = carton.z * LidLabelShare * TextShare / TextWorldScale;
        float bagWidth = BagLabelWidth * TextShare / TextWorldScale;
        float bagHeight = BagLabelHeight * TextShare / TextWorldScale;

        const float probeSize = 10f;
        float savedSize = sample.fontSize;
        bool savedAuto = sample.enableAutoSizing;
        sample.enableAutoSizing = false;
        sample.fontSize = probeSize;
        Vector2 cartonLine = sample.GetPreferredValues(LongestCartonLine);
        Vector2 bagLine = sample.GetPreferredValues(LongestBagLine);
        float threeLines = sample.GetPreferredValues("A\nA\nA").y;
        sample.fontSize = savedSize;
        sample.enableAutoSizing = savedAuto;

        float size = Mathf.Min(
            probeSize * cartonWidth / cartonLine.x,
            probeSize * bagWidth / bagLine.x,
            probeSize * cartonHeight / threeLines,
            probeSize * bagHeight / threeLines);
        return Mathf.Floor(size * 10f) / 10f;
    }

    private static void CheckSize(List<string> errors, string path, MeshFilter filter, Vector3 size)
    {
        Vector3 actual = Vector3.Scale(filter.sharedMesh.bounds.size, Abs(filter.transform.lossyScale));
        if (Mathf.Abs(actual.x - size.x) > SizeTolerance || Mathf.Abs(actual.y - size.y) > SizeTolerance
            || Mathf.Abs(actual.z - size.z) > SizeTolerance)
            errors.Add($"{path}: size {actual.ToString("F3")}, expected {size.ToString("F3")}");
    }

    private static void CheckLabels(List<string> errors, string path, MeshFilter filter, List<TMP_Text> labels, float fontSize)
    {
        var data = filter.GetComponentInParent<PackageData>(true);
        foreach (TMP_Text text in labels)
        {
            if (text.enableAutoSizing || Mathf.Abs(text.fontSize - fontSize) > 0.05f)
                errors.Add($"{path}: label font {text.fontSize:F1} auto {text.enableAutoSizing}, expected {fontSize:F1} fixed");

            if (data == null)
                continue;

            string[] lines = text.text.Split('\n').Select(l => l.Trim()).ToArray();
            if (lines.Length != 3 || lines[1] != $"Mat.-Nr: {data.MaterialNumber}" || lines[2] != $"VPE: {data.Vpe} ST")
                errors.Add($"{path}: label text '{text.text.Replace("\n", " / ")}' does not match PackageData");
        }
    }

    private static List<TMP_Text> VisibleLabels(Transform package)
    {
        return package.GetComponentsInChildren<TMP_Text>(true)
            .Where(t => t.gameObject.activeSelf && t.enabled)
            .Where(t =>
            {
                var plate = t.transform.parent != null ? t.transform.parent.GetComponent<MeshRenderer>() : null;
                return plate == null || plate.enabled;
            })
            .ToList();
    }

    // Renderer bounds are empty on inactive objects (storage boxes of later positions), so use the mesh corners.
    private static Bounds WorldBounds(MeshFilter filter)
    {
        Bounds local = filter.sharedMesh.bounds;
        var world = new Bounds(filter.transform.TransformPoint(local.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
        {
            var corner = new Vector3(
                (i & 1) == 0 ? local.min.x : local.max.x,
                (i & 2) == 0 ? local.min.y : local.max.y,
                (i & 4) == 0 ? local.min.z : local.max.z);
            world.Encapsulate(filter.transform.TransformPoint(corner));
        }

        return world;
    }

    private static void Record(Object target)
    {
        if (PrefabUtility.IsPartOfPrefabInstance(target))
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        EditorUtility.SetDirty(target);
    }

    private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;

    private static Vector3 Abs(Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    private static Vector3 Divide(Vector3 a, Vector3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
}
