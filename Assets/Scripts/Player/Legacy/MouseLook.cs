using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float mouseSense = 100f;
    public Transform playerBody;
    // Freeze look if pauseMenu is active.
    public PauseMenu pauseMenu;
    public IntroScreen introScreen;

    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if(introScreen.getIsActive()) {
            Cursor.lockState = CursorLockMode.None;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(pauseMenu.getIsActive() || introScreen.getIsActive()) {
            return;
        }
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSense * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSense * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
