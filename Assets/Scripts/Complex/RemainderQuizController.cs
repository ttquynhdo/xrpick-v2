using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// #19, docs/decisions/016: before position 1 of ComplexScene the trainee answers how many full cartons and
/// how many loose pieces the order line needs. A/B/C toggles show the answers (decision 015 style) and are
/// rated as soon as one is switched on; the red button stays F6 for the order. Both answers right → the
/// first storage box grows in. Answers come from the order line (RemainderQuiz), so the numbers live only
/// in orderLines. Every rated touch is logged as quiz_answer (#28, decision 021). #85, decision 024 (5) as in
/// ConceptScene: after the right first answer „Richtig!“ stays under question 1 for
/// PackageQuizController.CorrectHoldSeconds with the toggles not interactable, then question 2 comes with the
/// neutral how-to line.
/// #93, decision 026 (6): once both answers are right, shelf card 2 shows the result („2 Kartons + 5 Stück“) and,
/// after the same hold and once the storage box stands at the work spot, the cluster and the worked example hide.
/// #123, docs/decisions/039: a third question („Frage 3 von 3“) asks how the goods go into the order container; its
/// body never names the Empf Kz value, so the trainee has to read it in the LVS. The storage box comes only after
/// all three answers are right.
/// #130/#155 (decisions 040, 041): the worked example is a caption at the screen's depth (Screen Captions/Caption
/// example); the question and „Frage n von 3“ stand on the cluster's block right above the A/B/C row near the hands
/// (book layout), the feedback line under the buttons stays empty until an answer is touched. All fade out together.
/// </summary>
public class RemainderQuizController : MonoBehaviour
{
    // Loc table UI (#11, #54). The rich text (colour, bold) is part of the table entries. {count} = number.
    private const string QuestionCartonsKey = "complex.quiz.q_cartons";
    private const string QuestionLooseKey = "complex.quiz.q_loose";
    private const string QuestionPlacementKey = "complex.quiz.q_placement";
    private const string PlacementOptionKeyPrefix = "complex.quiz.placement.";   // + RemainderQuiz.Placement*
    private const string CartonOneKey = "complex.quiz.option_carton_one";
    private const string CartonsKey = "complex.quiz.option_cartons";
    private const string PieceOneKey = "complex.quiz.option_piece_one";
    private const string PiecesKey = "complex.quiz.option_pieces";
    private const string WrongCartonsKey = "complex.quiz.wrong_cartons";
    private const string WrongLooseKey = "complex.quiz.wrong_loose";
    private const string WrongPlacementKey = "complex.quiz.wrong_placement";
    private const string NextQuestionKey = "complex.quiz.next";
    private const string ThirdQuestionKey = "complex.quiz.next_third";
    private const string DoneKey = "complex.quiz.done";
    private const string ProgressKey = "concept.quiz.progress";
    private const string ResultCardKey = "complex.card.step2_body_answered";   // {cartons}, {pieces}
    private const string SelectedMark = "» ";

    private const string CartonsId = "cartons";
    private const string LooseId = "loose";
    private const string PlacementId = "ekz";

    private const int CartonsQuestion = 0;
    private const int LooseQuestion = 1;
    private const int PlacementQuestion = 2;
    private const int QuestionCount = 3;

    private static readonly string[] Letters = { "A", "B", "C" };

    [Header("Order")]
    [SerializeField] private PiecesContainerCounter container;
    [SerializeField] private PositionStockSwitcher stock;

    [Header("Answer Toggles")]
    [SerializeField] private Toggle optionAToggle;
    [SerializeField] private Toggle optionBToggle;
    [SerializeField] private Toggle optionCToggle;
    [SerializeField] private TMP_Text optionALabel;
    [SerializeField] private TMP_Text optionBLabel;
    [SerializeField] private TMP_Text optionCLabel;

