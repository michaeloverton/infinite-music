using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    Enemy enemy;
    bool weakPointDestroyed = false;
    public Material destroyedMaterial;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        if(enemy == null) {
            throw new System.Exception("weak point must have an enemy as parent");
        }
    }

    public void DoDamage() {
        Debug.Log("doing damage at weak point level");
        // Only do damage if weak point is not destroyed. Currently weak point can only be used once.
        if(!weakPointDestroyed) {
            enemy.Damage();
            weakPointDestroyed = true;
            
            // Set the weak point to use the destroyed material.
            GetComponent<MeshRenderer>().material = destroyedMaterial;
        }
    }
}
