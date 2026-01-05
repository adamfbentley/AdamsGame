using UnityEngine;

/// <summary>
/// Dark Souls-style player controller
/// Camera-relative movement, instant rotation response, dodge roll
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EnhancedPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpGravityMultiplier = 1.2f; // Faster fall for snappier feel
    
    [Header("Dodge Roll")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.35f;
    [SerializeField] private float dodgeCooldown = 0.8f;
    
    private CharacterController controller;
    private ThirdPersonCamera thirdPersonCamera;
    private Animator animator;

    
    private Vector3 moveVelocity = Vector3.zero;
    private Vector3 verticalVelocity = Vector3.zero;
    private bool isGrounded;
    private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;
    
    // Dodge state
    private bool isDodging = false;
    private float dodgeTimer = 0f;
    private float dodgeCooldownTimer = 0f;
    private Vector3 dodgeDirection;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
        animator = GetComponentInChildren<Animator>();
        
        // Disable root motion - animation is visual only, code handles movement
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }
    
    void Update()
    {
        if (controller == null) return;
        
        // Ground check - use multiple methods for reliability
        isGrounded = controller.isGrounded;
        
        // Additional raycast check for more reliable ground detection
        if (!isGrounded || verticalVelocity.y <= 0)
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.2f, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 0.4f))
            {
                isGrounded = true;
            }
        }
        
        // Coyote time (grace period after leaving ground)
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;
        
        // Update timers
        if (dodgeCooldownTimer > 0)
            dodgeCooldownTimer -= Time.deltaTime;
        
        if (isDodging)
        {
            HandleDodge();
        }
        else
        {
            HandleMovement();
            HandleJump();
            HandleDodgeInput();
        }
        
        ApplyGravity();
        
        // Safety reset
        if (transform.position.y < -10f)
        {
            transform.position = new Vector3(0, 2, 0);
            verticalVelocity = Vector3.zero;
        }
    }
    
    private void HandleMovement()
    {
        // Get input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(h, v);
        
        // Normalize diagonal movement
        if (input.magnitude > 1f)
            input.Normalize();
        
        // Camera-relative movement direction
        Vector3 cameraForward, cameraRight;
        GetCameraDirections(out cameraForward, out cameraRight);
        
        Vector3 moveDir = (cameraForward * input.y + cameraRight * input.x);
        
        // Speed based on input - sprint in any direction
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && input.magnitude > 0.1f;
        float speed = isRunning ? runSpeed : walkSpeed;
        
        if (moveDir.magnitude > 0.1f)
        {
            // Smooth rotation toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            
            // Move in the direction character is facing
            moveVelocity = moveDir * speed;
        }
        else if (isGrounded)
        {
            // Only slow down when grounded - preserve momentum during jumps
            moveVelocity = Vector3.Lerp(moveVelocity, Vector3.zero, Time.deltaTime * 10f);
        }
        
        controller.Move(moveVelocity * Time.deltaTime);
        
        // Update animator with smooth speed transition
        if (animator != null)
        {
            // Only update movement parameters when grounded to not override jump animation
            if (isGrounded)
            {
                float currentSpeed = animator.GetFloat("Speed");
                float targetSpeed = moveVelocity.magnitude;
                animator.SetFloat("Speed", Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 12f));
                animator.SetBool("IsRunning", isRunning);
            }
            animator.SetBool("Grounded", isGrounded);
        }
    }
    
    private void HandleJump()
    {
        // Jump with Space
        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimer > 0 && !isDodging)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalVelocity.y = jumpVelocity;
            coyoteTimer = 0f;
            isGrounded = false;
            
            if (animator != null)
            {
                animator.SetTrigger("Jump");
                // Start jump animation from frame 19 (approximately 0.32 normalized time for 60fps animation)
                animator.Play("Jump", 0, 0.32f);
            }
            
            // Maintain momentum when jumping while moving
            if (moveVelocity.magnitude > 0.5f)
            {
                // Keep current movement speed during jump
                moveVelocity = moveVelocity.normalized * moveVelocity.magnitude;
            }
        }
    }
    
    private void HandleDodgeInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && dodgeCooldownTimer <= 0 && isGrounded)
        {
            // Get movement input for dodge direction
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            
            Vector3 cameraForward, cameraRight;
            GetCameraDirections(out cameraForward, out cameraRight);
            
            dodgeDirection = (cameraForward * v + cameraRight * h).normalized;
            
            // If no input, dodge forward
            if (dodgeDirection.magnitude < 0.1f)
                dodgeDirection = transform.forward;
            
            // Face dodge direction immediately
            if (dodgeDirection.magnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(dodgeDirection);
            
            // Start dodge
            isDodging = true;
            dodgeTimer = dodgeDuration;
            dodgeCooldownTimer = dodgeCooldown;
            
            if (animator != null)
            {
                // Start dodge animation from frame 15 (approximately 0.25 normalized time for 60fps animation)
                animator.Play("Dodge", 0, 0.25f);
            }
        }
    }
    
    private void HandleDodge()
    {
        dodgeTimer -= Time.deltaTime;
        
        if (dodgeTimer <= 0)
        {
            isDodging = false;
            return;
        }
        
        // Fast movement during dodge - maintain direction set at start
        controller.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
    }
    
    private void ApplyGravity()
    {
        if (isGrounded)
        {
            // Keep slight downward force when grounded
            if (verticalVelocity.y < 0)
                verticalVelocity.y = -2f;
        }
        else
        {
            // Apply gravity - stronger when falling for snappier jump arc
            float gravityMultiplier = verticalVelocity.y < 0 ? jumpGravityMultiplier : 1f;
            verticalVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
            
            // Cap falling speed
            if (verticalVelocity.y < -25f)
                verticalVelocity.y = -25f;
        }
        
        controller.Move(verticalVelocity * Time.deltaTime);
    }
    
    private void GetCameraDirections(out Vector3 forward, out Vector3 right)
    {
        if (thirdPersonCamera != null)
        {
            forward = thirdPersonCamera.GetCameraForward();
            right = thirdPersonCamera.GetCameraRight();
        }
        else
        {
            Transform cam = Camera.main?.transform;
            forward = cam != null ? cam.forward : transform.forward;
            right = cam != null ? cam.right : transform.right;
        }
        
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
    }
    

}
