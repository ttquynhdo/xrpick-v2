using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// #19, docs/decisions/016: an instruction card that shows only the text of the current position.
/// #55: each position names a Loc key (table UI, docs/i18n/strings-todo.csv), not the text itself.
/// #97, decision 035: with station stock the next position's text shows only once the storage containers stopped.
/// </summary>
public class PositionTexts : MonoBehaviour
{
    [Tooltip("Component implementing IPositionedOrder (MixedPickingContainer, PiecesContainerCounter).")]
    [SerializeField] private MonoBehaviour container;

    [SerializeField] private TMP_Text target;

    [Tooltip("One Loc key per order line; index 0 = position 1.")]
    [FormerlySerializedAs("textPerPosition")]
    [SerializeField] private string[] keyPerPosition;

    [Tooltip("Optional. Storage containers on the station lanes (IStationStock); the text changes when they stop.")]
    [SerializeField] private MonoBehaviour stationStock;

    private IPositionedOrder order;
    private IStationStock stock;

    private void Awake()
    {
        order = container as IPositionedOrder;
        stock = stationStock as IStationStock;
    }

    private void OnEnable()
    {
        if (order != null)
        {
            order.LineAdvanced += HandleLineAdvanced;
        }
    }

    private void OnDisable()
    {
        if (order != null)
        {
            order.LineAdvanced -= HandleLineAdvanced;
        }
    }

    private void Start()
    {
        if (order == null)
        {
            Debug.LogError("PositionTexts: container is not assigned or is not an IPositionedOrder.", this);
            return;
        }
        if (RequiredReference.IsMissing(target, nameof(target), this))
            return;

        int count = keyPerPosition != null ? keyPerPosition.Length : 0;
        if (count != order.OrderLines.Count)
        {
            Debug.LogError($"PositionTexts: {count} keys but {order.OrderLines.Count} order lines.", this);
        }

        Show(order.CurrentPosition);
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
        int index = position - 1;
        if (target == null || keyPerPosition == null || index < 0 || index >= keyPerPosition.Length)
            return;

        target.text = Loc.Get(keyPerPosition[index]);
    }
}
