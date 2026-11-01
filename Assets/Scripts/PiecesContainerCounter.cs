using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PiecesContainerCounter : MonoBehaviour
{
    // #15: counters show only what is placed, never the required amount.
    // Keys in docs/i18n/strings-todo.csv until #11/#12 move them to Loc.Get.
    private const string PiecesPlacedFormat = "Ist: {0} ST";            // complex.counter.ist
    private const string PackagesPlacedFormat = "Cartons placed: {0}"; // complex.counter.cartons_placed

    [Header("Order settings")]
    [SerializeField] private int piecesPerGGA = 50;
    [SerializeField] private int piecesPerGGB = 25;

    [Header("Display")]
    [SerializeField] private TMP_Text piecesPlacedText;
    [SerializeField] private TMP_Text packagesPlacedText;

    private readonly HashSet<PackageData> objectsInside = new();

    public int GGACount { get; private set; }
    public int GGBCount { get; private set; }
    public int LooseItemCount { get; private set; }

    // #3: any other package type in the container makes the order wrong.
    public int OtherCount { get; private set; }

    public int TotalPieces =>
        (GGACount * piecesPerGGA) + (GGBCount * piecesPerGGB) + LooseItemCount;

    // #4: raised when a package enters or leaves, so an old result can be cleared.
    public event Action ContentsChanged;

    private void Start()
    {
        Recalculate();
    }

    private void OnTriggerEnter(Collider other)
    {
        PackageData item = other.GetComponentInParent<PackageData>();

        if (item == null)
            return;

        if (objectsInside.Add(item))
        {
            Recalculate();
            ContentsChanged?.Invoke();
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
            ContentsChanged?.Invoke();
        }
    }

    private void Recalculate()
    {
        GGACount = 0;
        GGBCount = 0;
        LooseItemCount = 0;
        OtherCount = 0;

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
            else
            {
                OtherCount++;
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (piecesPlacedText != null)
        {
            piecesPlacedText.text = string.Format(PiecesPlacedFormat, TotalPieces);
        }
        if (packagesPlacedText != null)
        {
            packagesPlacedText.text =
                string.Format(PackagesPlacedFormat, GGACount + GGBCount + OtherCount);
        }
    }

    public bool IsCorrect()
    {
        return GGBCount == 2 && GGACount == 0 && LooseItemCount == 5 && OtherCount == 0;
    }
}