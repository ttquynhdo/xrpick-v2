using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #20, docs/decisions/010: walks the trainee through the WMS dialog one field at a time. Each step shows its markers
/// outside the screen border; nothing is drawn on the dialog itself. #78, decision 024: a step about one field writes
/// its title and what the field is into a caption next to that field.
/// #89, decision 026: one bar centred below the screen holds the step text of the steps without a caption, the
/// "what to do" line with a hand icon for every step, and [Zurück] "Schritt n von 11" [Weiter] (poke toggles used as
/// push buttons, #80). The bar is hidden at the start and opens through <see cref="Open"/> (card 1 done) or after
/// <see cref="autoOpenSeconds"/>; step 1 is shown and logged only then.
/// #104, decision 036: every action line says what to do and why (no held-package variant any more). A step with
/// <see cref="Step.hasFollowUp"/> (material_flow) first shows a short text and "watch"; <see cref="ShowFollowUp"/>
/// (the lane demo, once the order container has left) switches it to concept.tour.&lt;id&gt;.body_after/.action_after
/// and only then shows „Zu den Fragen“.
/// #93, decision 026 (4): while <see cref="OffersQuestions"/> is set, the right button of the last step reads
/// „Zu den Fragen“ and raises <see cref="QuestionsRequested"/> instead of starting over; the quiz then hides the bar
/// through <see cref="SetBarVisible"/> and shows it again at the same step (no step change, no tour_step).
/// #129, decision 040: everything to read is at the depth of the screen. The step text of a step without its own caption
/// goes into <see cref="stepCaption"/> right of the screen; the hand icon + action line (<see cref="actionRow"/>) moves
/// into the current step's caption (or <see cref="stepCaption"/>). The bar near the hands only holds the buttons and
/// „Schritt n von 11“ and fades in and out (<see cref="PanelFade"/>).
/// </summary>
public class WmsFieldTour : MonoBehaviour
{
    // Loc table UI (#54).
    private const string ProgressKey = "concept.tour.progress";
    private const string NextKey = "concept.tour.next";
    private const string RestartKey = "concept.tour.restart";
    private const string BackKey = "concept.tour.back";
    private const string ToQuestionsKey = "concept.tour.to_questions";
    private const float DimmedAlpha = 0.5f;
    private const string StepKeyPrefix = "concept.tour.";

    [Serializable]
    public class Step
    {
        // #55: the texts are the Loc keys concept.tour.<id>.title / .body / .action (#89).
        [Tooltip("Stable id, e.g. soll (for #28 logging later); also names the Loc keys concept.tour.<id>.title/.body/.action.")]
        public string id;

        [Tooltip("Shown only during this step; placed outside the monitor border.")]
        public GameObject[] markers;

        // #78, decision 024: a caption next to the field (one of the markers). Empty = the text goes on the bar.
        [Tooltip("Caption title next to the field; leave empty to show the step text in stepCaption right of the screen.")]
        public TMP_Text captionTitle;

        public TMP_Text captionBody;

        // #104, decision 036 (3): the step waits for something to happen (the lane demo) before its second text.
        [Tooltip("Shows .body/.action first and .body_after/.action_after after ShowFollowUp(); while the questions are offered, the right button stays hidden until then.")]
        public bool hasFollowUp;

        // #155, decision 041 (4): a step whose caption and markers already say what to do has no action line; the hand
        // icon + line (actionRow) is then hidden, never shown empty.
        [Tooltip("No action line at this step (decision 041): the action row is hidden and no .action key is read.")]
        public bool noAction;
    }

    [SerializeField] private Step[] steps;

    [Header("Bar (#89, decision 026; #129: buttons only)")]
    [Tooltip("Interactables/Tour Nav Bar; hidden until the tour opens.")]
    [SerializeField] private GameObject barRoot;

    [Header("Texts at the screen's depth (#129, decision 040)")]
    [Tooltip("Caption right of the screen for the steps without their own caption (intro, not_used, material_flow).")]
    [SerializeField] private GameObject stepCaption;

