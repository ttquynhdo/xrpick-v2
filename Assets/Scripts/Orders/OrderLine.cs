using System;

/// <summary>
/// One order line in WMS units (docs/decisions/005): material, Soll in pieces, VPE (pieces per carton or bag).
/// Full cartons and loose pieces are derived, never declared. Public fields because this assembly has no
/// UnityEngine reference ([SerializeField] is not available); Unity still serializes them on the counters.
/// </summary>
[Serializable]
public class OrderLine
{
    // Stable, language-independent key (snake_case), equal to PackageData.MaterialKey (#17).
    public string materialKey;

    // Material name as on the label and in the WMS (fake data, not translated).
    public string materialName;

    public int soll;
    public int vpe;

    public OrderLine()
    {
    }

    public OrderLine(string materialKey, string materialName, int soll, int vpe)
    {
        this.materialKey = materialKey;
        this.materialName = materialName;
        this.soll = soll;
        this.vpe = vpe;
    }

    public int RequiredFull => vpe > 0 ? soll / vpe : 0;

    public int RequiredLoose => vpe > 0 ? soll % vpe : soll;
}
