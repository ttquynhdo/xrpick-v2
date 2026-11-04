using System;

/// <summary>
/// #42: a lesson without an order (WelcomeScene practice, ConceptScene quiz) raises one "lesson done" event.
/// Picking lessons use IOrderCompletion instead. MenuCurrentLesson unlocks the next lesson on either.
/// </summary>
public interface ILessonCompletion
{
    bool IsLessonCompleted { get; }

    event Action LessonCompleted;
}
