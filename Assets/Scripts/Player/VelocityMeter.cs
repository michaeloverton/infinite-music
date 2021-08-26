using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VelocityMeter : MonoBehaviour
{
    public TextMeshProUGUI velocityText;
    public Rigidbody player;

    // Start is called before the first frame update
    void Start()
    {
        velocityText.SetText(Vector3.Magnitude(player.velocity).ToString());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        velocityText.SetText(Vector3.Magnitude(player.velocity).ToString());
    }
}
