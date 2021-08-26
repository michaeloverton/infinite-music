using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void Damage() {
        Debug.Log("doing damage at enemy level");
        if(currentHealth > 1) {
            currentHealth--;
        } else {
            gameObject.SetActive(false);
        }
       
    }
}
