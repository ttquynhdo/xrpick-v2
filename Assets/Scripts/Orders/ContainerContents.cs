using System.Collections.Generic;

/// <summary>
/// What is inside the order container, per material key: closed cartons/bags and loose pieces.
/// A package without a material key is collected under <see cref="UnknownKey"/>.
/// </summary>
public sealed class ContainerContents
{
    public const string UnknownKey = "other";

    private readonly Dictionary<string, int> full = new();
    private readonly Dictionary<string, int> loose = new();
    private readonly List<string> keysInOrderSeen = new();

    public void AddFull(string materialKey)
    {
        Add(full, Normalize(materialKey));
    }

    public void AddLoose(string materialKey)
    {
        Add(loose, Normalize(materialKey));
    }

    public int FullOf(string materialKey) => full.TryGetValue(Normalize(materialKey), out int n) ? n : 0;

    public int LooseOf(string materialKey) => loose.TryGetValue(Normalize(materialKey), out int n) ? n : 0;

    public int TotalFull
    {
        get
        {
            int sum = 0;
            foreach (int n in full.Values)
            {
                sum += n;
            }
            return sum;
        }
    }

    public bool IsEmpty => full.Count == 0 && loose.Count == 0;

    // Keys in the order they were first added, so results are deterministic.
    public IReadOnlyList<string> Keys => keysInOrderSeen;

    private void Add(Dictionary<string, int> counts, string key)
    {
        counts.TryGetValue(key, out int n);
        counts[key] = n + 1;

        if (!keysInOrderSeen.Contains(key))
        {
            keysInOrderSeen.Add(key);
        }
    }

    private static string Normalize(string materialKey) =>
        string.IsNullOrEmpty(materialKey) ? UnknownKey : materialKey;
}