    [Header("Texts")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text feedbackText;

    [Tooltip("Faded out once all answers are right and the storage box has arrived (#93): the A/B/C cluster, the question caption and the worked-example caption (#130).")]
    [SerializeField] private GameObject[] hideWhenPositionDone;

    [Tooltip("Body of shelf card 2; shows the trainee's result after both answers (#93). PositionTexts writes it again at position 2.")]
    [SerializeField] private TMP_Text resultCardText;

    private Toggle[] toggles;
    private TMP_Text[] labels;
    private UnityAction<bool>[] listeners;
    // Answer keys of the shown question, button order A, B, C: invariant numbers or RemainderQuiz.Placement*.
    private IReadOnlyList<string> options;
    private int question;
    private bool isSetUp;
    private bool switching;

    public bool IsAnswered { get; private set; }

    // Answer of the last switched-on toggle: an invariant integer, e.g. "2", or a placement key (for #28).
    public string LastAnswerKey { get; private set; }

    // Question id ("cartons" / "loose" / "ekz"), answer key, correct. #28 logs it.
    public event Action<string, string, bool> AnswerChosen;

    // #97, decision 035: all answers are right (raised once; three questions since #123).
    public event Action Answered;

    private void Awake()
    {
        toggles = new[] { optionAToggle, optionBToggle, optionCToggle };
        labels = new[] { optionALabel, optionBLabel, optionCLabel };
        listeners = new UnityAction<bool>[toggles.Length];

        for (int i = 0; i < toggles.Length; i++)
        {
            int index = i;
            listeners[i] = isOn => OnToggleChanged(index, isOn);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i] != null)
            {
                toggles[i].onValueChanged.AddListener(listeners[i]);
            }
        }

