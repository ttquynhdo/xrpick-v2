using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PiecesContainerCounter : MonoBehaviour, IWmsOrderSource, IPositionedOrder
{
    // #15: counters show only what is placed, never the required amount.
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string PiecesPlacedFormat = "Ist: {0} ST";            // complex.counter.ist
    private const string PackagesPlacedFormat = "Cartons placed: {0}"; // complex.counter.cartons_placed
    private const string OrderLineFormat =                              // complex.wms.order_line
        "───────────────────────\nProduct Material  {0}\n\nOrder quantity     {1} pcs\n\nPackage size       {2} pcs / package\n\n──────────────────────";

    // #6, docs/decisions/005: the order is declared once, in WMS units (Soll pieces + VPE).
    // Full cartons (Soll / VPE) and loose pieces (remainder) are derived by OrderRule.
    [Header("Order")]
    [SerializeField] private List<OrderLine> orderLines = new();

    [Header("Display")]
    [SerializeField] private TMP_Text piecesPlacedText;
    [SerializeField] private TMP_Text packagesPlacedText;

    [Tooltip("One WMS text per order line, in the same order as Order Lines.")]
    [SerializeField] private TMP_Text[] orderLineTexts;

    private readonly HashSet<PackageData> objectsInside = new();

    // #19, decisions 012 and 016: packages of finished positions stay in the container but no longer count
    // for Ist or as foreign material of the current position.
    private readonly HashSet<PackageData> confirmedPackages = new();

    private OrderSequence sequence;

    public IReadOnlyList<OrderLine> OrderLines => orderLines;

    // #19: position number from 1, number of positions; for the result card and for #28.
    public int CurrentPosition => Sequence.Position;

    public int PositionCount => Sequence.Count;

    public bool IsLastPosition => Sequence.IsLast;

    public OrderLine CurrentLine => Sequence.CurrentLine;

    // Closed cartons of the current position inside, of any material.
    public int CartonCount { get; private set; }

    // Pieces of the current position inside: every carton counts its VPE (#17 data), every loose piece
    // counts one, an opened carton counts nothing (#19).
    public int TotalPieces { get; private set; }

    // #10, decision 008: Rest = stock of the current material left in the scene after this position
    // (as CalcForWMS in Basic, decision 001). Set when the position starts, never changed by picking.
    public int RestPieces { get; private set; }

    int IWmsOrderSource.IstPieces => TotalPieces;

    private OrderSequence Sequence => sequence ??= new OrderSequence(orderLines);

    // #4: raised when a package enters or leaves, and when the position changes, so an old result is cleared
    // and the WMS dialog redraws.
    public event Action ContentsChanged;

    // #19: raised after the position changed, with the new position number. Storage boxes and the
    // per-position texts listen to it; #28 will log it.
    public event Action<int> LineAdvanced;

    private void Start()
    {
        OrderContents.LogSetupProblems(orderLines, true, orderLineTexts, this);
        UpdateRest();
        Recalculate();
        UpdateOrderTexts();
    }

    private void OnTriggerEnter(Collider other)
    {
        PackageData item = other.GetComponentInParent<PackageData>();

        if (item == null)
            return;

        if (objectsInside.Add(item))
        {
            Recalculate();
            ContentsChanged?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PackageData item = other.GetComponentInParent<PackageData>();

        if (item == null)
            return;

        if (objectsInside.Remove(item))
        {
            Recalculate();
            ContentsChanged?.Invoke();
        }
    }

    public bool Contains(PackageData package) => objectsInside.Contains(package);

    // Rates only the current position (decisions 012, 016).
    public OrderResult CheckOrder()
    {
        return Sequence.CheckCurrent(OrderContents.From(CurrentPackages()), true);
    }

    // Called by the validator after a correct position. Returns false at the last position.
    public bool AdvanceToNextLine()
    {
        if (!Sequence.TryAdvance())
            return false;

        confirmedPackages.UnionWith(objectsInside);

        // Listeners show the next storage box first, so its stock is in the scene when Rest is computed.
        LineAdvanced?.Invoke(CurrentPosition);

        UpdateRest();
        Recalculate();
        ContentsChanged?.Invoke();
        return true;
    }

    private IEnumerable<PackageData> CurrentPackages()
    {
        // A package destroyed or switched off while inside never sends OnTriggerExit.
        objectsInside.RemoveWhere(p => p == null || !p.isActiveAndEnabled);

        foreach (PackageData item in objectsInside)
        {
            if (!confirmedPackages.Contains(item))
                yield return item;
        }
    }

    private void Recalculate()
    {
        CartonCount = 0;
        TotalPieces = 0;

        foreach (PackageData item in CurrentPackages())
        {
            switch (OrderContents.Form(item.packageType))
            {
                case PackageForm.LoosePiece:
                    TotalPieces++;
                    break;
                case PackageForm.FullCarton:
                    CartonCount++;
                    TotalPieces += item.Vpe;
                    break;
            }
        }

        UpdateDisplay();
    }

    private void UpdateRest()
    {
        OrderLine line = CurrentLine;
        RestPieces = line != null ? OrderContents.StockInScene(line.materialKey) - line.soll : 0;
    }

    private void UpdateDisplay()
    {
        if (piecesPlacedText != null)
        {
            piecesPlacedText.text = string.Format(PiecesPlacedFormat, TotalPieces);
        }
        if (packagesPlacedText != null)
        {
            packagesPlacedText.text = string.Format(PackagesPlacedFormat, CartonCount);
        }
    }

    private void UpdateOrderTexts()
    {
        if (orderLineTexts == null)
            return;

        for (int i = 0; i < orderLineTexts.Length && i < orderLines.Count; i++)
        {
            OrderLine line = orderLines[i];

            if (orderLineTexts[i] != null)
            {
                orderLineTexts[i].text = string.Format(OrderLineFormat, line.materialName, line.soll, line.vpe);
            }
        }
    }
}
