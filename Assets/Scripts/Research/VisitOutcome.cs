using System.Collections.Generic;
using System.Text;

/// <summary>
/// Outcome counters of one scene visit (#28 step 2): numbers the outcome events and computes the outcome
/// columns of summary.csv by the rules of research-logging-spec §5. Pure C#, covered by VisitOutcomeTests.
/// A counted attempt is a quiz touch in ConceptScene and a rated red-button press elsewhere;
/// remainder-quiz touches and ignored presses never count.
/// </summary>
public sealed class VisitOutcome
{
    public const string WelcomeScene = "WelcomeScene";
    public const string ConceptScene = "ConceptScene";

    private readonly bool hasOutcome;
    private readonly bool countsQuiz;
    private readonly Dictionary<string, int> attemptsPerQuestion = new();
    private readonly Dictionary<string, int> codeCounts = new();
    private readonly List<string> codeOrder = new();

    private int confirmNo;
    private int quizNo;
    private int ignored;
    private int counted;
    private int currentPosition;
    private int attemptsInPosition;
    private bool? firstAttemptCorrect;
    private double? timeToFirstConfirmS;
    private int? attemptsToCorrect;
    private double? timeToCorrectS;

    public VisitOutcome(string sceneName)
    {
        hasOutcome = sceneName != WelcomeScene;
        countsQuiz = sceneName == ConceptScene;
    }

    public bool TaskCompleted => attemptsToCorrect.HasValue;

    public JsonFields Confirm(OrderResult result, int position, int positionCount, bool ekzPlacementWrong,
        double timeSinceSceneEnterS)
    {
        if (position != currentPosition)
        {
            currentPosition = position;
            attemptsInPosition = 0;
        }

        confirmNo++;
        attemptsInPosition++;
        List<OrderError> errors = OutcomeEvents.ConfirmErrors(result, ekzPlacementWrong);
        if (!countsQuiz)
        {
            Count(errors, timeSinceSceneEnterS);
        }

        return OutcomeEvents.ConfirmPressed(confirmNo, position, positionCount, attemptsInPosition, result, errors,
            timeSinceSceneEnterS);
    }

    public JsonFields Ignored(string reason, int position, double timeSinceSceneEnterS)
    {
        ignored++;
        return OutcomeEvents.ConfirmIgnored(reason, position, timeSinceSceneEnterS);
    }

    // attempts_in_position = rated presses at that position.
    public JsonFields PositionCompleted(int position)
    {
        return OutcomeEvents.PositionCompleted(position, position == currentPosition ? attemptsInPosition : 0);
    }

    // Returns null when the task was already completed in this visit: one task_completed per visit.
    public JsonFields Complete(double timeSinceSceneEnterS)
    {
        if (TaskCompleted)
        {
            return null;
        }

        attemptsToCorrect = counted;
        timeToCorrectS = timeSinceSceneEnterS;
        return OutcomeEvents.TaskCompleted(counted, timeToFirstConfirmS, timeSinceSceneEnterS);
    }

    public JsonFields QuizAnswer(string quiz, string questionId, string selected, bool correct,
        double timeSinceSceneEnterS)
    {
        quizNo++;
        string key = quiz + "/" + questionId;
        attemptsPerQuestion.TryGetValue(key, out int n);
        attemptsPerQuestion[key] = ++n;

        if (countsQuiz && quiz == OutcomeEvents.QuizConcept)
        {
            var errors = new List<OrderError>();
            if (!correct)
            {
                errors.Add(new OrderError(OrderErrorCode.WrongAnswer, null));
            }
            Count(errors, timeSinceSceneEnterS);
        }

        return OutcomeEvents.QuizAnswer(quizNo, quiz, questionId, selected, correct, n, timeSinceSceneEnterS);
    }

    /// <summary>Writes the outcome columns into the summary row (spec §5); Welcome leaves them all empty.</summary>
    public void FillSummary(VisitSummary row)
    {
        if (!hasOutcome)
        {
            return;
        }

        row.ConfirmCount = counted;
        row.ConfirmIgnored = countsQuiz ? (int?)null : ignored;
        row.FirstAttemptCorrect = firstAttemptCorrect;
        row.TimeToFirstConfirmS = timeToFirstConfirmS;
        row.AttemptsToCorrect = attemptsToCorrect;
        row.TimeToCorrectS = timeToCorrectS;
        row.Completed = TaskCompleted;
        row.ErrorCodesAll = ErrorCodesAll();
    }

    private void Count(IReadOnlyList<OrderError> errors, double timeSinceSceneEnterS)
    {
        // After completion nothing is rated any more; a late touch must not change the task's numbers.
        if (TaskCompleted)
        {
            return;
        }

        counted++;
        if (counted == 1)
        {
            firstAttemptCorrect = errors.Count == 0;
            timeToFirstConfirmS = timeSinceSceneEnterS;
        }

        foreach (OrderError error in errors)
        {
            string code = OrderErrorCodes.ToLogName(error.Code);
            if (!codeCounts.ContainsKey(code))
            {
                codeOrder.Add(code);
                codeCounts[code] = 0;
            }
            codeCounts[code]++;
        }
    }

    // "TOO_FEW:2;EMPTY:1" in order of first appearance; null (empty cell) when no counted attempt had an error.
    private string ErrorCodesAll()
    {
        if (codeOrder.Count == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        foreach (string code in codeOrder)
        {
            if (sb.Length > 0)
            {
                sb.Append(';');
            }
            sb.Append(code).Append(':').Append(codeCounts[code]);
        }

        return sb.ToString();
    }
}
