using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicCollision : MonoBehaviour
{
    AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Vary the volume with collision intensity.
        // if (collision.relativeVelocity.magnitude > 2)
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
