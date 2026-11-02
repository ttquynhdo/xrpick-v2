/// <summary>
/// Step of the picking procedure the instruction shelf marks as current (#40, docs/decisions/017).
/// </summary>
public enum InstructionStep
{
    Read,
    Pick,
    Done
}

/// <summary>
/// #40, docs/decisions/017: which instruction card is current, driven only by actions (grab, packages in the
/// order container, position change, whole order done), never by whether the order is right. Card 2 stays
/// current until the position ends; card 3 only gets a "ready" hint while the container holds packages of the
/// current position. Pure C#, covered by InstructionStepTrackerTests.
/// </summary>
public sealed class InstructionStepTracker
{
    private int packagesInBox;

    public InstructionStep Step { get; private set; } = InstructionStep.Read;

    // Hint on card 3: the container holds at least one package of the current position (any material).
    public bool ConfirmReady => Step != InstructionStep.Done && packagesInBox > 0;

    // Each method returns true when Step or ConfirmReady changed.
    public bool PackageGrabbed()
    {
        if (Step != InstructionStep.Read)
            return false;

        Step = InstructionStep.Pick;
        return true;
    }

    // Packages of the current position in the order container; finished positions' packages are not counted.
    public bool PackagesInBoxChanged(int count)
    {
        if (Step == InstructionStep.Done)
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
