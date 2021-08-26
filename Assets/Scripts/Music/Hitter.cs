using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitter : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < -100) {
            transform.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            transform.position = new Vector3(Random.Range(-20f,20f), Random.Range(-30f, 200f), Random.Range(-20f, 20f));
        }
    }
}
