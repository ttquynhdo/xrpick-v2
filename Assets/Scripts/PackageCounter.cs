using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PackageCounter : MonoBehaviour
{
    [Header("Order Requirement")]
    [SerializeField] private int requiredGGCount = 2;

    [Header("WMS UI")]
    [SerializeField] private TMP_Text packagePlacedText;
    [SerializeField] private TMP_Text orderFeedbackText;
    [SerializeField] private TMP_Text instructionText;

    // Mỗi package chỉ xuất hiện một lần trong HashSet.
    private readonly HashSet<GameObject> ggPackagesInside = new();
    private readonly HashSet<GameObject> vpePackagesInside = new();

    private void Start()
    {
        UpdatePackageDisplay();

        if (orderFeedbackText != null)
        {
            orderFeedbackText.text = "";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject packageObject = GetPackageRoot(other);

        if (packageObject == null)
            return;

        if (packageObject.CompareTag("GGPackage"))
        {
            // Add trả về true chỉ khi package chưa có trong HashSet.
            if (ggPackagesInside.Add(packageObject))
            {
                Debug.Log(
                    "GG entered: " + packageObject.name +
                    ". GG count: " + ggPackagesInside.Count
                );
            }
        }
        else if (packageObject.CompareTag("VPEPackage"))
        {
            if (vpePackagesInside.Add(packageObject))
            {
                Debug.Log(
                    "VPE entered: " + packageObject.name +
                    ". VPE count: " + vpePackagesInside.Count
                );
            }
        }

        UpdatePackageDisplay();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject packageObject = GetPackageRoot(other);

        if (packageObject == null)
            return;

        if (packageObject.CompareTag("GGPackage"))
        {
            ggPackagesInside.Remove(packageObject);
        }
        else if (packageObject.CompareTag("VPEPackage"))
        {
            vpePackagesInside.Remove(packageObject);
        }

        UpdatePackageDisplay();
    }

    private GameObject GetPackageRoot(Collider other)
    {
        // Tìm Rigidbody chính của package.
        // Các collider con thường cùng thuộc Rigidbody này.
        if (other.attachedRigidbody != null)
        {
            return other.attachedRigidbody.gameObject;
        }

        return other.gameObject;
    }

    private void UpdatePackageDisplay()
    {
        int totalPackages =
            ggPackagesInside.Count + vpePackagesInside.Count;

        if (packagePlacedText != null)
        {
            packagePlacedText.text =
                "PACKAGE PLACED: " + totalPackages;
        }
    }

    public void CheckOrder()
    {
        if (orderFeedbackText == null)
            return;

        int ggCount = ggPackagesInside.Count;
        int vpeCount = vpePackagesInside.Count;

        if (ggCount == requiredGGCount && vpeCount == 0)
        {
            orderFeedbackText.text =
                "Correct! You selected 2 GG packages.\n\n";

            orderFeedbackText.color = Color.green;

            instructionText.text = "You have successfully completed the task.\n" +
            "Select the next scenario option in the MENU.";
        }
        else if (vpeCount > 0)
        {
            orderFeedbackText.text =
                "Incorrect package type. Remove the VPE package.";

            orderFeedbackText.color = Color.red;
        }
        else if (ggCount < requiredGGCount)
        {
            orderFeedbackText.text =
                "Not enough GG packages. Check the WMS and try again.";

            orderFeedbackText.color = Color.red;
        }
        else
        {
            orderFeedbackText.text =
                "Too many GG packages. Check the required quantity.";

            orderFeedbackText.color = Color.red;
        }
    }
}