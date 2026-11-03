using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #40, docs/decisions/017: marks the current step on the instruction shelf. The current card gets a thick
/// blue frame and the other step cards' texts are dimmed (a second cue besides colour); card 3 gets a thin
/// frame as a "ready" hint while the order container holds packages of the current position. Steps follow
/// actions only (InstructionStepTracker), never the order result. The „Aufgabe“ card is never touched.
/// Frames are built at runtime from each card's own sprite, without raycast targets.
/// </summary>
public class InstructionStepHighlighter : MonoBehaviour
{
    [Header("Cards (InstructionShelf/Panel/Cards)")]
    [Tooltip("Card „1. Angaben im LVS ablesen“.")]
    [SerializeField] private RectTransform readCard;

    [Tooltip("Card „2. … kommissionieren“.")]
    [SerializeField] private RectTransform pickCard;

    [Tooltip("Card „3. Auftrag bestätigen“.")]
    [SerializeField] private RectTransform confirmCard;

    [Header("Scene")]
    [Tooltip("Counter on the order container; must implement IOrderBoxContents (IPositionedOrder too in Mixed/Complex).")]
    [SerializeField] private MonoBehaviour orderBox;

    [Tooltip("Validator of this scene; must implement IOrderCompletion.")]
    [SerializeField] private MonoBehaviour validator;

    // #71, decision 019 (assumption D27): card 1 lights up when the next storage container has stopped.
    [Tooltip("Optional. Storage containers on the station lanes (IStationStock); a new position starts when they stop.")]
    [SerializeField] private MonoBehaviour stationStock;

    [Header("Look")]
    [Tooltip("Frame colour; same blue as the Menu's Next Lesson Highlight (#39).")]
    [SerializeField] private Color frameColor = new(0.012f, 0.569f, 0.867f, 1f);

    [Tooltip("Alpha of the thin \"ready\" frame on card 3.")]
    [Range(0f, 1f)]
    [SerializeField] private float hintAlpha = 0.6f;

    [Tooltip("Frame thickness: higher = thinner (Image.pixelsPerUnitMultiplier of the sliced card sprite).")]
    [SerializeField] private float currentFrameThinness = 1f;

    [SerializeField] private float hintFrameThinness = 3f;

    [Tooltip("How far the frame reaches outside the card, in card units.")]
    [SerializeField] private float frameOutset = 3f;

    [Tooltip("Text alpha factor for step cards that are not current; they stay readable.")]
    [Range(0.2f, 1f)]
    [SerializeField] private float dimFactor = 0.55f;

    private readonly InstructionStepTracker tracker = new();
    private readonly List<(Grabbable grabbable, Action<PointerEvent> handler)> packageGrabs = new();

    private IOrderBoxContents contents;
    private IPositionedOrder positions;
    private IOrderCompletion completion;
    private IStationStock stock;
    private bool positionStartPending;

    private CardLook read;
    private CardLook pick;
    private CardLook confirm;

    private void Awake()
    {
        contents = orderBox as IOrderBoxContents;
        positions = orderBox as IPositionedOrder;
        completion = validator as IOrderCompletion;
        stock = stationStock as IStationStock;

        // Storage boxes of later positions are inactive at start; their packages must be tracked too.
        foreach (Grabbable grabbable in FindObjectsByType<Grabbable>(FindObjectsInactive.Include))
        {
            PackageData package = grabbable.GetComponentInParent<PackageData>(true);

            if (package != null)
            {
                packageGrabs.Add((grabbable, evt => HandlePointerEvent(evt, package)));
            }
        }
    }

