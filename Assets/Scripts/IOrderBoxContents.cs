using System;

/// <summary>
/// #40, docs/decisions/017: how many packages of the current position lie in the order container, of any
/// material. Implemented by the scene counters; the instruction shelf uses it for its "ready" hint.
/// </summary>
public interface IOrderBoxContents
{
    // Packages of finished positions (Mixed, Complex) are not counted.
    int PackageCount { get; }

    event Action ContentsChanged;
}
