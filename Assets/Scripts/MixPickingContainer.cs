using System;
using System.Collections.Generic;
using UnityEngine;

public class MixedPickingContainer : MonoBehaviour, IWmsOrderSource, IPositionedOrder, IOrderBoxContents
{
    // #6, docs/decisions/005: the order is declared once, in WMS units (Soll pieces + VPE).
    [Header("Order")]
    [SerializeField] private List<OrderLine> orderLines = new();

    // Every package counts, also foreign material: it must make the order wrong.
    private readonly HashSet<PackageData> packagesInside = new();

    // #21, decision 012: packages of finished positions stay in the container but no longer count
    // for Ist or as foreign material of the current position.
    private readonly HashSet<PackageData> confirmedPackages = new();

    private OrderSequence sequence;

    public IReadOnlyList<OrderLine> OrderLines => orderLines;

    // #21: position number from 1, number of positions; for the result card and for #28.
    public int CurrentPosition => Sequence.Position;

    public int PositionCount => Sequence.Count;

    public bool IsLastPosition => Sequence.IsLast;

    // Pieces of the current position inside: every carton or bag counts its VPE (#17 data).
    public int IstPieces { get; private set; }

    // #10, decision 001: stock of the current material left in the scene after this position. Set when the
    // position starts, never changed by picking.
    public int RestPieces { get; private set; }

    public OrderLine CurrentLine => Sequence.CurrentLine;

    // #40: packages of the current position inside, of any material (instruction shelf hint).
    public int PackageCount
    {
        get
        {
            int count = 0;

            foreach (PackageData _ in CurrentPackages())
            {
                count++;
            }

            return count;
        }
    }

    private OrderSequence Sequence => sequence ??= new OrderSequence(orderLines);

    // #4: raised when a package enters or leaves, and when the position changes, so an old result is cleared
    // and the WMS dialog redraws.
    public event Action ContentsChanged;

    // #21: raised after the position changed, with the new position number. Storage boxes listen to it;
    // #28 will log it.
    public event Action<int> LineAdvanced;

    private void Start()
    {
        OrderContents.LogSetupProblems(orderLines, false, this);
        UpdateRest();
        Recalculate();
    }

    private void OnTriggerEnter(Collider other)
    {
        PackageData package = other.GetComponentInParent<PackageData>();

        if (package == null)
            return;

        if (packagesInside.Add(package))
        {
            OrderContainerLog.Changed(this, true, package, packagesInside.Count);
            Recalculate();
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
            OrderContainerLog.Changed(this, false, package, packagesInside.Count);
            Recalculate();
            ContentsChanged?.Invoke();
        }
    }

    public bool Contains(PackageData package) => packagesInside.Contains(package);

    // Rates only the current position (decision 012).
    public OrderResult CheckOrder()
    {
        return Sequence.CheckCurrent(OrderContents.From(CurrentPackages()), false);
    }

    // Called by the validator after a correct position. Returns false at the last position.
    public bool AdvanceToNextLine()
    {
        if (!Sequence.TryAdvance())
            return false;

        confirmedPackages.UnionWith(packagesInside);

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
        packagesInside.RemoveWhere(p => p == null || !p.isActiveAndEnabled);

        foreach (PackageData package in packagesInside)
        {
            if (!confirmedPackages.Contains(package))
                yield return package;
        }
    }

    private void Recalculate()
    {
        int pieces = 0;

        foreach (PackageData package in CurrentPackages())
        {
            pieces += package.Vpe;
        }

        IstPieces = pieces;
    }

    private void UpdateRest()
    {
        OrderLine line = CurrentLine;
        RestPieces = line != null ? OrderContents.StockInScene(line.materialKey) - line.soll : 0;
    }
}
