using UnityEngine;
using TMPro;

public class Text_Animation : MonoBehaviour
{
    public float minScale = 0.5f;
    public float maxScale = 1f;
    public float speed = 0.1f;
    private bool isScaling = false;  // Flag to control animation
    public  TMP_Text textInfomation;
    public static Text_Animation instance;
    private void Start()
    {
        StartScaling();
        instance = this;
    }
    void Update()
    {
        if (isScaling)
        {
            float scale = Mathf.PingPong(Time.time * speed, maxScale - minScale) + minScale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    // Call this method to start scaling animation
    public void StartScaling()
    {
        isScaling = true;
    }

    // Call this method to stop scaling animation
    public void StopScaling()
    {
        isScaling = false;
    }
}
