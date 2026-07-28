using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text SGIText; // Changed to TMP_Text
    public TMP_Text FHIText;  // Changed to TMP_Text
    public TMP_Text CBIText; // Changed to TMP_Text
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
            UpdateSGIUI(GlobalDataManager.instance.globalData.SGI);
            UpdateFHIUI(GlobalDataManager.instance.globalData.FHI);
            UpdateCGIUI(GlobalDataManager.instance.globalData.CGI);
        }
        else
        {
            Debug.LogError("GlobalDataManager instance is missing!");
        }
    }


    // UI update methods
    public void UpdateSGIUI(int SGI)
    {
        SGIText.text = $"{SGI}";
    }

    public void UpdateFHIUI(int FHI)
    {
        FHIText.text = $"{FHI}";
    }

    public void UpdateCGIUI(int CGI)
    {
        CBIText.text = $"{CGI}";
    }
}
