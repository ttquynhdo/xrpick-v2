/// <summary>
/// #156: a tour marker that may refuse to be shown. <see cref="WmsFieldTour"/> asks it before it activates the marker,
/// so a marker that is already done is never switched on (not even for one frame) and then off again.
/// </summary>
public interface IStepMarkerGate
{
    /// <summary>False when the marker has nothing left to show; the tour then leaves it inactive.</summary>
    bool MayShow { get; }
}