    [SerializeField] private TMP_Text stepCaptionTitle;
    [SerializeField] private TMP_Text stepCaptionBody;

    [Tooltip("Hand icon + action line; moved under the current step's caption.")]
    [SerializeField] private RectTransform actionRow;

    [Tooltip("The \"what to do\" line next to the hand icon (inside actionRow).")]
    [SerializeField] private TMP_Text actionText;

    [Tooltip("Gap in caption units between a caption without a layout group (cartons) and the action row below it.")]
    [SerializeField] private float actionRowGap = 8f;

    [SerializeField] private TMP_Text progressText;

    [Tooltip("The tour opens by itself after this many seconds (#155, decision 041: 2 s, fixed per layout_version; R reads it as a constant); 0 = never.")]
    [SerializeField] private float autoOpenSeconds = 2f;

    [Header("Next control")]
    [SerializeField] private Toggle nextToggle;
    [SerializeField] private TMP_Text nextLabel;

    [Tooltip("Whole right button incl. its poke surface; hidden while a step waits for its follow-up (#104). Empty = the toggle's GameObject.")]
    [SerializeField] private GameObject nextButtonRoot;

    [Header("Back control (decision 024)")]
    [SerializeField] private Toggle backToggle;
    [SerializeField] private TMP_Text backLabel;

    [Tooltip("Dimmed at step 1, where Zurück does nothing.")]
    [SerializeField] private CanvasGroup backGroup;

    private int current;
    private bool ready;
    private bool offersQuestions;
    private bool questionsDone;
    private bool followUpShown;
    // #129: false while the quiz has taken the bar's place; the texts at the screen hide with the bar.
    private bool textsShown = true;
    private float actionRowHeight;

    // Step id of the step just shown, raised on every step change incl. the first (#73: the quiz unlocks at a step).
    public event Action<string> StepShown;

    // #93: the right button was pressed at the last step while OffersQuestions is set.
    public event Action QuestionsRequested;

    public bool IsOpen { get; private set; }

    // #93: set by the quiz while it is open and not yet answered; the last step's right button leads to it.
    public bool OffersQuestions
    {
        get => offersQuestions;
        set
        {
            offersQuestions = value;
            if (IsOpen && ready)
            {
                ShowNextLabel();
            }
        }
    }

    private bool IsLastStep => current == steps.Length - 1;

    // Null until the bar is open (#89).
    public string CurrentStepId => IsOpen ? steps[current].id : null;

    private void OnEnable()
    {
        if (nextToggle != null)
        {
            nextToggle.onValueChanged.AddListener(OnNextToggled);
        }

        if (backToggle != null)
        {
            backToggle.onValueChanged.AddListener(OnBackToggled);
        }
    }

    private void OnDisable()
    {
        if (nextToggle != null)
        {
            nextToggle.onValueChanged.RemoveListener(OnNextToggled);
        }

        if (backToggle != null)
        {
            backToggle.onValueChanged.RemoveListener(OnBackToggled);
        }
    }

    private void Start()
    {
        if (RequiredReference.IsMissing(barRoot, nameof(barRoot), this)
            | RequiredReference.IsMissing(stepCaption, nameof(stepCaption), this)
            | RequiredReference.IsMissing(stepCaptionTitle, nameof(stepCaptionTitle), this)
            | RequiredReference.IsMissing(stepCaptionBody, nameof(stepCaptionBody), this)
            | RequiredReference.IsMissing(actionRow, nameof(actionRow), this)
            | RequiredReference.IsMissing(actionText, nameof(actionText), this)
            | RequiredReference.IsMissing(nextToggle, nameof(nextToggle), this)
            | RequiredReference.IsMissing(backToggle, nameof(backToggle), this))
            return;

        if (steps == null || steps.Length == 0)
        {
            Debug.LogError("WmsFieldTour: steps is empty.", this);
            return;
        }

        if (backLabel != null)
        {
            backLabel.text = Loc.Get(BackKey);
        }

        // No step shown yet: every marker off.
        ShowMarkersOf(null);

        actionRowHeight = actionRow.rect.height;
        stepCaption.SetActive(false);
        actionRow.gameObject.SetActive(false);
        ready = true;

        // Open() may already have run (card 1 done before this Start).
        if (IsOpen)
        {
            PanelFade.SetVisible(barRoot, true);
            ShowStep(0);
            return;
        }

        barRoot.SetActive(false);
        if (autoOpenSeconds > 0f)
        {
            StartCoroutine(OpenAfterDelay());
        }
    }

