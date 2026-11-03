using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// #45 + #20 + #52 + #73, docs/decisions/007 (5), 010, 015 and 020: ConceptScene quiz. One question at a time above up
/// to three answer toggles (A–C) that show the answer text. A toggle switched on is rated at once, as in ComplexScene's
/// RemainderQuizController: a wrong answer names its reason, a correct one moves on to the next question. Feedback is a
/// line inside the cluster. The cluster stays locked until the WMS tour reaches its last step and stays open after that.
/// No logging yet (#28 reads AnswerChosen).
/// </summary>
public class PackageQuizController : MonoBehaviour
{
    // Loc table UI (#11, #54). The rich text (colour, bold) is part of the table entries.
    private const string ProgressKey = "concept.quiz.progress";
    private const string LockedKey = "concept.quiz.locked";
    private const string HowToKey = "concept.quiz.how_to";
    private const string CorrectKey = "concept.quiz.correct";
    private const string NextQuestionKey = "concept.quiz.next_question";
    private const string AllDoneKey = "concept.quiz.all_done";        // {lesson} = menu label of the next lesson
    private const string WrongKey = "concept.quiz.wrong";
    private const string NextScene = "BasicPickingScene";
    // #55: question concept.quiz.<id>.q, answer concept.quiz.<id>.<key>, reason concept.quiz.<id>.<key>.wrong.
    private const string QuestionKeyPrefix = "concept.quiz.";
    private const string SelectedMark = "» ";

    // Decision 015: each toggle shows its letter and answer, so the trainee reads and presses in one place.
    private static readonly string[] Letters = { "A", "B", "C" };

    [Serializable]
    public class Option
    {
        [Tooltip("Short, language-independent answer key (for #28).")]
        public string key;
    }

    [Serializable]
    public class Question
    {
        [Tooltip("Stable question id (for #28).")]
        public string id;

        public string correctKey;

        [Tooltip("Two or three answers; toggles without an answer are hidden.")]
        public Option[] options;
    }

    [SerializeField] private Question[] questions;

    [Header("Answer Toggles")]
    [FormerlySerializedAs("ggToggle")]
    [SerializeField] private Toggle optionAToggle;
    [FormerlySerializedAs("vpeToggle")]
    [SerializeField] private Toggle optionBToggle;
    [FormerlySerializedAs("bothToggle")]
    [SerializeField] private Toggle optionCToggle;

    [SerializeField] private TMP_Text optionALabel;
    [SerializeField] private TMP_Text optionBLabel;
    [SerializeField] private TMP_Text optionCLabel;

    [Header("Question cluster")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;

    [Tooltip("Instruction and feedback line inside the cluster (decision 020).")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("Unlock (decision 020)")]
    [SerializeField] private WmsFieldTour tour;

    [Tooltip("Tour step id that opens the questions for good.")]
    [SerializeField] private string unlockStepId = "material_flow";

    [Tooltip("Hidden while locked: the row with the answer toggles.")]
    [SerializeField] private GameObject answerArea;

    private Toggle[] toggles;
    private TMP_Text[] labels;
    private UnityAction<bool>[] listeners;
    private int current;
    private bool isSetUp;
    private bool switching;

    public bool IsUnlocked { get; private set; }

    public bool IsAnswered { get; private set; }

    // Question id, answer key, correct. #28 will log it.
    public event Action<string, string, bool> AnswerChosen;

    // Raised once when the questions open (#28: quiz_unlocked).
    public event Action Unlocked;

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

        if (tour != null)
        {
            tour.StepShown += OnTourStepShown;
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

        if (tour != null)
        {
            tour.StepShown -= OnTourStepShown;
        }

        // A disabled object stops its coroutines: never stay locked between two questions.
        switching = false;
    }

    private void Start()
    {
        isSetUp = !HasMissingReferences();
        if (!isSetUp)
            return;

        ShowQuestion(0);

        if (tour.CurrentStepId == unlockStepId)
        {
            Unlock();
        }
        else
        {
            ShowLocked();
        }
    }

    private bool HasMissingReferences()
    {
        bool missing = RequiredReference.IsMissing(optionAToggle, nameof(optionAToggle), this)
            | RequiredReference.IsMissing(optionBToggle, nameof(optionBToggle), this)
            | RequiredReference.IsMissing(optionCToggle, nameof(optionCToggle), this)
            | RequiredReference.IsMissing(questionText, nameof(questionText), this)
            | RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this)
            | RequiredReference.IsMissing(tour, nameof(tour), this)
            | RequiredReference.IsMissing(answerArea, nameof(answerArea), this);

        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("PackageQuizController: questions is empty.", this);
            return true;
        }

