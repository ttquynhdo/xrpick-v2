using System;
using UnityEngine;

/// <summary>
/// #20, docs/decisions/010: a fixed sample order for the view-only WMS dialog in ConceptScene. Nothing is picked
/// there, so the values never change. Numbers differ from BasicPickingScene's order so the demo doesn't solve it.
/// </summary>
public class SampleWmsOrder : MonoBehaviour, IWmsOrderSource
{
    [Header("Sample order (fake data, B10)")]
    [SerializeField] private OrderLine line = new();

    [Tooltip("Ist in pieces: what the scale would already show for this sample.")]
    [SerializeField] private int istPieces;

    [Tooltip("Rest in pieces: stock left in the storage container after the full order (decision 001).")]
    [SerializeField] private int restPieces;

    public OrderLine CurrentLine => line;

    public int IstPieces => istPieces;

    public int RestPieces => restPieces;

    // The sample never changes, so nobody is ever notified.
    public event Action ContentsChanged
    {
        add { }
        remove { }
    }
}
