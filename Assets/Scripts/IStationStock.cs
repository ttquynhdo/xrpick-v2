using System;

/// <summary>
/// #71, docs/decisions/019: storage containers that ride in and out on the station lanes. While one is moving,
/// the WMS dialog and the instruction shelf keep the old position and the red button does nothing; they catch up
/// on <see cref="Settled"/>.
/// </summary>
public interface IStationStock
{
    // True from a correct confirm until the next storage container has stopped and the old one has left the station.
    bool IsMoving { get; }

    // Raised once when IsMoving turns false.
    event Action Settled;
}
