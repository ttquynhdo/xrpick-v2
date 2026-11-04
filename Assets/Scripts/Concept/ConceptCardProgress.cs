using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #79, docs/decisions/024 (6, 7) and 025: the ConceptScene task cards show what is done and what is next. Each card
/// gets the Welcome check mark (#39) once its task is done (card 1: a package of every material grabbed, card 2: the
/// tour reached its last step, card 3: all questions right), and the current card gets the blue frame of the picking
/// scenes (#40) while the other step cards are dimmed. Decided by the pure ConceptCardTracker; no log events.
/// </summary>
public class ConceptCardProgress : MonoBehaviour
{
    [Header("Cards (InstructionShelf/Panel/Cards)")]
    [Tooltip("Card „1. Etikett lesen“.")]
    [SerializeField] private RectTransform labelCard;

    [Tooltip("Card „2. LVS kennenlernen“.")]
    [SerializeField] private RectTransform wmsCard;

    [Tooltip("Card „3. Fragen beantworten“.")]
    [SerializeField] private RectTransform quizCard;

    [Header("Check marks (inactive in the scene)")]
    [SerializeField] private GameObject labelCheck;
    [SerializeField] private GameObject wmsCheck;
    [SerializeField] private GameObject quizCheck;

    [Header("Scene")]
    [SerializeField] private WmsFieldTour tour;
    [SerializeField] private PackageQuizController quiz;

    [Header("Look (same as InstructionStepHighlighter)")]
    [SerializeField] private Color frameColor = new(0.012f, 0.569f, 0.867f, 1f);
    [SerializeField] private float frameThinness = 1f;
    [SerializeField] private float frameOutset = 3f;

    [Range(0.2f, 1f)]
    [SerializeField] private float dimFactor = 0.55f;

    private readonly List<(Grabbable grabbable, Action<PointerEvent> handler)> packageGrabs = new();

    private ConceptCardTracker tracker;
    private string firstStepId;
    private InstructionCardLook label;
    private InstructionCardLook wms;
    private InstructionCardLook quizLook;

    private void Awake()
    {
        var materials = new HashSet<string>();

        // Grabbable packages only: the static cartons and EKZ samples have no Grabbable, so they never count.
        foreach (Grabbable grabbable in FindObjectsByType<Grabbable>(FindObjectsInactive.Include))
        {
            PackageData package = grabbable.GetComponentInParent<PackageData>(true);

            if (package != null && !string.IsNullOrEmpty(package.MaterialKey))
            {
                materials.Add(package.MaterialKey);
                packageGrabs.Add((grabbable, evt => HandlePointerEvent(evt, package)));
            }
        }

        tracker = new ConceptCardTracker(materials.Count);
    }

    private void OnEnable()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in packageGrabs)
        {
            grabbable.WhenPointerEventRaised += handler;
        }
        if (tour != null)
        {
            tour.StepShown += HandleStepShown;
        }
        if (quiz != null)
        {
            quiz.Unlocked += HandleQuizOpened;
            quiz.LessonCompleted += HandleQuizCompleted;
        }
    }

    private void OnDisable()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in packageGrabs)
        {
            if (grabbable != null)
            {
                grabbable.WhenPointerEventRaised -= handler;
            }
        }
        if (tour != null)
        {
            tour.StepShown -= HandleStepShown;
        }
        if (quiz != null)
        {
            quiz.Unlocked -= HandleQuizOpened;
            quiz.LessonCompleted -= HandleQuizCompleted;
        }
    }

    private void Start()
    {
        bool missing = RequiredReference.IsMissing(labelCard, nameof(labelCard), this)
                       | RequiredReference.IsMissing(wmsCard, nameof(wmsCard), this)
                       | RequiredReference.IsMissing(quizCard, nameof(quizCard), this)
                       | RequiredReference.IsMissing(labelCheck, nameof(labelCheck), this)
                       | RequiredReference.IsMissing(wmsCheck, nameof(wmsCheck), this)
                       | RequiredReference.IsMissing(quizCheck, nameof(quizCheck), this)
                       | RequiredReference.IsMissing(tour, nameof(tour), this)
                       | RequiredReference.IsMissing(quiz, nameof(quiz), this);
        if (missing)
            return;

        // The tour shows its first step in its own Start; any other step id later means "Weiter" was pressed.
        firstStepId = tour.CurrentStepId;

        label = new InstructionCardLook(labelCard, frameOutset);
        wms = new InstructionCardLook(wmsCard, frameOutset);
        quizLook = new InstructionCardLook(quizCard, frameOutset);

        // The frames are added as the cards' last children; the check marks must still draw on top of them.
        labelCheck.transform.SetAsLastSibling();
        wmsCheck.transform.SetAsLastSibling();
        quizCheck.transform.SetAsLastSibling();

        if (quiz.IsUnlocked)
        {
            tracker.QuizOpened();
        }

        Apply();
    }

    private void HandlePointerEvent(PointerEvent evt, PackageData package)
    {
        if (evt.Type == PointerEventType.Select && tracker.PackageGrabbed(package.MaterialKey))
        {
            Apply();
        }
    }

    private void HandleStepShown(string stepId)
    {
        if (firstStepId != null && stepId != firstStepId && tracker.TourAdvanced())
        {
            Apply();
        }
    }

    private void HandleQuizOpened()
    {
        if (tracker.QuizOpened())
        {
            Apply();
        }
    }

    private void HandleQuizCompleted()
    {
        if (tracker.QuizCompleted())
        {
            Apply();
        }
    }

    private void Apply()
    {
        // Not built yet (before Start or missing references).
        if (label == null)
            return;

        ConceptCard current = tracker.Current;
        bool allDone = current == ConceptCard.None;

        Show(label, current == ConceptCard.Label, allDone);
        Show(wms, current == ConceptCard.Wms, allDone);
        Show(quizLook, current == ConceptCard.Quiz, allDone);

        // Marks only turn on (decision 024: "Von vorn" and "Zurück" keep them).
        if (tracker.LabelDone)
        {
            labelCheck.SetActive(true);
        }
        if (tracker.WmsDone)
        {
            wmsCheck.SetActive(true);
        }
        if (tracker.QuizDone)
        {
            quizCheck.SetActive(true);
        }
    }

    private void Show(InstructionCardLook card, bool isCurrent, bool allDone)
    {
        card.Show(isCurrent ? frameColor : Color.clear, frameThinness, dimmed: !allDone && !isCurrent, dimFactor);
    }
}
