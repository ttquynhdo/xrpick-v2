using System.Collections.Generic;

/// <summary>
/// #160: the Empf Kz 003 snap places at one red-button press, for confirm_pressed.placement
/// (research-logging-spec §4.2). Object ids are the ones of grab_start (hierarchy paths). Pure data, so
/// research code stays free of scene types; EkzPlacementCheck fills it.
/// </summary>
public sealed class PlacementInfo
{
    public PlacementInfo(int places, IReadOnlyList<string> placed, IReadOnlyList<string> loose,
        IReadOnlyList<string> placesHeldByOther, bool placesShown)
    {
        Places = places;
        Placed = placed ?? new string[0];
        Loose = loose ?? new string[0];
        PlacesHeldByOther = placesHeldByOther ?? new string[0];
        PlacesShown = placesShown;
    }

    /// <summary>Snap places of the order container.</summary>
    public int Places { get; }

    /// <summary>Packages of the current position in the order container that sit in a place.</summary>
    public IReadOnlyList<string> Placed { get; }

    /// <summary>Packages of the current position in the order container that sit in no place.</summary>
    public IReadOnlyList<string> Loose { get; }

    /// <summary>Packages a place still holds although they are not counted in the order container.</summary>
    public IReadOnlyList<string> PlacesHeldByOther { get; }

    /// <summary>The place outlines were visible when the button was pressed.</summary>
    public bool PlacesShown { get; }

    public string ToJson()
    {
        return new JsonFields()
            .Add("places", Places)
            .AddStringList("placed", Placed)
            .AddStringList("loose", Loose)
            .AddStringList("places_held_by_other", PlacesHeldByOther)
            .Add("places_shown", PlacesShown)
            .ToJson();
    }
}
