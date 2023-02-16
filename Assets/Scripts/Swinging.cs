using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header ("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header ("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    //public PlayerMovementGrappling pm;

    [Header ("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;

    [Header ("Swinging Physics")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header ("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;


    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(swingKey)){
            StartSwing();
        }
        if(Input.GetKeyUp(swingKey)){
            StopSwing();
        }

        CheckForSwingPoints();

        if (joint != null){
            swingMovement();
        }
    }

    void LateUpdate(){
        DrawRope();
    }

    private void StartSwing(){

        // Stop trying if there isn't even a prediction
        if (predictionHit.point == Vector3.zero){
            return;
        }

        //pm.swinging = true;

        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        // The distance grapple will try to keep from grapple point
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        // Customization values
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        // Line renderer to see the shot
        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
    }

    private void StopSwing(){
        
        //pm.swinging = false;
        lr.positionCount = 0;
        Destroy(joint);
    }

    private void DrawRope(){
        // Check if grappling before drawing
        if (!joint){
            return;
        }
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }

    private void swingMovement(){

        // Right Control
        if (Input.GetKey(KeyCode.D)){
            rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        }

        // Left control
        if (Input.GetKey(KeyCode.A)){
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }

        // Boost control
        if (Input.GetKey(KeyCode.W)){
            rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);
        }

        // Cable shortening
        if (Input.GetKey(KeyCode.Space)){

            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint *0.25f;
        }

        // Cable lengthening
        if (Input.GetKey(KeyCode.S)){
            
            float extendDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendDistanceFromPoint * 0.8f;
            joint.minDistance = extendDistanceFromPoint *0.25f;
        }
    }

    private void CheckForSwingPoints(){

        if (joint != null){
            return;
        }

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        // Player is looking at a grappleable object
        if (raycastHit.point != Vector3.zero){
            realHitPoint = raycastHit.point;
        }

        else if (sphereCastHit.point !=Vector3.zero){
            realHitPoint = sphereCastHit.point;
        }  

        else {
            realHitPoint = Vector3.zero;
        }

        if (realHitPoint != Vector3.zero){
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }

        else {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;

    }
    
}
