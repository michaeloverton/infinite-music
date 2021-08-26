using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRMovement : MonoBehaviour
{
    // public CharacterController character;
    private Rigidbody body;
    private CapsuleCollider collider;
    public XRRig rig;
    public float accelerationSmoothing = 1f;
    public XRNode leftInputSource;
    public GameObject leftHandPosition;
    public XRNode rightInputSource;
    public GameObject rightHandPosition;
    private Vector2 inputAxis;
    public float additionalCharacterHeight = 0.2f;

    private Vector3 flyingVelocityVector = new Vector3(0,0,0);
    private Vector3 fallingVelocityVector = new Vector3(0,0,0);

    // Flying.
    public float maxFlyingSpeed = 50f;
    public float flyingAcceleration = 2f;
    private float flyingVelocity = 0f;
    private bool fly;
    private bool isFlying = false;
    private bool flyingToggleCooldown = false;

    // Walking.
    public float maxWalkingSpeed = 5;
    public float maxWalkingVelocityChange = 1f;
    public float gravity = -19.81f;
    private float fallingVelocity;

    // Flashlight.
    public Light flashlight;
    private bool flashlightToggleCooldown = false;

    // Grapple.
    private bool rightGrappleExists = false;
    private bool leftGrappleExists = false;
    private Grappler rightGrappler;
    private Grappler leftGrappler;

    void Start() {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.freezeRotation = true;

        collider = GetComponent<CapsuleCollider>();

        rightGrappler = GameObject.Find("RightGrappler").GetComponentInChildren<Grappler>();
        leftGrappler = GameObject.Find("LeftGrappler").GetComponentInChildren<Grappler>();

        if(!rightGrappler || !leftGrappler) {
            throw new System.Exception("failed to find a grappler");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Left hand for movement
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);

        // Right hand for button inputs.
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        
        bool toggleFlying = false;
        rightDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out toggleFlying);
        if(toggleFlying && !flyingToggleCooldown) {
            isFlying = !isFlying;
            Invoke("ResetFlyingCooldown", 1.0f);
            flyingToggleCooldown = true;
        }

        bool toggleFlashlight = false;
        rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out toggleFlashlight);
        if(toggleFlashlight && !flashlightToggleCooldown) {
            flashlight.gameObject.SetActive(!flashlight.gameObject.activeSelf);
            Invoke("ResetFlashlightCooldown", 0.5f);
            flashlightToggleCooldown = true;
        }

        bool rightTriggerDown = false;
        rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerDown);
        if(rightTriggerDown && !rightGrappleExists) {
            rightGrappler.CreateGrapplePoint(body);
            rightGrappleExists = true;

        } else if(!rightTriggerDown && rightGrappleExists) {
            // If trigger is not pressed and grapple exists, destroy it.
            rightGrappler.DestroyGrapplePoint();
            rightGrappleExists = false;
        }

        // If we press grip trigger and attached to weak point, do damage. Otherwise retract.
        bool rightGripTriggerDown = false;
        rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out rightGripTriggerDown);
        if(rightGripTriggerDown && rightGrappleExists) {
            if(rightGrappler.AttachedToWeakPoint()) {
                rightGrappler.DoDamage();
            } else {
                rightGrappler.RetractGrapple();
            }

        } else if(!rightGripTriggerDown && !rightGrappleExists) {
            rightGrappler.ResetGrappleDistance();
        }

        rightGrappler.UpdateGrappleLine(rightHandPosition.transform.position);

        bool leftTriggerDown = false;
        leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerDown);
        if(leftTriggerDown && !leftGrappleExists) {
            leftGrappler.CreateGrapplePoint(body);
            leftGrappleExists = true;

        } else if(!leftTriggerDown && leftGrappleExists) {
            // If trigger is not pressed and grapple exists, destroy it.
            leftGrappler.DestroyGrapplePoint();
            leftGrappleExists = false;
        }

        // If we press grip trigger and attached to weak point, do damage. Otherwise retract.
        bool leftGripTriggerDown = false;
        leftDevice.TryGetFeatureValue(CommonUsages.gripButton, out leftGripTriggerDown);
        if(leftGripTriggerDown && leftGrappleExists) {
            if(leftGrappler.AttachedToWeakPoint()) {
                leftGrappler.DoDamage();
            } else {
                leftGrappler.RetractGrapple();
            }

        } else if(!leftGripTriggerDown && !leftGrappleExists) {
            leftGrappler.ResetGrappleDistance();
        }

        leftGrappler.UpdateGrappleLine(leftHandPosition.transform.position);
    }

    void ResetFlyingCooldown() {
        flyingToggleCooldown = false;
    }

    void ResetFlashlightCooldown() {
        flashlightToggleCooldown = false;
    }

    /*
    private void FixedUpdate() {
        CapsuleFollowHeadset();

        // Calculate flying velocity vector.
        if(flyingVelocity < 0.1f) {
            flyingVelocity = 0;
        }

        if(inputAxis.y > -0.05f && inputAxis.y < 0.05f && flyingVelocity != 0) {
            flyingVelocity -= flyingAcceleration * Time.fixedDeltaTime;
        } else if((inputAxis.y < -0.05f || inputAxis.y > 0.05f) && isFlying) {
            // TODO: NEED TO HANDLE BACKWARDS ACCELERATION.
            flyingVelocity += flyingAcceleration * inputAxis.y * Time.fixedDeltaTime;
            if(flyingVelocity > maxFlyingSpeed) {
                flyingVelocity = maxFlyingSpeed;
            }
        }

        Vector3 headAngles = rig.cameraGameObject.transform.eulerAngles;
        Quaternion headRotation = Quaternion.Euler(headAngles.x, headAngles.y, headAngles.z);
        Vector3 flyDirection = headRotation * Vector3.forward;

        flyingVelocityVector = flyDirection * flyingVelocity;

        // Calculate falling velocity vector.
        if(isFlying && fallingVelocity >= 0) {
            fallingVelocity = 0;
        }

        bool grounded = isGrounded();
        if (grounded && !isFlying) {
            fallingVelocity = 0;
        } else if(!isFlying) {
            fallingVelocity += gravity * Time.fixedDeltaTime;
        } else if(isFlying && fallingVelocity != 0) {
            fallingVelocity += flyingAcceleration * Time.fixedDeltaTime;
        }

        fallingVelocityVector = Vector3.up * fallingVelocity;

        if(isFlying) {  
            // Flying movement.
            character.Move(flyingVelocityVector * Time.fixedDeltaTime);

            // Account for any previous falling velocity.
            character.Move(fallingVelocityVector * Time.fixedDeltaTime);
            
        } else {
            // Walking movement.
            Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0);
            Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
            character.Move(direction * Time.fixedDeltaTime * maxWalkingSpeed);
            
            // Account for falling.
            character.Move(fallingVelocityVector * Time.fixedDeltaTime);

            // Account for any previous flying velocity.
            character.Move(flyingVelocityVector * Time.fixedDeltaTime);
        }
    }

    

    void CapsuleFollowHeadset() {
        character.height = rig.cameraInRigSpaceHeight + additionalCharacterHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.cameraGameObject.transform.position);
        character.center = new Vector3(capsuleCenter.x, character.height/2 + character.skinWidth, capsuleCenter.z);
    }
    */

    void FixedUpdate () {
        CapsuleFollowHeadset();

        if(isFlying) {
            // TODO: MAYBE USE THE OLD SYSTEM HERE.
            Vector3 headAngles = rig.cameraGameObject.transform.eulerAngles;
            Quaternion headRotation = Quaternion.Euler(headAngles.x, headAngles.y, headAngles.z);
            Vector3 flyDirection = headRotation * Vector3.forward;

            // if(Vector3.Magnitude(body.velocity) <= maxFlyingSpeed) {
                body.AddForce(flyDirection * body.mass * 10 * inputAxis.y);
            // }
            
        } else {
            // TODO: MAYBE JUST USE THE OLD SYSTEM.

	        // Calculate how fast we should be moving
            Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0);
            Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
            // Vector3 targetVelocity = direction * maxWalkingSpeed;           
 
	        // Apply a force that attempts to reach our target velocity
	        // Vector3 velocity = body.velocity;
	        // Vector3 velocityChange = (targetVelocity - velocity);
	        // velocityChange.x = Mathf.Clamp(velocityChange.x, -maxWalkingVelocityChange, maxWalkingVelocityChange);
	        // velocityChange.z = Mathf.Clamp(velocityChange.z, -maxWalkingVelocityChange, maxWalkingVelocityChange);
	        // velocityChange.y = 0;
	        // body.AddForce(velocityChange, ForceMode.VelocityChange);

            // Debug.Log("total velocity: " + Vector3.Magnitude(body.velocity));
            // Debug.Log("horizontal velocity: " + Mathf.Sqrt(Mathf.Pow(body.velocity.x, 2) + Mathf.Pow(body.velocity.z, 2)));

            if(Mathf.Sqrt(Mathf.Pow(body.velocity.x, 2) + Mathf.Pow(body.velocity.z, 2)) <= maxWalkingSpeed) {
                body.AddForce(direction * body.mass * 70);
            }

            // We apply gravity manually for more tuning control
	        body.AddForce(new Vector3 (0, gravity * body.mass, 0));
        }
 
	}

    void CapsuleFollowHeadset() {
        collider.height = rig.cameraInRigSpaceHeight + additionalCharacterHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.cameraGameObject.transform.position);
        collider.center = new Vector3(capsuleCenter.x, collider.height/2, capsuleCenter.z);
    }

    bool isGrounded() {
        Vector3 rayStart = transform.TransformPoint(collider.center);
        float rayLength = collider.center.y + 0.01f;
        bool rayHasHit = Physics.SphereCast(rayStart, collider.radius - 0.2f, Vector3.down, out RaycastHit hitInfo, rayLength);
        return rayHasHit;
    }

}
