using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechTrigegrController : MonoBehaviour
{ 
    public GameObject[] triggers; // Array of speech trigger GameObjects
    private int currentIndex = 0; // Index of the active trigger

    private void Start()
    {
        // Enable only the first trigger, disable the rest
        for (int i = 0; i < triggers.Length; i++)
        {
            triggers[i].SetActive(i == 0);
        }
    }

    public void ActivateNextTrigger()
    {
        // Disable current trigger
        if (currentIndex < triggers.Length)
        {
            triggers[currentIndex].SetActive(false);
        }
        currentIndex++;
        if (currentIndex < triggers.Length)
        {
            triggers[currentIndex].SetActive(true);
        }
    }
}
