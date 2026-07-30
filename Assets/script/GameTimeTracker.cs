using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTimeTracker : MonoBehaviour
{
    public TMP_Text totalTimeText; // Assign in first scene via Inspector or tag

    [Header("Needle Rotation")]
    public Transform needle; // Assign Needle Transform
    public bool rotateClockwise = true;

    private static GameTimeTracker instance;
    private float totalTime = 0f;
    private bool isTracking = false;

    private const float rotationSpeed = 12f; // 360° in 60 seconds

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Week_1")
        {
            totalTime = 0f;
        }

        isTracking = true;
        UpdateTimeUI();
    }

    void Update()
    {
        if (isTracking)
        {
            totalTime += Time.deltaTime;
            UpdateTimeUI();

            // Rotate Needle
            if (needle != null)
            {
                float direction = rotateClockwise ? -1f : 1f;
                needle.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Week_1")
        {
            totalTime = 0f;
        }

        isTracking = true;

        // Reassign TMP_Text automatically
        if (totalTimeText == null)
        {
            GameObject textObj = GameObject.FindWithTag("TotalTimeText");
            if (textObj != null)
            {
                totalTimeText = textObj.GetComponent<TMP_Text>();
            }
        }

        UpdateTimeUI();
    }

    void UpdateTimeUI()
    {
        if (totalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(totalTime / 60f);
            int seconds = Mathf.FloorToInt(totalTime % 60f);
            totalTimeText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public float GetRawTotalTime()
    {
        return totalTime;
    }

    public string GetFormattedTotalTime()
    {
        int minutes = Mathf.FloorToInt(totalTime / 60f);
        int seconds = Mathf.FloorToInt(totalTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}