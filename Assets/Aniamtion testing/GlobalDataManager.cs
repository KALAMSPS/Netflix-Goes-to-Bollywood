using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager instance;
    public GlobalGameData globalData = new GlobalGameData();

    // Custom path
    //private string customSavePath = @"E:\LUCase2V1\LUCase2V1\Assets\Resources\DabaseReportFile.json";
    private string customSavePath = Path.Combine(Application.dataPath, "Resources", "GameData.json");



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();  // Load data on start
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Increment methods
    public void AddRevenue(int amount)
    {
        globalData.revenue += amount;
        // UIManager.instance.UpdateRevenueUI(globalData.revenue);
    }

    public void AddDemand(int amount)
    {
        globalData.demand += amount;
        // UIManager.instance.UpdateDemandUI(globalData.demand);
    }

    public void AddRetailer(int amount)
    {
        globalData.retailer += amount;
        // UIManager.instance.UpdateRetailerUI(globalData.retailer);
    }

    // New Set Methods

    public void SetPlayerName(string name)
    {
        globalData.playerName = name;
        Debug.Log("Player Name set to: " + name);
    }

    public void SetPassword(string pass)
    {
        globalData.password = pass;
        Debug.Log("Password set.");
    }

    public void SetTotalWeeks(int weeks)
    {
        globalData.totalWeeks += weeks;
        Debug.Log("Total Weeks set to: " + weeks);
        SaveData();
    }

    // Save Data with custom path
    public void SaveData()
    {
        try
        {
            string directory = Path.GetDirectoryName(customSavePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string data = JsonUtility.ToJson(globalData, true);
            File.WriteAllText(customSavePath, data);

        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save data: " + e.Message);
        }
    }

    // Load Data from custom path
    public void LoadData()
    {
        try
        {
            if (File.Exists(customSavePath))
            {
                string data = File.ReadAllText(customSavePath);
                globalData = JsonUtility.FromJson<GlobalGameData>(data);
                Debug.Log("Data loaded successfully from: " + customSavePath);
            }
            else
            {
                Debug.LogWarning("No save file found. Using default values.");
                globalData = new GlobalGameData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load data: " + e.Message);
        }
    }

    // RESET JSON SAVED FILE
    public void ResetData()
    {
        globalData.revenue = 0;
        globalData.demand = 0;
        globalData.retailer = 0;
        globalData.currentDateTime = "";
        globalData.totalWeeks = 1;
        SaveData();
      /*   UIManager.instance.UpdateRevenueUI(0);
        UIManager.instance.UpdateDemandUI(0);
        UIManager.instance.UpdateRetailerUI(0); */
    }


    // Auto-save on quit
    private void OnApplicationQuit()
    {
        SaveData();
    }
}

// Data structure
[System.Serializable]
public class GlobalGameData
{
    public int revenue;
    public int demand;
    public int retailer;
    public string playerName;
    public string password;
    public string currentDateTime;
    public int totalMissions;
    public int totalWeeks;
    public int totalCollectedCoins;
}

