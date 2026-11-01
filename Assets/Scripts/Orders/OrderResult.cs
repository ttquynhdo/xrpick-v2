using System.Collections.Generic;

/// <summary>One error of an order check: code plus the material key it is about (null for EMPTY).</summary>
public readonly struct OrderError
{
    public OrderError(OrderErrorCode code, string item)
    {
        Code = code;
        Item = item;
    }

    public OrderErrorCode Code { get; }

    public string Item { get; }
}

/// <summary>Required and placed amounts for one order line. PlacedLoose is null when the scene does not count loose pieces.</summary>
public sealed class OrderLineResult
{
    public OrderLineResult(OrderLine line, int placedFull, int? placedLoose)
    {
        Line = line;
        PlacedFull = placedFull;
        PlacedLoose = placedLoose;
    }

    public OrderLine Line { get; }

    public int PlacedFull { get; }

    public int? PlacedLoose { get; }
}

/// <summary>A material in the container that has no order line: closed cartons/bags and loose pieces kept apart.</summary>
public readonly struct ForeignMaterial
{
    public ForeignMaterial(string item, int full, int loose)
    {
        Item = item;
        Full = full;
        Loose = loose;
    }

    public string Item { get; }

    public int Full { get; }

    public int Loose { get; }
}

/// <summary>
/// Result of <see cref="OrderRule.Check"/> (docs/decisions/006). Holds no display text and no target numbers
/// as sentences: screen text and log codes are both built from it.
/// </summary>
public sealed class OrderResult
{
    public OrderResult(List<OrderError> errors, List<OrderLineResult> lines, List<ForeignMaterial> foreign)
    {
        Errors = errors;
        Lines = lines;
        Foreign = foreign;
    }

    // Sorted by OrderErrorCodes.Priority; the first one is the one shown to the trainee.
    public IReadOnlyList<OrderError> Errors { get; }

    public IReadOnlyList<OrderLineResult> Lines { get; }

    public IReadOnlyList<ForeignMaterial> Foreign { get; }

    public bool Correct => Errors.Count == 0;
}
