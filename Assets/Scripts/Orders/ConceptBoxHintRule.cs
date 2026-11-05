using System.Collections.Generic;

/// <summary>Which container of the ConceptScene station a package lies in (#100).</summary>
public enum ConceptBox
{
    Order,
    Storage
}

/// <summary>Why a package in a ConceptScene container gets a hint (#100).</summary>
public enum ConceptHintReason
{
    WrongMaterial,
    TooMany
}

/// <summary>One package lying in a container right now: inside it and not held.</summary>
public readonly struct LyingPackage
{
    public LyingPackage(int id, ConceptBox box, string materialKey, PackageForm form, int vpe)
    {
        Id = id;
        Box = box;
        MaterialKey = materialKey;
        Form = form;
        Vpe = vpe;
    }

    public int Id { get; }

    public ConceptBox Box { get; }

    public string MaterialKey { get; }

    public PackageForm Form { get; }

    public int Vpe { get; }
}

/// <summary>The hint shown on one package.</summary>
public readonly struct ConceptHint
{
    public ConceptHint(int packageId, ConceptBox box, ConceptHintReason reason)
    {
        PackageId = packageId;
        Box = box;
        Reason = reason;
    }

    public int PackageId { get; }

    public ConceptBox Box { get; }

    public ConceptHintReason Reason { get; }
}

/// <summary>
/// #100, docs/decisions/030: what the ConceptScene station says about the packages the trainee puts into its two
/// containers. A package counts once it has lain in a container, not held, for <see cref="SettleSeconds"/>.
/// Order container: Ist = the pieces of the sample material shown by the demo plus those lying in it; another material
/// (opened cartons and loose pieces included) is wrong material, a package of the sample material that brings Ist above
/// Soll is too many. Storage container: another material is wrong material. Only one hint at a time: the package that
/// was laid down first. Pure C#, covered by ConceptBoxHintRuleTests.
/// </summary>
public sealed class ConceptBoxHintRule
{
    public const double SettleSeconds = 0.5;

    private readonly string materialKey;
    private readonly int sollPieces;
    private readonly Dictionary<int, Entry> entries = new();
    private readonly List<int> gone = new();

    public ConceptBoxHintRule(string materialKey, int sollPieces)
    {
        this.materialKey = materialKey;
        this.sollPieces = sollPieces;
    }

    /// <summary>Pieces of the sample material that have settled in the order container.</summary>
    public int PlacedPieces { get; private set; }

    /// <summary>The hint to show; null when nothing is wrong.</summary>
    public ConceptHint? Current { get; private set; }

    /// <summary>True when no package of another material lies in either container, settled or not.</summary>
    public bool NoWrongMaterial { get; private set; } = true;

    /// <summary>Settled packages of the sample material in the given container, in the order they were laid down.</summary>
    public List<int> SettledSampleIn(ConceptBox box)
    {
        var ids = new List<int>();
        foreach (Entry entry in Ordered())
        {
            if (entry.Package.Box == box && entry.Package.MaterialKey == materialKey && entry.IsSettled)
            {
                ids.Add(entry.Package.Id);
            }
        }

        return ids;
    }

    /// <param name="now">Seconds, any clock that only goes forward.</param>
    /// <param name="lying">Every package lying in a container now; a package missing from the list has left or is held.</param>
    /// <param name="shownPieces">Pieces of the sample material the demo shows in the order container.</param>
    public void Update(double now, IReadOnlyList<LyingPackage> lying, int shownPieces)
    {
        var seen = new HashSet<int>();
        foreach (LyingPackage package in lying)
        {
            seen.Add(package.Id);
            if (!entries.TryGetValue(package.Id, out Entry entry) || entry.Package.Box != package.Box)
            {
                entries[package.Id] = new Entry(package, now);
            }
        }

        gone.Clear();
        foreach (int id in entries.Keys)
        {
            if (!seen.Contains(id))
            {
                gone.Add(id);
            }
        }
        foreach (int id in gone)
        {
            entries.Remove(id);
        }

        Evaluate(now, shownPieces);
    }

    private void Evaluate(double now, int shownPieces)
    {
        int ist = shownPieces;
        int placed = 0;
        ConceptHint? first = null;
        NoWrongMaterial = true;

        foreach (Entry entry in Ordered())
        {
            entry.IsSettled = now - entry.Since >= SettleSeconds;
            LyingPackage package = entry.Package;
            bool sample = package.MaterialKey == materialKey;

            if (!sample)
            {
                NoWrongMaterial = false;
            }

            if (!entry.IsSettled)
                continue;

            ConceptHintReason? reason = null;
            if (!sample)
            {
                reason = ConceptHintReason.WrongMaterial;
            }
            else if (package.Box == ConceptBox.Order)
            {
                int pieces = Pieces(package);
                ist += pieces;
                placed += pieces;
                if (pieces > 0 && ist > sollPieces)
                {
                    reason = ConceptHintReason.TooMany;
                }
            }

            if (reason.HasValue && !first.HasValue)
            {
                first = new ConceptHint(package.Id, package.Box, reason.Value);
            }
        }

        PlacedPieces = placed;
        Current = first;
    }

    // Laid down first comes first; equal times by id, so results are deterministic.
    private List<Entry> Ordered()
    {
        var ordered = new List<Entry>(entries.Values);
        ordered.Sort((a, b) => a.Since != b.Since ? a.Since.CompareTo(b.Since) : a.Package.Id.CompareTo(b.Package.Id));
        return ordered;
    }

    // As the order counters: a carton or bag counts its VPE, a loose piece one, an opened carton nothing (#19).
    private static int Pieces(LyingPackage package) => package.Form switch
    {
        PackageForm.FullCarton => package.Vpe,
        PackageForm.LoosePiece => 1,
        _ => 0
    };

    private sealed class Entry
    {
        public Entry(LyingPackage package, double since)
        {
            Package = package;
            Since = since;
        }

        public LyingPackage Package { get; }

        public double Since { get; }

        public bool IsSettled { get; set; }
    }
}
