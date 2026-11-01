using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalcForWMS : MonoBehaviour, IWmsOrderSource
{
    public int currentSize;
    public int necessarySize;
    // #34, decision 001 option C: Rest is the stock left in the storage box after the
    // full order is picked. Set in Start from the GG packages in the scene, never changed by picking.
    public int restSize;
    public TextMeshProUGUI istText;
    public TextMeshProUGUI sollText;
    public TextMeshProUGUI restText;

    // #14: the material of the order, so OrderConfirm can use OrderRule (docs/decisions/006). Soll stays necessarySize.
    [Header("Order material")]
    [Tooltip("Same key as PackageData.MaterialKey on the order cartons (#17).")]
    [SerializeField] private string materialKey = "training_b";

    [Tooltip("Material name as on the label and in the WMS (Bez.).")]
    [SerializeField] private string materialName = "SCHLAUCHSCHELLE B";

    [Tooltip("Pieces per carton (VPE).")]
    [SerializeField] private int vpe = 50;

    // #10, decision 008: shown in the WMS dialog (fake data in real formats).
    [Tooltip("Fake Mat.-Nr, equal to PackageData.MaterialNumber of the order cartons (#17).")]
    [SerializeField] private string materialNumber = "443890";

    [Tooltip("Empf Kz: EKZ class as three digits (decision 003: Basic is EKZ 3).")]
    [SerializeField] private string empfKz = "003";

    [Tooltip("Gew. in kg of the whole order line (fake).")]
    [SerializeField] private float weightKg = 4.5f;

    public OrderLine OrderLine => new(materialKey, materialName, necessarySize, vpe)
    {
        materialNumber = materialNumber,
        empfKz = empfKz,
        weightKg = weightKg
    };

    OrderLine IWmsOrderSource.CurrentLine => OrderLine;

    int IWmsOrderSource.IstPieces => currentSize;

    int IWmsOrderSource.RestPieces => restSize;

    // Packages counted in Ist, as PackageData for OrderRule.
    public IEnumerable<PackageData> PackagesInside
    {
        get
        {
            foreach (BoxInfo box in collidersInside.Keys)
            {
                // A carton destroyed or disabled inside the trigger gets no OnTriggerExit and stays as a key (#41).
                if (box == null || !box.isActiveAndEnabled)
                    continue;

                PackageData package = box.GetComponent<PackageData>();

                if (package != null)
                    yield return package;
            }
        }
    }

    // #2: a package counts once, however many of its colliders are inside the trigger.
    private readonly Dictionary<BoxInfo, int> collidersInside = new();

    // #4: raised when a package enters or leaves, so an old result can be cleared.
    public event Action ContentsChanged;

    private void Start()
    {
        if (istText == null || sollText == null || restText == null)
        {
            Debug.LogError("CalcForWMS: istText, sollText or restText is not assigned.", this);
        }

        currentSize = 0;
        restSize = GetStockInScene() - necessarySize;
        UpdateDisplay();
    }

    private static int GetStockInScene()
    {
        int stock = 0;

        foreach (BoxInfo box in FindObjectsByType<BoxInfo>())
        {
            if (box.CompareTag("GGPackage"))
            {
                stock += box.packageSize;
            }
        }

        return stock;
    }

    private void OnTriggerEnter(Collider other)
    {
        BoxInfo box = GetPackage(other);

        if (box == null)
            return;

        collidersInside.TryGetValue(box, out int count);
        collidersInside[box] = count + 1;

        if (count == 0)
        {
            currentSize += box.packageSize;
            UpdateDisplay();
            ContentsChanged?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BoxInfo box = GetPackage(other);

        if (box == null || !collidersInside.TryGetValue(box, out int count))
            return;

        if (count > 1)
        {
            collidersInside[box] = count - 1;
            return;
        }

        collidersInside.Remove(box);
        currentSize -= box.packageSize;
        UpdateDisplay();
        ContentsChanged?.Invoke();
    }

    // Hands and other loose colliders have no BoxInfo; only GG packages are counted here.
    private static BoxInfo GetPackage(Collider other)
    {
        BoxInfo box = other.GetComponentInParent<BoxInfo>();

        if (box == null || !box.CompareTag("GGPackage"))
            return null;

        return box;
    }

    private void UpdateDisplay()
    {
        if (istText != null)
        {
            istText.text = currentSize.ToString();
        }
        if (sollText != null)
        {
            sollText.text = necessarySize.ToString();
        }
        if (restText != null)
        {
            restText.text = restSize.ToString();
        }
    }
}
