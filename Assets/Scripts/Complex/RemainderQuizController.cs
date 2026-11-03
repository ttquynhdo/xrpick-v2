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
/// in orderLines. No logging yet (#28 reads AnswerChosen).
/// </summary>
public class RemainderQuizController : MonoBehaviour
{
    // Loc table UI (#11, #54). The rich text (colour, bold) is part of the table entries. {count} = number.
    private const string QuestionCartonsKey = "complex.quiz.q_cartons";
    private const string QuestionLooseKey = "complex.quiz.q_loose";
    private const string CartonOneKey = "complex.quiz.option_carton_one";
    private const string CartonsKey = "complex.quiz.option_cartons";
    private const string PieceOneKey = "complex.quiz.option_piece_one";
    private const string PiecesKey = "complex.quiz.option_pieces";
    private const string WrongCartonsKey = "complex.quiz.wrong_cartons";
    private const string WrongLooseKey = "complex.quiz.wrong_loose";
    private const string NextQuestionKey = "complex.quiz.next";
    private const string DoneKey = "complex.quiz.done";
    private const string ProgressKey = "concept.quiz.progress";
    private const string SelectedMark = "» ";

    private const string CartonsId = "cartons";
    private const string LooseId = "loose";

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

    [Tooltip("Hidden when position 1 is done: the question cluster and the worked-example board.")]
    [SerializeField] private GameObject[] hideWhenPositionDone;

    private Toggle[] toggles;
    private TMP_Text[] labels;
    private UnityAction<bool>[] listeners;
    private IReadOnlyList<int> options;
    private int question;
    private bool isSetUp;
    private bool switching;

    public bool IsAnswered { get; private set; }

    // Answer of the last switched-on toggle as an invariant integer, e.g. "2" (for #28).
    public string LastAnswerKey { get; private set; }

    // Question id ("cartons" / "loose"), answer key, correct. #28 will log it.
    public event Action<string, string, bool> AnswerChosen;

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
        switching = false;
    }

    private void Start()
    {
        isSetUp = !HasMissingReferences();
        if (!isSetUp)
            return;

        feedbackText.text = string.Empty;
        ShowQuestion(0);
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
            | RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this);
    }

    private void ShowQuestion(int index)
    {
        question = index;
        OrderLine line = container.CurrentLine;
        options = index == 0 ? RemainderQuiz.CartonOptions(line) : RemainderQuiz.LooseOptions(line);
        questionText.text = Loc.Get(index == 0 ? QuestionCartonsKey : QuestionLooseKey);

        if (progressText != null)
        {
            progressText.text = Loc.Get(ProgressKey, ("n", index + 1), ("total", 2));
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

        int answer = options[index];
        OrderLine line = container.CurrentLine;
        int correct = question == 0 ? RemainderQuiz.FullCartons(line) : RemainderQuiz.LoosePieces(line);
        bool right = answer == correct;

        LastAnswerKey = answer.ToString(CultureInfo.InvariantCulture);
        AnswerChosen?.Invoke(question == 0 ? CartonsId : LooseId, LastAnswerKey, right);

        if (!right)
        {
            feedbackText.text = Loc.Get(question == 0 ? WrongCartonsKey : WrongLooseKey);
            RefreshLabels();
            return;
        }

        if (question == 0)
        {
            feedbackText.text = Loc.Get(NextQuestionKey);
            StartCoroutine(ShowSecondQuestionNextFrame());
            return;
        }

        IsAnswered = true;
        feedbackText.text = Loc.Get(DoneKey);
        RefreshLabels();
        foreach (Toggle toggle in toggles)
        {
            toggle.interactable = false;
        }
        stock.RevealFirstSet();
    }

    // We are inside the pressed toggle's onValueChanged(true). The SDK's AnimatorOverrideLayerWeight listens after us,
    // so switching that toggle off here would reach it before the outer "true" and leave it looking selected.
    private IEnumerator ShowSecondQuestionNextFrame()
    {
        switching = true;
        yield return null;
        switching = false;
        ShowQuestion(1);
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

    private string AnswerText(int value)
    {
        if (question == 0)
            return value == 1 ? Loc.Get(CartonOneKey) : Loc.Get(CartonsKey, ("count", value));

        return value == 1 ? Loc.Get(PieceOneKey) : Loc.Get(PiecesKey, ("count", value));
    }

    private void OnLineAdvanced(int position)
    {
        if (hideWhenPositionDone == null)
            return;

        foreach (GameObject target in hideWhenPositionDone)
        {
            if (target != null)
            {
                target.SetActive(false);
            }
        }
    }
}
