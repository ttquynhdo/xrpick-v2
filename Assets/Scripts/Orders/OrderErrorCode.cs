/// <summary>
/// Order and quiz error codes (docs/decisions/006). The log names must match analysis/skill_map.csv
/// and docs/research-logging-spec.md §4.3 letter for letter.
/// </summary>
public enum OrderErrorCode
{
    Empty,
    // Reserved: no rule emits it under A2 (no GG/VPE package types). Kept so the log vocabulary stays stable.
    WrongType,
    WrongMaterial,
    TooFew,
    TooMany,
    MissingLoose,
    ExtraLoose,
    NoAnswer,
    WrongAnswer,
    // #41, decision 021: Basic cartons not lying ordered in the 6 snap places (EKZ 3). Decided by
    // OrderConfirm + EkzPlacementCheck, not by OrderRule; logged after the OrderRule codes.
    EkzPlacement
}

public static class OrderErrorCodes
{
    public static string ToLogName(OrderErrorCode code)
    {
        switch (code)
        {
            case OrderErrorCode.Empty: return "EMPTY";
            case OrderErrorCode.WrongType: return "WRONG_TYPE";
            case OrderErrorCode.WrongMaterial: return "WRONG_MATERIAL";
            case OrderErrorCode.TooFew: return "TOO_FEW";
            case OrderErrorCode.TooMany: return "TOO_MANY";
            case OrderErrorCode.MissingLoose: return "MISSING_LOOSE";
            case OrderErrorCode.ExtraLoose: return "EXTRA_LOOSE";
            case OrderErrorCode.NoAnswer: return "NO_ANSWER";
            case OrderErrorCode.WrongAnswer: return "WRONG_ANSWER";
            case OrderErrorCode.EkzPlacement: return "EKZ_PLACEMENT";
            default: return code.ToString();
        }
    }

    // Lower = shown first: read the material, then count cartons, then loose pieces (decision 006 §2).
    public static int Priority(OrderErrorCode code)
    {
        switch (code)
        {
            case OrderErrorCode.Empty: return 0;
            case OrderErrorCode.WrongType: return 1;
            case OrderErrorCode.WrongMaterial: return 2;
            case OrderErrorCode.TooFew:
            case OrderErrorCode.TooMany: return 3;
            case OrderErrorCode.MissingLoose:
            case OrderErrorCode.ExtraLoose: return 4;
            case OrderErrorCode.EkzPlacement: return 6;
            default: return 5;
        }
    }
}
