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
    public void AddSGI(int amount)
    {
        globalData.SGI += amount;
        UIManager.instance.UpdateSGIUI(globalData.SGI);
    }

    public void AddFHI(int amount)
    {
        globalData.FHI += amount;
        UIManager.instance.UpdateFHIUI(globalData.FHI);
    }

    public void AddCGI(int amount)
    {
        globalData.CGI += amount;
        UIManager.instance.UpdateCGIUI(globalData.CGI);
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
    // public void ResetData()
    // {
    //     globalData.SGI = 0;
    //     globalData.FHI = 0;
    //     globalData.CGI = 0;
    //     globalData.currentDateTime = "";
    //     globalData.totalWeeks = 1;
    //     SaveData();
    //     UIManager.instance.UpdateSGIUI(0);
    //     UIManager.instance.UpdateFHIUI(0);
    //     UIManager.instance.UpdateCGIUI(0);
    // }
    public void ResetData()
    {
        // Game progress reset
        globalData.SGI = 0;
        globalData.FHI = 0;
        globalData.CGI = 0;

        globalData.currentDateTime = "";

        globalData.totalMissions = 0;
        globalData.totalWeeks = 1;
        globalData.totalCollectedCoins = 0;

        // IMPORTANT:
        // playerName and password NOT reset

        SaveData();

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateSGIUI(globalData.SGI);
            UIManager.instance.UpdateFHIUI(globalData.FHI);
            UIManager.instance.UpdateCGIUI(globalData.CGI);
        }

        Debug.Log("Game progress reset successfully.");
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
    public int SGI;
    public int FHI;
    public int CGI;
    public string playerName;
    public string password;
    public string currentDateTime;
    public int totalMissions;
    public int totalWeeks;
    public int totalCollectedCoins;
}

