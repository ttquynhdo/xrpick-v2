using System.Collections.Generic;

/// <summary>
/// Task card of the ConceptScene shelf (#79, docs/decisions/024 items 6 and 7).
/// </summary>
public enum ConceptCard
{
    Label,
    Wms,
    Quiz,
    None
}

/// <summary>
/// #79, docs/decisions/024 (6, 7) and 025: which ConceptScene task card is current and which are done, from
/// actions only. Card 1 is done once a package of every material was grabbed, card 2 when the tour first reaches
/// its last step (the questions open), card 3 when all questions are right. The current card moves forward only:
/// card 1 at the start, card 2 after the first "Weiter" or when card 1 is done, card 3 when the questions open,
/// none when they are all right. Done marks never go away. Pure C#, covered by ConceptCardTrackerTests.
/// </summary>
public sealed class ConceptCardTracker
{
    private readonly int materialsNeeded;
    private readonly HashSet<string> grabbedMaterials = new();

    public ConceptCardTracker(int materialsNeeded)
    {
        this.materialsNeeded = materialsNeeded;
    }

    public ConceptCard Current { get; private set; } = ConceptCard.Label;

    public bool LabelDone { get; private set; }

    public bool WmsDone { get; private set; }

    public bool QuizDone { get; private set; }

    // Each method returns true when Current or a done mark changed.
    public bool PackageGrabbed(string materialKey)
    {
        if (LabelDone || string.IsNullOrEmpty(materialKey) || !grabbedMaterials.Add(materialKey))
            return false;

        if (grabbedMaterials.Count < materialsNeeded)
            return false;

        LabelDone = true;
        MoveTo(ConceptCard.Wms);
        return true;
    }

    // The tour left its first step for the first time ("Weiter").
    public bool TourAdvanced() => MoveTo(ConceptCard.Wms);

    public bool QuizOpened()
    {
        bool changed = !WmsDone;
        WmsDone = true;
        return MoveTo(ConceptCard.Quiz) | changed;
    }

    public bool QuizCompleted()
    {
        if (QuizDone)
            return false;

        QuizDone = true;
        Current = ConceptCard.None;
        return true;
    }

    private bool MoveTo(ConceptCard card)
    {
        if (QuizDone || card <= Current)
            return false;

        Current = card;
        return true;
    }
}
