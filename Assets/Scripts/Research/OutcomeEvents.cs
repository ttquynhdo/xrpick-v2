using System.Collections.Generic;
using System.Text;

/// <summary>
/// Builds the data objects of the outcome events (#28 step 2, decision 021, research-logging-spec §4.2, §6).
/// Pure C#, so the shapes can be checked in EditMode tests without a running session.
/// </summary>
public static class OutcomeEvents
{
    // confirm_ignored.reason values.
    public const string IgnoredStationMoving = "station_moving";
    public const string IgnoredCompleted = "completed";
    public const string IgnoredQuizFirst = "quiz_first";

    // quiz_answer.quiz values.
    public const string QuizConcept = "concept";
    public const string QuizComplexRemainder = "complex_remainder";

    // station_move_*.what values.
    public const string MoveStorageOut = "storage_out";
    public const string MoveStorageIn = "storage_in";
    public const string MoveOrderBox = "order_box";

    // welcome_practice_done.action values.
    public const string PracticeGrab = "grab";
    public const string PracticeButton = "button";

    /// <summary>
    /// The error codes of one rated press: the OrderRule codes, then EKZ_PLACEMENT when the cartons don't lie
    /// ordered (Basic). The press is correct exactly when this list is empty (spec §6 invariant).
    /// </summary>
    public static List<OrderError> ConfirmErrors(OrderResult result, bool ekzPlacementWrong)
    {
        var errors = new List<OrderError>(result.Errors);
        if (ekzPlacementWrong)
        {
            errors.Add(new OrderError(OrderErrorCode.EkzPlacement, null));
        }

        return errors;
    }

    public static JsonFields ConfirmPressed(int attemptNo, int position, int positionCount, int attemptInPosition,
        OrderResult result, IReadOnlyList<OrderError> errors, double timeSinceSceneEnterS)
    {
        return new JsonFields()
            .Add("attempt_no", attemptNo)
            .Add("position", position)
            .Add("position_count", positionCount)
            .Add("attempt_in_position", attemptInPosition)
            .AddRaw("line", LineJson(result))
            .AddRaw("foreign", ForeignJson(result.Foreign))
            .Add("correct", errors.Count == 0)
            .AddRaw("error_codes", ErrorCodesJson(errors))
            .Add("time_since_scene_enter_s", Round(timeSinceSceneEnterS));
    }

    public static JsonFields ConfirmIgnored(string reason, int position, double timeSinceSceneEnterS)
    {
        return new JsonFields()
            .Add("reason", reason)
            .Add("position", position)
            .Add("time_since_scene_enter_s", Round(timeSinceSceneEnterS));
    }

    public static JsonFields PositionCompleted(int position, int attemptsInPosition)
    {
        return new JsonFields()
            .Add("position", position)
            .Add("attempts_in_position", attemptsInPosition);
    }

    public static JsonFields TaskCompleted(int attemptsToCorrect, double? timeToFirstConfirmS, double timeToCorrectS)
    {
        return new JsonFields()
            .Add("attempts_to_correct", attemptsToCorrect)
            .Add("time_to_first_confirm_s", timeToFirstConfirmS.HasValue ? Round(timeToFirstConfirmS.Value) : (double?)null)
            .Add("time_to_correct_s", Round(timeToCorrectS));
    }

    public static JsonFields QuizAnswer(int attemptNo, string quiz, string questionId, string selected, bool correct,
        int attemptInQuestion, double timeSinceSceneEnterS)
    {
        var errors = new List<OrderError>();
        if (!correct)
        {
            errors.Add(new OrderError(OrderErrorCode.WrongAnswer, null));
        }

        return new JsonFields()
            .Add("attempt_no", attemptNo)
            .Add("quiz", quiz)
            .Add("question_id", questionId)
            .Add("selected", selected)
            .Add("correct", correct)
            .Add("attempt_in_question", attemptInQuestion)
            .AddRaw("error_codes", ErrorCodesJson(errors))
            .Add("time_since_scene_enter_s", Round(timeSinceSceneEnterS));
    }

    public static JsonFields TourStep(string stepId, int stepIndex)
    {
        return new JsonFields()
            .Add("step_id", stepId)
            .Add("step_index", stepIndex);
    }

    public static JsonFields StationMove(string what, int position)
    {
        return new JsonFields()
            .Add("what", what)
            .Add("position", position);
    }

    public static JsonFields WelcomePracticeDone(string action)
    {
        return new JsonFields().Add("action", action);
    }

    /// <summary>[{"code":"TOO_FEW","item":"connector_a"}, …]; item null for codes without a material.</summary>
    public static string ErrorCodesJson(IReadOnlyList<OrderError> errors)
    {
        var sb = new StringBuilder("[");
        for (int i = 0; i < errors.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(new JsonFields()
                .Add("code", OrderErrorCodes.ToLogName(errors[i].Code))
                .Add("item", errors[i].Item)
                .ToJson());
        }

        return sb.Append(']').ToString();
    }

    // Current position only: the validators check one order line at a time.
    private static string LineJson(OrderResult result)
    {
        if (result.Lines.Count == 0)
        {
            return "null";
        }

        OrderLineResult line = result.Lines[0];
        bool countsLoose = line.PlacedLoose.HasValue;
        return new JsonFields()
            .Add("item", line.Line.materialKey)
            .Add("soll", line.Line.soll)
            .Add("vpe", line.Line.vpe)
            .Add("empf_kz", line.Line.empfKz)
            .Add("required_full", line.Line.RequiredFull)
            .Add("required_loose", countsLoose ? line.Line.RequiredLoose : (int?)null)
            .Add("placed_full", line.PlacedFull)
            .Add("placed_loose", line.PlacedLoose)
            .ToJson();
    }

    private static string ForeignJson(IReadOnlyList<ForeignMaterial> foreign)
    {
        var sb = new StringBuilder("[");
        for (int i = 0; i < foreign.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(new JsonFields()
                .Add("item", foreign[i].Item)
                .Add("count", foreign[i].Full + foreign[i].Loose)
                .ToJson());
        }

        return sb.Append(']').ToString();
    }

    private static double Round(double seconds)
    {
        return System.Math.Round(seconds, 3);
    }
}
