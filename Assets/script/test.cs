using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Username after delete: " + PlayerPrefs.GetString("Username", "NOT FOUND"));
        Debug.Log("Password after delete: " + PlayerPrefs.GetString("Password", "NOT FOUND"));
    }

    
}
