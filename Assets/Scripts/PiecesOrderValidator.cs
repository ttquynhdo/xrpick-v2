using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class PiecesOrderValidator : MonoBehaviour, IOrderCompletion
{
    // #47: raised once when the last position is correct (#19); later confirms and content changes are ignored.
    public bool IsCompleted { get; private set; }

    public event Action OrderCompleted;

    // Loc table UI (#54). Last lesson: the Congratulations panel tells the trainee what to do next.
    private const string CorrectKey = "complex.feedback.correct";

    // #19, decision 016: {done} = finished position, {next} = next position. The middle line is the
    // Anbruchmenge reminder (Detail Description 5.12); it is not rated.
    private const string PositionDoneKey = "complex.feedback.position_done";

    // #58: red button before the questions below the screen are answered; not a wrong order.
    private const string AnswerQuizFirstKey = "complex.feedback.answer_quiz_first";

    [Header("References")]
    [SerializeField] private PiecesContainerCounter containerCounter;
    [SerializeField] private TMP_Text feedbackText;

    // #59: bold coloured first line („Richtig!“ / „Nicht richtig“) above feedbackText.
    [Tooltip("Optional title line of the result card; without it the title is written into feedbackText.")]
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private GameObject congratulationsPanel;

    // #10, decision 008: the WMS dialog has no instruction line; the task text stays on the Instruction Table.
    [Tooltip("Optional. Left empty when the scene uses the WMS dialog.")]
    [SerializeField] private TMP_Text instructionText;

    // #10 (#33 Complex part): result card next to the red button; hidden until a confirm.
    [Tooltip("Optional card that holds feedbackText; shown on confirm, hidden when the container changes.")]
    [SerializeField] private GameObject resultCard;

    // #58, decision 016: the storage container stays hidden until both questions are answered right.
    [Tooltip("Optional. Questions below the screen that must be answered before position 1 is rated.")]
    [SerializeField] private RemainderQuizController remainderQuiz;

    // #71, decision 019: while a storage container rides in or out, the red button does nothing.
    [Tooltip("Optional. Storage containers on the station lanes (IStationStock).")]
    [SerializeField] private MonoBehaviour stationStock;

    // #99, decision 033: the end panel stands in front of the monitor and would hide the middle lane while the
    // order container leaves, so it becomes visible only a little after that ride has started.
    [Tooltip("Optional. Order container conveyor; Congratulations stays invisible until it has started to move.")]
    [SerializeField] private OrderBoxConveyor orderBoxConveyor;

    [Tooltip("Seconds after the order container starts to move before Congratulations becomes visible.")]
    [SerializeField] private float congratulationsDelay = 2f;

    // A panel that never shows would hide the end of the course and the participant code.
    [Tooltip("Show Congratulations after this many seconds even if the order container never starts.")]
    [SerializeField] private float congratulationsFallback = 15f;

    private string initialInstruction;
    private CanvasGroup congratulationsGroup;
    private readonly List<MonoBehaviour> hiddenInteractables = new();
    private bool orderBoxDeparted;

    private void Awake()
    {
        if (instructionText != null)
        {
            initialInstruction = instructionText.text;
        }
    }

    private void OnEnable()
    {
        if (containerCounter != null)
        {
            containerCounter.ContentsChanged += ClearResult;
        }
    }

    private void OnDisable()
    {
        if (containerCounter != null)
        {
            containerCounter.ContentsChanged -= ClearResult;
        }
        if (orderBoxConveyor != null)
        {
            orderBoxConveyor.Departed -= OnOrderBoxDeparted;
        }
    }

    private void Start()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(false);
        }
        if (resultCard != null)
        {
            resultCard.SetActive(false);
        }
    }

    public void ConfirmOrder()
    {
        // #47: the order is shipped; the result stays as it is. #71: nothing to rate while a container moves.
        if (IsCompleted || (stationStock is IStationStock stock && stock.IsMoving))
        {
            ResearchLogger.LogConfirmIgnored(
                IsCompleted ? OutcomeEvents.IgnoredCompleted : OutcomeEvents.IgnoredStationMoving,
                containerCounter != null ? containerCounter.CurrentPosition : 0);
            return;
        }

        // Non-short-circuit | so every missing reference is logged at once.
        bool missing =
            RequiredReference.IsMissing(containerCounter, nameof(containerCounter), this) |
            RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this) |
            RequiredReference.IsMissing(congratulationsPanel, nameof(congratulationsPanel), this);

        if (missing)
            return;

        // #58: the order can't be picked yet, so tell the trainee what to do instead of rating an empty box.
        if (containerCounter.CurrentPosition == 1 && remainderQuiz != null && !remainderQuiz.IsAnswered)
        {
            ShowCard();
            ResultCardStyle.ShowPlain(titleText, feedbackText, Loc.Get(AnswerQuizFirstKey), ResultCardStyle.Reminder);
            ResearchLogger.LogConfirmIgnored(OutcomeEvents.IgnoredQuizFirst, containerCounter.CurrentPosition);
            return;
        }

        // #6: one rule decides right/wrong (docs/decisions/005, 006); #19: only the current position.
        OrderResult result = containerCounter.CheckOrder();

        // #28, decision 021: logged before the position advances.
        ResearchLogger.LogConfirmPressed(result, containerCounter.CurrentPosition, containerCounter.PositionCount);
        if (result.Correct)
        {
            ResearchLogger.LogPositionCompleted(containerCounter.CurrentPosition);
        }

        if (result.Correct && !containerCounter.IsLastPosition)
        {
            int finished = containerCounter.CurrentPosition;
            // Advancing raises ContentsChanged, which clears the card: write the result after it.
            containerCounter.AdvanceToNextLine();
            ShowCard();
            ResultCardStyle.Show(titleText, feedbackText, OrderFeedback.CorrectTitle(), ResultCardStyle.Correct,
                Loc.Get(PositionDoneKey, ("done", finished), ("next", containerCounter.CurrentPosition)));
            return;
        }

        ShowCard();

        if (result.Correct)
        {
            ResultCardStyle.Show(titleText, feedbackText, OrderFeedback.CorrectTitle(), ResultCardStyle.Correct,
                Loc.Get(CorrectKey));
            // Active at once (ParticipantCodeLabel reads the code before EndSession below), but invisible until
            // the order container has started to leave (#99).
            congratulationsPanel.SetActive(true);
            HideCongratulationsUntilDeparture();
            if (instructionText != null)
            {
                instructionText.text = string.Empty;
            }
            IsCompleted = true;
            ResearchLogger.LogTaskCompleted();
            OrderCompleted?.Invoke();
            // After the listeners, so the menu lock still sees the session; Congratulations is already active,
            // so ParticipantCodeLabel has read the code (#27). Rides that start later are not logged (spec 4.2).
            ResearchLogger.EndSession("completed");
            return;
        }

        // #14: the instruction stays visible; the feedback names the first error, never the target numbers.
        bool emptyLaterPosition = containerCounter.CurrentPosition > 1 && result.Errors[0].Code == OrderErrorCode.Empty;
        string reason = emptyLaterPosition
            ? OrderFeedback.ExplainEmptyPosition(containerCounter.CurrentLine)
            : OrderFeedback.Explain(result);
        ResultCardStyle.Show(titleText, feedbackText, OrderFeedback.Title(), ResultCardStyle.Wrong, reason);
        if (instructionText != null)
        {
            instructionText.text = initialInstruction;
        }

        // #4: a wrong order after a correct one must not leave the panel visible.
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(false);
        }
    }

    private void HideCongratulationsUntilDeparture()
    {
        if (orderBoxConveyor == null)
            return;

        if (!congratulationsPanel.TryGetComponent(out congratulationsGroup))
        {
            congratulationsGroup = congratulationsPanel.AddComponent<CanvasGroup>();
        }
        congratulationsGroup.alpha = 0f;
        congratulationsGroup.blocksRaycasts = false;

        // An invisible panel must not take touches either.
        foreach (MonoBehaviour behaviour in congratulationsPanel.GetComponentsInChildren<MonoBehaviour>())
        {
            if (behaviour is IInteractable && behaviour.enabled)
            {
                behaviour.enabled = false;
                hiddenInteractables.Add(behaviour);
            }
        }

        orderBoxDeparted = false;
        orderBoxConveyor.Departed += OnOrderBoxDeparted;
        StartCoroutine(ShowCongratulationsLater());
    }

    private void OnOrderBoxDeparted()
    {
        orderBoxConveyor.Departed -= OnOrderBoxDeparted;
        orderBoxDeparted = true;
    }

    private IEnumerator ShowCongratulationsLater()
    {
        float waited = 0f;
        while (!orderBoxDeparted && waited < congratulationsFallback)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        if (orderBoxDeparted && congratulationsDelay > 0f)
        {
            yield return new WaitForSeconds(congratulationsDelay);
        }

        if (orderBoxConveyor != null)
        {
            orderBoxConveyor.Departed -= OnOrderBoxDeparted;
        }
        if (congratulationsGroup != null)
        {
            congratulationsGroup.alpha = 1f;
            congratulationsGroup.blocksRaycasts = true;
        }
        foreach (MonoBehaviour behaviour in hiddenInteractables)
        {
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }
        hiddenInteractables.Clear();
    }

    private void ShowCard()
    {
        if (resultCard != null)
        {
            resultCard.SetActive(true);
        }
    }

    // #4: an old result must not stay visible once the container content changes.
    private void ClearResult()
    {
        if (IsCompleted)
            return;

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(false);
        }
        if (instructionText != null)
        {
            instructionText.text = initialInstruction;
        }
        if (resultCard != null)
        {
            resultCard.SetActive(false);
        }
    }
}