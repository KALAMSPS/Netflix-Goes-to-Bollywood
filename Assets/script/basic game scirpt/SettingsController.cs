using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SettingsController : MonoBehaviour
{
    [Header("UI Panel")]
    [Tooltip("Panel to show/hide on Escape key")]
    public GameObject settingsPanel;

    private bool isPanelActive = false;

    [Header("Time Scale Settings")]
    public Slider timeScaleSlider;
    public int minTimeScale = 1;
    public int maxTimeScale = 7;

    [Header("Mouse Sensitivity Settings")]
    public Slider mouseSensitivitySlider;
    public float minMouseSensitivity = 0.1f;
    public float maxMouseSensitivity = 10f;
    public static float MouseSensitivity { get; private set; }
    private static CameraController cm;

    [Header("Sound Settings")]
    public Slider SoundSlider;
    private List<AudioSource> allAudioSources = new List<AudioSource>();
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();

    void Start()
    {
        QualitySettings.vSyncCount = 0;

        // Time Scale slider setup
        if (timeScaleSlider != null)
        {
            timeScaleSlider.minValue = minTimeScale;
            timeScaleSlider.maxValue = maxTimeScale;
            timeScaleSlider.wholeNumbers = true;
            timeScaleSlider.value = Mathf.Clamp(Time.timeScale, minTimeScale, maxTimeScale);
            timeScaleSlider.onValueChanged.AddListener(SetTimeScale);
        }

        // Mouse sensitivity slider setup
        SetupMouseSensitivitySlider();

        // Sound slider setup
        allAudioSources = new List<AudioSource>(FindObjectsOfType<AudioSource>());
        originalVolumes.Clear();
        foreach (var source in allAudioSources)
        {
            if (source != null)
                originalVolumes[source] = source.volume;
        }

        if (SoundSlider != null)
        {
            SoundSlider.minValue = 0f;
            SoundSlider.maxValue = 1f;
            SoundSlider.value = 1f; // Full volume by default
            SoundSlider.onValueChanged.AddListener(SetSoundVolume);
        }

        // Ensure panel is off at start
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPanelActive = !isPanelActive;
            if (settingsPanel != null)
                settingsPanel.SetActive(isPanelActive);
        }
    }

    private void SetTimeScale(float value)
    {
        Time.timeScale = Mathf.Clamp(value, minTimeScale, maxTimeScale);
        Debug.Log("Time scale set to: " + Time.timeScale);
    }

    private void SetupMouseSensitivitySlider()
    {
        if (cm == null)
            cm = FindObjectOfType<CameraController>();

        if (cm == null)
        {
            Debug.LogWarning("CameraController not found!");
            return;
        }

        mouseSensitivitySlider.minValue = 0f;
        mouseSensitivitySlider.maxValue = 20f;
        float normalized = Mathf.InverseLerp(minMouseSensitivity, maxMouseSensitivity, cm.sensitivity);
        mouseSensitivitySlider.value = normalized * 10f;
        mouseSensitivitySlider.onValueChanged.AddListener(value =>
        {
            float newSensitivity = Mathf.Lerp(minMouseSensitivity, maxMouseSensitivity, value / 10f);
            MouseSensitivity = newSensitivity;
            cm.sensitivity = newSensitivity;
        });
    }

    private void SetSoundVolume(float volume)
    {
        AudioListener.volume = volume;

        foreach (var source in allAudioSources)
        {
            if (source != null && originalVolumes.ContainsKey(source))
            {
                source.volume = originalVolumes[source] * volume;
            }
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
    }

    public void LoadSceneByName()
    {
        SceneManager.LoadScene("Home");
    }
    private void OnApplicationQuit()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", sceneName);
        PlayerPrefs.Save(); 
    }
}
