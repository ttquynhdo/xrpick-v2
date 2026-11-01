using UnityEngine;

public class OrderConfirm : MonoBehaviour
{
    public CalcForWMS calc;
    [SerializeField]
    private GameObject correct;
    [SerializeField]
    private GameObject incorrect;

    private void OnEnable()
    {
        if (calc != null)
        {
            calc.ContentsChanged += ClearResult;
        }
    }

    private void OnDisable()
    {
        if (calc != null)
        {
            calc.ContentsChanged -= ClearResult;
        }
    }

    public void OrderConfirmation()
    {
        // Non-short-circuit | so every missing reference is logged at once.
        bool missing =
            RequiredReference.IsMissing(calc, nameof(calc), this) |
            RequiredReference.IsMissing(correct, nameof(correct), this) |
            RequiredReference.IsMissing(incorrect, nameof(incorrect), this);

        if (missing)
            return;

        if (calc.currentSize == calc.necessarySize)
        {
            correct.SetActive(true);
            incorrect.SetActive(false);
        } else
        {
            correct.SetActive(false);
            incorrect.SetActive(true);
        }
    }

    // #4: an old result must not stay visible once the container content changes.
    private void ClearResult()
    {
        if (correct != null)
        {
            correct.SetActive(false);
        }
        if (incorrect != null)
        {
            incorrect.SetActive(false);
        }
    }
}
