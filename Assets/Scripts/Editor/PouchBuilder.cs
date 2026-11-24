using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// #173, decision 042 item 2: the clear plastic pouch that replaces the PlasticBag02 bags.
/// BuildAssets writes the pouch mesh (flat on its bottom, thin welded seam, 3.5 cm thick in the middle, pivot at the
/// bottom centre, label side up), the mesh of the goods inside (one combined mesh, view only, no colliders) and the
/// clear material. ConvertOpenScene turns every PlasticBag02 package of the open scene into a pouch: same GameObject
/// (Grabbable, Rigidbody, PackageData, hand-grab interactable kept), mesh and material swapped, scale 1, one solid
/// BoxCollider = pouch bounds, goods as child "Contents", bottom left where the bag's bottom was.
/// Labels are placed by PackageStandardizer afterwards (flat on the pouch top). #174 adds the RING D pouch here.
/// </summary>
public static class PouchBuilder
{
    private const string MeshFolder = "Assets/Meshes";
    private const string PouchMaterialPath = "Assets/Materials/PouchClear.mat";
    private const string ContentsMaterialPath = "Assets/Materials/HoseClampSteel.mat";
    private const string OldBagMeshName = "PlasticBag02";
    private const string ContentsChildName = "Contents";

    /// <summary>A pouch kind: its size and what lies inside.</summary>
    private sealed class PouchSpec
    {
        public string MaterialKey;
        public string MeshName;        // must end with "Pouch" (PackageStandardizer finds pouches by that)
        public string ContentsMeshName;
        public Vector3 Size;           // width (x), thickness (y), depth (z)
        public int Pieces;
        public int Seed;
    }

    // Mixed SCHRAUBE B: 12 x 10 cm, 3.5 cm thick, 20 screws (VPE 20). User choice 2026-11-24 (clear_pouch_*.png).
    private static readonly PouchSpec ScrewPouch = new PouchSpec
    {
        MaterialKey = "screw_b",
        MeshName = "ScrewPouch",
        ContentsMeshName = "ScrewPouchScrews",
        Size = new Vector3(0.12f, 0.035f, 0.10f),
        Pieces = 20,
        Seed = 173,
    };

    private static readonly PouchSpec[] Specs = { ScrewPouch };

    // Pouch shape: a thin welded seam all round, then the filled body rises to the full thickness.
    private const float SeamWidth = 0.003f;
    private const float SeamThickness = 0.002f;
    private const float SlopeWidth = 0.008f;
    private const float Dome = 0.03f;        // the top sinks 3 % towards the corners
    private const float GridStep = 0.005f;

    // Screw: flat head + shank, 8 sides.
    private const float ScrewLength = 0.030f;
    private const float ShankRadius = 0.0025f;
    private const float HeadRadius = 0.0045f;
    private const float HeadHeight = 0.0025f;
    private const int Sides = 8;

    [MenuItem("XRPick/Packages/Build pouch assets")]
    private static void BuildMenu() => Debug.Log(BuildAssets());

    [MenuItem("XRPick/Packages/Convert bags to pouches (open scene)")]
    private static void ConvertMenu() => Debug.Log(ConvertOpenScene(false));

    /// <summary>Writes (or overwrites in place, keeping GUIDs) the pouch meshes and the clear material.</summary>
    public static string BuildAssets()
    {
        var report = new StringBuilder("PouchBuilder assets:");
        foreach (PouchSpec spec in Specs)
        {
            Mesh pouch = SaveMesh(BuildPouchMesh(spec), spec.MeshName);
            Mesh contents = SaveMesh(BuildScrewsMesh(spec), spec.ContentsMeshName);
            report.Append($"\n- {pouch.name}: {pouch.triangles.Length / 3} tris, bounds {pouch.bounds.size.ToString("F3")}");
            report.Append($"\n- {contents.name}: {contents.triangles.Length / 3} tris, bounds {contents.bounds.size.ToString("F3")}");
        }

        Material material = PouchMaterial();
        report.Append($"\n- {material.name}: {material.shader.name}, queue {material.renderQueue}");
        AssetDatabase.SaveAssets();
        return report.ToString();
    }

