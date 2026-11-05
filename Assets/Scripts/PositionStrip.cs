using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// #97, docs/decisions/035: one row above the step cards of the instruction shelf (Mixed, Complex) that keeps the
/// lesson's progress while the step cards' done marks start again at each position: „Position 1 ✓ · Position 2“.
/// A finished position carries the Done Check badge, the current one is blue and underlined (two cues besides
/// colour). It changes when the next storage container has stopped, together with the cards and the WMS dialog.
/// </summary>
public class PositionStrip : MonoBehaviour
{
    private const string LabelKey = "common.position.label";
    private const string CheckName = "Done Check";

    [Header("Scene")]
    [Tooltip("Counter on the order container; must implement IPositionedOrder.")]
    [SerializeField] private MonoBehaviour container;

    [Tooltip("Validator of this scene; must implement IOrderCompletion.")]
    [SerializeField] private MonoBehaviour validator;

    [Tooltip("Optional. Storage containers on the station lanes (IStationStock); the row changes when they stop.")]
    [SerializeField] private MonoBehaviour stationStock;

    [Header("Row")]
    [Tooltip("Item of position 1 (a TMP label + a child \"Done Check\"); copied for the other positions.")]
    [SerializeField] private RectTransform itemTemplate;

    [Tooltip("Same blue as the step frame (#40).")]
    [SerializeField] private Color currentColor = new(0.012f, 0.569f, 0.867f, 1f);

    private readonly List<(TMP_Text label, GameObject check)> items = new();

    private IPositionedOrder order;
    private IOrderCompletion completion;
    private IStationStock stock;
    private Color baseColor;
    private int shownPosition;
    private bool completed;

    private void Awake()
    {
        order = container as IPositionedOrder;
        completion = validator as IOrderCompletion;
        stock = stationStock as IStationStock;
    }

    private void OnEnable()
    {
        if (order != null)
        {
            order.LineAdvanced += HandleLineAdvanced;
        }
        if (completion != null)
        {
            completion.OrderCompleted += HandleOrderCompleted;
        }
    }

    private void OnDisable()
    {
        if (order != null)
        {
            order.LineAdvanced -= HandleLineAdvanced;
        }
        if (completion != null)
        {
            completion.OrderCompleted -= HandleOrderCompleted;
        }
    }

    private void Start()
    {
        if (order == null)
        {
            Debug.LogError("PositionStrip: container is not assigned or is not an IPositionedOrder.", this);
            return;
        }
        if (completion == null)
        {
            Debug.LogError("PositionStrip: validator is not assigned or does not implement IOrderCompletion.", this);
        }
        if (RequiredReference.IsMissing(itemTemplate, nameof(itemTemplate), this))
            return;

        for (int i = 0; i < order.OrderLines.Count; i++)
        {
            RectTransform item = i == 0 ? itemTemplate : Instantiate(itemTemplate, itemTemplate.parent, false);
            item.name = $"Position {i + 1}";

            TMP_Text label = item.GetComponentInChildren<TMP_Text>(true);
            Transform check = item.Find(CheckName);
            if (label == null || check == null)
            {
                Debug.LogError($"PositionStrip: {itemTemplate.name} needs a TMP label and a child \"{CheckName}\".", this);
                return;
            }

            label.text = Loc.Get(LabelKey, ("position", i + 1));
            items.Add((label, check.gameObject));
        }

        baseColor = items[0].label.color;
        shownPosition = order.CurrentPosition;
        if (completion != null && completion.IsCompleted)
        {
            completed = true;
        }

        Apply();
    }

    private void HandleLineAdvanced(int position)
    {
        if (stock == null)
        {
            Show(position);
            return;
        }

        StartCoroutine(ShowWhenStockSettled());
    }

    private IEnumerator ShowWhenStockSettled()
    {
        yield return StationStockWait.UntilSettled(stock);

        Show(order.CurrentPosition);
    }

    private void Show(int position)
    {
        shownPosition = position;
        Apply();
    }

    private void HandleOrderCompleted()
    {
        completed = true;
        Apply();
    }

    private void Apply()
    {
        for (int i = 0; i < items.Count; i++)
        {
            int position = i + 1;
            bool isDone = completed || position < shownPosition;
            bool isCurrent = !completed && position == shownPosition;

            (TMP_Text label, GameObject check) = items[i];
            check.SetActive(isDone);
            label.color = isCurrent ? currentColor : baseColor;
            label.fontStyle = isCurrent ? FontStyles.Bold | FontStyles.Underline : FontStyles.Bold;
        }
    }
}
