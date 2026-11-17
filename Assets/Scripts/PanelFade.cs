using System.Collections;
using UnityEngine;

/// <summary>
/// #129, decision 040: shows and hides a world-space panel with a short alpha fade instead of switching it on or off at
/// once, so a panel near the reading spot never pops in or out. Hiding fades out, then deactivates the GameObject;
/// while fading out the panel takes no pokes. Showing again during a fade-out turns it around.
/// Added at runtime to the panel by <see cref="SetVisible"/>; it adds a CanvasGroup when the panel has none.
/// </summary>
public class PanelFade : MonoBehaviour
{
    public const float FadeSeconds = 0.25f;

    private CanvasGroup group;
    private Coroutine running;

    // True from the start of a fade-out until the panel is deactivated.
    public bool IsFadingOut { get; private set; }

    /// <summary>Fades the panel in (activating it) or out (deactivating it at the end).</summary>
    public static void SetVisible(GameObject panel, bool visible)
    {
        if (panel == null)
            return;

        if (!visible && !panel.activeSelf)
            return;

        PanelFade fade = panel.GetComponent<PanelFade>();
        if (fade == null)
        {
            fade = panel.AddComponent<PanelFade>();
        }

        if (visible)
        {
            fade.FadeIn();
        }
        else
        {
            fade.FadeOut();
        }
    }

    /// <summary>Shown and not on its way out.</summary>
    public static bool IsShown(GameObject panel)
    {
        if (panel == null || !panel.activeSelf)
            return false;

        PanelFade fade = panel.GetComponent<PanelFade>();
        return fade == null || !fade.IsFadingOut;
    }

    private CanvasGroup Group
    {
        get
        {
            if (group == null)
            {
                group = GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = gameObject.AddComponent<CanvasGroup>();
                }
            }

            return group;
        }
    }

    private void FadeIn()
    {
        bool wasHidden = !gameObject.activeSelf;
        IsFadingOut = false;
        if (wasHidden)
        {
            Group.alpha = 0f;
            gameObject.SetActive(true);
        }

        // Pokes count only once the panel is half visible (see Fade).
        Group.interactable = Group.alpha >= 0.5f;
        Group.blocksRaycasts = Group.interactable;
        Restart(Fade(1f, false));
    }

    private void FadeOut()
    {
        // A panel under an inactive parent cannot run a coroutine: hide it at once.
        if (!isActiveAndEnabled)
        {
            Group.alpha = 1f;
            gameObject.SetActive(false);
            return;
        }

        IsFadingOut = true;
        Group.interactable = false;
        Group.blocksRaycasts = false;
        Restart(Fade(0f, true));
    }

    private void Restart(IEnumerator fade)
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }

        if (isActiveAndEnabled)
        {
            running = StartCoroutine(fade);
        }
        else
        {
            Group.alpha = 1f;
        }
    }

    private IEnumerator Fade(float target, bool deactivateAtEnd)
    {
        float start = Group.alpha;
        float step = 1f / FadeSeconds;
        for (float t = 0f; t < 1f; t += Time.deltaTime * step)
        {
            Group.alpha = Mathf.Lerp(start, target, t);
            if (!deactivateAtEnd && Group.alpha >= 0.5f && !Group.interactable)
            {
                Group.interactable = true;
                Group.blocksRaycasts = true;
            }
            yield return null;
        }

        Group.alpha = target;
        if (!deactivateAtEnd)
        {
            Group.interactable = true;
            Group.blocksRaycasts = true;
        }
        running = null;
        if (deactivateAtEnd)
        {
            IsFadingOut = false;
            // Ready to show at full alpha the next time something activates it directly.
            Group.alpha = 1f;
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Deactivated from outside during a fade: never stay half visible or untouchable.
        if (running != null)
        {
            running = null;
        }

        IsFadingOut = false;
        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }
}
