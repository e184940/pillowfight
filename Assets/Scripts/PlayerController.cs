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
    
    // Ledge climb system
    public float ledgeClimbHeight = 1.5f; // Max høyde spilleren kan klatre opp
    public float ledgeClimbSpeed = 2f; // Hvor raskt spilleren klatrer opp
    public float ledgeDetectionDistance = 0.6f; // Hvor langt fremover vi sjekker for ledge
    private bool isClimbing = false;
    
    // Camera
    public float mouseSensitivity = 3f;
    public float keyboardRotationSpeed = 90f;
    private float verticalRotation;
    private Transform cameraTransform;
    
    // Shooting
    public GameObject pillowPrefab;
    public Transform shootPoint;
    public float shootForce = 20f;
    public float shootCooldown = 0.5f;
    private float lastShootTime;
    
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
        
        // Setup shoot point hvis ikke assigned
        if (shootPoint == null)
        {
            GameObject sp = new GameObject("ShootPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0, 1.5f, 1.5f);
            shootPoint = sp.transform;
        }
        
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
        
        // Shooting
        if (Input.GetKeyDown(KeyCode.P) && Time.time >= lastShootTime + shootCooldown)
        {
            ShootPillow();
        }
    }
    
    void HandleRotation()
    {
        // Sjekk om pil venstre/høyre er trykt
        float arrowHorizontal = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
            arrowHorizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            arrowHorizontal = 1f;
        
        // KUN piler roterer spilleren
        if (Mathf.Abs(arrowHorizontal) > 0.1f)
        {
            float rotationAmount = arrowHorizontal * keyboardRotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);
        }
    }

    void FixedUpdate()
    {
        // Sjekk for ledge climb først
        if (!isClimbing)
        {
            CheckLedgeClimb();
        }
        
        // Normal movement hvis ikke klatrer
        if (!isClimbing)
        {
            MovePlayer();
            ApplyJumpPhysics();
        }
        
        // VIKTIG: Nullstill angular velocity hver frame for å forhindre random rotasjon
        rb.angularVelocity = Vector3.zero;
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
    
    void ShootPillow()
    {
        if (pillowPrefab == null)
        {
            Debug.LogError("PlayerController: No pillow prefab assigned!");
            return;
        }
        
        GameObject pillow = Instantiate(pillowPrefab, shootPoint.position, shootPoint.rotation);
        
        Vector3 shootDirection = transform.forward;
        
        Rigidbody pillowRb = pillow.GetComponent<Rigidbody>();
        if (pillowRb == null)
        {
            pillowRb = pillow.AddComponent<Rigidbody>();
        }
        
        // Ignorer kollisjon mellom spiller og denne puten
        Collider pillowCollider = pillow.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (pillowCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(pillowCollider, playerCollider);
        }
        
        pillowRb.linearVelocity = shootDirection * shootForce;
        
        lastShootTime = Time.time;
        
        Destroy(pillow, 5f);
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
    
    void CheckLedgeClimb()
    {
        // Kun sjekk hvis spilleren er i luften og beveger seg fremover
        if (isGrounded || rb.linearVelocity.y > 0)
            return;
        
        // Sjekk om spilleren prøver å bevege seg fremover
        float vertical = Input.GetAxisRaw("Vertical");
        if (vertical <= 0)
            return;
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        
        // Sjekk i brysthøyde (midt på spilleren)
        Vector3 chestPos = transform.position + Vector3.up * (capsule.height * 0.6f);
        RaycastHit wallHit;
        
        // Er det en vegg/kant foran oss?
        if (Physics.Raycast(chestPos, transform.forward, out wallHit, ledgeDetectionDistance, groundLayer))
        {
            Debug.DrawRay(chestPos, transform.forward * ledgeDetectionDistance, Color.red);
            
            // Sjekk om det er gulv over denne veggen (ledge)
            Vector3 aboveWallPos = wallHit.point + Vector3.up * ledgeClimbHeight;
            RaycastHit ledgeHit;
            
            // Raycast nedover for å finne toppen av platformet
            if (Physics.Raycast(aboveWallPos, Vector3.down, out ledgeHit, ledgeClimbHeight, groundLayer))
            {
                Debug.DrawRay(aboveWallPos, Vector3.down * ledgeClimbHeight, Color.green);
                
                // Sjekk om ledge er over spillerens hode men innenfor climb-rekkevidde
                float ledgeHeight = ledgeHit.point.y - transform.position.y;
                
                if (ledgeHeight > capsule.height * 0.5f && ledgeHeight <= ledgeClimbHeight)
                {
                    // Sjekk om toppen er flat (walkable)
                    if (Vector3.Dot(ledgeHit.normal, Vector3.up) > 0.8f)
                    {
                        Debug.Log($"Ledge detected! Height: {ledgeHeight:F2}m");
                        StartCoroutine(PerformLedgeClimb(ledgeHit.point));
                    }
                }
            }
        }
    }
    
    System.Collections.IEnumerator PerformLedgeClimb(Vector3 ledgeTop)
    {
        isClimbing = true;
        
        // Disable physics temporarily
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(ledgeTop.x, ledgeTop.y, ledgeTop.z) + transform.forward * 0.3f;
        
        float climbTime = 0f;
        float climbDuration = 0.5f; // 0.5 sekunder å klatre opp
        
        Debug.Log($"Climbing from {startPos.y:F2} to {targetPos.y:F2}");
        
        // Smooth climb animation
        while (climbTime < climbDuration)
        {
            climbTime += Time.deltaTime;
            float t = climbTime / climbDuration;
            
            // Ease out curve for natural movement
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        // Ensure final position
        transform.position = targetPos;
        
        // Re-enable physics
        rb.useGravity = true;
        isClimbing = false;
        
        Debug.Log("Climb complete!");
    }
}