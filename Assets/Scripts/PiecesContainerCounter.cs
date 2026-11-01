using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PiecesContainerCounter : MonoBehaviour
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

    public IReadOnlyList<OrderLine> OrderLines => orderLines;

    // Closed cartons inside, of any material.
    public int CartonCount { get; private set; }

    // Pieces inside: every carton counts its VPE (#17 data), every loose piece counts one.
    public int TotalPieces { get; private set; }

    // #4: raised when a package enters or leaves, so an old result can be cleared.
    public event Action ContentsChanged;

    private void Start()
    {
        OrderContents.LogSetupProblems(orderLines, true, orderLineTexts, this);
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

    public OrderResult CheckOrder()
    {
        return OrderRule.Check(orderLines, OrderContents.From(objectsInside), true);
    }

    private void Recalculate()
    {
        CartonCount = 0;
        TotalPieces = 0;

        foreach (PackageData item in objectsInside)
        {
            if (item.packageType == PackageType.LooseItemDisplay)
            {
                TotalPieces++;
            }
            else
            {
                CartonCount++;
                TotalPieces += item.Vpe;
            }
        }

        UpdateDisplay();
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
