using System;
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
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string QuestionCartons = "Wie viele volle Kartons nehmen Sie für diese Position?";       // complex.quiz.q_cartons
    private const string QuestionLoose = "Wie viele lose Teile nehmen Sie zusätzlich?";                    // complex.quiz.q_loose
    private const string CartonOne = "1 Karton";                                                          // complex.quiz.option_carton_one
    private const string CartonsFormat = "{0} Kartons";                                                   // complex.quiz.option_cartons
    private const string PieceOne = "1 Teil";                                                             // complex.quiz.option_piece_one
    private const string PiecesFormat = "{0} Teile";                                                      // complex.quiz.option_pieces
    private const string WrongCartons =
        "<color=#EB5757><b>Nicht richtig.</b></color> Teilen Sie Soll durch VPE. Nur volle Kartons zählen."; // complex.quiz.wrong_cartons
    private const string WrongLoose =
        "<color=#EB5757><b>Nicht richtig.</b></color> Lose Teile = Soll minus (volle Kartons × VPE).";     // complex.quiz.wrong_loose
    private const string NextQuestion = "<color=#27AE60><b>Richtig!</b></color> Beantworten Sie jetzt die zweite Frage."; // complex.quiz.next
    private const string Done =
        "<color=#27AE60><b>Richtig!</b></color> Nehmen Sie jetzt die vollen Kartons und die losen Teile aus dem Lagerbehälter."; // complex.quiz.done
    private const string ProgressFormat = "Frage {0} von {1}";                                            // concept.quiz.progress
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
        questionText.text = index == 0 ? QuestionCartons : QuestionLoose;

        if (progressText != null)
        {
            progressText.text = string.Format(ProgressFormat, index + 1, 2);
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
        if (!isSetUp || IsAnswered)
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
            feedbackText.text = question == 0 ? WrongCartons : WrongLoose;
            RefreshLabels();
            return;
        }

        if (question == 0)
        {
            feedbackText.text = NextQuestion;
            ShowQuestion(1);
            return;
        }

        IsAnswered = true;
        feedbackText.text = Done;
        RefreshLabels();
        foreach (Toggle toggle in toggles)
        {
            toggle.interactable = false;
        }
        stock.RevealFirstSet();
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
            return value == 1 ? CartonOne : string.Format(CartonsFormat, value);

        return value == 1 ? PieceOne : string.Format(PiecesFormat, value);
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
