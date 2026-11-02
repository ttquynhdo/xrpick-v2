using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// #45 + #20 + #52, docs/decisions/007 (5), 010 and 015: ConceptScene quiz. One question at a time on the card right
/// above up to three answer toggles (A–C) that show the answer text, checked with the red button. A correct answer moves
/// on to the next question; a wrong one names the reason. Feedback has its own card (#32). No logging yet (#28).
/// </summary>
public class PackageQuizController : MonoBehaviour
{
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string ProgressFormat = "Frage {0} von {1}";                                   // concept.quiz.progress
    private const string SelectFirst = "<color=#F2C94C><b>Wählen Sie zuerst eine Antwort.</b></color>";    // concept.quiz.select_first
    private const string Correct = "<color=#27AE60><b>Richtig!</b></color>";                    // concept.quiz.correct
    private const string NextQuestion = "Beantworten Sie jetzt die nächste Frage.";              // concept.quiz.next_question
    private const string AllDoneFormat = "Sie haben alle Fragen richtig beantwortet. Wählen Sie jetzt im Menü „{0}“.";  // concept.quiz.all_done
    private const string Wrong = "<color=#EB5757><b>Nicht richtig</b></color>";                  // concept.quiz.wrong
    private const string NextLesson = "3. Kommissionieren nach EKZ 3";                       // lesson.basic (LessonNames)
    private const string SelectedMark = "» ";

    // Decision 015: each toggle shows its letter and answer, so the trainee reads and presses in one place.
    private static readonly string[] Letters = { "A", "B", "C" };

    [Serializable]
    public class Option
    {
        [Tooltip("Short, language-independent answer key (for #28).")]
        public string key;

        public string label;

        [Tooltip("Shown after 'Nicht richtig' when this wrong answer is chosen.")]
        [TextArea(1, 3)]
        public string wrongReason;
    }

    [Serializable]
    public class Question
    {
        [Tooltip("Stable question id (for #28).")]
        public string id;

        [TextArea(2, 4)]
        public string text;

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

    [Header("Question card")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;

    [Header("Feedback")]
    [Tooltip("Own card next to the red button; inactive until the first check.")]
    [SerializeField] private GameObject feedbackCard;
    [SerializeField] private TMP_Text feedbackText;

    private Toggle[] toggles;
    private TMP_Text[] labels;
    private int current;

    // Checked once in Start; a broken setup is logged there and the red button does nothing.
    private bool isSetUp;

    private void Awake()
    {
        toggles = new[] { optionAToggle, optionBToggle, optionCToggle };
        labels = new[] { optionALabel, optionBLabel, optionCLabel };
    }

    private void OnEnable()
    {
        foreach (Toggle toggle in toggles)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnSelectionChanged);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Toggle toggle in toggles)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnSelectionChanged);
            }
        }
    }

    private void Start()
    {
        isSetUp = !HasMissingReferences();
        if (!isSetUp)
            return;

        if (feedbackCard != null)
        {
            feedbackCard.SetActive(false);
        }

        ShowQuestion(0);
    }

    // Gán function này vào button đỏ
    public void CheckAnswer()
    {
        if (!isSetUp)
            return;

        if (feedbackCard != null)
        {
            feedbackCard.SetActive(true);
        }

        Question question = questions[current];
        int selected = SelectedIndex();

        if (selected < 0)
        {
            feedbackText.text = SelectFirst;
            return;
        }

        Option answer = question.options[selected];

        if (answer.key != question.correctKey)
        {
            feedbackText.text = Wrong + "\n" + answer.wrongReason;
            return;
        }

        if (current < questions.Length - 1)
        {
            feedbackText.text = Correct + "\n" + NextQuestion;
            ShowQuestion(current + 1);
        }
        else
        {
            feedbackText.text = Correct + "\n" + string.Format(AllDoneFormat, NextLesson);
        }
    }

    private bool HasMissingReferences()
    {
        bool missing = RequiredReference.IsMissing(optionAToggle, nameof(optionAToggle), this)
            | RequiredReference.IsMissing(optionBToggle, nameof(optionBToggle), this)
            | RequiredReference.IsMissing(optionCToggle, nameof(optionCToggle), this)
            | RequiredReference.IsMissing(questionText, nameof(questionText), this)
            | RequiredReference.IsMissing(feedbackText, nameof(feedbackText), this);

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

    private void ShowQuestion(int index)
    {
        current = index;
        Question question = questions[index];

        questionText.text = question.text;

        if (progressText != null)
        {
            progressText.text = string.Format(ProgressFormat, index + 1, questions.Length);
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

    private void OnSelectionChanged(bool isOn)
    {
        if (!isSetUp)
            return;

        RefreshLabels();
    }

    // #32/#52: the chosen answer is marked with a text prefix and bold type, not by colour only.
    private void RefreshLabels()
    {
        Option[] options = questions[current].options;

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] == null || options == null || i >= options.Length)
                continue;

            string answer = Letters[i] + ": " + options[i].label;
            labels[i].text = toggles[i].isOn ? "<b>" + SelectedMark + answer + "</b>" : answer;
        }
    }

    private int SelectedIndex()
    {
        Option[] options = questions[current].options;

        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn && options != null && i < options.Length)
                return i;
        }

        return -1;
    }
}
