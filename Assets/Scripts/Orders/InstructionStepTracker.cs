/// <summary>
/// Step of the picking procedure the instruction shelf marks as current (#40, docs/decisions/017).
/// </summary>
public enum InstructionStep
{
    Read,
    Pick,

    // #97, decision 035: the red button confirmed the position as correct; the next position (if any) has not
    // started yet because the storage containers are still changing.
    Confirmed,

    Done
}

/// <summary>
/// #40, docs/decisions/017: which instruction card is current, driven only by actions (grab, packages in the
/// order container, position change, whole order done), never by whether the order is right. Card 2 stays
/// current until the position ends; card 3 only gets a "ready" hint while the container holds packages of the
/// current position.
/// #97, decision 035: which step cards carry a done mark. Card 1 once the read step is over (first grab, or
/// in Complex both quiz answers right); cards 2 and 3 only when the red button confirmed the position as correct,
/// never on the container contents alone. The marks stay until the next position starts.
/// Pure C#, covered by InstructionStepTrackerTests.
/// </summary>
public sealed class InstructionStepTracker
{
    private int packagesInBox;

    public InstructionStep Step { get; private set; } = InstructionStep.Read;

    // Hint on card 3: the container holds at least one package of the current position (any material).
    public bool ConfirmReady => Step == InstructionStep.Pick && packagesInBox > 0;

    // Done marks of the three step cards.
    public bool ReadDone => Step != InstructionStep.Read;

    public bool PickDone => Step == InstructionStep.Confirmed || Step == InstructionStep.Done;

    public bool ConfirmDone => PickDone;

    // Each method returns true when Step or ConfirmReady changed.
    public bool PackageGrabbed()
    {
        return ReadFinished();
    }

    // The read step is over without a grab (Complex position 1: both quiz answers right).
    public bool ReadFinished()
    {
        if (Step != InstructionStep.Read)
            return false;

        Step = InstructionStep.Pick;
        return true;
    }

    // Packages of the current position in the order container; finished positions' packages are not counted.
    public bool PackagesInBoxChanged(int count)
    {
        if (Step == InstructionStep.Confirmed || Step == InstructionStep.Done)
            return false;

        bool wasReady = ConfirmReady;
        InstructionStep oldStep = Step;

        packagesInBox = count;

        // A package can only get into the container by picking it.
        if (count > 0 && Step == InstructionStep.Read)
        {
            Step = InstructionStep.Pick;
        }

        return Step != oldStep || ConfirmReady != wasReady;
    }

    // The red button confirmed the current (not last) position as correct.
    public bool PositionConfirmed()
    {
        if (Step == InstructionStep.Confirmed || Step == InstructionStep.Done)
            return false;

        Step = InstructionStep.Confirmed;
        return true;
    }

    // After a correct position the WMS shows the next one: read it first.
    public bool PositionStarted()
    {
        if (Step == InstructionStep.Done)
            return false;

        bool changed = Step != InstructionStep.Read || ConfirmReady;
        Step = InstructionStep.Read;
        packagesInBox = 0;
        return changed;
    }

    public bool OrderCompleted()
    {
        if (Step == InstructionStep.Done)
            return false;

        Step = InstructionStep.Done;
        return true;
    }
}
