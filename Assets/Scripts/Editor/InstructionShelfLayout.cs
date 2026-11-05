using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// #87 (decision 027): lays out the instruction cards on the InstructionShelf of the open scene.
/// Each card gets a fixed height from its longest text (DE and EN, every position of PositionTexts, the
/// Complex quiz result text), the cards hang from the first card's top with one gap for all scenes, the
/// board ends one margin below the last card and the post reaches up behind the board. The goal card
/// („Darum geht es“) gets the cream look of InstructionCardLook. Run it again after changing a card text.
/// </summary>
public static class InstructionShelfLayout
{
    // Metres in the shelf's Panel space (cards are scaled 0.0016 per UI unit).
    public const float CardGap = 0.012f;
    public const float BoardMargin = 0.015f;
    private const float PostOverlap = 0.05f;
    private const float GoalBorderThinness = 2f;

    private const string TableName = "UI";
    private const string GoalTitleKey = "common.card.task_title";
    private const string QuizResultKey = "complex.card.step2_body_answered";

    [MenuItem("XRPick/Instruction Shelf/Check cards in open scene")]
    private static void CheckMenu()
    {
        Debug.Log(LayOutOpenScene(true));
    }

    [MenuItem("XRPick/Instruction Shelf/Lay out cards in open scene")]
    private static void LayOutMenu()
    {
        Debug.Log(LayOutOpenScene(false));
    }

