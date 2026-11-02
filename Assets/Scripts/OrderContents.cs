using System.Collections.Generic;
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
            contents.Add(package.MaterialKey, Form(package.packageType));
        }

        return contents;
    }

    // #19, decision 016: the opened carton is never a full carton (its loose pieces count one by one).
    public static PackageForm Form(PackageType type)
    {
        switch (type)
        {
            case PackageType.LooseItemDisplay:
                return PackageForm.LoosePiece;
            case PackageType.VPEOpenedDisplay:
                return PackageForm.OpenedCarton;
            default:
                return PackageForm.FullCarton;
        }
    }

    // #10: pieces of one material in the scene: closed cartons count their VPE, loose pieces one each.
    // Opened display cartons are skipped, their loose pieces are counted on their own.
    // #19: hidden storage boxes count too (ComplexScene shows its first box only after the division question).
    public static int StockInScene(string materialKey)
    {
        int stock = 0;

        foreach (PackageData package in Object.FindObjectsByType<PackageData>(FindObjectsInactive.Include))
        {
            if (package.MaterialKey != materialKey)
                continue;

            if (package.packageType == PackageType.LooseItemDisplay)
            {
                stock++;
            }
            else if (package.packageType != PackageType.VPEFull && package.packageType != PackageType.VPEOpenedDisplay)
            {
                stock += package.Vpe;
            }
        }

        return stock;
    }

    // #10: the Mat.-Nr shown in the WMS must be the one printed on the cartons of the same material (#17).
    public static void LogMaterialNumberMismatch(OrderLine line, Object context)
    {
        if (line == null || string.IsNullOrEmpty(line.materialNumber))
            return;

        foreach (PackageData package in Object.FindObjectsByType<PackageData>(FindObjectsInactive.Include))
        {
            if (package.MaterialKey == line.materialKey && package.MaterialNumber != line.materialNumber)
            {
                Debug.LogError($"{context.GetType().Name}: Mat.-Nr {line.materialNumber} of order line '{line.materialKey}' differs from {package.MaterialNumber} on {package.name}.", package);
                return;
            }
        }
    }

    // Order lines are typed in the Inspector: log every mistake, naming the counter, so the WMS dialog
    // and the result can never disagree silently.
    public static void LogSetupProblems(IReadOnlyList<OrderLine> lines, bool countsLoose, Object context)
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
    }
}
