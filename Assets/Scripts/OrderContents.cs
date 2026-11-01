using System.Collections.Generic;
using TMPro;
using UnityEngine;

// #6: links the scene counters to OrderRule (docs/decisions/005, 006).
public static class OrderContents
{
    // Turns the packages a counter sees into the plain input of OrderRule.
    public static ContainerContents From(IEnumerable<PackageData> packages)
    {
        var contents = new ContainerContents();

        foreach (PackageData package in packages)
        {
            if (package.packageType == PackageType.LooseItemDisplay)
            {
                contents.AddLoose(package.MaterialKey);
            }
            else
            {
                contents.AddFull(package.MaterialKey);
            }
        }

        return contents;
    }

    // Order lines are typed in the Inspector: log every mistake, naming the counter, so the WMS text
    // and the result can never disagree silently.
    public static void LogSetupProblems(IReadOnlyList<OrderLine> lines, bool countsLoose, TMP_Text[] lineTexts, Object context)
    {
        string owner = context.GetType().Name;

        foreach (string problem in OrderRule.FindLineProblems(lines, countsLoose))
        {
            Debug.LogError($"{owner}: {problem}", context);
        }

        // A typo in a key (e.g. Ring_D) would turn every confirm into WRONG_MATERIAL + TOO_FEW in the log.
        var keysInScene = new HashSet<string>();
        foreach (PackageData package in Object.FindObjectsByType<PackageData>(FindObjectsInactive.Include))
        {
            keysInScene.Add(package.MaterialKey);
        }
        foreach (OrderLine line in lines)
        {
            if (!string.IsNullOrEmpty(line.materialKey) && !keysInScene.Contains(line.materialKey))
            {
                Debug.LogError($"{owner}: no package in the scene has materialKey '{line.materialKey}'.", context);
            }
        }

        int textCount = lineTexts != null ? lineTexts.Length : 0;
        if (textCount != lines.Count)
        {
            Debug.LogError($"{owner}: orderLineTexts has {textCount} entries but there are {lines.Count} order lines.", context);
        }
    }
}
