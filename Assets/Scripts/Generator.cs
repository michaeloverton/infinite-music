using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public List<GameObject> colliders;
    public List<GameObject> hitters;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<200; i++) {
            GameObject collider = colliders[Random.Range(0, colliders.Count-1)];
            Vector3 position = new Vector3(Random.Range(-20f, 20f), Random.Range(-80f, 80f), Random.Range(-20f, 20f));
            Quaternion rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            Instantiate(collider, position, rotation);
        }

        for(int i=0; i<60; i++) {
            GameObject hitter = hitters[Random.Range(0, hitters.Count-1)];
            Vector3 position = new Vector3(Random.Range(-20f,20f), Random.Range(-30f, 200f), Random.Range(-20f, 20f));
            Instantiate(hitter, position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
