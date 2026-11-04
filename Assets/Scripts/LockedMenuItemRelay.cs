using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// #29, decision 021: added at runtime by MenuCurrentLesson to each Menu item. A locked item's Toggle ignores
/// the touch (CanvasGroup not interactable), but the click still reaches this GameObject, so the attempt is
/// logged as menu_select with blocked = true. Open items log through SceneMenuController instead.
/// </summary>
public class LockedMenuItemRelay : MonoBehaviour, IPointerClickHandler
{
    private string targetScene;
    private CanvasGroup group;

    public void Init(string scene)
    {
        targetScene = scene;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // The CanvasGroup is added only when an item gets locked, so look it up at click time.
        if (group == null)
            group = GetComponent<CanvasGroup>();

        if (group != null && !group.interactable)
            ResearchLogger.LogMenuSelect(targetScene, true);
    }
}
