using UnityEngine;

public class OrderConfirm : MonoBehaviour
{
    public CalcForWMS calc;
    [SerializeField]
    private GameObject correct;
    [SerializeField]
    private GameObject incorrect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OrderConfirmation()
    {
        if (calc.currentSize == calc.necessarySize)
        {
            correct.SetActive(true);
            incorrect.SetActive(false);
        } else
        {
            correct.SetActive(false);
            incorrect.SetActive(true);
        }
    }
}
