using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Oculus.Interaction;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// #135: measures every TMP text and every poke button of the build scenes against docs/text-comfort-checklist.md
/// (distance, angles, size, contrast, hit targets) as seen from the standing spot, eye at (0, 1.6, 0) facing +z.
/// Opens each scene in turn without saving, then reopens the scene that was open. Writes to
/// TestEvidence/text-comfort/: texts.tsv (one row per text), pokes.tsv (one row per button), report.md (violations).
/// Positions are the scenes' start poses: texts placed by code at runtime (hint tags, totals) are measured where
/// they wait. 3D TextMeshPro sizes come from the rendered glyphs. Contrast is approximate: Image colours only
/// (sprite pixels ignored), and an unknown backdrop counts as mid grey.
/// </summary>
public static class TextComfortAudit
{
    public static readonly Vector3 Eye = new(0f, 1.6f, 0f);

    // Checklist thresholds (docs/text-comfort-checklist.md).
    public const float TouchMax = 0.46f;            // T2: touch reach up to 46 cm, horizontal (decision 040 addendum)
    public const float MiddleMin = 0.5f;            // T1/T2: avoid 0.5-0.8 m
    public const float MiddleMax = 0.8f;
    public const float ReadMin = 1.0f;              // T1: read-only text at 1 m or more
    public const float EmFloorDeg = 0.5f;           // T4: hard floor
    public const float EmTargetDeg = 0.8f;          // T4: target for sentences
    public const float YawMax = 45f;                // T5
    public const float PitchBest = -35f;            // T6: best 0..-35
    public const float PitchMin = -60f;             // T6: avoid below
    public const float PitchMax = 10f;              // T6: avoid above
    public const float FacingWarn = 45f;            // T7
    public const float FacingMax = 60f;
    public const float SameRoleDistance = 0.05f;    // T9
    public const float SameRoleYaw = 5f;
    public const float ContrastBody = 4.5f;         // T11
    public const float ContrastLarge = 3f;
    public const float HitMinMm = 22f;              // T12
    public const float HitMinDeg = 2.5f;
    public const float SpacingMinMm = 12f;

    // Cap height of LiberationSans / Roboto is about 0.7 em; used to compare 3D labels with UI text.
    private const float CapPerEm = 0.7f;

    private const string OutputFolder = "TestEvidence/text-comfort";

    // Paths that are exempt from some checks, with the reason written into the report.
    private static readonly (string pathPart, string checks, string reason)[] Exemptions =
    {
        ("WMS_Kommissionierung", "T4 T11", "copy of the real WMS dialog, decision 008"),
        ("FacilitatorEndSession", "T1 T2 T4 T5 T6 T7 T12", "facilitator only"),
        ("Box Hints/Hint Tag", "T1 T2 T5 T6 T7", "placed by code at runtime; check in Play"),
        ("Carton Count Demo/Total", "T1 T2 T5 T6 T7", "placed by code at runtime; check in Play"),
        ("Layout Slots/EKZ Sample", "T7", "cartons tilted on purpose, decision 036"),
        ("Menu/CurvedUnityCanvas", "T2", "Menu stays, used once per lesson, decision 040"),
        ("Participant Entry/", "T2", "keypad stays, pressed by the facilitator, decision 040"),
        ("Tour Nav Bar/Progress", "T1", "step counter beside the buttons, decision 040"),
    };

    // Labels on packages are read up close when the package is held; only their size from the standing spot counts.
    private const string PackageLabelChecks = "T1 T5 T6 T7";

    [MenuItem("XRPick/Text Comfort/Audit all build scenes")]
    private static void AuditMenu()
    {
        Debug.Log(Check());
    }

    /// <summary>Audits all enabled build scenes and writes the files; returns a short summary.</summary>
    public static string Check()
    {
        Scene start = SceneManager.GetActiveScene();
        if (start.isDirty)
            return "TextComfortAudit: the open scene has unsaved changes; save or discard them first.";

        string startPath = start.path;
        var texts = new List<TextRow>();
        var pokes = new List<PokeRow>();
        string[] scenePaths = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        try
        {
            foreach (string path in scenePaths)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                RebuildLayouts(scene);
                HashSet<Canvas> pokeCanvases = MeasurePokes(scene, pokes);
                MeasureTexts(scene, pokeCanvases, texts);
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(startPath))
                EditorSceneManager.OpenScene(startPath, OpenSceneMode.Single);
        }

