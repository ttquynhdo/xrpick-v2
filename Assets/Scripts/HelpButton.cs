using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #182 (design docs/superpowers/specs/2026-11-26-help-button-design.md): the „Hilfe“ poke button in every scene.
/// Shown only while a research session runs. A press rings once, turns the button into „Hilfe kommt …“ (dimmed, not
/// pressable) and logs help_requested with the context the scene has (tour step, question shown, order position).
/// It opens again after a rated attempt (red-button press or quiz answer, ResearchLogger.AttemptLogged), after
/// <see cref="HelpRequestState.DefaultTimeoutSeconds"/> or with the next scene (a new instance), and logs help_reset.
/// The facilitator watches the stream; the bell is a fallback.
/// </summary>
public class HelpButton : MonoBehaviour
{
    private const string ButtonKey = "help.button";
    private const string WaitingKey = "help.waiting";
    private const float WaitingAlpha = 0.45f;

    [Tooltip("Poke toggle used as a push button.")]
    [SerializeField] private Toggle pokeToggle;

    [Tooltip("Button text; written here (no LocalizeStringEvent on it).")]
    [SerializeField] private TMP_Text label;

    [Tooltip("CanvasGroup on the button canvas: dimmed and not interactable while help is on its way.")]
    [SerializeField] private CanvasGroup buttonGroup;

    [Tooltip("Everything visible and pokeable; active only while a research session runs.")]
    [SerializeField] private GameObject visualRoot;

    [Tooltip("Soft bell, played once per accepted press.")]
    [SerializeField] private AudioSource bell;

    private readonly HelpRequestState state = new();
    private static bool quitting;

    private WmsFieldTour tour;
    private PackageQuizController conceptQuiz;
    private RemainderQuizController remainderQuiz;
    private IPositionedOrder order;
    private bool broken;

    public bool IsWaiting => state.IsWaiting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        quitting = false;
        Application.quitting -= MarkQuitting;
        Application.quitting += MarkQuitting;
    }

    private static void MarkQuitting() => quitting = true;

    private void OnEnable()
    {
        if (pokeToggle != null)
        {
            pokeToggle.onValueChanged.AddListener(OnToggled);
        }
        ResearchLogger.AttemptLogged += OnAttemptLogged;
    }

    private void OnDisable()
    {
        if (pokeToggle != null)
        {
            pokeToggle.onValueChanged.RemoveListener(OnToggled);
        }
        ResearchLogger.AttemptLogged -= OnAttemptLogged;
    }

    private void Start()
    {
        broken = RequiredReference.IsMissing(pokeToggle, nameof(pokeToggle), this)
                 | RequiredReference.IsMissing(label, nameof(label), this)
                 | RequiredReference.IsMissing(buttonGroup, nameof(buttonGroup), this)
                 | RequiredReference.IsMissing(visualRoot, nameof(visualRoot), this);

        tour = FindFirstObjectByType<WmsFieldTour>(FindObjectsInactive.Include);
        conceptQuiz = FindFirstObjectByType<PackageQuizController>(FindObjectsInactive.Include);
        remainderQuiz = FindFirstObjectByType<RemainderQuizController>(FindObjectsInactive.Include);
        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (behaviour is IPositionedOrder positioned)
            {
                order = positioned;
                break;
            }
        }

        ApplyLook();
        UpdateVisible();
    }

    private void Update()
    {
        UpdateVisible();
        if (state.IsTimedOut(Time.unscaledTime))
        {
            Reopen(OutcomeEvents.HelpResetTimeout);
        }
    }

    private void OnDestroy()
    {
        // Leaving the scene while help is on its way; not when the app quits or the facilitator ended the session.
        if (state.IsWaiting && !quitting && ResearchLogger.IsSessionActive)
        {
            ResearchLogger.LogHelpReset(OutcomeEvents.HelpResetSceneChange);
        }
    }

    /// <summary>The poke; also callable from tests.</summary>
    public void Press()
    {
        if (broken || !ResearchLogger.IsSessionActive || !state.TryRequest(Time.unscaledTime))
            return;

        if (bell != null)
        {
            bell.Play();
        }
        ApplyLook();
        ResearchLogger.LogHelpRequested(tour != null ? tour.CurrentStepId : null, CurrentQuestionId(),
            order != null ? order.CurrentPosition : (int?)null);
    }

    private void OnToggled(bool isOn)
    {
        if (!isOn)
            return;

        // Push button: release at once.
        pokeToggle.SetIsOnWithoutNotify(false);
        Press();
    }

    private void OnAttemptLogged()
    {
        Reopen(OutcomeEvents.HelpResetAttempt);
    }

    private void Reopen(string reason)
    {
        if (!state.Reset())
            return;

        ApplyLook();
        ResearchLogger.LogHelpReset(reason);
    }

    private string CurrentQuestionId()
    {
        string concept = conceptQuiz != null ? conceptQuiz.ShownQuestionId : null;
        if (concept != null)
            return concept;
        return remainderQuiz != null && remainderQuiz.isActiveAndEnabled ? remainderQuiz.ShownQuestionId : null;
    }

    private void ApplyLook()
    {
        if (broken)
            return;

        label.text = Loc.Get(state.IsWaiting ? WaitingKey : ButtonKey);
        buttonGroup.alpha = state.IsWaiting ? WaitingAlpha : 1f;
        buttonGroup.interactable = !state.IsWaiting;
    }

    private void UpdateVisible()
    {
        if (broken)
            return;

        bool show = ResearchLogger.IsSessionActive;
        if (visualRoot.activeSelf != show)
        {
            visualRoot.SetActive(show);
        }
    }
}
