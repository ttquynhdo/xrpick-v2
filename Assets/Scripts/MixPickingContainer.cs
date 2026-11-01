using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MixedPickingContainer : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string PackagesPlacedFormat = "PACKAGE PLACED: {0}"; // mixed.counter.package_placed
    private const string OrderLineFormat =                            // mixed.wms.order_line
        "───────────────────\n{0}.\nMaterial          {1}\nOrder quantity    {2} pcs\nPackage size      {3} pcs / package\n\n───────────────────";

    // #6, docs/decisions/005: the order is declared once, in WMS units (Soll pieces + VPE).
    [Header("Order")]
    [SerializeField] private List<OrderLine> orderLines = new();

    [Header("WMS texts")]
    [SerializeField] private TMP_Text packageCountText;

    [Tooltip("One WMS text per order line, in the same order as Order Lines.")]
    [SerializeField] private TMP_Text[] orderLineTexts;

    // Every package counts, also foreign material: it must show up in PACKAGE PLACED and make the order wrong.
    private readonly HashSet<PackageData> packagesInside = new();

    public IReadOnlyList<OrderLine> OrderLines => orderLines;

    // #4: raised when a package enters or leaves, so an old result can be cleared.
    public event Action ContentsChanged;

    private void Start()
    {
        OrderContents.LogSetupProblems(orderLines, false, orderLineTexts, this);
        UpdateCounts();
        UpdateOrderTexts();
    }

    private void OnTriggerEnter(Collider other)
    {
        PackageData package = other.GetComponentInParent<PackageData>();

        if (package == null)
            return;

        if (packagesInside.Add(package))
        {
            UpdateCounts();
            ContentsChanged?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PackageData package = other.GetComponentInParent<PackageData>();

        if (package == null)
            return;

        if (packagesInside.Remove(package))
        {
            UpdateCounts();
            ContentsChanged?.Invoke();
        }
    }

    public OrderResult CheckOrder()
    {
        return OrderRule.Check(orderLines, OrderContents.From(packagesInside), false);
    }

    private void UpdateCounts()
    {
        if (packageCountText != null)
        {
            packageCountText.text = string.Format(PackagesPlacedFormat, packagesInside.Count);
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
                orderLineTexts[i].text = string.Format(OrderLineFormat, i + 1, line.materialName, line.soll, line.vpe);
            }
        }
    }
}
