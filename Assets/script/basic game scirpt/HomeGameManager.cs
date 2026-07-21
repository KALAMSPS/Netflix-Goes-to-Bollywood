using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HomeGameController : MonoBehaviour
{
    public static HomeGameController Instance;

    [Header("Video Panel")]
    public CanvasGroup introPanel;
    public VideoPlayer introVideo;

    [Header("Login Panel")]
    public CanvasGroup loginPanel;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public Button loginButton;

    [Header("Login Credentials")]
    public string userName = "admin";
    public string password = "1234";

    [Header("Home Panel")]
    public CanvasGroup homePanel;

    [Header("Home Buttons")]
    public Button startButton;
    public Button aboutButton;
    public Button settingsButton;
    public Button exitButton;
    public Button clearDataButton;

    [Header("Exit Popup")]
    public CanvasGroup exitPopup;

    public Button exitYesButton;
    public Button exitNoButton;

    [Header("Clear Data Popup")]
    public CanvasGroup clearPopup;

    public Button clearYesButton;
    public Button clearNoButton;

    [Header("Screens")]
    public List<UIScreen> screens = new List<UIScreen>();

    [Header("Animation")]
    public float fadeDuration = 0.35f;
    public float popupScaleDuration = 0.25f;

    [Header("Scene")]
    public string firstGameScene = "Week_1";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    [Header("About Panel")]
public CanvasGroup aboutPanel;
public Button aboutCloseButton;
// public Button CloseAbout;

    [System.Serializable]
    public class UIScreen
    {
        public string screenName;
        public CanvasGroup screen;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializePanels();
        RegisterButtons();

        if (introVideo != null)
            introVideo.loopPointReached += OnVideoFinished;
    }
    void InitializePanels()
    {
        ShowOnly(introPanel);

        HideInstant(loginPanel);
        HideInstant(homePanel);

        HideInstant(exitPopup);
        HideInstant(clearPopup);
        HideInstant(aboutPanel);

        foreach (UIScreen screen in screens)
        {
            if (screen.screen != null)
                HideInstant(screen.screen);
        }
    }
    void RegisterButtons()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(Login);

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (aboutButton != null)
            aboutButton.onClick.AddListener(OpenAbout);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(OpenExitPopup);

        if (clearDataButton != null)
            clearDataButton.onClick.AddListener(OpenClearPopup);

        if (exitYesButton != null)
            exitYesButton.onClick.AddListener(ExitGame);

        if (exitNoButton != null)
            exitNoButton.onClick.AddListener(CloseExitPopup);

        if (clearYesButton != null)
            clearYesButton.onClick.AddListener(ClearData);

        if (clearNoButton != null)
            clearNoButton.onClick.AddListener(CloseClearPopup);
        if (aboutButton != null)
            aboutButton.onClick.AddListener(OpenAbout);
        if (aboutCloseButton != null)
            aboutCloseButton.onClick.AddListener(CloseAbout);
            
    }

    //-------------------------------------------------------------

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(SwitchPanel(introPanel, loginPanel));
    }

    void ShowOnly(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        panel.alpha = 1;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    void HideInstant(CanvasGroup panel)
    {
        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }

    void PlayClick()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    void Login()
    {
        PlayClick();

        string user = usernameInput.text.Trim();
        string pass = passwordInput.text.Trim();

        if (user == userName && pass == password)
        {
            Debug.Log("Login Success");

            StartCoroutine(SwitchPanel(loginPanel, homePanel));
        }
        else
        {
            Debug.Log("Invalid Username or Password");
        }
    }

    IEnumerator SwitchPanel(CanvasGroup current, CanvasGroup next)
    {
        yield return StartCoroutine(FadeOut(current));
        yield return StartCoroutine(FadeIn(next));
    }

    IEnumerator FadeIn(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);

        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            panel.alpha = Mathf.Lerp(0, 1, time / fadeDuration);

            yield return null;
        }

        panel.alpha = 1;

        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    IEnumerator FadeOut(CanvasGroup panel)
    {
        float time = 0;

        panel.interactable = false;
        panel.blocksRaycasts = false;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            panel.alpha = Mathf.Lerp(1, 0, time / fadeDuration);

            yield return null;
        }

        panel.alpha = 0;

        panel.gameObject.SetActive(false);
    }

    public void OpenScreen(string screenName)
    {
        PlayClick();

        foreach (UIScreen screen in screens)
        {
            if (screen.screen == null)
                continue;

            if (screen.screenName == screenName)
            {
                StartCoroutine(FadeIn(screen.screen));
            }
            else
            {
                StartCoroutine(FadeOut(screen.screen));
            }
        }
    }

    void StartGame()
    {
        PlayClick();

        SceneManager.LoadScene(firstGameScene);
    }

   void OpenAbout()
{
    PlayClick();
    StartCoroutine(FadeIn(aboutPanel));
}
void CloseAbout()
{
    PlayClick();
    StartCoroutine(FadeOut(aboutPanel));
}

    void OpenSettings()
    {
        PlayClick();

        OpenScreen("Settings");
    }


    public void BackToHome()
    {
        PlayClick();

        foreach (UIScreen screen in screens)
        {
            if (screen.screen != null)
                StartCoroutine(FadeOut(screen.screen));
        }
    }

    #region Exit Popup

    void OpenExitPopup()
    {
        PlayClick();

        StartCoroutine(FadeCanvas(homePanel, 0.4f)); // ya StatsScreen ka CanvasGroup

        StartCoroutine(ShowPopup(exitPopup));
    }

    void CloseExitPopup()
    {
        PlayClick();

        StartCoroutine(FadeCanvas(homePanel, 1f));

        StartCoroutine(HidePopup(exitPopup));
    }

    void ExitGame()
    {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    //=========================================================

    #region Clear Data Popup

    void OpenClearPopup()
    {
        PlayClick();

        StartCoroutine(FadeCanvas(homePanel, 0.4f));

        StartCoroutine(ShowPopup(clearPopup));
    }

    void CloseClearPopup()
    {
        PlayClick();

        StartCoroutine(FadeCanvas(homePanel, 1f));

        StartCoroutine(HidePopup(clearPopup));
    }

    void ClearData()
    {
        PlayClick();

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("All PlayerPrefs Cleared Successfully.");

        StartCoroutine(HidePopup(clearPopup));
    }

    #endregion

    IEnumerator ShowPopup(CanvasGroup popup)
    {
        popup.gameObject.SetActive(true);

        popup.alpha = 0;
        popup.interactable = false;
        popup.blocksRaycasts = false;

        RectTransform rect = popup.GetComponent<RectTransform>();

        if (rect != null)
            rect.localScale = Vector3.zero;

        float time = 0;

        while (time < popupScaleDuration)
        {
            time += Time.deltaTime;

            float t = time / popupScaleDuration;

            popup.alpha = Mathf.Lerp(0, 1, t);

            if (rect != null)
                rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            yield return null;
        }

        popup.alpha = 1;

        if (rect != null)
            rect.localScale = Vector3.one;

        popup.interactable = true;
        popup.blocksRaycasts = true;
    }

    IEnumerator HidePopup(CanvasGroup popup)
    {
        popup.interactable = false;
        popup.blocksRaycasts = false;

        RectTransform rect = popup.GetComponent<RectTransform>();

        float time = 0;

        while (time < popupScaleDuration)
        {
            time += Time.deltaTime;

            float t = time / popupScaleDuration;

            popup.alpha = Mathf.Lerp(1, 0, t);

            if (rect != null)
                rect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

            yield return null;
        }

        popup.alpha = 0;

        if (rect != null)
            rect.localScale = Vector3.zero;

        popup.gameObject.SetActive(false);
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float targetAlpha)
    {
        float start = cg.alpha;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            cg.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);

            yield return null;
        }

        cg.alpha = targetAlpha;
    }

    public void LoadLastScene()
    {
        PlayClick();

        if (PlayerPrefs.HasKey("LastScene"))
        {
            string scene = PlayerPrefs.GetString("LastScene");

            if (!string.IsNullOrEmpty(scene))
            {
                SceneManager.LoadScene(scene);
            }
            else
            {
                Debug.Log("LastScene is Empty.");
            }
        }
        else
        {
            Debug.Log("LastScene Key Not Found.");
        }
    }
    

    private void OnDestroy()
    {
        if (introVideo != null)
            introVideo.loopPointReached -= OnVideoFinished;
    }
    
}