using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// #10, docs/decisions/008: the new WMS dialog "Kommissionierung" (TMA, CSC4) as in the confidential spec
/// docs/requirements/wms-specs/kommissionierung-dialog.md. Labels live in the prefab and are never translated
/// (decision 002); this script fills the dynamic fields from the scene's order source and only on events.
/// </summary>
public class WmsDialogView : MonoBehaviour
{
    private const string Modus = "Kommissionierung";
    private const float ConfirmFlashSeconds = 0.3f;

    // Colours: relative to the screenshots (B1); proposals from the spec's colour table.
    private static readonly Color Red = new(0.878f, 0.208f, 0.169f);           // #E0352B
    private static readonly Color Yellow = new(0.949f, 0.824f, 0.235f);        // #F2D23C, EKZ 003 (B3)
    private static readonly Color Grey = new(0.651f, 0.651f, 0.651f);          // #A6A6A6, EKZ 000
    private static readonly Color KeyEnabled = new(0.745f, 0.745f, 0.745f);    // #BEBEBE
    private static readonly Color KeyPressed = new(0.478f, 0.478f, 0.478f);    // #7A7A7A

    [Header("Order source")]
    [Tooltip("Component implementing IWmsOrderSource (CalcForWMS, PiecesContainerCounter).")]
    [SerializeField] private MonoBehaviour orderSource;

    [Header("R1 Modus")]
    [SerializeField] private TMP_Text modusValue;

    // R2 Hinweise / Infos: all indicators stay off, as built in the prefab; the training uses none of them
    // (docs/decisions/009).

    [Header("R3 Mengen")]
    [SerializeField] private TMP_Text soll;
    [SerializeField] private TMP_Text ist;
    [SerializeField] private TMP_Text rest;

    [Header("R4 Material")]
    [SerializeField] private TMP_Text empfKz;
    [SerializeField] private Image empfKzBackground;
    [SerializeField] private TMP_Text verpEinh;
    [SerializeField] private TMP_Text matNr;
    [SerializeField] private TMP_Text bez;

    [Header("R5 Footer")]
    [SerializeField] private TMP_Text gew;

    [Header("R6 Funktionstasten")]
    [Tooltip("Background of the F6 Bestätigen key; flashes when the red button (foot pedal) is pressed.")]
    [SerializeField] private Image confirmKey;

    private IWmsOrderSource source;
    private Coroutine flash;

    private void Awake()
    {
        source = orderSource as IWmsOrderSource;

        if (source == null)
        {
            Debug.LogError("WmsDialogView: orderSource is not assigned or does not implement IWmsOrderSource.", this);
        }
    }

    private void OnEnable()
    {
        if (source != null)
        {
            source.ContentsChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (source != null)
        {
            source.ContentsChanged -= Refresh;
        }

        // A disabled object stops its coroutines: don't leave F6 in the pressed colour.
        flash = null;
        if (confirmKey != null)
        {
            confirmKey.color = KeyEnabled;
        }
    }

    private void Start()
    {
        // Counters set Rest in their Start; Start order between objects is undefined, so wait one frame.
        StartCoroutine(RefreshAfterFirstFrame());

        if (source != null)
        {
            OrderContents.LogMaterialNumberMismatch(source.CurrentLine, this);
        }
    }

    // Hooked to the red button: it stands for the foot pedal, which is F6 Bestätigen on screen (decision 008).
    public void FlashConfirmKey()
    {
        if (confirmKey == null || !isActiveAndEnabled)
            return;

        if (flash != null)
        {
            StopCoroutine(flash);
        }
        flash = StartCoroutine(FlashKey());
    }

    private IEnumerator RefreshAfterFirstFrame()
    {
        yield return null;
        Refresh();
    }

    private IEnumerator FlashKey()
    {
        confirmKey.color = KeyPressed;
        yield return new WaitForSeconds(ConfirmFlashSeconds);
        confirmKey.color = KeyEnabled;
        flash = null;
    }

    private void Refresh()
    {
        OrderLine line = source?.CurrentLine;

        if (line == null)
            return;

        SetText(modusValue, Modus);
        SetText(soll, line.soll.ToString(CultureInfo.InvariantCulture));
        SetText(ist, source.IstPieces.ToString(CultureInfo.InvariantCulture));
        SetText(rest, source.RestPieces.ToString(CultureInfo.InvariantCulture));
        SetText(verpEinh, line.vpe.ToString(CultureInfo.InvariantCulture));
        SetText(matNr, line.materialNumber);
        SetText(bez, line.materialName);
        SetText(gew, line.weightKg.ToString("0.##", CultureInfo.GetCultureInfo("de-DE")));
        SetEmpfKz(line.empfKz);
    }

    private void SetEmpfKz(string code)
    {
        SetText(empfKz, code);

        if (empfKzBackground == null || empfKz == null)
            return;

        // B3: 006 red with white text, 000 grey, 003 yellow; other classes are not known yet.
        switch (code)
        {
            case "006":
                empfKzBackground.color = Red;
                empfKz.color = Color.white;
                break;
            case "003":
                empfKzBackground.color = Yellow;
                empfKz.color = Color.black;
                break;
            default:
                empfKzBackground.color = Grey;
                empfKz.color = Color.black;
                break;
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }
}
