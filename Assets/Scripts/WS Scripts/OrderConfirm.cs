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
