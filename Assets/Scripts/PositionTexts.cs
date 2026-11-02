using TMPro;
using UnityEngine;

/// <summary>
/// #19, docs/decisions/016: an instruction card that shows only the text of the current position.
/// Texts are typed per scene in the Inspector (German, keys in docs/i18n/strings-todo.csv).
/// </summary>
public class PositionTexts : MonoBehaviour
{
    [Tooltip("Component implementing IPositionedOrder (MixedPickingContainer, PiecesContainerCounter).")]
    [SerializeField] private MonoBehaviour container;

    [SerializeField] private TMP_Text target;

    [Tooltip("One text per order line; index 0 = position 1.")]
    [TextArea]
    [SerializeField] private string[] textPerPosition;

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

        int count = textPerPosition != null ? textPerPosition.Length : 0;
        if (count != order.OrderLines.Count)
        {
            Debug.LogError($"PositionTexts: {count} texts but {order.OrderLines.Count} order lines.", this);
        }

        Show(order.CurrentPosition);
    }

    private void Show(int position)
    {
        int index = position - 1;
        if (target == null || textPerPosition == null || index < 0 || index >= textPerPosition.Length)
            return;

        target.text = textPerPosition[index];
    }
}
