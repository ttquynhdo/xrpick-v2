using UnityEngine;

// #7: a missing Inspector reference is reported with class, field and GameObject
// instead of a NullReferenceException at the moment the trainee confirms.
public static class RequiredReference
{
    public static bool IsMissing(Object value, string fieldName, MonoBehaviour owner)
    {
        if (value != null)
            return false;

        Debug.LogError(
            $"{owner.GetType().Name} on '{owner.gameObject.name}': {fieldName} is not assigned.",
            owner);
        return true;
    }
}