        foreach (Question question in questions)
        {
            if (!IsAnswerable(question))
            {
                Debug.LogError($"PackageQuizController: question '{question.id}' needs 2–{Letters.Length} options and one with key '{question.correctKey}'.", this);
                missing = true;
            }
        }

        return missing;
    }

    // A typo in correctKey would make a question impossible to pass without any error.
    private static bool IsAnswerable(Question question)
    {
        if (question.options == null || question.options.Length < 2 || question.options.Length > Letters.Length)
            return false;

        foreach (Option option in question.options)
        {
            if (option.key == question.correctKey)
                return true;
        }

        return false;
    }

    private void OnTourStepShown(string stepId)
    {
        if (isSetUp && !IsUnlocked && stepId == unlockStepId)
        {
            Unlock();
        }
    }

    // Decision 020: before the tour's last step only one line says when the questions come; nothing can be pressed.
    private void ShowLocked()
    {
        answerArea.SetActive(false);
        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
        }
        questionText.text = Loc.Get(LockedKey);
        feedbackText.text = string.Empty;
    }

    private void Unlock()
    {
        IsUnlocked = true;
        answerArea.SetActive(true);
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
        }
        ShowQuestion(current);
        feedbackText.text = Loc.Get(HowToKey);
        Unlocked?.Invoke();
    }

    private void ShowQuestion(int index)
    {
        current = index;
        Question question = questions[index];

        questionText.text = Loc.Get(QuestionKeyPrefix + question.id + ".q");

        if (progressText != null)
        {
            progressText.text = Loc.Get(ProgressKey, ("n", index + 1), ("total", questions.Length));
        }

        for (int i = 0; i < toggles.Length; i++)
        {
            bool used = question.options != null && i < question.options.Length;
            // With notify: the SDK's AnimatorOverrideLayerWeight only drops the "selected" look on the event.
            toggles[i].isOn = false;
            toggles[i].gameObject.SetActive(used);
        }

        RefreshLabels();
    }

    private void OnToggleChanged(int index, bool isOn)
    {
        // While the next question is one frame away, presses are not rated against the old options.
        if (!isSetUp || !IsUnlocked || IsAnswered || switching)
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

        Question question = questions[current];
        if (question.options == null || index >= question.options.Length)
            return;

        Option answer = question.options[index];
        bool right = answer.key == question.correctKey;
        AnswerChosen?.Invoke(question.id, answer.key, right);

        if (!right)
        {
            feedbackText.text = Loc.Get(WrongKey) + "\n" + Loc.Get(OptionKey(question, answer) + ".wrong");
            RefreshLabels();
            return;
        }

        if (current < questions.Length - 1)
        {
            feedbackText.text = Loc.Get(CorrectKey) + "\n" + Loc.Get(NextQuestionKey);
            StartCoroutine(ShowQuestionNextFrame(current + 1));
            return;
        }

        if (!LessonNames.TryGetDisplayName(NextScene, out string nextLesson))
        {
            Debug.LogError($"PackageQuizController: no lesson name for {NextScene}.", this);
        }

        IsAnswered = true;
        feedbackText.text = Loc.Get(CorrectKey) + "\n" + Loc.Get(AllDoneKey, ("lesson", nextLesson));
        RefreshLabels();
        foreach (Toggle toggle in toggles)
        {
            toggle.interactable = false;
        }
    }

    // We are inside the pressed toggle's onValueChanged(true). The SDK's AnimatorOverrideLayerWeight listens after us,
    // so switching that toggle off here would reach it before the outer "true" and leave it looking selected.
    private IEnumerator ShowQuestionNextFrame(int index)
    {
        switching = true;
        yield return null;
        switching = false;
        ShowQuestion(index);
    }

    private static string OptionKey(Question question, Option option) =>
        QuestionKeyPrefix + question.id + "." + option.key;

    // #32/#52: the chosen answer is marked with a text prefix and bold type, not by colour only.
    private void RefreshLabels()
    {
        Option[] options = questions[current].options;

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] == null || options == null || i >= options.Length)
                continue;

            string answer = Letters[i] + ": " + Loc.Get(OptionKey(questions[current], options[i]));
            labels[i].text = toggles[i].isOn ? "<b>" + SelectedMark + answer + "</b>" : answer;
        }
    }
}
