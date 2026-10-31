using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScaleItemCounter : MonoBehaviour
{
    [Header("Scale display")]
    [SerializeField] private TMP_Text itemCountText;

    private readonly HashSet<GameObject> itemsOnScale = new();

    public int ItemCount => itemsOnScale.Count;

    private void Start()
    {
        UpdateDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject itemRoot = GetItemRoot(other);

        if (itemRoot == null || !itemRoot.CompareTag("LooseItem"))
            return;

        if (itemsOnScale.Add(itemRoot))
        {
            UpdateDisplay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject itemRoot = GetItemRoot(other);

        if (itemRoot == null || !itemRoot.CompareTag("LooseItem"))
            return;

        if (itemsOnScale.Remove(itemRoot))
        {
            UpdateDisplay();
        }
    }

    private GameObject GetItemRoot(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            return other.attachedRigidbody.gameObject;
        }

        return other.transform.root.gameObject;
    }

    private void UpdateDisplay()
    {
        if (itemCountText != null)
        {
            itemCountText.text = $"PIECES: {ItemCount}";
        }
    }
}