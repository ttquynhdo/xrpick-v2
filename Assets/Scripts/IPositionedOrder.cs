using System;
using System.Collections.Generic;

/// <summary>
/// #19, docs/decisions/012 and 016: an order container worked one position at a time (Mixed, Complex).
/// Storage boxes and per-position texts listen to it.
/// </summary>
public interface IPositionedOrder
{
    IReadOnlyList<OrderLine> OrderLines { get; }

    // Position number from 1.
    int CurrentPosition { get; }

    bool Contains(PackageData package);

    // Raised after the position changed, with the new position number.
    event Action<int> LineAdvanced;
}