    /// <summary>Turns every PlasticBag02 package of the open scene into its pouch. Dry run only reports.</summary>
    public static string ConvertOpenScene(bool dryRun)
    {
        Scene scene = SceneManager.GetActiveScene();
        var report = new StringBuilder($"PouchBuilder {(dryRun ? "dry run" : "convert")}: {scene.name}");
        Material pouchMaterial = AssetDatabase.LoadAssetAtPath<Material>(PouchMaterialPath);
        Material contentsMaterial = AssetDatabase.LoadAssetAtPath<Material>(ContentsMaterialPath);

        IEnumerable<MeshFilter> bags = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<MeshFilter>(true))
            .Where(f => f.sharedMesh != null && f.sharedMesh.name == OldBagMeshName)
            .ToList();

        foreach (MeshFilter bag in bags)
        {
            var data = bag.GetComponentInParent<PackageData>(true);
            PouchSpec spec = data != null ? Specs.FirstOrDefault(s => s.MaterialKey == data.MaterialKey) : null;
            if (spec == null)
            {
                report.Append($"\n- skipped {PathOf(bag.transform)}: no pouch for this material");
                continue;
            }

            report.Append($"\n- {PathOf(bag.transform)} -> {spec.MeshName}");
            if (dryRun)
                continue;

            Mesh pouch = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshFolder}/{spec.MeshName}.asset");
            Mesh contents = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshFolder}/{spec.ContentsMeshName}.asset");
            if (pouch == null || contents == null || pouchMaterial == null)
            {
                report.Append(" FAILED: run BuildAssets first");
                continue;
            }

            if (!Convert(bag, pouch, contents, pouchMaterial, contentsMaterial))
                report.Append(" SKIPPED: inside another prefab instance, unpack it by hand first");
        }

        if (!dryRun)
            EditorSceneManager.MarkSceneDirty(scene);

        return report.ToString();
    }

    // Returns false (and changes nothing) when the bag sits inside a larger prefab instance, e.g. a storage box:
    // unpacking that would unpack the whole box.
    private static bool Convert(MeshFilter bag, Mesh pouch, Mesh contents, Material pouchMaterial, Material contentsMaterial)
    {
        GameObject go = bag.gameObject;
        if (PrefabUtility.IsPartOfPrefabInstance(go))
        {
            GameObject outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(outermost);
            if (outermost != go || source == null || !AssetDatabase.GetAssetPath(source).EndsWith($"/{OldBagMeshName}.prefab"))
                return false;

            PrefabUtility.UnpackPrefabInstance(outermost, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }

        // The pouch's pivot is its bottom: keep the bottom where the bag's bottom was.
        float bottom = WorldBottom(bag);
        Transform t = go.transform;

        Undo.RecordObject(t, "Pouch transform");
        t.localScale = Vector3.one;
        t.position = new Vector3(t.position.x, bottom, t.position.z);

        Undo.RecordObject(bag, "Pouch mesh");
        bag.sharedMesh = pouch;
        var renderer = go.GetComponent<MeshRenderer>();
        Undo.RecordObject(renderer, "Pouch material");
        renderer.sharedMaterials = new[] { pouchMaterial };
        renderer.shadowCastingMode = ShadowCastingMode.Off;

        foreach (Collider old in go.GetComponents<Collider>())
            Undo.DestroyObjectImmediate(old);

        var box = Undo.AddComponent<BoxCollider>(go);
        box.center = pouch.bounds.center;
        box.size = pouch.bounds.size;

        Transform child = t.Find(ContentsChildName);
        if (child == null)
        {
            var contentsGo = new GameObject(ContentsChildName, typeof(MeshFilter), typeof(MeshRenderer));
            Undo.RegisterCreatedObjectUndo(contentsGo, "Pouch contents");
            contentsGo.layer = go.layer;
            child = contentsGo.transform;
            child.SetParent(t, false);
        }

        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        child.GetComponent<MeshFilter>().sharedMesh = contents;
        var contentsRenderer = child.GetComponent<MeshRenderer>();
        contentsRenderer.sharedMaterials = new[] { contentsMaterial };
        contentsRenderer.shadowCastingMode = ShadowCastingMode.Off;   // ~1,100 tris per pouch, no visible gain

        EditorUtility.SetDirty(go);
        return true;
    }

    private static float WorldBottom(MeshFilter filter)
    {
        Bounds local = filter.sharedMesh.bounds;
        float min = float.MaxValue;
        for (int i = 0; i < 8; i++)
        {
            var corner = new Vector3(
                (i & 1) == 0 ? local.min.x : local.max.x,
                (i & 2) == 0 ? local.min.y : local.max.y,
                (i & 4) == 0 ? local.min.z : local.max.z);
            min = Mathf.Min(min, filter.transform.TransformPoint(corner).y);
        }

        return min;
    }

    // ---- Pouch mesh ----

    private static Mesh BuildPouchMesh(PouchSpec spec)
    {
        float halfX = spec.Size.x * 0.5f, halfZ = spec.Size.z * 0.5f;
        int nx = Mathf.CeilToInt(spec.Size.x / GridStep), nz = Mathf.CeilToInt(spec.Size.z / GridStep);
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        // Top: a height field over the outline.
        for (int iz = 0; iz <= nz; iz++)
        for (int ix = 0; ix <= nx; ix++)
        {
            float x = -halfX + spec.Size.x * ix / nx;
            float z = -halfZ + spec.Size.z * iz / nz;
            vertices.Add(new Vector3(x, TopHeight(spec, x, z), z));
        }

        for (int iz = 0; iz < nz; iz++)
        for (int ix = 0; ix < nx; ix++)
        {
            int a = iz * (nx + 1) + ix, b = a + 1, c = a + nx + 1, d = c + 1;
            triangles.AddRange(new[] { a, c, b, b, c, d });
        }

        // Perimeter of the top grid, counter-clockwise seen from above.
        var rim = new List<int>();
        for (int ix = 0; ix < nx; ix++) rim.Add(ix);
        for (int iz = 0; iz < nz; iz++) rim.Add(iz * (nx + 1) + nx);
        for (int ix = nx; ix > 0; ix--) rim.Add(nz * (nx + 1) + ix);
        for (int iz = nz; iz > 0; iz--) rim.Add(iz * (nx + 1));

        // Side band: the seam edge, from the top rim down to the floor (own vertices for a hard edge).
        int sideStart = vertices.Count;
        foreach (int i in rim)
        {
            Vector3 p = vertices[i];
            vertices.Add(p);
            vertices.Add(new Vector3(p.x, 0f, p.z));
        }

        for (int k = 0; k < rim.Count; k++)
        {
            int top0 = sideStart + 2 * k, bot0 = top0 + 1;
            int top1 = sideStart + 2 * ((k + 1) % rim.Count), bot1 = top1 + 1;
            triangles.AddRange(new[] { top0, top1, bot0, bot0, top1, bot1 });
        }

        // Bottom: flat, a fan from the centre.
        int centre = vertices.Count;
        vertices.Add(Vector3.zero);
        int bottomStart = vertices.Count;
        foreach (int i in rim)
            vertices.Add(new Vector3(vertices[i].x, 0f, vertices[i].z));

        for (int k = 0; k < rim.Count; k++)
            triangles.AddRange(new[] { centre, bottomStart + k, bottomStart + (k + 1) % rim.Count });

        var mesh = new Mesh { name = spec.MeshName };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static float TopHeight(PouchSpec spec, float x, float z)
    {
        float halfX = spec.Size.x * 0.5f, halfZ = spec.Size.z * 0.5f;
        float edge = Mathf.Min(halfX - Mathf.Abs(x), halfZ - Mathf.Abs(z));
        float rise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(SeamWidth, SeamWidth + SlopeWidth, edge));
        float u = x / halfX, v = z / halfZ;
        float dome = 1f - Dome * (u * u + v * v) * 0.5f;
        return Mathf.Lerp(SeamThickness, spec.Size.y * dome, rise);
    }

    // ---- Contents: screws lying in two layers ----

    private static Mesh BuildScrewsMesh(PouchSpec spec)
    {
        var random = new System.Random(spec.Seed);
        float limitX = spec.Size.x * 0.5f - SeamWidth - SlopeWidth - HeadRadius * 0.5f;
        float limitZ = spec.Size.z * 0.5f - SeamWidth - SlopeWidth - HeadRadius * 0.5f;
        float[] layers = { HeadRadius + 0.001f, HeadRadius * 3f + 0.002f };
        var placed = new List<Vector3>();
        var combine = new List<CombineInstance>();
        Mesh screw = BuildScrew();

        for (int n = 0; n < spec.Pieces; n++)
        {
            float y = layers[n % layers.Length];
            for (int attempt = 0; attempt < 200; attempt++)
            {
                float yaw = (float)random.NextDouble() * 360f;
                float pitch = ((float)random.NextDouble() - 0.5f) * 12f;
                Quaternion rotation = Quaternion.Euler(0f, yaw, pitch);
                Vector3 axis = rotation * Vector3.right;
                var centre = new Vector3(
                    ((float)random.NextDouble() * 2f - 1f) * limitX,
                    y,
                    ((float)random.NextDouble() * 2f - 1f) * limitZ);
                Vector3 start = centre - axis * (ScrewLength * 0.5f);
                Vector3 end = centre + axis * (ScrewLength * 0.5f);
                bool inside = Mathf.Abs(start.x) <= limitX && Mathf.Abs(end.x) <= limitX
                    && Mathf.Abs(start.z) <= limitZ && Mathf.Abs(end.z) <= limitZ;
                bool crowded = placed.Any(p => Mathf.Abs(p.y - y) < 0.001f
                    && Vector2.Distance(new Vector2(p.x, p.z), new Vector2(centre.x, centre.z)) < 0.014f);
                if (!inside || (crowded && attempt < 150))
                    continue;

                placed.Add(centre);
                combine.Add(new CombineInstance
                {
                    mesh = screw,
                    transform = Matrix4x4.TRS(start, rotation, Vector3.one),
                });
                break;
            }
        }

        var mesh = new Mesh { name = spec.ContentsMeshName };
        mesh.CombineMeshes(combine.ToArray(), true, true);
        mesh.RecalculateBounds();
        Object.DestroyImmediate(screw);
        return mesh;
    }

    // Axis along +x from the head top (x = 0) to the tip.
    private static Mesh BuildScrew()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        AddCylinder(vertices, triangles, 0f, HeadHeight, HeadRadius);
        AddCylinder(vertices, triangles, HeadHeight, ScrewLength, ShankRadius);
        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void AddCylinder(List<Vector3> vertices, List<int> triangles, float x0, float x1, float radius)
    {
        Vector3 Ring(float x, int i)
        {
            float a = i * Mathf.PI * 2f / Sides;
            return new Vector3(x, Mathf.Cos(a) * radius, Mathf.Sin(a) * radius);
        }

        int side = vertices.Count;
        for (int i = 0; i < Sides; i++)
        {
            vertices.Add(Ring(x0, i));
            vertices.Add(Ring(x1, i));
        }

        for (int i = 0; i < Sides; i++)
        {
            int a = side + 2 * i, b = a + 1, c = side + 2 * ((i + 1) % Sides), d = c + 1;
            triangles.AddRange(new[] { a, c, b, b, c, d });
        }

        // Flat caps with their own vertices; the start cap faces -x, the end cap +x.
        foreach ((float x, bool start) in new[] { (x0, true), (x1, false) })
        {
            int cap = vertices.Count;
            for (int i = 0; i < Sides; i++)
                vertices.Add(Ring(x, i));

            for (int i = 1; i < Sides - 1; i++)
            {
                if (start)
                    triangles.AddRange(new[] { cap, cap + i + 1, cap + i });
                else
                    triangles.AddRange(new[] { cap, cap + i, cap + i + 1 });
            }
        }
    }

    // ---- Assets ----

    private static Mesh SaveMesh(Mesh mesh, string name)
    {
        string path = $"{MeshFolder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        // Overwrite in place so scene references (GUID + fileID) stay valid.
        existing.Clear();
        existing.SetVertices(mesh.vertices);
        existing.SetNormals(mesh.normals);
        existing.SetTriangles(mesh.triangles, 0);
        existing.RecalculateBounds();
        EditorUtility.SetDirty(existing);
        Object.DestroyImmediate(mesh);
        return existing;
    }

    private static Material PouchMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(PouchMaterialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "PouchClear" };
            AssetDatabase.CreateAsset(material, PouchMaterialPath);
        }

        // URP Lit, transparent (surface 1, blend mode Alpha): a clear film that shows the goods inside. Only the
        // surface options are set here; URP's material validation derives the blend factors and keywords from them
        // (it saves the Alpha mode with preserved specular, i.e. premultiplied factors).
        material.SetColor("_BaseColor", new Color(0.93f, 0.95f, 0.97f, 0.30f));
        material.SetFloat("_Smoothness", 0.85f);
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_Cull", (float)CullMode.Back);
        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
}
