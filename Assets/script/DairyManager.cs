using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro; // Add this namespace for TextMeshPro

public class DairyPanelManager : MonoBehaviour
{
    public Button iconButton;
    public GameObject panelToToggle;
    public GameObject[] buttonsToReveal;
    public float revealDelay = 0.5f;
    public AudioSource revealSound;

    public AudioClip clickClip; // ✅ Click sound clip
    public GameObject thisWeekObject;
    public GameObject lastWeekObject;



    private bool isPanelVisible = false;

    void Start()
    {
        panelToToggle.SetActive(false);

        foreach (var btn in buttonsToReveal)
        {
            btn.SetActive(false);
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0f;
        }


        iconButton.onClick.AddListener(() =>
        {
            PlayClickSound();
            TogglePanel();
        });

        if (buttonsToReveal.Length >= 2)
        {
            Button btn0 = buttonsToReveal[0].GetComponent<Button>();
            Button btn1 = buttonsToReveal[1].GetComponent<Button>();

            if (btn0 != null)
                btn0.onClick.AddListener(() =>
                {
                    PlayClickSound();
                    thisWeekObject.SetActive(true);
                    lastWeekObject.SetActive(false);
                });

            if (btn1 != null)
                btn1.onClick.AddListener(() =>
                {
                    PlayClickSound();
                    lastWeekObject.SetActive(true);
                    thisWeekObject.SetActive(false);
                });
        }
    }




    void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        panelToToggle.SetActive(isPanelVisible);

        if (isPanelVisible)
            StartCoroutine(ButtonAnimation());
        else
        {
            foreach (var btn in buttonsToReveal)
            {
                btn.SetActive(false);
                CanvasGroup cg = btn.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.alpha = 0f;
            }
        }
    }

    IEnumerator ButtonAnimation()
    {
        foreach (var btn in buttonsToReveal)
        {
            yield return new WaitForSeconds(revealDelay);

            btn.SetActive(true);
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f);
            }

            if (revealSound != null)
                revealSound.Play();
        }
    }

    void PlayClickSound()
    {
        if (clickClip != null)
            AudioSource.PlayClipAtPoint(clickClip, Camera.main.transform.position);
    }
}
