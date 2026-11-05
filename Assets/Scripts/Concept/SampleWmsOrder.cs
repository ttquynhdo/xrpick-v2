using System;
using UnityEngine;

/// <summary>
/// #20, docs/decisions/010: the sample order for the view-only WMS dialog in ConceptScene. Numbers differ from
/// BasicPickingScene's order so the demo doesn't solve it.
/// #91, docs/decisions/030: Ist follows what lies in the order container (set by <see cref="CartonCountDemo"/>);
/// Rest is the system's stock after the full order and stays fixed (decision 001).
/// </summary>
public class SampleWmsOrder : MonoBehaviour, IWmsOrderSource
{
    [Header("Sample order (fake data, B10)")]
    [SerializeField] private OrderLine line = new();

    [Tooltip("Rest in pieces: stock left in the storage container after the full order (decision 001).")]
    [SerializeField] private int restPieces;

    private int istPieces;

    public OrderLine CurrentLine => line;

    public int IstPieces => istPieces;

    public int RestPieces => restPieces;

    public event Action ContentsChanged;

    public void SetIstPieces(int pieces)
    {
        if (pieces == istPieces)
            return;

        istPieces = pieces;
        ContentsChanged?.Invoke();
    }
}
