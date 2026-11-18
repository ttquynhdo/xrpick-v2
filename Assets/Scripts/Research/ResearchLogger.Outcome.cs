using System;

/// <summary>
/// Outcome events of #28 step 2 (decision 021, research-logging-spec §4.2). Scene scripts call these
/// unconditionally; without a running session or open scene visit every call is a silent no-op.
/// Numbering (attempt_no, attempt_in_position, attempt_in_question) and the summary columns come from the
/// visit's <see cref="VisitOutcome"/>.
/// </summary>
public sealed partial class ResearchLogger
{
    /// <summary>One rated red-button press, written before the position advances.</summary>
    /// <param name="ekzPlacementWrong">Basic: the quantity is right but a carton lies outside the snap places.</param>
    /// <param name="placement">#160: the snap places at an Empf Kz 003 position (Basic, Mixed position 1); null
    /// elsewhere.</param>
    /// <param name="feedbackKey">#161: Loc key of the result-card sentence this press shows.</param>
    public static void LogConfirmPressed(OrderResult result, int position, int positionCount,
        bool ekzPlacementWrong = false, PlacementInfo placement = null, string feedbackKey = null)
    {
        if (result == null)
        {
            return;
        }

        LogOutcome("confirm_pressed",
            o => o.Confirm(result, position, positionCount, ekzPlacementWrong, instance.SceneActiveSeconds(),
                placement, feedbackKey));
    }

    /// <summary>A press that was not rated; reason is one of the OutcomeEvents.Ignored* values.</summary>
    public static void LogConfirmIgnored(string reason, int position)
    {
        LogOutcome("confirm_ignored", o => o.Ignored(reason, position, instance.SceneActiveSeconds()));
    }

    public static void LogPositionCompleted(int position)
    {
        LogOutcome("position_completed", o => o.PositionCompleted(position));
    }

    /// <summary>Once per visit: picking scenes at OrderCompleted, ConceptScene when all questions are right.</summary>
    public static void LogTaskCompleted()
    {
        LogOutcome("task_completed", o => o.Complete(instance.SceneActiveSeconds()));
    }

    /// <summary>One rated quiz touch; quiz is OutcomeEvents.QuizConcept or QuizComplexRemainder.</summary>
    public static void LogQuizAnswer(string quiz, string questionId, string selected, bool correct)
    {
        LogOutcome("quiz_answer",
            o => o.QuizAnswer(quiz, questionId, selected, correct, instance.SceneActiveSeconds()));
    }

    public static void LogQuizUnlocked()
    {
        Log("quiz_unlocked", new JsonFields());
    }

    /// <param name="stepIndex">From 1, as shown on the tour board.</param>
    public static void LogTourStep(string stepId, int stepIndex)
    {
        Log("tour_step", OutcomeEvents.TourStep(stepId, stepIndex));
    }

    /// <summary>A machine-driven ride; what is one of the OutcomeEvents.Move* values.</summary>
    public static void LogStationMove(bool arrived, string what, int position)
    {
        Log(arrived ? "station_move_arrived" : "station_move_start", OutcomeEvents.StationMove(what, position));
    }

    /// <summary>First grab of the box / first red-button press in WelcomeScene (#39).</summary>
    public static void LogWelcomePracticeDone(string action)
    {
        Log("welcome_practice_done", OutcomeEvents.WelcomePracticeDone(action));
    }

    /// <summary>WelcomeScene „Zu den Aufgaben“ pressed (#121, decision 038); once per visit.</summary>
    public static void LogWelcomeTasksOpened()
    {
        LogOutcome("welcome_tasks_opened", o => OutcomeEvents.WelcomeTasksOpened(instance.SceneActiveSeconds()));
    }

    private static void LogOutcome(string eventName, Func<VisitOutcome, JsonFields> build)
    {
        if (!IsSessionActive || !instance.visitOpen || instance.writeFailed)
        {
            return;
        }

        JsonFields data;
        try
        {
            data = build(instance.outcome);
        }
        catch (Exception e)
        {
            instance.StopAfterException(eventName, e);
            return;
        }

        // null: nothing to write (a second task_completed in the same visit).
        if (data != null)
        {
            instance.WriteEvent(eventName, data);
        }
    }
}
