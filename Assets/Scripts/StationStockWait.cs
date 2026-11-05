using System.Collections;

/// <summary>
/// #97, decision 035: shelf texts of the next position change only when the storage containers have stopped,
/// together with the WMS dialog and the step frame (one "page turn").
/// </summary>
public static class StationStockWait
{
    // LineAdvanced can reach a script before the storage containers hear it: look one frame later.
    public static IEnumerator UntilSettled(IStationStock stock)
    {
        yield return null;

        while (stock != null && stock.IsMoving)
        {
            yield return null;
        }
    }
}
