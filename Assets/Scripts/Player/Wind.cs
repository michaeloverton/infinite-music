using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public AudioSource wind;
    public Rigidbody player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Magnitude(player.velocity) > 50) {
            if(!wind.isPlaying) {
                wind.Play();
            }
            
        } else {
            wind.Stop();
        }
    }
}
