using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded = true;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public MovementState state;

    public enum MovementState{
        walking,
        sprinting,
        air
    }

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    private void Update()
    {
        
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight*0.5f + 0.2f, whatIsGround);

        playerInput();
        SpeedControl();
        StateHandler();

        // Handle drag
        if(grounded){
            rb.drag = groundDrag;
        } 
        else {
            rb.drag = 0;
        }

    }

    private void FixedUpdate()
    {
        movePlayer();
    }

    private void playerInput(){

        //Debug.Log("playerInputGood");

        // Planar Movement
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump = Space
        if(Input.GetKey(jumpKey) && readyToJump && grounded){
            
            Debug.Log("Spacebar read");

            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler(){

        //Debug.Log("StateHandlerGood");

        // Sprinting
        if (grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        else if(grounded){
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else{
            state = MovementState.air;
        }
    }

    private void movePlayer(){
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        

        if(grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if(!grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

    }

    private void SpeedControl(){

        //Debug.Log("SpeedControlGood");

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit the speed of the player if they're going too fast
        if (flatVel.magnitude > moveSpeed){
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump(){
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Debug.Log("Jumped!");
    }

    private void ResetJump(){
        readyToJump = true;
    }

    
}
