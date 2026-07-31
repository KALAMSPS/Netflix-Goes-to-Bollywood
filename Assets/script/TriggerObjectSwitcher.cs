using UnityEngine;

public class TriggerObjectSwitcher : MonoBehaviour
{
    [Header("Enable this object")]
    public GameObject objectToEnable;

    [Header("Disable this object")]
    public GameObject objectToDisable; 

    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (objectToEnable != null)
            {
                objectToEnable.SetActive(true);
            }

            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }
        }
    }
}