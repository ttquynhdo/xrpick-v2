using System;
using UnityEngine;

/// <summary>
/// #20, docs/decisions/010: the sample order for the view-only WMS dialog in ConceptScene. Numbers differ from
/// BasicPickingScene's order so the demo doesn't solve it.
/// #91, docs/decisions/030: Ist follows what lies in the order container (set by <see cref="CartonCountDemo"/>);
/// Rest is the system's stock after the full order and stays fixed (decision 001).
/// #100: Ist = the cartons the demo shows plus the sample-material pieces the trainee has put in (ConceptBoxHints).
/// </summary>
public class SampleWmsOrder : MonoBehaviour, IWmsOrderSource
{
    [Header("Sample order (fake data, B10)")]
    [SerializeField] private OrderLine line = new();

    [Tooltip("Rest in pieces: stock left in the storage container after the full order (decision 001).")]
    [SerializeField] private int restPieces;

    private int shownPieces;
    private int placedPieces;

    public OrderLine CurrentLine => line;

    public int IstPieces => shownPieces + placedPieces;

    public int RestPieces => restPieces;

    // Pieces of the cartons CartonCountDemo shows in the order container.
    public int ShownPieces => shownPieces;

    public event Action ContentsChanged;

    public void SetShownPieces(int pieces)
    {
        if (pieces == shownPieces)
            return;

        shownPieces = pieces;
        ContentsChanged?.Invoke();
    }

    // Pieces of the sample material the trainee has put into the order container (#100).
    public void SetPlacedPieces(int pieces)
    {
        if (pieces == placedPieces)
            return;

        placedPieces = pieces;
        ContentsChanged?.Invoke();
    }
}
