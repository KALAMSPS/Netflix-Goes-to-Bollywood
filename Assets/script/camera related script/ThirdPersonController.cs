
using UnityEngine;
public class ThirdPersonController : MonoBehaviour
{

    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;
    [Tooltip("The higher the value, the higher the character will jump.")]
    public float jumpForce = 18f;
    [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
    public float jumpTime = 0.85f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;

    float jumpElapsedTime = 0;

    // Player states
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    Animator animator;
    CharacterController cc;

    bool canMove = true;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Message informing the user that they forgot to add an animator
        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
    }


    // Update is only being used here to identify keys and trigger animations
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            canMove = !canMove;
        }


        // Input checkers
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        // Unfortunately GetAxis does not work with GetKeyDown, so inputs must be taken individually
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

       
        if ( cc.isGrounded && animator != null )
        {

            // Crouch
            // Note: The crouch animation does not shrink the character's collider
            //animator.SetBool("crouch", isCrouching);

            //// Run
            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed );
            // Walk / Idle transition
            bool isMoving = Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f;
            animator.SetBool("run", isMoving);

            // Sprint
            //isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
            //animator.SetBool("sprint", isSprinting );

        }

        // Jump animation
        if ( animator != null )
            animator.SetBool("air", cc.isGrounded == false );
            
        if ( inputJump && cc.isGrounded )
        {
            isJumping = true;
            // Disable crounching when jumping
            //isCrouching = false; 
        }

        HeadHittingDetect();

    }

    private float currentVelocityAddition = 0f; // To smoothly transition sprint speed

    private void FixedUpdate()
    {

        if (!canMove)
        {
            cc.Move(Vector3.down * gravity * Time.deltaTime); // Still apply gravity
            return;
        }
        // Sprinting velocity boost or crouching deceleration
        float targetVelocityAddition = 0f;

        if (inputSprint && (inputHorizontal != 0 || inputVertical != 0))
        {
            isSprinting = true;
            targetVelocityAddition = sprintAdittion;
        }
        else
        {
            isSprinting = false;
        }

        // Smooth transition of sprint speed
        currentVelocityAddition = Mathf.Lerp(currentVelocityAddition, targetVelocityAddition, Time.deltaTime * 5f);

        // Crouching reduces speed
        if (isCrouching)
            currentVelocityAddition = -(velocity * 0.50f); // -50% velocity

        // Direction movement
        float directionX = inputHorizontal * (velocity + currentVelocityAddition) * Time.deltaTime;
        float directionZ = inputVertical * (velocity + currentVelocityAddition) * Time.deltaTime;
        float directionY = 0;

        // Jump handler
        if (isJumping)
        {
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        // Apply gravity
        directionY -= gravity * Time.deltaTime;

        // --- Character rotation ---
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        forward *= directionZ;
        right *= directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        // --- End rotation ---

        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 movement = verticalDirection + horizontalDirection;
        cc.Move(movement);
    }

    //This function makes the character end his jump if he hits his head on something
    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        // Uncomment this line to see the Ray drawed in your characters head
        // Debug.DrawRay(ccCenter, Vector3.up * headHeight, Color.red);

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }

}
