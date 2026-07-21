using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text revenueText; // Changed to TMP_Text
    public TMP_Text demandText;  // Changed to TMP_Text
    public TMP_Text retailerText; // Changed to TMP_Text
    public GameObject InvisibleParams;
    
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure GlobalDataManager instance is ready before UI initializes
        if (GlobalDataManager.instance != null)
        {
            UpdateRevenueUI(GlobalDataManager.instance.globalData.revenue);
            UpdateDemandUI(GlobalDataManager.instance.globalData.demand);
            UpdateRetailerUI(GlobalDataManager.instance.globalData.retailer);
        }
        else
        {
            Debug.LogError("GlobalDataManager instance is missing!");
        }
    }


    // UI update methods
    public void UpdateRevenueUI(int revenue)
    {
        revenueText.text = $"Retention: {revenue}";
    }

    public void UpdateDemandUI(int demand)
    {
        demandText.text = $"Engagement: {demand}";
    }

    public void UpdateRetailerUI(int retailer)
    {
        retailerText.text = $"Productivity: {retailer}";
    }
}
