using TMPro;
using UnityEngine;

public class OrderConfirm : MonoBehaviour
{
    public CalcForWMS calc;
    [SerializeField]
    private GameObject correct;
    [SerializeField]
    private GameObject incorrect;

    // #14: the Incorrect card's text shows the reason of the first error (German, Sie).
    [SerializeField]
    private TMP_Text incorrectText;

    // #41, decision 003: EKZ 3 needs every carton in its own snap place. Empty = no placement rule (EKZ 0).
    [SerializeField]
    private EkzPlacementCheck placementCheck;

    private void OnEnable()
    {
        if (calc != null)
        {
            calc.ContentsChanged += ClearResult;
        }
        if (placementCheck != null)
        {
            placementCheck.PlacementChanged += ClearResult;
        }
    }

    private void OnDisable()
    {
        if (calc != null)
        {
            calc.ContentsChanged -= ClearResult;
        }
        if (placementCheck != null)
        {
            placementCheck.PlacementChanged -= ClearResult;
        }
    }

    private void Start()
    {
        if (calc == null)
            return;

        foreach (string problem in OrderRule.FindLineProblems(new[] { calc.OrderLine }, false))
        {
            Debug.LogError($"OrderConfirm: {problem}", calc);
        }
    }

    public void OrderConfirmation()
    {
        // Non-short-circuit | so every missing reference is logged at once.
        bool missing =
            RequiredReference.IsMissing(calc, nameof(calc), this) |
            RequiredReference.IsMissing(correct, nameof(correct), this) |
            RequiredReference.IsMissing(incorrect, nameof(incorrect), this) |
            RequiredReference.IsMissing(incorrectText, nameof(incorrectText), this);

        if (missing)
            return;

        // #14: the same rule as Mixed/Complex decides (docs/decisions/006); Soll is still CalcForWMS.necessarySize.
        OrderResult result = OrderRule.Check(new[] { calc.OrderLine }, OrderContents.From(calc.PackagesInside), false);

        if (!result.Correct)
        {
            ShowIncorrect(OrderFeedback.Explain(result, FeedbackLanguage.German));
        }
        // Quantity errors come first; the placement is checked only once the quantity is right (decision 003, D6).
        else if (placementCheck != null && !placementCheck.AllPlaced(calc.PackagesInside))
        {
            ShowIncorrect(OrderFeedback.ExplainEkz3Placement(FeedbackLanguage.German));
        }
        else
        {
            correct.SetActive(true);
            incorrect.SetActive(false);
        }
    }

    private void ShowIncorrect(string reason)
    {
        incorrectText.text = reason;
        correct.SetActive(false);
        incorrect.SetActive(true);
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
