using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Grappler : MonoBehaviour
{
    private GameObject grapplePoint;
    private GameObject grappledObject;
    private Vector3 localGrapplePointPosition; // The position of the grapple point relative to the object that we are grappling.
    private GrappleCollisionHandler grappleCollisionHandler;
    private XRRayInteractor rayInteractor;
    private LineRenderer grappleLine;
    private XRInteractorLineVisual controllerLine;
    public float maxDistance = 50;
    public float retractionSpeed = 2;
    private float originalMaxDistance;
    public float damper = 4;
    public float springStrength = 2;

    // Reticle.
    public GameObject grappleReticle;
    public Vector3 originalReticleScale;

    // Start is called before the first frame update
    void Start()
    {
        rayInteractor = GetComponentInParent<XRRayInteractor>();

        controllerLine = GetComponentInParent<XRInteractorLineVisual>();
        
        grappleLine = GetComponent<LineRenderer>();
        grappleLine.startWidth = 0.04f;
        grappleLine.endWidth = 0.5f;
        grappleLine.positionCount = 2;

        // Create the grapple point. We will disable and enable it as needed.
        grapplePoint = new GameObject("GrapplePoint");

        Rigidbody grappleBody = grapplePoint.AddComponent<Rigidbody>();
        grappleBody.mass = float.PositiveInfinity;
        grappleBody.useGravity = false;

        SpringJoint spring = grapplePoint.AddComponent<SpringJoint>();
        spring.autoConfigureConnectedAnchor = false;
        spring.maxDistance = maxDistance;
        spring.damper = damper;
        spring.spring = springStrength;

        SphereCollider grappleCollider = grapplePoint.AddComponent<SphereCollider>();
        grappleCollider.radius = 1f;
        grappleCollider.isTrigger = true;

        // Use this for handling handling what the grappling point is attached to.
        grappleCollisionHandler = grapplePoint.AddComponent<GrappleCollisionHandler>();

        // Disable grapple point until we need it.
        grapplePoint.SetActive(false);

        // Remember the original grapple distance.
        originalMaxDistance = maxDistance;

        // Deactivate grapple reticle until we need it.
        originalReticleScale = grappleReticle.transform.localScale;
        grappleReticle.SetActive(false);
    }

    public void Update() {
        // Handle grapple reticle.
        RaycastHit hit;
        bool canGrapple = rayInteractor.GetCurrentRaycastHit(out hit);
        if(canGrapple) {
            // Move grapple point to the position of the hit.
            grappleReticle.transform.position = hit.point;
            // Adjust scale based on distance.
            float reticleScale = hit.distance / rayInteractor.maxRaycastDistance;
            grappleReticle.transform.localScale = new Vector3(originalReticleScale.x * reticleScale, originalReticleScale.y * reticleScale, originalReticleScale.z * reticleScale);

            if(!grappleReticle.activeSelf && !grapplePoint.activeSelf) {
                // If grapple point is not active, activate reticle.
                grappleReticle.SetActive(true);
            } else if(grappleReticle.activeSelf && grapplePoint.activeSelf) {
                // If grapple point is active, deactivate reticle.
                grappleReticle.SetActive(false);
            }
        } else {  
            if(grappleReticle.activeSelf) {
                // Reset scale.
                grappleReticle.transform.localScale = originalReticleScale;
                //Deactivate.
                grappleReticle.SetActive(false);
            }
        }
    }

    public void FixedUpdate() {
        if(grapplePoint.activeSelf) {
            // Move the grapple point with the grappled object.
            Vector3 grappledObjectScale = grappledObject.transform.localScale;
            Vector3 unrotatedGrapplePositionWithScale = new Vector3(
                localGrapplePointPosition.x * grappledObjectScale.x, 
                localGrapplePointPosition.y * grappledObjectScale.y, 
                localGrapplePointPosition.z * grappledObjectScale.z
            );
            Vector3 grapplePointLocalPositionWithScale = grappledObject.transform.rotation * unrotatedGrapplePositionWithScale;

            grapplePoint.transform.position = grappledObject.transform.position + grapplePointLocalPositionWithScale;
        }
    }

    public void CreateGrapplePoint(Rigidbody connected) {
        RaycastHit hit;
        bool hitSomething = rayInteractor.GetCurrentRaycastHit(out hit);
        if(hitSomething) {
            // Move grapple point to the position of the hit.
            grapplePoint.transform.position = hit.point;
            
            // Store information about the object that we hit.
            grappledObject = hit.transform.gameObject;
            localGrapplePointPosition = hit.transform.InverseTransformPoint(grapplePoint.transform.position);

            // Connect the spring joint and activat grapple point.
            SpringJoint springJoint = grapplePoint.GetComponent<SpringJoint>();
            springJoint.connectedBody = connected;
            springJoint.connectedAnchor = connected.gameObject.GetComponent<CapsuleCollider>().center;
            grapplePoint.SetActive(true);

            controllerLine.enabled = false;
        }
    }

    public void DestroyGrapplePoint() {
        // Disconnect the spring joint.
        SpringJoint springJoint = grapplePoint.GetComponent<SpringJoint>();
        springJoint.connectedBody = null;
        springJoint.connectedAnchor = new Vector3(0,0,0);

        // Disable grapple point.
        grapplePoint.SetActive(false);
        controllerLine.enabled = true;

        // Reset the grapple collision handler.
        grappleCollisionHandler.reset();
    }

    public void UpdateGrappleLine(Vector3 handPosition) {
        if(grapplePoint.activeSelf) {
            grappleLine.SetPositions(new Vector3[] {handPosition, grapplePoint.transform.position});
        }

        if(grapplePoint.activeSelf)
            grappleLine.enabled = true;
        else
            grappleLine.enabled = false;
    }

    public void RetractGrapple() {
        SpringJoint springJoint = grapplePoint.GetComponent<SpringJoint>();
        if(springJoint.maxDistance > 0) {
            springJoint.maxDistance -= retractionSpeed;
        }
    }

    public void ResetGrappleDistance() {
        SpringJoint springJoint = grapplePoint.GetComponent<SpringJoint>();
        springJoint.maxDistance = originalMaxDistance;
    }

    public bool AttachedToWeakPoint() {        
        return grappleCollisionHandler.isAttachedToWeakPoint();
    }

    public void DoDamage() {
        // If we are attached to a weak point, do damage.
        WeakPoint weakPoint = grappleCollisionHandler.GetWeakPoint();
        if(weakPoint == null) {
            throw new System.Exception("no weak point for damage");
        }

        weakPoint.DoDamage();
    }
}
