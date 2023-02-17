using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float swingSpeed;

    public float groundDrag;

    [Header ("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header ("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header ("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header ("HUD Data")]
    public TextMeshProUGUI speedText;

    [Header ("Respawning")]
    public int OutOfBoundsY;
    public GameObject respawnPoint;


    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public MovementState state;

    public enum MovementState{
        walking,
        sprinting,
        crouching,
        swinging,
        air
    }

    public bool swinging;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    private void Update(){
    
        // Check if grounded
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight*0.5f + 0.2f, whatIsGround);

        playerInput();
        SpeedControl();
        StateHandler();
        updateHUD();
        checkBounds();

        // Handle drag
        if(grounded){
            rb.drag = groundDrag;
        } 
        else {
            rb.drag = 0;
        }

    }

    private void FixedUpdate(){
        movePlayer();
    }

    private void respawn(){
        rb.position = respawnPoint.transform.position;
    }

    private void checkBounds(){
        if(rb.position.y < OutOfBoundsY){
            Debug.Log("Repsawning... Out of bounds");
            respawn();
        }
    }

    private void playerInput(){

        //Debug.Log("playerInputGood");

        // Planar Movement
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Debug.Log(horizontalInput + "   " + verticalInput);

        // Jump = Space
        if(Input.GetKey(jumpKey) && readyToJump && grounded){
            
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start Crouching
        if (Input.GetKeyDown(crouchKey)){

            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Stop Crouching
        if (Input.GetKeyUp(crouchKey)){

            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler(){

        // Crouching
        if(Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Sprinting
        else if (grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // Walking
        else if(grounded){
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else if(swinging){
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
        }

        // Airborne
        else {
            state = MovementState.air;
        }

    }

    private void movePlayer(){

        // Check if swinging
        if (swinging){
            return;
        }

        // Calculate current ddirection of motion
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Kids on the slope
        if(OnSlope() && !exitingSlope){
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            // Fixes weird "jumping" while going up the slope
            if(rb.velocity.y > 0){
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // On the ground
        if(grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // In the air
        else if(!grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Turn off gravity while on slope
        rb.useGravity = !OnSlope();


    }

    private void SpeedControl(){

        // Control speed on slopes
        if(OnSlope() && !exitingSlope){
            if(rb.velocity.magnitude > moveSpeed){
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        // Control speed if on non-sloped surface
        else {

            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Limit the speed of the player if they're going too fast
            if (flatVel.magnitude > moveSpeed){
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        
    }

    private void Jump(){

        exitingSlope = true;

        // Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump(){
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope(){
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)){
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0 ;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection(){
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    // Updates the player's HUD
    private void updateHUD(){
        speedText.text = "Velocity: " + rb.velocity.magnitude.ToString("0.00");
    }
    
}
