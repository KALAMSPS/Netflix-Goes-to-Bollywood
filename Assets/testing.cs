using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing : MonoBehaviour
{
    void Awake()
    {
        DestroyImmediate(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
        Debug.Log("after destroy");
        for(int i=0;i<=1000;i++)
        {
            Debug.Log(i);
        }
    }

   
}
