using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    Vector3 velocity;
    bool isGrounded;
    // Disable movement if pauseMenu is active.
    public PauseMenu pauseMenu;
    public IntroScreen introScreen;

    // Walk sounds
    public float footstepDistance = 3.5f;
    public AudioSource footstep;
    Vector3 lastFootstepLocation;
    

    void Start() {
        lastFootstepLocation = controller.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(pauseMenu.getIsActive() || introScreen.getIsActive()) {
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0) {
            // This -2f is a quirk, but 0f should theoretically basically work.
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x) + (transform.forward * z);

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