    // #89: shows the bar at step 1; later calls do nothing. Called when card 1 is done (ConceptCardProgress).
    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        if (!ready)
            return;

        PanelFade.SetVisible(barRoot, true);
        ShowStep(0);
    }

    // Also callable from a UnityEvent.
    public void NextStep()
    {
        if (!IsOpen)
            return;

        if (offersQuestions && IsLastStep)
        {
            QuestionsRequested?.Invoke();
            return;
        }

        ShowStep((current + 1) % steps.Length);
    }

    // #93: all questions are right; the last step goes back to „Von vorn“ and its action line points to the Menu.
    public void FinishQuestions()
    {
        questionsDone = true;
        OffersQuestions = false;
        ShowAction();
    }

    // #93: the quiz takes the bar's place while it is shown. Does nothing before the bar is open.
    // #129: the step texts at the screen go with the bar (the question takes their place); both fade.
    public void SetBarVisible(bool visible)
    {
        if (!IsOpen || !ready)
            return;

        textsShown = visible;
        PanelFade.SetVisible(barRoot, visible);
        PanelFade.SetVisible(actionRow.gameObject, visible && HasAction(steps[current]));
        if (!HasCaption(steps[current]))
        {
            // #156: the quiz's question caption takes this spot when the bar hides, so leave at once (no overlap).
            if (visible)
            {
                PanelFade.SetVisible(stepCaption, true);
            }
            else
            {
                PanelFade.HideNow(stepCaption);
            }
        }
    }

    // #104: the current step's second text (after the lane demo, or when a hint keeps the demo from starting). Only
    // steps with hasFollowUp react; later calls change nothing.
    public void ShowFollowUp()
    {
        if (!IsOpen || !ready || !steps[current].hasFollowUp || followUpShown)
            return;

        followUpShown = true;
        ShowTexts();
        ShowAction();
        ShowNextLabel();
    }

    // Decision 024: back one step; does nothing at step 1 (no step change, no log).
    public void PreviousStep()
    {
        if (!IsOpen || current == 0)
            return;

        ShowStep(current - 1);
    }

    private IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(autoOpenSeconds);
        Open();
    }

    private void OnNextToggled(bool isOn)
    {
        if (!isOn)
            return;

        // Push button: release at once so the next poke counts again.
        nextToggle.SetIsOnWithoutNotify(false);
        NextStep();
    }

    private void OnBackToggled(bool isOn)
    {
        if (!isOn)
            return;

        backToggle.SetIsOnWithoutNotify(false);
        PreviousStep();
    }

    private void ShowStep(int index)
    {
        current = index;
        followUpShown = false;

        ShowMarkersOf(steps[index]);

        Step step = steps[index];
        ShowTexts();
        ShowAction();
        PlaceActionRow();

        if (progressText != null)
        {
            progressText.text = Loc.Get(ProgressKey, ("n", index + 1), ("total", steps.Length));
        }

        ShowNextLabel();

        bool canGoBack = index > 0;
        backToggle.interactable = canGoBack;
        if (backGroup != null)
        {
            backGroup.alpha = canGoBack ? 1f : DimmedAlpha;
        }

        ResearchLogger.LogTourStep(step.id, index + 1);
        StepShown?.Invoke(step.id);
    }

    private void ShowTexts()
    {
        Step step = steps[current];
        string title = Loc.Get(StepKeyPrefix + step.id + ".title");
        string body = Loc.Get(StepKeyPrefix + step.id + (followUpShown ? ".body_after" : ".body"));
        if (HasCaption(step))
        {
            step.captionTitle.text = title;
            step.captionBody.text = body;
            stepCaption.SetActive(false);
        }
        else
        {
            stepCaptionTitle.text = title;
            stepCaptionBody.text = body;
            stepCaption.SetActive(textsShown);
        }
    }

    private static bool HasCaption(Step step) => step.captionTitle != null && step.captionBody != null;

    private static bool HasAction(Step step) => !step.noAction;

    // #129: the action line sits under the step's own caption (the marker that holds captionTitle), or in stepCaption.
    private void PlaceActionRow()
    {
        Step step = steps[current];
        RectTransform host = null;
        if (HasCaption(step) && step.markers != null)
        {
            foreach (GameObject marker in step.markers)
            {
                if (marker != null && step.captionTitle.transform.IsChildOf(marker.transform))
                {
                    host = marker.transform as RectTransform;
                    break;
                }
            }
        }

        if (host == null)
        {
            host = (RectTransform)stepCaption.transform;
        }

        actionRow.SetParent(host, false);
        actionRow.SetAsLastSibling();
        if (host.GetComponent<LayoutGroup>() == null)
        {
            // A caption without a layout group: the row hangs below it, as wide as the caption. Since #154 every caption
            // has a layout group (Caption cartons keeps its hand-laid division in a fixed block), so the row sits inside.
            actionRow.anchorMin = new Vector2(0f, 0f);
            actionRow.anchorMax = new Vector2(1f, 0f);
            actionRow.pivot = new Vector2(0.5f, 1f);
            actionRow.anchoredPosition3D = new Vector3(0f, -actionRowGap, 0f);
            actionRow.sizeDelta = new Vector2(0f, actionRowHeight);
            // Lay the row out at the caption's width, then make it as high as its text.
            LayoutRebuilder.ForceRebuildLayoutImmediate(actionRow);
            float height = Mathf.Max(actionRowHeight, LayoutUtility.GetPreferredHeight(actionRow));
            actionRow.sizeDelta = new Vector2(0f, height);
        }

        actionRow.gameObject.SetActive(textsShown && HasAction(step));
        LayoutRebuilder.MarkLayoutForRebuild(host);
    }

    private void ShowNextLabel()
    {
        // #104: „Zu den Fragen“ comes only after the step's follow-up (watch first).
        bool waiting = offersQuestions && IsLastStep && steps[current].hasFollowUp && !followUpShown;
        (nextButtonRoot != null ? nextButtonRoot : nextToggle.gameObject).SetActive(!waiting);

        if (nextLabel == null)
            return;

        string key = !IsLastStep ? NextKey : offersQuestions ? ToQuestionsKey : RestartKey;
        nextLabel.text = Loc.Get(key);
    }

    // The "what to do" line of the current step (#104: task + purpose).
    private void ShowAction()
    {
        if (!IsOpen || !ready)
            return;

        Step step = steps[current];
        if (!HasAction(step))
        {
            actionText.text = string.Empty;
            return;
        }

        if (questionsDone && IsLastStep)
        {
            actionText.text = Loc.Get(StepKeyPrefix + step.id + ".action_done");
            return;
        }

        actionText.text = Loc.Get(StepKeyPrefix + step.id + (followUpShown ? ".action_after" : ".action"));
    }

    // #156: steps may share a marker. Each marker is set once to its final state: a marker of both the old and the new
    // step stays on (no OnDisable/OnEnable, so a bobbing arrow doesn't jump), and a marker whose gate says it is done
    // (IStepMarkerGate) is never switched on, not even for one frame.
    private void ShowMarkersOf(Step shown)
    {
        var show = new HashSet<GameObject>();
        if (shown?.markers != null)
        {
            foreach (GameObject marker in shown.markers)
            {
                if (marker != null && (!marker.TryGetComponent(out IStepMarkerGate gate) || gate.MayShow))
                {
                    show.Add(marker);
                }
            }
        }

        foreach (Step step in steps)
        {
            if (step.markers == null)
                continue;

            foreach (GameObject marker in step.markers)
            {
                if (marker != null && marker.activeSelf != show.Contains(marker))
                {
                    marker.SetActive(show.Contains(marker));
                }
            }
        }
    }
}