    /// <summary>Lays out the shelf of the active scene; with dryRun only reports. Returns a report.</summary>
    public static string LayOutOpenScene(bool dryRun)
    {
        Scene scene = SceneManager.GetActiveScene();
        Transform shelf = scene.GetRootGameObjects().Select(root => root.transform)
            .FirstOrDefault(root => root.name.StartsWith("InstructionShelf"));
        if (shelf == null)
            return $"InstructionShelfLayout: no InstructionShelf in {scene.name}.";

        Transform panel = shelf.Find("Panel");
        Transform cardsRoot = panel != null ? panel.Find("Cards") : null;
        Transform board = panel != null ? panel.Find("Board") : null;
        Transform post = shelf.Find("Post");
        if (cardsRoot == null || board == null || post == null)
            return $"InstructionShelfLayout: {scene.name} shelf lacks Panel/Cards, Panel/Board or Post.";
        if (cardsRoot.localPosition != Vector3.zero || cardsRoot.localScale != Vector3.one)
            return $"InstructionShelfLayout: {scene.name} Panel/Cards must sit at the Panel origin with scale 1.";

        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
        if (collection == null)
            return $"InstructionShelfLayout: no String Table Collection '{TableName}'.";

        List<RectTransform> cards = cardsRoot.Cast<Transform>()
            .Where(card => card.gameObject.activeSelf && card.GetComponent<Canvas>() != null)
            .Select(card => (RectTransform)card)
            .OrderByDescending(card => card.anchoredPosition3D.y)
            .ToList();
        if (cards.Count == 0)
            return $"InstructionShelfLayout: {scene.name} has no cards.";

        Dictionary<TMP_Text, List<string>> keysByText = KeysByText(scene, collection);
        var report = new StringBuilder($"InstructionShelfLayout {(dryRun ? "(dry run) " : "")}{scene.name}:\n");

        float top = cards[0].anchoredPosition3D.y;
        float cursor = top;
        foreach (RectTransform card in cards)
        {
            float height = Mathf.Ceil(CardHeight(card, keysByText, collection, report));
            float scale = card.localScale.y;
            Vector3 position = card.anchoredPosition3D;
            bool isGoal = IsGoalCard(card, keysByText);
            report.AppendLine($"  {card.name}{(isGoal ? " [goal]" : "")}: y {position.y:F4} -> {cursor:F4}, height -> {height:F1}");

            if (!dryRun)
            {
                FixHeight(card, height);
                Undo.RecordObject(card, "Lay out instruction cards");
                card.anchoredPosition3D = new Vector3(position.x, cursor, position.z);
                PrefabUtility.RecordPrefabInstancePropertyModifications(card);
                if (isGoal)
                    ApplyGoalLook(card);
            }

            cursor -= height * scale + CardGap;
        }

        float lastBottom = cursor + CardGap;
        float boardTop = top + BoardMargin;
        float boardBottom = lastBottom - BoardMargin;
        float boardSize = boardTop - boardBottom;
        // Board and post are unit primitives (cube extents 0.5, cylinder half height 1).
        float postHeight = panel.localPosition.y + boardBottom + PostOverlap;
        report.AppendLine($"  Board: centre y {board.localPosition.y:F4} -> {(boardTop + boardBottom) / 2f:F4}, height {board.localScale.y:F4} -> {boardSize:F4}");
        report.AppendLine($"  Post: height {post.localScale.y * 2f:F4} -> {postHeight:F4}");

        if (!dryRun)
        {
            Undo.RecordObject(board, "Lay out instruction cards");
            board.localPosition = new Vector3(board.localPosition.x, (boardTop + boardBottom) / 2f, board.localPosition.z);
            board.localScale = new Vector3(board.localScale.x, boardSize, board.localScale.z);
            PrefabUtility.RecordPrefabInstancePropertyModifications(board);

            Undo.RecordObject(post, "Lay out instruction cards");
            post.localPosition = new Vector3(post.localPosition.x, postHeight / 2f, post.localPosition.z);
            post.localScale = new Vector3(post.localScale.x, postHeight / 2f, post.localScale.z);
            PrefabUtility.RecordPrefabInstancePropertyModifications(post);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        return report.ToString();
    }

    // Height the card's VerticalLayoutGroup needs when every text shows its tallest candidate.
    private static float CardHeight(RectTransform card, Dictionary<TMP_Text, List<string>> keysByText,
        StringTableCollection collection, StringBuilder report)
    {
        var layout = card.GetComponent<VerticalLayoutGroup>();
        float height = layout.padding.top + layout.padding.bottom;
        int children = 0;
        // Layout-driven sizes are not saved, so a scene opened by code has zero rects until the next frame:
        // take the text width from the card width and the padding instead.
        float textWidth = card.sizeDelta.x - layout.padding.left - layout.padding.right;

        foreach (RectTransform child in card)
        {
            if (!child.gameObject.activeSelf)
                continue;
            LayoutElement element = child.GetComponent<LayoutElement>();
            if (element != null && element.ignoreLayout)
                continue;

            children++;
            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text == null || !keysByText.TryGetValue(text, out List<string> keys))
            {
                height += LayoutUtility.GetPreferredHeight(child);
                if (text != null)
                    report.AppendLine($"    note: {card.name}/{child.name} has no key, measured as it is");
                continue;
            }

            float tallest = 0f;
            foreach (string key in keys)
            {
                foreach (StringTable table in collection.StringTables)
                {
                    StringTableEntry entry = table.GetEntry(key);
                    if (entry == null)
                    {
                        report.AppendLine($"    missing: {key} ({table.LocaleIdentifier.Code})");
                        continue;
                    }

                    string value = entry.Value.Replace("{cartons}", "2").Replace("{pieces}", "5");
                    tallest = Mathf.Max(tallest, text.GetPreferredValues(value, textWidth, float.PositiveInfinity).y);
                }
            }

            height += tallest;
        }

        return height + layout.spacing * Mathf.Max(0, children - 1);
    }

    private static void FixHeight(RectTransform card, float height)
    {
        var fitter = card.GetComponent<ContentSizeFitter>();
        if (fitter != null)
        {
            Undo.RecordObject(fitter, "Lay out instruction cards");
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            PrefabUtility.RecordPrefabInstancePropertyModifications(fitter);
        }

        var layout = card.GetComponent<VerticalLayoutGroup>();
        Undo.RecordObject(layout, "Lay out instruction cards");
        layout.childAlignment = TextAnchor.UpperLeft;
        PrefabUtility.RecordPrefabInstancePropertyModifications(layout);

        Undo.RecordObject(card, "Lay out instruction cards");
        card.sizeDelta = new Vector2(card.sizeDelta.x, height);
        PrefabUtility.RecordPrefabInstancePropertyModifications(card);
    }

