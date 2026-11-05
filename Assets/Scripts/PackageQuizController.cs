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
/// line inside the cluster. The questions stay locked until the WMS tour reaches its last step.
/// #93, decision 026 (4): the cluster then takes the bar's place only after „Zu den Fragen“ on the tour bar, and its
/// own „Zurück“ brings the bar back at the same step; bar and cluster are never shown together. After the last correct
/// answer „Richtig!“ stays CorrectHoldSeconds, then the bar comes back with „Von vorn“ and the cluster stays hidden.
/// Logs quiz_unlocked, quiz_answer and task_completed (#28, decision 021); switching bar ↔ cluster logs nothing.
/// </summary>
public class PackageQuizController : MonoBehaviour, ILessonCompletion
{
    // Loc table UI (#11, #54). The rich text (colour, bold) is part of the table entries.
    private const string ProgressKey = "concept.quiz.progress";
    private const string LockedKey = "concept.quiz.locked";
    private const string HowToKey = "concept.quiz.how_to";
    private const string CorrectKey = "concept.quiz.correct";
    private const string NextQuestionKey = "concept.quiz.next_question";
    private const string WrongKey = "concept.quiz.wrong";
    private const string BackKey = "concept.tour.back";
    // #55: question concept.quiz.<id>.q, answer concept.quiz.<id>.<key>, reason concept.quiz.<id>.<key>.wrong.
    private const string QuestionKeyPrefix = "concept.quiz.";
    private const string SelectedMark = "» ";

    // Decision 015: each toggle shows its letter and answer, so the trainee reads and presses in one place.
    private static readonly string[] Letters = { "A", "B", "C" };

    // Decision 024 (5): "Richtig!" stays under the answered question this long; touches meanwhile are not rated.
    public const float CorrectHoldSeconds = 1.5f;

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

    [Tooltip("#78, decision 024: the whole cluster; #93: shown only after „Zu den Fragen“ on the tour bar.")]
    [SerializeField] private GameObject clusterRoot;

    [Header("Back to the tour (#93)")]
    [Tooltip("Push button in the cluster that brings the tour bar back at its last step.")]
    [SerializeField] private Toggle backToggle;

    [SerializeField] private TMP_Text backLabel;

    [Tooltip("Inactive highlight behind the next lesson's Menu item, shown when all questions are right (as in WelcomeScene).")]
    [SerializeField] private GameObject nextLessonHighlight;

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

    // #42: all questions answered right = lesson done (Menu unlock).
    public bool IsLessonCompleted => IsAnswered;

    public event Action LessonCompleted;

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
            tour.QuestionsRequested += ShowQuestions;
        }

        if (backToggle != null)
        {
            backToggle.onValueChanged.AddListener(OnBackToggled);
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
            tour.QuestionsRequested -= ShowQuestions;
        }

        if (backToggle != null)
        {
            backToggle.onValueChanged.RemoveListener(OnBackToggled);
        }

        // A disabled object stops its coroutines: never stay locked between two questions.
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

        if (backLabel != null)
        {
            backLabel.text = Loc.Get(BackKey);
        }

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
            | RequiredReference.IsMissing(answerArea, nameof(answerArea), this)
            | RequiredReference.IsMissing(clusterRoot, nameof(clusterRoot), this)
            | RequiredReference.IsMissing(backToggle, nameof(backToggle), this)
            | RequiredReference.IsMissing(nextLessonHighlight, nameof(nextLessonHighlight), this);

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

    // #78, decision 024: before the tour's last step the whole cluster is hidden (card 3 says when it comes).
    private void ShowLocked()
    {
        clusterRoot.SetActive(false);
        answerArea.SetActive(false);
        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
        }
        questionText.text = Loc.Get(LockedKey);
        feedbackText.text = string.Empty;
    }

    // #93: the questions are ready but stay hidden until „Zu den Fragen“ on the tour bar.
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
        tour.OffersQuestions = true;
        ResearchLogger.LogQuizUnlocked();
        Unlocked?.Invoke();
    }

    private void ShowQuestions()
    {
        if (!isSetUp || !IsUnlocked || IsAnswered)
            return;

        tour.SetBarVisible(false);
        clusterRoot.SetActive(true);
    }

    private void ShowTour()
    {
        clusterRoot.SetActive(false);
        tour.SetBarVisible(true);
    }

    private void OnBackToggled(bool isOn)
    {
        if (!isOn)
            return;

        // Push button: release at once so the next poke counts again.
        backToggle.SetIsOnWithoutNotify(false);
        ShowTour();
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
        ResearchLogger.LogQuizAnswer(OutcomeEvents.QuizConcept, question.id, answer.key, right);
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
            StartCoroutine(ShowQuestionAfterHold(current + 1));
            return;
        }

        IsAnswered = true;
        feedbackText.text = Loc.Get(CorrectKey);
        RefreshLabels();
        SetTogglesInteractable(false);
        tour.FinishQuestions();
        nextLessonHighlight.SetActive(true);

        ResearchLogger.LogTaskCompleted();
        LessonCompleted?.Invoke();
        StartCoroutine(ShowTourAfterHold());
    }

    // #93: „Richtig!“ stays under the last question, then the cluster folds away for good.
    private IEnumerator ShowTourAfterHold()
    {
        yield return new WaitForSeconds(CorrectHoldSeconds);
        ShowTour();
    }

    // We are inside the pressed toggle's onValueChanged(true). The SDK's AnimatorOverrideLayerWeight listens after us,
    // so switching that toggle off here would reach it before the outer "true" and leave it looking selected.
    // Decision 024 (5): "Richtig!" stays under the answered question for CorrectHoldSeconds, then the next question
    // comes with the neutral how-to line. The toggles are not interactable meanwhile, so nothing is rated or logged.
    private IEnumerator ShowQuestionAfterHold(int index)
    {
        switching = true;
        SetTogglesInteractable(false);
        yield return new WaitForSeconds(CorrectHoldSeconds);
        switching = false;
        SetTogglesInteractable(true);
        ShowQuestion(index);
        feedbackText.text = Loc.Get(HowToKey);
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
