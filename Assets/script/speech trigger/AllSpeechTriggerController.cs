using Unity.VisualScripting;
using UnityEngine;

public class AllSpeechTriggerController : MonoBehaviour
{
    public string TagName;
    private string PlayerTagName = "Player";

    [Header("Drag the dialog canvas")]
    public GameObject ChatBox;
    public int objectiveIndex;

    [Header("Assign the player in scene")]
    public GameObject player;

    [Header("Assign the prefab for reference")]
    public GameObject playerPrefab;

    private void Start()
    {
        ChatBox.SetActive(false);

        // Copy position and rotation from prefab to the actual player
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerTagName) && this.CompareTag(TagName))
        {
            ChatBox.SetActive(true);

            // Notify the manager to enable the next trigger
            FindObjectOfType<SpeechTrigegrController>().ActivateNextTrigger();
        }
        if (player != null && playerPrefab != null)
        {
            Debug.Log("reached");
            player.transform.localPosition = playerPrefab.transform.position;
            player.transform.rotation = playerPrefab.transform.rotation;
        }
    }
}
