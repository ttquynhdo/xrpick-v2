using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

/// <summary>
/// #12: moves the fixed scene texts into the String Table Collection "UI".
/// ImportCsv copies docs/i18n/strings-todo.csv (key, context, en, de, max_chars) into the table.
/// BindOpenScene adds a LocalizeStringEvent to every TMP text whose German text matches a CSV row,
/// except texts a project script writes (they move to Loc.Get in #54/#55). Running both again
/// changes nothing new.
/// </summary>
public static class LocalizationBinder
{
    private const string TableName = "UI";
    private const string CsvPath = "docs/i18n/strings-todo.csv";
    private const string ProjectAssembly = "Assembly-CSharp";

    // Scene name -> key prefix used to pick between rows with the same German text.
    private static readonly Dictionary<string, string> ScenePrefixes = new()
    {
        { "WelcomeScene", "welcome." },
        { "ConceptScene", "concept." },
        { "BasicPickingScene", "basic." },
        { "MixedPickingScene", "mixed." },
        { "ComplexScene", "complex." },
    };

    [MenuItem("XRPick/Localization/Import strings CSV into UI table")]
    private static void ImportCsvMenu()
    {
        Debug.Log(ImportCsv());
    }

    [MenuItem("XRPick/Localization/Bind texts in open scene")]
    private static void BindOpenSceneMenu()
    {
        Debug.Log(BindOpenScene(false));
    }

    /// <summary>Writes every CSV row with real texts into the table. Returns a report.</summary>
    public static string ImportCsv()
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
        if (collection == null)
            return $"LocalizationBinder: no String Table Collection '{TableName}'.";

        int added = 0, changed = 0, skipped = 0, removed = 0;
        var csvKeys = new HashSet<string>();
        foreach (Row row in ReadRows())
        {
            if (!row.IsText)
            {
                skipped++;
                continue;
            }

            csvKeys.Add(row.Key);

            foreach (StringTable table in collection.StringTables)
            {
                string value = table.LocaleIdentifier.Code == "de" ? row.De : row.En;
                StringTableEntry entry = table.GetEntry(row.Key);
                if (entry == null)
                {
                    table.AddEntry(row.Key, value);
                    added++;
                }
                else if (entry.Value != value)
                {
                    entry.Value = value;
                    changed++;
                }
                else
                {
                    continue;
                }

                EditorUtility.SetDirty(table);
            }
        }

        // The CSV is the source: a key renamed or deleted there leaves the table too (#55).
        foreach (SharedTableData.SharedTableEntry shared in collection.SharedData.Entries.ToList())
        {
            if (csvKeys.Contains(shared.Key))
                continue;

            collection.RemoveEntry(shared.Key);
            removed++;
        }