        string folder = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", OutputFolder);
        Directory.CreateDirectory(folder);
        List<string> sameRole = MarkSameRole(texts);
        File.WriteAllText(Path.Combine(folder, "texts.tsv"), TextsTsv(texts), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(folder, "pokes.tsv"), PokesTsv(pokes), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(folder, "report.md"), Report(texts, pokes, sameRole), new UTF8Encoding(false));

        var summary = new StringBuilder($"TextComfortAudit: {texts.Count} texts, {pokes.Count} poke buttons in {scenePaths.Length} scenes -> {OutputFolder}/\n");
        foreach (string check in new[] { "T1", "T2", "T4", "T5", "T6", "T7", "T9", "T10", "T11", "T12" })
        {
            int fails = texts.Count(t => t.Fails.Any(f => f.StartsWith(check + " ")))
                + pokes.Count(p => p.Fails.Any(f => f.StartsWith(check + " ")));
            int warns = texts.Count(t => t.Warnings.Any(f => f.StartsWith(check + " ")))
                + pokes.Count(p => p.Warnings.Any(f => f.StartsWith(check + " ")));
            summary.AppendLine($"  {check}: {fails} fail, {warns} warn");
        }
        return summary.ToString();
    }

    // ---------- texts ----------

    private sealed class TextRow
    {
        public string Scene, ShortPath, FullPath, Key, Text, Canvas, State;
        public bool Active, Hidden, Is3D, AutoSize, OnPokeCanvas, OnPackage;
        public Vector3 Centre;
        public float Distance, Yaw, Pitch, Facing, EmMm, EmDeg, CapMm = float.NaN, FontSize, CanvasScale = float.NaN;
        public float Contrast = float.NaN;
        public string ContrastNote = "";
        public readonly List<string> Fails = new();
        public readonly List<string> Warnings = new();
        public readonly List<string> Exempt = new();
    }

    private static void MeasureTexts(Scene scene, HashSet<Canvas> pokeCanvases, List<TextRow> rows)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                RectTransform rect = text.rectTransform;
                var row = new TextRow
                {
                    Scene = scene.name,
                    FullPath = PathOf(text.transform),
                    ShortPath = ShortPath(text.transform),
                    Active = text.gameObject.activeInHierarchy,
                    Is3D = !(text is TextMeshProUGUI),
                    AutoSize = text.enableAutoSizing,
                    FontSize = text.fontSize,
                    Key = KeyOf(text),
                    Text = Clean(text.text, 50),
                    Canvas = text.canvas != null ? text.canvas.name : "-",
                    CanvasScale = text.canvas != null ? text.canvas.transform.lossyScale.x : float.NaN,
                    Centre = rect.TransformPoint(rect.rect.center),
                };
                row.State = row.Active ? "active" : "inactive";
                row.Hidden = IsHidden(text);
                row.OnPokeCanvas = text.canvas != null && pokeCanvases.Contains(text.canvas.rootCanvas);
                row.OnPackage = text.GetComponentInParent<PackageData>(true) != null;

                Vector3 toText = row.Centre - Eye;
                row.Distance = toText.magnitude;
                row.Yaw = Mathf.Atan2(toText.x, toText.z) * Mathf.Rad2Deg;
                row.Pitch = Mathf.Atan2(toText.y, new Vector2(toText.x, toText.z).magnitude) * Mathf.Rad2Deg;
                row.Facing = Vector3.Angle(rect.forward, toText.normalized);

                // Same formula as the #125 measurement (TestEvidence/bl125/texts.tsv); right for UI text only.
                float em = text.fontSize * rect.lossyScale.y;
                row.EmMm = em * 1000f;
                row.EmDeg = Angle(em, row.Distance);
                row.CapMm = CapHeight(text) * 1000f;
                if (row.Is3D && !float.IsNaN(row.CapMm))
                {
                    // A 3D TextMeshPro uses another size unit: take the em from the rendered capitals.
                    row.EmMm = row.CapMm / CapPerEm;
                    row.EmDeg = Angle(row.EmMm / 1000f, row.Distance);
                }

                if (!row.Is3D)
                    Contrast((TextMeshProUGUI)text, row);

                Rate(row);
                rows.Add(row);
            }
        }
    }

    private static void Rate(TextRow row)
    {
        if (row.Hidden)
            return;

        bool Exempted(string check)
        {
            if (row.OnPackage && PackageLabelChecks.Split(' ').Contains(check))
            {
                row.Exempt.Add($"{check} (label on a package, read when held)");
                return true;
            }
            foreach ((string part, string checks, string reason) in Exemptions)
            {
                if (row.FullPath.Contains(part) && checks.Split(' ').Contains(check))
                {
                    row.Exempt.Add($"{check} ({reason})");
                    return true;
                }
            }
            return false;
        }

        float d = row.Distance;
        bool middle = d >= MiddleMin && d < MiddleMax;
        if (row.OnPokeCanvas)
        {
            // Decision 040 addendum: a panel near the hands holds buttons only and is rated by horizontal reach
            // (the 0.5-0.8 m rule is about touch/ray handoff; the app has no ray). Reading text stays rated by T1.
            float reach = new Vector2(row.Centre.x - Eye.x, row.Centre.z - Eye.z).magnitude;
            if (!Exempted("T2") && reach > TouchMax)
                row.Fails.Add($"T2 poke panel {reach:0.00} m away horizontally, beyond 0.46 m touch reach");
        }
        else if (!Exempted("T1"))
        {
            if (middle)
                row.Fails.Add($"T1 read-only text at {d:0.00} m, in the 0.5-0.8 m middle zone");
            else if (d < MiddleMin)
                row.Fails.Add($"T1 read-only text at {d:0.00} m, closer than 0.5 m");
            else if (d < ReadMin)
                row.Warnings.Add($"T1 read-only text at {d:0.00} m, under 1.0 m");
        }

        bool sizeKnown = !row.Is3D || !float.IsNaN(row.CapMm);
        if (sizeKnown && !Exempted("T4"))
        {
            if (row.EmDeg < EmFloorDeg)
                row.Fails.Add($"T4 em {row.EmDeg:0.00} deg under the 0.5 deg floor");
            else if (row.EmDeg < EmTargetDeg)
                row.Warnings.Add($"T4 em {row.EmDeg:0.00} deg under the 0.8 deg target");
        }

        if (Mathf.Abs(row.Yaw) > YawMax && !Exempted("T5"))
            row.Fails.Add($"T5 yaw {row.Yaw:0} deg beyond 45 deg");

        if (!Exempted("T6"))
        {
            if (row.Pitch < PitchMin || row.Pitch > PitchMax)
                row.Fails.Add($"T6 pitch {row.Pitch:0} deg outside -60..+10 deg");
            else if (row.Pitch < PitchBest)
                row.Warnings.Add($"T6 pitch {row.Pitch:0} deg below -35 deg");
        }

        if (!Exempted("T7"))
        {
            if (row.Facing > FacingMax)
                row.Fails.Add($"T7 seen {row.Facing:0} deg off its face");
            else if (row.Facing > FacingWarn)
                row.Warnings.Add($"T7 seen {row.Facing:0} deg off its face");
        }

        if (row.AutoSize && !row.Is3D)
            row.Fails.Add("T10 auto size on: the font size changes with the text");

        if (!float.IsNaN(row.Contrast) && !Exempted("T11"))
        {
            bool large = row.EmDeg >= 1.2f;
            float needed = large ? ContrastLarge : ContrastBody;
            if (row.Contrast < needed)
                row.Fails.Add($"T11 contrast {row.Contrast:0.0}:1 under {needed:0.0}:1");
        }
    }

    // Never seen: the component or renderer is off, or a 3D text sits on a surface whose renderer is off (the hidden
    // front stickers of the cartons, kept for their collider).
    private static bool IsHidden(TMP_Text text)
    {
        if (!text.enabled)
            return true;
        if (text is TextMeshPro && text.TryGetComponent(out MeshRenderer renderer) && !renderer.enabled)
            return true;
        if (text is TextMeshPro && text.transform.parent != null
            && text.transform.parent.TryGetComponent(out MeshRenderer surface) && !surface.enabled)
            return true;
        return text.transform.GetComponentsInParent<CanvasGroup>(true).Any(group => group.alpha <= 0.01f && !group.ignoreParentGroups);
    }

    // World cap height of the rendered text: height of the first capital letter or digit, NaN when there is none.
    private static float CapHeight(TMP_Text text)
    {
        if (string.IsNullOrEmpty(text.text))
            return float.NaN;

        text.ForceMeshUpdate(true);
        TMP_TextInfo info = text.textInfo;
        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];
            if (!character.isVisible || !(char.IsUpper(character.character) || char.IsDigit(character.character)))
                continue;

            Vector3 bottom = text.transform.TransformPoint(new Vector3(0f, character.baseLine, 0f));
            Vector3 top = text.transform.TransformPoint(new Vector3(0f, character.topLeft.y, 0f));
            return Vector3.Distance(bottom, top);
        }
        return float.NaN;
    }

    // ---------- contrast (T11) ----------

    private static void Contrast(TextMeshProUGUI text, TextRow row)
    {
        Canvas canvas = text.canvas;
        if (canvas == null)
            return;

        List<Graphic> drawn = canvas.rootCanvas.GetComponentsInChildren<Graphic>(true).ToList();
        int own = drawn.IndexOf(text);
        if (own < 0)
            return;

        // Graphics drawn before the text whose rect holds the text's centre, bottom first.
        var behind = new List<Graphic>();
        for (int i = 0; i < own; i++)
        {
            Graphic graphic = drawn[i];
            if (graphic is TMP_Text || !graphic.enabled || graphic.color.a < 0.02f || !ShownWith(graphic.transform, text.transform))
                continue;
            if (graphic.TryGetComponent(out Mask mask) && mask.enabled && !mask.showMaskGraphic)
                continue;

            Vector3 local = graphic.rectTransform.InverseTransformPoint(row.Centre);
            if (graphic.rectTransform.rect.Contains(new Vector2(local.x, local.y)))
                behind.Add(graphic);
        }

        Color background = new(0.5f, 0.5f, 0.5f, 1f);
        float cover = 0f;
        string note = "";
        foreach (Graphic graphic in behind)
        {
            Color c = DrawnColour(graphic, ref note);
            background = Color.Lerp(background, c, c.a);
            cover = 1f - (1f - cover) * (1f - c.a);
        }

        Color foreground = text.color;
        Material face = text.fontSharedMaterial;
        if (face != null && face.HasProperty("_FaceColor"))
            foreground *= face.GetColor("_FaceColor");
        if (TryLeadingRichColour(text.text, out Color rich))
        {
            foreground = rich;
            note += "rich colour; ";
        }
        foreground = Color.Lerp(background, foreground, foreground.a);
        if (cover < 0.95f)
            note += "backdrop partly unknown (mid grey assumed); ";

        float lighter = Mathf.Max(Luminance(foreground), Luminance(background));
        float darker = Mathf.Min(Luminance(foreground), Luminance(background));
        row.Contrast = (lighter + 0.05f) / (darker + 0.05f);
        row.ContrastNote = note.TrimEnd(' ', ';');
    }

    // Colour a graphic paints at its centre: vertex colour × the Meta rounded-panel fill (material) × the sprite's
    // centre pixel.
    private static Color DrawnColour(Graphic graphic, ref string note)
    {
        Color colour = graphic.color;
        // A button's target graphic is tinted with its normal colour at runtime.
        Selectable owner = graphic.GetComponentInParent<Selectable>(true);
        if (owner != null && owner.targetGraphic == graphic && owner.transition == Selectable.Transition.ColorTint)
            colour *= owner.colors.normalColor * owner.colors.colorMultiplier;
        Material material = graphic.material;
        if (material != null && material.HasProperty("_FillColorA") && material.HasProperty("_FillColorB"))
        {
            Color fill = Color.Lerp(material.GetColor("_FillColorA"), material.GetColor("_FillColorB"), 0.5f);
            colour = new Color(colour.r * fill.r, colour.g * fill.g, colour.b * fill.b, colour.a * fill.a);
        }
        else if (material != null && material.HasProperty("_Color") && material != Graphic.defaultGraphicMaterial)
        {
            colour *= material.GetColor("_Color");
        }

        if (graphic is Image image && image.sprite != null)
        {
            if (TrySpriteCentre(image.sprite, out Color pixel))
                colour *= pixel;
            else if (!note.Contains("sprite"))
                note += "sprite pixels unread; ";
        }
        return colour;
    }

    private static readonly Dictionary<Sprite, Color> SpriteCentres = new();

    // Centre pixel of a sprite, read through a temporary render texture (textures are not CPU-readable).
    private static bool TrySpriteCentre(Sprite sprite, out Color pixel)
    {
        if (SpriteCentres.TryGetValue(sprite, out pixel))
            return true;

        Texture2D texture = sprite.texture;
        if (texture == null)
            return false;

        RenderTexture target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        RenderTexture previous = RenderTexture.active;
        var reader = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        try
        {
            Graphics.Blit(texture, target);
            RenderTexture.active = target;
            Rect area = sprite.textureRect;
            int x = Mathf.Clamp(Mathf.RoundToInt(area.center.x), 0, texture.width - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(area.center.y), 0, texture.height - 1);
            // Both textureRect and ReadPixels count from the bottom-left corner.
            reader.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
            reader.Apply();
            pixel = reader.GetPixel(0, 0);
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(reader);
        }

        SpriteCentres[sprite] = pixel;
        return true;
    }

    // The graphic is shown whenever the text is: every object between it and their common parent is active.
    private static bool ShownWith(Transform graphic, Transform text)
    {
        for (Transform t = graphic; t != null; t = t.parent)
        {
            if (text.IsChildOf(t))
                return true;
            if (!t.gameObject.activeSelf)
                return false;
        }
        return true;
    }

    private static bool TryLeadingRichColour(string value, out Color colour)
    {
        colour = default;
        if (string.IsNullOrEmpty(value))
            return false;

        string trimmed = value.TrimStart();
        if (!trimmed.StartsWith("<color=", System.StringComparison.OrdinalIgnoreCase))
            return false;

        int end = trimmed.IndexOf('>');
        if (end < 0)
            return false;

        string spec = trimmed.Substring(7, end - 7).Trim('"', '\'');
        return ColorUtility.TryParseHtmlString(spec, out colour);
    }

    // WCAG relative luminance of a gamma-space colour.
    private static float Luminance(Color c)
    {
        static float Linear(float v) => v <= 0.03928f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
        return 0.2126f * Linear(c.r) + 0.7152f * Linear(c.g) + 0.0722f * Linear(c.b);
    }

    // ---------- poke buttons (T12) ----------

    private sealed class PokeRow
    {
        public string Scene, Path, Canvas, State;
        public float Distance, WidthMm, HeightMm, MinDeg, SpacingMm = float.NaN;
        public bool Curved;
        public readonly List<string> Fails = new();
        public readonly List<string> Warnings = new();
    }

    // Measures every Selectable on a canvas that a PokeInteractable points at; returns those canvases.
    private static HashSet<Canvas> MeasurePokes(Scene scene, List<PokeRow> rows)
    {
        var canvases = new HashSet<Canvas>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (PokeInteractable poke in root.GetComponentsInChildren<PokeInteractable>(true))
            {
                Object element = Reference(poke, "_pointableElement");
                // The curved Menu pokes a PointableCanvasMesh that renders a flat canvas onto a cylinder.
                bool curved = element != null && element.GetType().Name.Contains("CanvasMesh");
                PointableCanvas pointable = element as PointableCanvas ?? FindPointableCanvas(element);
                // A canvas without buttons (a read-only board that happens to carry poke components) is no poke panel.
                if (pointable == null || pointable.Canvas == null
                    || pointable.Canvas.GetComponentInChildren<Selectable>(true) == null
                    || !canvases.Add(pointable.Canvas))
                    continue;

                MeasureButtons(scene.name, pointable.Canvas, curved, rows);
            }
        }
        return canvases;
    }

    private static PointableCanvas FindPointableCanvas(Object element)
    {
        if (element == null)
            return null;

        var serialized = new SerializedObject(element);
        SerializedProperty property = serialized.GetIterator();
        while (property.Next(true))
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is PointableCanvas found)
                return found;
        }
        return element is Component component ? component.GetComponentInChildren<PointableCanvas>(true) : null;
    }

    private static void MeasureButtons(string sceneName, Canvas canvas, bool curved, List<PokeRow> rows)
    {
        // Every button, also those on panels shown later; spacing only between buttons shown together.
        List<Selectable> buttons = canvas.GetComponentsInChildren<Selectable>(true)
            .Where(button => button.enabled).ToList();
        var visuals = buttons.ToDictionary(button => button, button => VisualRect(button, canvas.transform));
        float scale = Mathf.Abs(canvas.transform.lossyScale.x);

        foreach (Selectable button in buttons)
        {
            Graphic hit = HitGraphic(button);
            if (hit == null)
                continue;

            RectTransform rect = hit.rectTransform;
            Vector4 padding = hit.raycastPadding;
            float width = (rect.rect.width - padding.x - padding.z) * Mathf.Abs(rect.lossyScale.x);
            float height = (rect.rect.height - padding.y - padding.w) * Mathf.Abs(rect.lossyScale.y);
            Vector3 centre = rect.TransformPoint(rect.rect.center);
            float distance = Vector3.Distance(centre, Eye);

            var row = new PokeRow
            {
                Scene = sceneName,
                Path = PathOf(button.transform),
                Canvas = canvas.name,
                State = button.gameObject.activeInHierarchy ? "active" : "inactive",
                Curved = curved,
                Distance = distance,
                WidthMm = width * 1000f,
                HeightMm = height * 1000f,
                MinDeg = Angle(Mathf.Min(width, height), distance),
            };

            Rect own = visuals[button];
            foreach (Selectable other in buttons)
            {
                if (other == button || other.transform.IsChildOf(button.transform) || button.transform.IsChildOf(other.transform))
                    continue;
                if (!ShownWith(other.transform, button.transform) || !ShownWith(button.transform, other.transform))
                    continue;

                Rect next = visuals[other];
                float gapX = Mathf.Max(next.xMin - own.xMax, own.xMin - next.xMax);
                float gapY = Mathf.Max(next.yMin - own.yMax, own.yMin - next.yMax);
                float gap = Mathf.Max(0f, Mathf.Max(gapX, gapY)) * scale * 1000f;
                row.SpacingMm = float.IsNaN(row.SpacingMm) ? gap : Mathf.Min(row.SpacingMm, gap);
            }

            bool facilitator = row.Path.Contains("FacilitatorEndSession");
            if (!facilitator)
            {
                if (Mathf.Min(row.WidthMm, row.HeightMm) < HitMinMm)
                    row.Fails.Add($"T12 hit area {row.WidthMm:0}x{row.HeightMm:0} mm under 22 mm");
                if (row.MinDeg < HitMinDeg)
                    row.Fails.Add($"T12 hit area {row.MinDeg:0.0} deg under 2.5 deg");
                if (!float.IsNaN(row.SpacingMm) && row.SpacingMm < SpacingMinMm)
                    row.Warnings.Add($"T12 {row.SpacingMm:0} mm to the next button, under 12 mm");
            }
            rows.Add(row);
        }
    }

    private static Graphic HitGraphic(Selectable button)
    {
        if (button.targetGraphic != null && button.targetGraphic.raycastTarget)
            return button.targetGraphic;

        return button.GetComponentsInChildren<Graphic>(true)
            .Where(graphic => graphic.raycastTarget)
            .OrderByDescending(graphic => graphic.rectTransform.rect.width * graphic.rectTransform.rect.height)
            .FirstOrDefault();
    }

    // The visible button: its own rect plus every visible non-text graphic under it, in the canvas plane.
    private static Rect VisualRect(Selectable button, Transform space)
    {
        var rect = new Rect();
        bool first = true;
        var corners = new Vector3[4];
        foreach (Graphic graphic in button.GetComponentsInChildren<Graphic>(true))
        {
            if (!graphic.enabled || graphic.color.a < 0.05f || graphic is TMP_Text || !ShownWith(graphic.transform, button.transform))
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

    // ---------- output ----------

    private static string TextsTsv(List<TextRow> rows)
    {
        var sb = new StringBuilder();
        // The first 18 columns match TestEvidence/bl125/texts.tsv.
        sb.AppendLine("scene\tpath\tactive\tkey\ttext\tx\ty\tz\tdist\tyaw\tpitch\tfacing\temMm\temDeg\tautoSize\tfontSize\tcanvas\tcanvasScale"
            + "\thidden\tis3D\tcapMm\tpoke\tcontrast\tcontrastNote\tfails\twarnings\texempt\tfullPath");
        foreach (TextRow r in rows)
        {
            sb.AppendLine(string.Join("\t", r.Scene, r.ShortPath, r.Active ? "True" : "False", r.Key, r.Text,
                F(r.Centre.x, 2), F(r.Centre.y, 2), F(r.Centre.z, 2), F(r.Distance, 2), F(r.Yaw, 0), F(r.Pitch, 0), F(r.Facing, 0),
                F(r.EmMm, 1), F(r.EmDeg, 2), r.AutoSize ? "True" : "False", F(r.FontSize, 1), r.Canvas, F(r.CanvasScale, 5),
                r.Hidden ? "True" : "False", r.Is3D ? "True" : "False", F(r.CapMm, 1), r.OnPokeCanvas ? "True" : "False",
                F(r.Contrast, 1), r.ContrastNote, string.Join(" | ", r.Fails), string.Join(" | ", r.Warnings),
                string.Join(" | ", r.Exempt), r.FullPath));
        }
        return sb.ToString();
    }

    private static string PokesTsv(List<PokeRow> rows)
    {
        var sb = new StringBuilder("scene\tpath\tstate\tcanvas\tcurved\tdist\twidthMm\theightMm\tminDeg\tspacingMm\tfails\twarnings\n");
        foreach (PokeRow r in rows)
        {
            sb.AppendLine(string.Join("\t", r.Scene, r.Path, r.State, r.Canvas, r.Curved ? "True" : "False", F(r.Distance, 2),
                F(r.WidthMm, 0), F(r.HeightMm, 0), F(r.MinDeg, 1), F(r.SpacingMm, 0),
                string.Join(" | ", r.Fails), string.Join(" | ", r.Warnings)));
        }
        return sb.ToString();
    }

    // T9: the same object (same hierarchy path) in several scenes must stand at the same distance and yaw.
    private static List<string> MarkSameRole(List<TextRow> texts)
    {
        var lines = new List<string>();
        foreach (var group in texts.Where(t => !t.Hidden).GroupBy(t => t.FullPath))
        {
            var perScene = group.GroupBy(t => t.Scene).Select(s => s.First()).ToList();
            if (perScene.Count < 2)
                continue;

            float distanceSpread = perScene.Max(t => t.Distance) - perScene.Min(t => t.Distance);
            float yawSpread = perScene.Max(t => t.Yaw) - perScene.Min(t => t.Yaw);
            if (distanceSpread <= SameRoleDistance && yawSpread <= SameRoleYaw)
                continue;

            foreach (TextRow t in perScene)
                t.Fails.Add($"T9 {distanceSpread:0.00} m / {yawSpread:0} deg spread across scenes");
            lines.Add($"- `{group.Key}`: " + string.Join(", ", perScene.Select(t => $"{t.Scene} {t.Distance:0.00} m yaw {t.Yaw:0}")));
        }
        return lines;
    }

    private static string Report(List<TextRow> texts, List<PokeRow> pokes, List<string> sameRole)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Text comfort audit");
        sb.AppendLine();
        sb.AppendLine($"Generated by `TextComfortAudit.Check()` on {System.DateTime.Now:yyyy-MM-dd HH:mm}. Thresholds and sources: `docs/text-comfort-checklist.md`. Eye at (0, 1.6, 0).");
        sb.AppendLine("Rows marked (inactive) are hidden at scene start and shown later. Hidden front stickers are left out.");
        sb.AppendLine("The curved `Menu` is measured on its flat source canvas, so its positions and sizes are approximate.");
        sb.AppendLine("Exemptions (with reasons) are in the `exempt` column of texts.tsv.");
        sb.AppendLine();

        foreach (string check in new[] { "T1", "T2", "T4", "T5", "T6", "T7", "T10", "T11" })
        {
            var hits = texts.Where(t => t.Fails.Concat(t.Warnings).Any(f => f.StartsWith(check + " "))).ToList();
            sb.AppendLine($"## {check} ({hits.Count} texts)");
            foreach (var group in hits.GroupBy(t => (t.Scene, Group(t.FullPath))).OrderBy(g => g.Key.Scene).ThenBy(g => g.Key.Item2))
            {
                IEnumerable<string> notes = group.SelectMany(t => t.Fails.Concat(t.Warnings).Where(f => f.StartsWith(check + " ")).Select(f => (t.Fails.Contains(f) ? "FAIL " : "warn ") + f.Substring(check.Length + 1)));
                string worst = notes.OrderBy(n => n).First();
                string state = group.All(t => !t.Active) ? " (inactive)" : "";
                sb.AppendLine($"- {group.Key.Scene} `{group.Key.Item2}`{state}: {group.Count()} texts, e.g. {worst}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"## T9 same object, different place across scenes ({sameRole.Count})");
        foreach (string line in sameRole)
            sb.AppendLine(line);
        if (sameRole.Count == 0)
            sb.AppendLine("- none");
        sb.AppendLine();

        var pokeHits = pokes.Where(p => p.Fails.Count + p.Warnings.Count > 0).ToList();
        sb.AppendLine($"## T12 poke buttons ({pokeHits.Count} of {pokes.Count})");
        foreach (PokeRow p in pokeHits.OrderBy(p => p.Scene).ThenBy(p => p.Path))
        {
            string curved = p.Curved ? " (curved Menu: sizes on the flat source canvas)" : "";
            sb.AppendLine($"- {p.Scene} `{p.Path}` ({p.State}){curved}: " + string.Join("; ", p.Fails.Select(f => "FAIL " + f.Substring(4)).Concat(p.Warnings.Select(w => "warn " + w.Substring(4)))));
        }
        sb.AppendLine();
        sb.AppendLine("Not checked by the tool: T3 (which texts are read one after another), T8 (head-facing scripts), runtime poses.");
        return sb.ToString();
    }

    // The part of a path that names the panel: the first 3 levels, enough to tell panels apart.
    private static string Group(string path)
    {
        string[] parts = path.Split('/');
        return string.Join("/", parts.Take(Mathf.Min(3, parts.Length - 1 > 0 ? parts.Length - 1 : 1)));
    }

    // ---------- helpers ----------

    private static void RebuildLayouts(Scene scene)
    {
        for (int pass = 0; pass < 3; pass++)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(false).Where(c => c.isRootCanvas))
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)canvas.transform);
            }
            Canvas.ForceUpdateCanvases();
        }
    }

    private static string KeyOf(TMP_Text text)
    {
        if (!text.TryGetComponent(out LocalizeStringEvent localize))
            return "";

        var entry = localize.StringReference.TableEntryReference;
        if (!string.IsNullOrEmpty(entry.Key))
            return entry.Key;
        try
        {
            var collection = UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(localize.StringReference.TableReference);
            return collection != null ? collection.SharedData.GetKey(entry.KeyId) : entry.KeyId.ToString();
        }
        catch (System.Exception)
        {
            return entry.KeyId.ToString();
        }
    }

    private static float Angle(float size, float distance)
    {
        return 2f * Mathf.Atan(size / 2f / Mathf.Max(distance, 0.001f)) * Mathf.Rad2Deg;
    }

    private static Object Reference(Component component, string field)
    {
        return new SerializedObject(component).FindProperty(field)?.objectReferenceValue;
    }

    private static string PathOf(Transform transform)
    {
        return transform.parent == null ? transform.name : PathOf(transform.parent) + "/" + transform.name;
    }

    // The object and up to 6 parents, as in TestEvidence/bl125/texts.tsv.
    private static string ShortPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        for (int i = 0; i < 6 && parent != null; i++, parent = parent.parent)
            path = parent.name + "/" + path;
        return path;
    }

    private static string Clean(string value, int max)
    {
        string text = (value ?? "").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");
        return text.Length > max ? text.Substring(0, max) : text;
    }

    private static string F(float value, int decimals)
    {
        return float.IsNaN(value) ? "" : value.ToString("F" + decimals, CultureInfo.InvariantCulture);
    }
}
