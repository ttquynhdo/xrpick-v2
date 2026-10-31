using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PiecesContainerCounter : MonoBehaviour
{
    [Header("Order settings")]
    [SerializeField] private int piecesPerGGA = 50;
    [SerializeField] private int piecesPerGGB = 25;
    [SerializeField] private int requiredPieces = 55;

    [Header("Display")]
    [SerializeField] private TMP_Text piecesPlacedText;
    [SerializeField] private TMP_Text packagesPlacedText;

    private readonly HashSet<PackageData> objectsInside = new();

    public int GGACount { get; private set; }
    public int GGBCount { get; private set; }
    public int LooseItemCount { get; private set; }

    public int TotalPieces =>
        (GGACount * piecesPerGGA) + (GGBCount * piecesPerGGB) + LooseItemCount;

    private void Start()
    {
        Recalculate();
    }

    private void OnTriggerEnter(Collider other)
    {
        PackageData item = other.GetComponentInParent<PackageData>();

        if (item == null)
            return;

        //if (!IsAcceptedType(item.packageType))
        //    return;

        if (objectsInside.Add(item))
        {
            Recalculate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PackageData item = other.GetComponentInParent<PackageData>();

        if (item == null)
            return;

        if (objectsInside.Remove(item))
        {
            Recalculate();
        }
    }

    private bool IsAcceptedType(PackageType type)
    {
        return type == PackageType.VPEFull ||
               type == PackageType.LooseItemDisplay;
    }

    private void Recalculate()
    {
        GGACount = 0;
        GGBCount = 0;
        LooseItemCount = 0;

        foreach (PackageData item in objectsInside)
        {
            if (item.packageType == PackageType.GGA)
            {
                GGACount++;
            }
            else if (item.packageType == PackageType.GGB)
            {
                GGBCount++;
            }
            else if (item.packageType == PackageType.LooseItemDisplay)
            {
                LooseItemCount++;
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (piecesPlacedText != null)
        {
            piecesPlacedText.text =
                $"PIECES PLACED: {TotalPieces} / {requiredPieces}";
        }
        if (packagesPlacedText != null)
        {
            packagesPlacedText.text =
                $"PACKAGES PLACED: {GGACount+ GGBCount} / 2";
        }
    }

    public bool IsCorrect()
    {
        return GGBCount == 2 && GGACount == 0 && LooseItemCount == 5;
    }
}