    private static bool IsGoalCard(RectTransform card, Dictionary<TMP_Text, List<string>> keysByText)
    {
        return card.GetComponentsInChildren<TMP_Text>(true)
            .Any(text => keysByText.TryGetValue(text, out List<string> keys) && keys.Contains(GoalTitleKey));
    }

    private static void ApplyGoalLook(RectTransform card)
    {
        var image = card.GetComponent<Image>();
        Undo.RecordObject(image, "Goal card look");
        image.color = InstructionCardLook.GoalCardColor;
        PrefabUtility.RecordPrefabInstancePropertyModifications(image);

        TMP_Text[] texts = card.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            Undo.RecordObject(texts[i], "Goal card look");
            texts[i].color = texts[i].name == "Primary Text" ? InstructionCardLook.GoalTitleColor : InstructionCardLook.GoalBodyColor;
            PrefabUtility.RecordPrefabInstancePropertyModifications(texts[i]);
        }

        Transform existing = card.Find(InstructionCardLook.GoalBorderName);
        GameObject border;
        if (existing != null)
        {
            border = existing.gameObject;
        }
        else
        {
            border = new GameObject(InstructionCardLook.GoalBorderName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            border.layer = card.gameObject.layer;
            Undo.RegisterCreatedObjectUndo(border, "Goal card look");
            border.transform.SetParent(card, false);
        }

        var rect = (RectTransform)border.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        border.GetComponent<LayoutElement>().ignoreLayout = true;

        var borderImage = border.GetComponent<Image>();
        borderImage.sprite = image.sprite;
        borderImage.type = Image.Type.Sliced;
        borderImage.fillCenter = false;
        borderImage.raycastTarget = false;
        borderImage.color = InstructionCardLook.GoalBorderColor;
        borderImage.pixelsPerUnitMultiplier = GoalBorderThinness;
        EditorUtility.SetDirty(border);
    }

    // Every key a card text can show: its LocalizeStringEvent, PositionTexts per position, and the Complex
    // quiz result line that RemainderQuizController writes into shelf card 2.
    private static Dictionary<TMP_Text, List<string>> KeysByText(Scene scene, StringTableCollection collection)
    {
        var keys = new Dictionary<TMP_Text, List<string>>();

        void Add(TMP_Text text, string key)
        {
            if (text == null || string.IsNullOrEmpty(key))
                return;
            if (!keys.TryGetValue(text, out List<string> list))
                keys[text] = list = new List<string>();
            if (!list.Contains(key))
                list.Add(key);
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (LocalizeStringEvent localized in root.GetComponentsInChildren<LocalizeStringEvent>(true))
            {
                string key = localized.StringReference.TableEntryReference.ResolveKeyName(collection.SharedData);
                Add(localized.GetComponent<TMP_Text>(), key);
            }

            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                    continue;

                string type = behaviour.GetType().Name;
                if (type == "PositionTexts")
                {
                    var serialized = new SerializedObject(behaviour);
                    var target = serialized.FindProperty("target").objectReferenceValue as TMP_Text;
                    SerializedProperty perPosition = serialized.FindProperty("keyPerPosition");
                    for (int i = 0; i < perPosition.arraySize; i++)
                        Add(target, perPosition.GetArrayElementAtIndex(i).stringValue);
                }
                else if (type == "RemainderQuizController")
                {
                    var serialized = new SerializedObject(behaviour);
                    Add(serialized.FindProperty("resultCardText").objectReferenceValue as TMP_Text, QuizResultKey);
                }
            }
        }

        return keys;
    }
}
