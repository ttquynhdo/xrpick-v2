using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// #19, docs/decisions/016: an instruction card that shows only the text of the current position.
/// #55: each position names a Loc key (table UI, docs/i18n/strings-todo.csv), not the text itself.
/// </summary>
public class PositionTexts : MonoBehaviour
{
    [Tooltip("Component implementing IPositionedOrder (MixedPickingContainer, PiecesContainerCounter).")]
    [SerializeField] private MonoBehaviour container;

    [SerializeField] private TMP_Text target;

    [Tooltip("One Loc key per order line; index 0 = position 1.")]
    [FormerlySerializedAs("textPerPosition")]
    [SerializeField] private string[] keyPerPosition;

    private IPositionedOrder order;

    private void Awake()
    {
        order = container as IPositionedOrder;
    }

    private void OnEnable()
    {
        if (order != null)
        {
            order.LineAdvanced += Show;
        }
    }

    private void OnDisable()
    {
        if (order != null)
        {
            order.LineAdvanced -= Show;
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

    private void Show(int position)
    {
        int index = position - 1;
        if (target == null || keyPerPosition == null || index < 0 || index >= keyPerPosition.Length)
            return;

        target.text = Loc.Get(keyPerPosition[index]);
    }
}
