using TMPro;
using UnityEngine;

/// <summary>
/// #59: one look for the result card next to the red button in Basic, Mixed and Complex: a bold, coloured
/// first line („Richtig!“ / „Nicht richtig“) and a light, uncoloured body with the reason or the next step.
/// Colours are tuned for the dark card background.
/// </summary>
public static class ResultCardStyle
{
    public static readonly Color Correct = new(0.29f, 0.87f, 0.50f);
    public static readonly Color Wrong = new(1f, 0.42f, 0.42f);

    // #58: a reminder is not a rating, so it has no first line and keeps its own colour.
    public static readonly Color Reminder = new(1f, 0.85f, 0.30f);

    public static readonly Color Body = new(0.94f, 0.94f, 0.94f);

    // titleText may be null (card without a title line): the title then becomes the body's first line.
    public static void Show(TMP_Text titleText, TMP_Text bodyText, string title, Color titleColor, string body)
    {
        if (titleText == null)
        {
            bodyText.text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(titleColor)}>{title}</color></b>\n{body}";
            bodyText.color = Body;
            return;
        }

        titleText.gameObject.SetActive(true);
        titleText.text = title;
        titleText.color = titleColor;
        bodyText.text = body;
        bodyText.color = Body;
    }

    // Body only, in one colour (the #58 reminder); hides the title line.
    public static void ShowPlain(TMP_Text titleText, TMP_Text bodyText, string body, Color bodyColor)
    {
        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }
        bodyText.text = body;
        bodyText.color = bodyColor;
    }
}
