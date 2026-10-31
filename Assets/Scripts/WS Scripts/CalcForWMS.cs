using UnityEngine;
using TMPro;

public class CalcForWMS : MonoBehaviour
{
    public int currentSize;
    public int necessarySize;
    public int restSize;
    public TextMeshProUGUI istText;
    public TextMeshProUGUI sollText;
    public TextMeshProUGUI restText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSize = 0;
        istText.text = currentSize.ToString();
        sollText.text = necessarySize.ToString();
        restText.text = restSize.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        istText.text = currentSize.ToString();
        sollText.text = necessarySize.ToString();
        restText.text = restSize.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "GGPackage")
        {
            currentSize += other.GetComponent<BoxInfo>().packageSize;
            restSize -= other.GetComponent<BoxInfo>().packageSize;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "GGPackage")
        {
            currentSize -= other.GetComponent<BoxInfo>().packageSize;
            restSize += other.GetComponent<BoxInfo>().packageSize;
        }
    }



}
