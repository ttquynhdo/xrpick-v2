using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #40, docs/decisions/017 (#79: shared with ConceptScene): one instruction card's look, i.e. a frame built at
/// runtime from the card's own sprite (no raycast target) and the original colours of its texts, so they can be
/// dimmed while the card is not current.
/// </summary>
public sealed class InstructionCardLook
{
    // #87 (decision 027): the goal card „Darum geht es“ is cream with a neutral border and dark text; step cards
    // stay dark. The goal card never gets a frame, dimming or check mark. Applied in the Editor by
    // InstructionShelfLayout, so the scene shows the same look without Play.
    public static readonly Color GoalCardColor = new(0.957f, 0.925f, 0.847f, 1f);
    public static readonly Color GoalBorderColor = new(0.725f, 0.678f, 0.580f, 1f);
    public static readonly Color GoalTitleColor = new(0.173f, 0.165f, 0.149f, 1f);
    public static readonly Color GoalBodyColor = new(0.227f, 0.212f, 0.188f, 1f);

    public const string GoalBorderName = "Goal Border";

    private readonly Image frame;
    private readonly TMP_Text[] texts;
    private readonly Color[] baseColors;

    public InstructionCardLook(RectTransform card, float outset)
    {
        texts = card.GetComponentsInChildren<TMP_Text>(true);
        baseColors = new Color[texts.Length];

        for (int i = 0; i < texts.Length; i++)
        {
            baseColors[i] = texts[i].color;
        }

        var frameObject = new GameObject("Step Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        frameObject.layer = card.gameObject.layer;

        var rect = (RectTransform)frameObject.transform;
        rect.SetParent(card, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-outset, -outset);
        rect.offsetMax = new Vector2(outset, outset);

        frameObject.GetComponent<LayoutElement>().ignoreLayout = true;

        Image cardImage = card.GetComponent<Image>();
        frame = frameObject.GetComponent<Image>();
        frame.sprite = cardImage != null ? cardImage.sprite : null;
        frame.type = Image.Type.Sliced;
        frame.fillCenter = false;
        frame.raycastTarget = false;
        frame.enabled = false;
    }

    public void Show(Color frameColor, float thinness, bool dimmed, float dimFactor)
    {
        frame.enabled = frameColor.a > 0f;
        frame.color = frameColor;
        frame.pixelsPerUnitMultiplier = thinness;

        for (int i = 0; i < texts.Length; i++)
        {
            Color color = baseColors[i];

            if (dimmed)
            {
                color.a *= dimFactor;
            }

            texts[i].color = color;
        }
    }
}
