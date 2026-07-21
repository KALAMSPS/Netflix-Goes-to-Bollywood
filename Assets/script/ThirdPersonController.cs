using UnityEngine;


    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 3.5f;
        public float runSpeed = 7f;
        public float rotationSmoothTime = 0.1f;
        public float acceleration = 10f;

        [Header("Jump / Gravity")]
        public float jumpHeight = 1.2f;
        public float gravity = -15f;
        public float groundedGravity = -2f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.25f;
        public LayerMask groundMask;

        [Header("Camera")]
        public Transform cameraTransform;   // Assign your main/orbit camera here

        [Header("Animation")]
        public Animator animator;
        public string speedParam = "speed";     // float 0-1, for blend trees
        public string runParam = "run";         // bool, used by NewMCQDialogBox
        public string jumpParam = "jump";       // trigger

        private CharacterController controller;
        private Vector3 velocity;
        private float currentSpeed;
        private float rotationVelocity;
        private bool isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Update()
        {
            GroundCheck();
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }

        private void GroundCheck()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
            }
            else
            {
                isGrounded = controller.isGrounded;
            }

            if (isGrounded && velocity.y < 0)
                velocity.y = groundedGravity;
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
            float targetSpeed = (isRunning ? runSpeed : walkSpeed) * inputDir.magnitude;

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            if (inputDir.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;

                if (cameraTransform != null)
                    targetAngle += cameraTransform.eulerAngles.y;

                float smoothedAngle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);

                transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, acceleration * Time.deltaTime);
            }

            if (animator != null)
            {
                animator.SetFloat(speedParam, currentSpeed / runSpeed);
                animator.SetBool(runParam, isRunning && inputDir.magnitude >= 0.1f);
            }
        }

        private void HandleJump()
        {
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                    animator.SetTrigger(jumpParam);
            }
        }

        private void ApplyGravity()
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }