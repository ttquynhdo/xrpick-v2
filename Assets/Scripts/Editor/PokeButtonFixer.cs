using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// #126: makes the poke buttons of the open scene reliable.
/// 1. Every ScrollRect above a Selectable is switched off. The Meta canvas buttons sit in a "Scroll View";
///    PointableCanvasModule starts a drag when the finger drifts a few millimetres during a press and then
///    cancels the click, although the finger is still on the button.
/// 2. Menu items get a raycast padding of half the list spacing (+1 unit), so a touch between two items hits one
///    of them.
/// 3. Each flat poke Surface (BoundsClipper) is grown, never shrunk, to cover its buttons plus a margin, so a
///    touch at the edge of a button is still a poke.
/// Run it again after adding a canvas button to a scene. Check() lists what is still wrong.
/// </summary>
public static class PokeButtonFixer
{
    // Metres around the visible button that still count as a poke. Small at the sides: neighbouring
    // surfaces (quiz A/B/C and its „Zurück“) sit side by side and must not overlap.
    public const float MarginSide = 0.002f;
    public const float MarginTopBottom = 0.01f;

    private const string MenuRootName = "Menu";

    [MenuItem("XRPick/Poke Buttons/Check open scene")]
    private static void CheckMenu()
    {
        Debug.Log(Check());
    }

    [MenuItem("XRPick/Poke Buttons/Fix open scene")]
    private static void FixMenu()
    {
        Debug.Log(FixOpenScene(false));
    }

    /// <summary>Reports what FixOpenScene would change in the active scene; changes nothing.</summary>
    public static string Check()
    {
        return FixOpenScene(true);
    }

    /// <summary>
    /// Fixes the active scene; with dryRun only reports. Returns a report. Sizes are measured after a layout
    /// pass; that pass would save layout-driven rects as 0, so the scene is reopened before the fixes are written.
    /// Save the scene afterwards.
    /// </summary>
    public static string FixOpenScene(bool dryRun)
    {
        Scene scene = SceneManager.GetActiveScene();
        var report = new StringBuilder($"PokeButtonFixer {(dryRun ? "(dry run) " : "")}{scene.name}:\n");
        bool wasClean = !scene.isDirty;
        if (!dryRun && !wasClean)
            return report.AppendLine("  scene has unsaved changes: save or discard them first").ToString();

        RebuildLayouts(scene);
        var changes = new List<Change>();
        DisableScrollRects(scene, changes);
        PadMenuItems(scene, changes, report);
        GrowSurfaces(scene, changes);
        ReportSurfaceOverlaps(scene, report);

        foreach (Change change in changes)
            report.AppendLine("  " + change.Text);
        if (changes.Count == 0)
            report.AppendLine("  nothing to change");

        // Drop the layout pass again (only possible when nothing else was unsaved).
        if (wasClean)
            scene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
        if (dryRun || changes.Count == 0)
            return report.ToString();

        foreach (Change change in changes)
        {
            Object target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(change.Target);
            if (target == null)
            {
                report.AppendLine("  ERROR target not found after reopening: " + change.Text);
                continue;
            }

            Undo.RecordObject(target, "Fix poke buttons");
            change.Apply(target);
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        return report.AppendLine("  applied; save the scene").ToString();
    }

    private readonly struct Change
    {
        public readonly GlobalObjectId Target;
        public readonly string Text;
        public readonly System.Action<Object> Apply;

        public Change(Object target, string text, System.Action<Object> apply)
        {
            Target = GlobalObjectId.GetGlobalObjectIdSlow(target);
            Text = text;
            Apply = apply;
        }
    }

    private static void DisableScrollRects(Scene scene, List<Change> changes)
    {
        foreach (ScrollRect scroll in Components<ScrollRect>(scene))
        {
            if (scroll.enabled && scroll.GetComponentInChildren<Selectable>(true) != null)
                changes.Add(new Change(scroll, $"ScrollRect off: {PathOf(scroll.transform)}", target => ((ScrollRect)target).enabled = false));
        }
    }

    // A Menu item's hit area is its raycast graphic; the list spacing between two items hits nothing.
    private static void PadMenuItems(Scene scene, List<Change> changes, StringBuilder report)
    {
        foreach (Toggle item in Components<Toggle>(scene))
        {
            if (item.transform.root.name != MenuRootName)
                continue;

            var list = item.transform.parent.GetComponent<VerticalLayoutGroup>();
            Graphic hitArea = HitGraphicOf(item);
            if (list == null || hitArea == null)
            {
                report.AppendLine($"  WARNING menu item without list or raycast graphic: {PathOf(item.transform)}");
                continue;
            }

            // Half the spacing plus one unit: neighbouring hit areas overlap a little, so no seam is left.
            float half = list.spacing / 2f + 1f;
            var padding = new Vector4(0f, -half, 0f, -half);
            if (hitArea.raycastPadding != padding)
                changes.Add(new Change(hitArea, $"menu item hit area +{half} units top/bottom: {PathOf(hitArea.transform)}", target => ((Graphic)target).raycastPadding = padding));
        }
    }

    private static void GrowSurfaces(Scene scene, List<Change> changes)
    {
        foreach (PokeInteractable poke in Components<PokeInteractable>(scene))
        {
            if (!(Reference(poke, "_pointableElement") is PointableCanvas pointable) || pointable.Canvas == null)
                continue;
            if (!(Reference(poke, "_surfacePatch") is ClippedPlaneSurface surface))
                continue;

            List<Selectable> buttons = pointable.Canvas.GetComponentsInChildren<Selectable>(true)
                .Where(button => button.gameObject.activeInHierarchy).ToList();
            BoundsClipper clipper = ClipperOf(surface);
            if (buttons.Count == 0 || clipper == null)
                continue;

            // Everything in the clipper's local space: x/y lie in the button plane.
            var serialized = new SerializedObject(clipper);
            Vector3 position = serialized.FindProperty("_position").vector3Value;
            Vector3 size = serialized.FindProperty("_size").vector3Value;
            Vector3 scale = clipper.transform.lossyScale;
            var margin = new Vector2(MarginSide / Mathf.Abs(scale.x), MarginTopBottom / Mathf.Abs(scale.y));

            Rect current = Rect.MinMaxRect(position.x - size.x / 2f, position.y - size.y / 2f,
                position.x + size.x / 2f, position.y + size.y / 2f);
            Rect needed = current;
            foreach (Selectable button in buttons)
            {
                Rect visual = VisualRectIn(button, clipper.transform);
                needed.xMin = Mathf.Min(needed.xMin, visual.xMin - margin.x);
                needed.xMax = Mathf.Max(needed.xMax, visual.xMax + margin.x);
                needed.yMin = Mathf.Min(needed.yMin, visual.yMin - margin.y);
                needed.yMax = Mathf.Max(needed.yMax, visual.yMax + margin.y);
            }

            if (Approximately(needed, current, scale))
                continue;

            var newPosition = new Vector3(needed.center.x, needed.center.y, position.z);
            var newSize = new Vector3(needed.width, needed.height, size.z);
            changes.Add(new Change(clipper, $"poke surface {Cm(current, scale)} -> {Cm(needed, scale)}: {PathOf(clipper.transform)}", target =>
            {
                var fresh = new SerializedObject(target);
                fresh.FindProperty("_position").vector3Value = newPosition;
                fresh.FindProperty("_size").vector3Value = newSize;
                fresh.ApplyModifiedPropertiesWithoutUndo();
            }));
        }
    }

    // Two flat surfaces that overlap in world space make the poke pick one of them; only report it.
    private static void ReportSurfaceOverlaps(Scene scene, StringBuilder report)
    {
        var boxes = new List<(string name, Bounds bounds)>();
        foreach (PokeInteractable poke in Components<PokeInteractable>(scene))
        {
            if (!poke.gameObject.activeInHierarchy || !(Reference(poke, "_surfacePatch") is ClippedPlaneSurface surface))
                continue;

            BoundsClipper clipper = ClipperOf(surface);
            if (clipper == null)
                continue;

            var serialized = new SerializedObject(clipper);
            Vector3 position = serialized.FindProperty("_position").vector3Value;
            Vector3 size = serialized.FindProperty("_size").vector3Value;
            var bounds = new Bounds(clipper.transform.TransformPoint(position), Vector3.zero);
            for (int corner = 0; corner < 8; corner++)
            {
                var sign = new Vector3((corner & 1) == 0 ? -0.5f : 0.5f, (corner & 2) == 0 ? -0.5f : 0.5f, (corner & 4) == 0 ? -0.5f : 0.5f);
                bounds.Encapsulate(clipper.transform.TransformPoint(position + Vector3.Scale(size, sign)));
            }

            // Surfaces that only touch at an edge are fine.
            bounds.extents = Vector3.Max(bounds.extents - Vector3.one * 0.001f, Vector3.zero);
            boxes.Add((PathOf(poke.transform), bounds));
        }

        for (int i = 0; i < boxes.Count; i++)
        {
            for (int j = i + 1; j < boxes.Count; j++)
            {
                if (boxes[i].bounds.Intersects(boxes[j].bounds))
                    report.AppendLine($"  WARNING poke surfaces overlap: {boxes[i].name} / {boxes[j].name}");
            }
        }
    }

    private static Graphic HitGraphicOf(Selectable item)
    {
        if (item.targetGraphic != null && item.targetGraphic.raycastTarget)
            return item.targetGraphic;

        return item.GetComponentsInChildren<Graphic>(true)
            .Where(graphic => graphic.raycastTarget)
            .OrderByDescending(graphic => graphic.rectTransform.rect.width * graphic.rectTransform.rect.height)
            .FirstOrDefault();
    }

    private static BoundsClipper ClipperOf(ClippedPlaneSurface surface)
    {
        SerializedProperty clippers = new SerializedObject(surface).FindProperty("_clippers");
        for (int i = 0; clippers != null && i < clippers.arraySize; i++)
        {
            if (clippers.GetArrayElementAtIndex(i).objectReferenceValue is BoundsClipper clipper)
                return clipper;
        }

        return null;
    }

    // The button's rect plus every visible non-text graphic under it (the grey plate is a child).
    private static Rect VisualRectIn(Selectable button, Transform space)
    {
        var rect = new Rect();
        bool first = true;
        var corners = new Vector3[4];
        foreach (Graphic graphic in button.GetComponentsInChildren<Graphic>())
        {
            if (!graphic.isActiveAndEnabled || graphic.color.a < 0.05f || graphic is TMPro.TMP_Text)
                continue;

            graphic.rectTransform.GetWorldCorners(corners);
            foreach (Vector3 corner in corners)
            {
                Vector3 local = space.InverseTransformPoint(corner);
                if (first)
                {
                    rect = new Rect(local.x, local.y, 0f, 0f);
                    first = false;
                }
                rect.xMin = Mathf.Min(rect.xMin, local.x);
                rect.xMax = Mathf.Max(rect.xMax, local.x);
                rect.yMin = Mathf.Min(rect.yMin, local.y);
                rect.yMax = Mathf.Max(rect.yMax, local.y);
            }
        }

        return rect;
    }

    // Layout-driven rects are saved as 0 until a layout pass runs; measure after one.
    private static void RebuildLayouts(Scene scene)
    {
        for (int pass = 0; pass < 3; pass++)
        {
            foreach (Canvas canvas in Components<Canvas>(scene).Where(canvas => canvas.isRootCanvas && canvas.isActiveAndEnabled))
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)canvas.transform);
            Canvas.ForceUpdateCanvases();
        }
    }

    private static IEnumerable<T> Components<T>(Scene scene) where T : Component
    {
        return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
    }

    // Within 1 mm counts as unchanged.
    private static bool Approximately(Rect a, Rect b, Vector3 scale)
    {
        float x = 0.001f / Mathf.Abs(scale.x);
        float y = 0.001f / Mathf.Abs(scale.y);
        return Mathf.Abs(a.xMin - b.xMin) < x && Mathf.Abs(a.xMax - b.xMax) < x
            && Mathf.Abs(a.yMin - b.yMin) < y && Mathf.Abs(a.yMax - b.yMax) < y;
    }

    // PokeInteractable sets its PointableElement and SurfacePatch properties in Awake; read the fields instead.
    private static Object Reference(Component component, string field)
    {
        return new SerializedObject(component).FindProperty(field)?.objectReferenceValue;
    }

    private static string Cm(Rect rect, Vector3 scale)
    {
        return $"{rect.width * Mathf.Abs(scale.x) * 100f:0.0}x{rect.height * Mathf.Abs(scale.y) * 100f:0.0} cm";
    }

    private static string PathOf(Transform transform)
    {
        return transform.parent == null ? transform.name : PathOf(transform.parent) + "/" + transform.name;
    }
}
