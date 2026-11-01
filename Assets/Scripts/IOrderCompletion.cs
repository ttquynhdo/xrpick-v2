using System;

/// <summary>
/// #47, docs/decisions/011: a validator raises one "order correct" event when the whole order is done
/// (used by the conveyor; later by #40 and #28). After that it ignores further confirms.
/// </summary>
public interface IOrderCompletion
{
    bool IsCompleted { get; }

    event Action OrderCompleted;
}
