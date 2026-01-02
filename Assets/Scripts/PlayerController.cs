using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 5f;
    
    // Jump
    public float jumpForce = 10f;
    public float descendFactor = 2.5f;
    public float ascendFactor = 2f;
    [HideInInspector] public bool isGrounded = true; // Public so AnimationController can read it
    public LayerMask groundLayer;
    
    // Step up for better platforming
    public float maxStepHeight = 0.4f; // Maximum height the player can step up
    public float stepCheckDistance = 0.1f; // How far forward to check for steps
    
    // Camera
    public float mouseSensitivity = 3f;
    public float keyboardRotationSpeed = 90f; // Grader per sekund
    private float verticalRotation;
    private Transform cameraTransform;
    
    // For movement
    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Rigidbody settings for solid collisions
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.99f; // High damping to prevent unwanted rotation
        rb.useGravity = true;
        rb.isKinematic = false;
        
        // CRITICAL: Use Continuous collision detection to prevent going through platforms
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Freeze X and Z rotations (tipping), but allow Y rotation (turning)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        // Ensure we have a capsule collider with proper settings
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider>();
        }
        
        // Critical: Set capsule so bottom is at feet (y=0) and top is at head (y=2)
        capsule.height = 2f;
        capsule.radius = 0.5f;
        capsule.center = new Vector3(0, 1f, 0); // Center at y=1, so bottom=0, top=2
        capsule.direction = 1; // Y-axis
        
        // Make sure it's NOT a trigger
        capsule.isTrigger = false;
        
        // Disable colliders on character model to avoid conflicts
        DisableCharacterColliders();
        
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("Player setup complete - Rigidbody and Collider configured");
    }
    
    void DisableCharacterColliders()
    {
        // Find character model and disable its colliders (they conflict with Player collider)
        foreach (Transform child in transform)
        {
            Collider[] childColliders = child.GetComponentsInChildren<Collider>();
            foreach (Collider col in childColliders)
            {
                col.enabled = false;
                Debug.Log($"Disabled collider on: {col.gameObject.name}");
            }
            
            // Also make sure character doesn't have Rigidbody
            Rigidbody[] childRigidbodies = child.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody childRb in childRigidbodies)
            {
                Destroy(childRb);
                Debug.Log($"Removed Rigidbody from: {childRb.gameObject.name}");
            }
        }
    }

    void Update()
    {
        RotateCamera();
        HandleRotation(); // Ny metode for rotasjon
        
        // Ground check - use capsule's actual bottom position
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        
        // Calculate the actual bottom of the capsule
        float capsuleBottom = transform.position.y + capsule.center.y - (capsule.height / 2f);
        
        // Use multiple raycasts for more reliable ground detection
        Vector3 center = new Vector3(transform.position.x, capsuleBottom + 0.05f, transform.position.z);
        float checkRadius = capsule.radius * 0.8f;
        float rayDistance = 0.15f;
        
        // Check center and 4 points around the edge
        bool centerGrounded = Physics.Raycast(center, Vector3.down, rayDistance, groundLayer);
        bool frontGrounded = Physics.Raycast(center + transform.forward * checkRadius, Vector3.down, rayDistance, groundLayer);
        bool backGrounded = Physics.Raycast(center - transform.forward * checkRadius, Vector3.down, rayDistance, groundLayer);
        bool leftGrounded = Physics.Raycast(center - transform.right * checkRadius, Vector3.down, rayDistance, groundLayer);
        bool rightGrounded = Physics.Raycast(center + transform.right * checkRadius, Vector3.down, rayDistance, groundLayer);
        
        RaycastHit hit;
        
        // Grounded if ANY raycast hits
        isGrounded = centerGrounded || frontGrounded || backGrounded || leftGrounded || rightGrounded;
        
        // Get detailed hit info for debugging
        if (isGrounded && Physics.Raycast(center, Vector3.down, out hit, rayDistance, groundLayer))
        {
            // Debug visualization - shows what was hit
            Debug.DrawRay(center, Vector3.down * rayDistance, Color.green);
            Debug.DrawLine(center, hit.point, Color.yellow);
            
            // Removed debug logging to prevent console spam
        }
        else
        {
            Debug.DrawRay(center, Vector3.down * rayDistance, Color.red);
        }
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }
    
    void HandleRotation()
    {
        // Få raw input fra både tastatur og piler
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Sjekk om pil venstre/høyre er trykt (KeyCode for raw arrow input)
        float arrowHorizontal = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
            arrowHorizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            arrowHorizontal = 1f;
        
        // Piler roterer ALLTID
        if (Mathf.Abs(arrowHorizontal) > 0.1f)
        {
            float rotationAmount = arrowHorizontal * keyboardRotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);
        }
        // A/D roterer KUN når W/S også er trykt
        else
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(vertical) > 0.1f && Mathf.Abs(horizontal) > 0.1f)
            {
                float rotationAmount = horizontal * keyboardRotationSpeed * Time.deltaTime;
                transform.Rotate(0, rotationAmount, 0);
            }
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        ApplyJumpPhysics();
        // StepUpCheck(); // Temporarily disabled - may cause collision issues
    }

    void MovePlayer()
    {
        // Get vertical input fra både W/S og pil opp/ned
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Get horizontal input KUN fra A/D (ikke piler!)
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.D))
            horizontal = 1f;
        
        // No movement if no input
        if (vertical == 0 && horizontal == 0)
        {
            // Only stop horizontal movement
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        
        // Move relative to where the character is facing
        // Forward/backward based on vertical input
        // Left/right based on horizontal input (strafing med A/D)
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;
        
        // Apply movement - preserve Y velocity for jumping/falling
        Vector3 targetVelocity = moveDirection * speed;
        currentVelocity = rb.linearVelocity;
        currentVelocity.x = targetVelocity.x;
        currentVelocity.z = targetVelocity.z;
        
        // Use velocity (better for physics collisions)
        rb.linearVelocity = currentVelocity;
    }

    void Jump()
    {
        // Preserve horizontal velocity, only change vertical
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }
    
    void ApplyJumpPhysics()
    {
        // Only modify vertical velocity
        Vector3 velocity = rb.linearVelocity;
        
        if (velocity.y < 0) // Falling down
        {
            velocity.y += Physics.gravity.y * (descendFactor - 1) * Time.fixedDeltaTime;
        }
        else if (velocity.y > 0) // Rising up
        {
            velocity.y += Physics.gravity.y * (ascendFactor - 1) * Time.fixedDeltaTime;
        }
        
        rb.linearVelocity = velocity;
    }

    void RotateCamera()
    {
        // Mouse X rotates the entire character (yaw)
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        // Mouse Y rotates only the camera (pitch)
        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    
    void StepUpCheck()
    {
        // Only check if player is grounded and moving
        if (!isGrounded || rb.linearVelocity.magnitude < 0.5f)
            return;
        
        // Get horizontal movement direction
        Vector3 movementDirection = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;
        
        if (movementDirection.magnitude < 0.1f)
            return;
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        // Use correct calculation that accounts for capsule center offset
        float capsuleBottom = transform.position.y + capsule.center.y - (capsule.height / 2f);
        
        // Check for obstacle at foot level
        Vector3 footCheckPos = new Vector3(transform.position.x, capsuleBottom + 0.1f, transform.position.z);
        RaycastHit hit;
        
        // Raycast forward from feet
        if (Physics.Raycast(footCheckPos, movementDirection, out hit, 0.5f, groundLayer))
        {
            // Check if it's a vertical surface (wall)
            if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f)
            {
                // Check if there's a walkable surface above
                Vector3 stepCheckPos = footCheckPos + movementDirection * 0.3f + Vector3.up * maxStepHeight;
                RaycastHit stepHit;
                
                if (Physics.Raycast(stepCheckPos, Vector3.down, out stepHit, maxStepHeight, groundLayer))
                {
                    // Check if the top surface is horizontal (walkable)
                    if (Vector3.Dot(stepHit.normal, Vector3.up) > 0.8f)
                    {
                        float stepHeight = stepHit.point.y - capsuleBottom;
                        
                        // Only step up small ledges
                        if (stepHeight > 0.05f && stepHeight <= maxStepHeight)
                        {
                            // Move player up - position at new ground level
                            Vector3 newPosition = rb.position;
                            newPosition.y = stepHit.point.y;
                            rb.MovePosition(newPosition);
                        }
                    }
                }
            }
        }
    }
}