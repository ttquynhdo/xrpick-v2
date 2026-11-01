using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalcForWMS : MonoBehaviour
{
    public int currentSize;
    public int necessarySize;
    // #34, decision 001 option C: Rest is the stock left in the storage box after the
    // full order is picked. Set in Start from the GG packages in the scene, never changed by picking.
    public int restSize;
    public TextMeshProUGUI istText;
    public TextMeshProUGUI sollText;
    public TextMeshProUGUI restText;

    // #2: a package counts once, however many of its colliders are inside the trigger.
    private readonly Dictionary<BoxInfo, int> collidersInside = new();

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