        if (container != null)
        {
            container.LineAdvanced += OnLineAdvanced;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i] != null)
            {
                toggles[i].onValueChanged.RemoveListener(listeners[i]);
            }
        }

        if (container != null)
        {
            container.LineAdvanced -= OnLineAdvanced;
        }

        // A disabled object stops its coroutines: never stay locked between the two questions.
        if (switching)
        {
            switching = false;
            SetTogglesInteractable(true);
        }
    }

    private void Start()
    {
        isSetUp = !HasMissingReferences();
        if (!isSetUp)
            return;

        // #123: the placement answer follows Empf Kz; an empty field would silently rate every answer wrong.
        if (string.IsNullOrEmpty(container.CurrentLine?.empfKz))
        {
            Debug.LogError($"{nameof(RemainderQuizController)}: the first order line has no Empf Kz, so the placement question cannot be rated.", this);
        }

        ShowQuestion(0);
        // #155, decision 041 (5): the line under the buttons stays empty until an answer is touched.
        feedbackText.text = string.Empty;
    }

    private bool HasMissingReferences()
    {
        return RequiredReference.IsMissing(container, nameof(container), this)
            | RequiredReference.IsMissing(stock, nameof(stock), this)
            | RequiredReference.IsMissing(optionAToggle, nameof(optionAToggle), this)
            | RequiredReference.IsMissing(optionBToggle, nameof(optionBToggle), this)
            | RequiredReference.IsMissing(optionCToggle, nameof(optionCToggle), this)
            | RequiredReference.IsMissing(optionALabel, nameof(optionALabel), this)
            | RequiredReference.IsMissing(optionBLabel, nameof(optionBLabel), this)
            | RequiredReference.IsMissing(optionCLabel, nameof(optionCLabel), this)
            | RequiredReference.IsMissing(questionText, nameof(questionText), this)
            | RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this)
            | RequiredReference.IsMissing(resultCardText, nameof(resultCardText), this);
    }

    private void ShowQuestion(int index)
    {
        question = index;
        OrderLine line = container.CurrentLine;
        options = index switch
        {
            CartonsQuestion => NumberKeys(RemainderQuiz.CartonOptions(line)),
            LooseQuestion => NumberKeys(RemainderQuiz.LooseOptions(line)),
            _ => RemainderQuiz.PlacementOptions,
        };
        questionText.text = Loc.Get(index switch
        {
            CartonsQuestion => QuestionCartonsKey,
            LooseQuestion => QuestionLooseKey,
            _ => QuestionPlacementKey,
        });

        if (progressText != null)
        {
            progressText.text = Loc.Get(ProgressKey, ("n", index + 1), ("total", QuestionCount));
        }

        foreach (Toggle toggle in toggles)
        {
            // With notify: the SDK's AnimatorOverrideLayerWeight only drops the "selected" look on the event.
            toggle.isOn = false;
        }

        RefreshLabels();
    }

    private void OnToggleChanged(int index, bool isOn)
    {
        // While the next question is one frame away, presses are not rated against the old options.
        if (!isSetUp || IsAnswered || switching)
            return;

        // Switching the other toggles off raises their events with isOn = false: nothing to rate.
        if (!isOn)
        {
            RefreshLabels();
            return;
        }

        for (int i = 0; i < toggles.Length; i++)
        {
            if (i != index)
            {
                toggles[i].isOn = false;
            }
        }

        OrderLine line = container.CurrentLine;
        string correct = question switch
        {
            CartonsQuestion => RemainderQuiz.FullCartons(line).ToString(CultureInfo.InvariantCulture),
            LooseQuestion => RemainderQuiz.LoosePieces(line).ToString(CultureInfo.InvariantCulture),
            _ => RemainderQuiz.PlacementAnswer(line),
        };

        LastAnswerKey = options[index];
        bool right = LastAnswerKey == correct;
        string questionId = question switch
        {
            CartonsQuestion => CartonsId,
            LooseQuestion => LooseId,
            _ => PlacementId,
        };
        ResearchLogger.LogQuizAnswer(OutcomeEvents.QuizComplexRemainder, questionId, LastAnswerKey, right);
        AnswerChosen?.Invoke(questionId, LastAnswerKey, right);

        if (!right)
        {
            feedbackText.text = Loc.Get(question switch
            {
                CartonsQuestion => WrongCartonsKey,
                LooseQuestion => WrongLooseKey,
                _ => WrongPlacementKey,
            });
            RefreshLabels();
            return;
        }

        if (question < PlacementQuestion)
        {
            feedbackText.text = Loc.Get(question == CartonsQuestion ? NextQuestionKey : ThirdQuestionKey);
            StartCoroutine(ShowNextQuestionAfterHold(question + 1));
            return;
        }

        IsAnswered = true;
        Answered?.Invoke();
        feedbackText.text = Loc.Get(DoneKey);
        RefreshLabels();
        foreach (Toggle toggle in toggles)
        {
            toggle.interactable = false;
        }
        resultCardText.text = Loc.Get(ResultCardKey,
            ("cartons", RemainderQuiz.FullCartons(line)), ("pieces", RemainderQuiz.LoosePieces(line)));
        stock.RevealFirstSet();
        StartCoroutine(HideAfterHold());
    }

    // #93: „Richtig!“ stays readable, and nothing changes in front of the trainee while the storage box rides in.
    private IEnumerator HideAfterHold()
    {
        yield return new WaitForSeconds(PackageQuizController.CorrectHoldSeconds);
        while (stock.IsMoving)
        {
            yield return null;
        }

        HideCluster();
    }

    // We are inside the pressed toggle's onValueChanged(true). The SDK's AnimatorOverrideLayerWeight listens after us,
    // so switching that toggle off here would reach it before the outer "true" and leave it looking selected.
    // #85: the wait is the "Richtig!" hold; touches meanwhile are not rated and not logged.
    private IEnumerator ShowNextQuestionAfterHold(int next)
    {
        switching = true;
        SetTogglesInteractable(false);
        yield return new WaitForSeconds(PackageQuizController.CorrectHoldSeconds);
        switching = false;
        SetTogglesInteractable(true);
        ShowQuestion(next);
        // #155, decision 041 (5): the line under the buttons stays empty until an answer is touched.
        feedbackText.text = string.Empty;
    }

    private void SetTogglesInteractable(bool interactable)
    {
        foreach (Toggle toggle in toggles)
        {
            if (toggle != null)
            {
                toggle.interactable = interactable;
            }
        }
    }

    // #32/#52: the chosen answer is marked with a text prefix and bold type, not by colour only.
    private void RefreshLabels()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            string answer = Letters[i] + ": " + AnswerText(options[i]);
            labels[i].text = toggles[i].isOn ? "<b>" + SelectedMark + answer + "</b>" : answer;
        }
    }

    private string AnswerText(string key)
    {
        if (question == PlacementQuestion)
            return Loc.Get(PlacementOptionKeyPrefix + key);

        int value = int.Parse(key, CultureInfo.InvariantCulture);
        if (question == CartonsQuestion)
            return value == 1 ? Loc.Get(CartonOneKey) : Loc.Get(CartonsKey, ("count", value));

        return value == 1 ? Loc.Get(PieceOneKey) : Loc.Get(PiecesKey, ("count", value));
    }

    private static IReadOnlyList<string> NumberKeys(IReadOnlyList<int> numbers)
    {
        var keys = new string[numbers.Count];
        for (int i = 0; i < numbers.Count; i++)
        {
            keys[i] = numbers[i].ToString(CultureInfo.InvariantCulture);
        }

        return keys;
    }

    // Also at the next position, in case the hold was cut short (object disabled).
    private void OnLineAdvanced(int position)
    {
        HideCluster();
    }

    private void HideCluster()
    {
        if (hideWhenPositionDone == null)
            return;

        foreach (GameObject target in hideWhenPositionDone)
        {
            PanelFade.SetVisible(target, false);
        }
    }
}
