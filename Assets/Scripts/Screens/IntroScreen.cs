using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroScreen : MonoBehaviour
{
    bool isActive = true;
    public GameObject canvas;

    public bool getIsActive() {
        return isActive;
    }

    public void disable() {
        Cursor.lockState = CursorLockMode.Locked;
        isActive = false;
        canvas.SetActive(false);
    }
}
