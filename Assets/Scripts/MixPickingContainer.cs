using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;

public class MixedPickingContainer : MonoBehaviour
{
    [Header("Required quantities")]
    [SerializeField] private int requiredGGA = 2;
    [SerializeField] private int requiredGGB = 2;
    [SerializeField] private int requiredVPE = 0;

    [Header("WMS texts")]
    [SerializeField] private TMP_Text packageCountText;

    private readonly HashSet<PackageData> packagesInside = new();

    public int GGACount { get; private set; }
    public int GGBCount { get; private set; }
    public int VPECount { get; private set; }

    private void Start()
    {
        UpdateCounts();
    }

    private void OnTriggerEnter(Collider other)
    {
        PackageData package =
            other.GetComponentInParent<PackageData>();

        if (package == null)
            return;

        if (!IsCountablePackage(package.packageType))
            return;

        if (packagesInside.Add(package))
        {
            UpdateCounts();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PackageData package =
            other.GetComponentInParent<PackageData>();

        if (package == null)
            return;

        if (packagesInside.Remove(package))
        {
            UpdateCounts();
        }
    }

    private bool IsCountablePackage(PackageType type)
    {
        return type == PackageType.GGA || type == PackageType.GGB ||
               type == PackageType.VPEFull;
    }

    private void UpdateCounts()
    {
        GGACount = 0;
        GGBCount = 0;
        VPECount = 0;

        foreach (PackageData package in packagesInside)
        {
            if (package.packageType == PackageType.GGA)
            {
                GGACount++;
            }
            else if (package.packageType == PackageType.GGB)
            {
                GGBCount++;
            }
            else if (package.packageType == PackageType.VPEFull)
            {
                VPECount++;
            }
        }

        if (packageCountText != null)
        {
            packageCountText.text =
                $"PACKAGE PLACED: {GGACount + GGBCount + VPECount}";
        }
    }

    public bool IsOrderCorrect()
    {
        return GGACount == requiredGGA && GGBCount == requiredGGB &&
               VPECount == requiredVPE;
    }
}