        EditorUtility.SetDirty(collection.SharedData);
        foreach (StringTable table in collection.StringTables)
            EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        return $"LocalizationBinder: import added {added}, changed {changed}, removed {removed} keys not in the CSV; skipped {skipped} non-text rows.";
    }

    /// <summary>
    /// Binds the fixed texts of the active scene. With dryRun nothing changes. Returns a report
    /// listing bound, already bound, script-written and unmatched texts.
    /// </summary>
    public static string BindOpenScene(bool dryRun)
    {
        Scene scene = SceneManager.GetActiveScene();
        List<Row> rows = ReadRows().Where(r => r.IsText).ToList();
        HashSet<TMP_Text> written = ScriptWrittenTexts(scene);
        ScenePrefixes.TryGetValue(scene.name, out string prefix);

        var report = new StringBuilder($"LocalizationBinder {scene.name}{(dryRun ? " (dry run)" : "")}\n");
        int bound = 0, kept = 0;
        var skippedWritten = new List<string>();
        var unmatched = new List<string>();

        foreach (TMP_Text text in AllTexts(scene))
        {
            string value = Normalize(text.text);
            if (value.Length == 0 || !value.Any(char.IsLetter))
                continue;

            if (written.Contains(text))
            {
                skippedWritten.Add($"{PathOf(text.transform)}: \"{Short(value)}\"");
                continue;
            }

            Row row = Pick(rows.Where(r => Normalize(r.De) == value).ToList(), prefix);
            if (row == null)
            {
                unmatched.Add($"{PathOf(text.transform)}: \"{Short(value)}\"");
                continue;
            }

            var existing = text.GetComponent<LocalizeStringEvent>();
            if (existing != null && existing.StringReference.TableEntryReference.Key == row.Key)
            {
                kept++;
                continue;
            }

            if (!dryRun)
                Bind(text, existing, row.Key);
            report.AppendLine($"  bind {row.Key} -> {PathOf(text.transform)}");
            bound++;
        }

        if (!dryRun && bound > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        report.AppendLine($"bound {bound}, already bound {kept}, script-written {skippedWritten.Count}, unmatched {unmatched.Count}");
        report.AppendLine("script-written (left for #54/#55):");
        foreach (string line in skippedWritten)
            report.AppendLine("  " + line);
        report.AppendLine("unmatched (no CSV row with this German text):");
        foreach (string line in unmatched)
            report.AppendLine("  " + line);
        return report.ToString();
    }

    private static void Bind(TMP_Text text, LocalizeStringEvent existing, string key)
    {
        LocalizeStringEvent localize = existing != null ? existing : Undo.AddComponent<LocalizeStringEvent>(text.gameObject);
        localize.StringReference.SetReference(TableName, key);

        while (localize.OnUpdateString.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(localize.OnUpdateString, 0);

        var setter = (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), text, "set_text");
        UnityEventTools.AddPersistentListener(localize.OnUpdateString, setter);
        EditorUtility.SetDirty(localize);
        PrefabUtility.RecordPrefabInstancePropertyModifications(localize);
    }

    // A TMP referenced by a project script's serialized field, on the same GameObject as a project
    // script, or below MenuCurrentLesson (which finds its labels by text), gets its text from code:
    // a LocalizeStringEvent would overwrite it.
    private static HashSet<TMP_Text> ScriptWrittenTexts(Scene scene)
    {
        var result = new HashSet<TMP_Text>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || behaviour.GetType().Assembly.GetName().Name != ProjectAssembly)
                    continue;

                // Scripts that fetch their label with GetComponent (e.g. ParticipantCodeLabel).
                foreach (TMP_Text own in behaviour.GetComponents<TMP_Text>())
                    result.Add(own);

                if (behaviour is MenuCurrentLesson)
                {
                    foreach (TMP_Text label in behaviour.GetComponentsInChildren<TMP_Text>(true))
                        result.Add(label);
                }

                SerializedProperty property = new SerializedObject(behaviour).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue is TMP_Text referenced)
                    {
                        result.Add(referenced);
                    }
                }
            }
        }

        return result;
    }

    private static IEnumerable<TMP_Text> AllTexts(Scene scene)
    {
        return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<TMP_Text>(true));
    }

    // Same German text in several rows: the row of this scene first, then common.*, then the only one.
    private static Row Pick(List<Row> candidates, string scenePrefix)
    {
        if (candidates.Count <= 1)
            return candidates.FirstOrDefault();

        return candidates.FirstOrDefault(r => scenePrefix != null && r.Key.StartsWith(scenePrefix))
            ?? candidates.FirstOrDefault(r => r.Key.StartsWith("common."))
            ?? candidates[0];
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty).Replace("\r\n", "\n").Trim();
    }

    private static string Short(string value)
    {
        string flat = value.Replace("\n", "⏎");
        return flat.Length > 60 ? flat.Substring(0, 60) + "…" : flat;
    }

    private static string PathOf(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private sealed class Row
    {
        public string Key;
        public string En;
        public string De;

        // Pseudo rows ("= lesson.*", "(WMS labels, see prefab)") and label templates are not texts. A text may start
        // with "=" (#91: "= 3 Kartons"), so only "= <keys>.*" counts as a pseudo row.
        public bool IsText => !string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(De)
            && !(En.StartsWith("=") && En.EndsWith(".*")) && !En.StartsWith("(") && !Key.StartsWith("label.");
    }

    private static IEnumerable<Row> ReadRows()
    {
        string csv = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), CsvPath), Encoding.UTF8);
        List<List<string>> records = ParseCsv(csv);
        List<string> header = records[0];
        int key = header.IndexOf("key"), en = header.IndexOf("en"), de = header.IndexOf("de");
        foreach (List<string> record in records.Skip(1))
        {
            if (record.Count <= Math.Max(key, Math.Max(en, de)))
                continue;
            // Some rows write line breaks as a literal backslash-n.
            yield return new Row
            {
                Key = record[key].Trim(),
                En = record[en].Replace("\\n", "\n"),
                De = record[de].Replace("\\n", "\n"),
            };
        }
    }

    // RFC 4180: commas, double quotes, "" escapes, newlines inside quotes.
    private static List<List<string>> ParseCsv(string text)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (quoted)
            {
                // Git for Windows checks the file out with CRLF, also inside quoted line breaks.
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else if (c != '\r') field.Append(c);
                continue;
            }

            switch (c)
            {
                case '"': quoted = true; break;
                case ',': record.Add(field.ToString()); field.Clear(); break;
                case '\r': break;
                case '\n':
                    record.Add(field.ToString()); field.Clear();
                    records.Add(record); record = new List<string>();
                    break;
                default: field.Append(c); break;
            }
        }

        if (field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            records.Add(record);
        }

        return records;
    }
}
