using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleCollisionHandler : MonoBehaviour
{
    // bool attachedToWeakPoint = false;
    WeakPoint weakPoint;

    private void OnTriggerEnter(Collider other) {
        WeakPoint wp = other.GetComponent<WeakPoint>();
        if(wp) {
            // attachedToWeakPoint = true;
            weakPoint = wp;
        }
    }

    public bool isAttachedToWeakPoint() {
        return weakPoint != null;
        // return attachedToWeakPoint;
    }

    public void reset() {
        // attachedToWeakPoint = false;
        weakPoint = null;
    }

    public WeakPoint GetWeakPoint() {
        return weakPoint;
    }
}
