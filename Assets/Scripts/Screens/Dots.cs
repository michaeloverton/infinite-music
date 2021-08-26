using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dots : MonoBehaviour
{
    public TextMeshProUGUI text;
    // private bool isLoading = false;
    // private float waitTime = 0.5f;
    private float timer = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 0.25) {
            text.SetText(text.text + ".");
            timer = 0;
        }
    }
}
