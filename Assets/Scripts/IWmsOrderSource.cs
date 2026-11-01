using System;

// #10, docs/decisions/008: what the WMS dialog shows for the current pick. Implemented by the scene counters.
public interface IWmsOrderSource
{
    OrderLine CurrentLine { get; }

    // Ist: pieces picked so far (weighing: grows while picking).
    int IstPieces { get; }

    // Rest: stock left in the storage container after the full order (decision 001), static while picking.
    int RestPieces { get; }

    event Action ContentsChanged;
}