    private void OnEnable()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in packageGrabs)
        {
            grabbable.WhenPointerEventRaised += handler;
        }
        if (contents != null)
        {
            contents.ContentsChanged += HandleContentsChanged;
        }
        if (positions != null)
        {
            positions.LineAdvanced += HandleLineAdvanced;
        }
        if (completion != null)
        {
            completion.OrderCompleted += HandleOrderCompleted;
        }
        if (stock != null)
        {
            stock.Settled += HandleStockSettled;
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
        if (contents != null)
        {
            contents.ContentsChanged -= HandleContentsChanged;
        }
        if (positions != null)
        {
            positions.LineAdvanced -= HandleLineAdvanced;
        }
        if (completion != null)
        {
            completion.OrderCompleted -= HandleOrderCompleted;
        }
        if (stock != null)
        {
            stock.Settled -= HandleStockSettled;
        }
        positionStartPending = false;
    }

    private void Start()
    {
        bool missing = RequiredReference.IsMissing(readCard, nameof(readCard), this)
                       | RequiredReference.IsMissing(pickCard, nameof(pickCard), this)
                       | RequiredReference.IsMissing(confirmCard, nameof(confirmCard), this);

        if (contents == null)
        {
            Debug.LogError("InstructionStepHighlighter: orderBox is not assigned or does not implement IOrderBoxContents.", this);
        }
        if (completion == null)
        {
            Debug.LogError("InstructionStepHighlighter: validator is not assigned or does not implement IOrderCompletion.", this);
        }
        if (missing)
            return;

        read = new CardLook(readCard, frameOutset);
        pick = new CardLook(pickCard, frameOutset);
        confirm = new CardLook(confirmCard, frameOutset);

        if (contents != null)
        {
            tracker.PackagesInBoxChanged(contents.PackageCount);
        }

        Apply();
    }

    private void HandlePointerEvent(PointerEvent evt, PackageData package)
    {
        if (evt.Type != PointerEventType.Select)
            return;

        // At the start of a position only finished positions' packages are in the container: touching
        // one of them is not picking (Mixed, Complex).
        if (tracker.Step == InstructionStep.Read && positions != null && positions.Contains(package))
            return;

        if (tracker.PackageGrabbed())
        {
            Apply();
        }
    }

    private void HandleContentsChanged()
    {
        if (tracker.PackagesInBoxChanged(contents.PackageCount))
        {
            Apply();
        }
    }

    private void HandleLineAdvanced(int position)
    {
        if (stock != null)
        {
            // The storage containers may hear LineAdvanced after this script: look one frame later.
            StartCoroutine(StartPositionWhenStockStopped());
            return;
        }

        if (tracker.PositionStarted())
        {
            Apply();
        }
    }

    private IEnumerator StartPositionWhenStockStopped()
    {
        yield return null;

        positionStartPending = true;
        if (!stock.IsMoving)
        {
            HandleStockSettled();
        }
    }

    private void HandleStockSettled()
    {
        if (!positionStartPending)
            return;

        positionStartPending = false;
        tracker.PositionStarted();
        if (contents != null)
        {
            tracker.PackagesInBoxChanged(contents.PackageCount);
        }
        Apply();
    }

    private void HandleOrderCompleted()
    {
        if (tracker.OrderCompleted())
        {
            Apply();
        }
    }

    private void Apply()
    {
        // Not built yet (before Start or missing references).
        if (read == null)
            return;

        InstructionStep step = tracker.Step;
        bool done = step == InstructionStep.Done;
        bool readCurrent = step == InstructionStep.Read;
        bool pickCurrent = step == InstructionStep.Pick;

        read.Show(readCurrent ? frameColor : Color.clear, currentFrameThinness, dimmed: !done && !readCurrent, dimFactor);
        pick.Show(pickCurrent ? frameColor : Color.clear, currentFrameThinness, dimmed: !done && !pickCurrent, dimFactor);

        Color hint = frameColor;
        hint.a *= hintAlpha;
        confirm.Show(tracker.ConfirmReady ? hint : Color.clear, hintFrameThinness, dimmed: !done, dimFactor);
    }

    // One instruction card: its frame and the original colours of its texts.
    private sealed class CardLook
    {
        private readonly Image frame;
        private readonly TMP_Text[] texts;
        private readonly Color[] baseColors;

        public CardLook(RectTransform card, float outset)
        {
            texts = card.GetComponentsInChildren<TMP_Text>(true);
            baseColors = new Color[texts.Length];

            for (int i = 0; i < texts.Length; i++)
            {
                baseColors[i] = texts[i].color;
            }

            var frameObject = new GameObject("Step Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            frameObject.layer = card.gameObject.layer;

            var rect = (RectTransform)frameObject.transform;
            rect.SetParent(card, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(-outset, -outset);
            rect.offsetMax = new Vector2(outset, outset);

            frameObject.GetComponent<LayoutElement>().ignoreLayout = true;

            Image cardImage = card.GetComponent<Image>();
            frame = frameObject.GetComponent<Image>();
            frame.sprite = cardImage != null ? cardImage.sprite : null;
            frame.type = Image.Type.Sliced;
            frame.fillCenter = false;
            frame.raycastTarget = false;
            frame.enabled = false;
        }

        public void Show(Color frameColor, float thinness, bool dimmed, float dimFactor)
        {
            frame.enabled = frameColor.a > 0f;
            frame.color = frameColor;
            frame.pixelsPerUnitMultiplier = thinness;

            for (int i = 0; i < texts.Length; i++)
            {
                Color color = baseColors[i];

                if (dimmed)
                {
                    color.a *= dimFactor;
                }

                texts[i].color = color;
            }
        }
    }
}
