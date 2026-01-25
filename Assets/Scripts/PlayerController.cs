using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 5f;
    
    // Jump
    public float jumpForce = 10f;
    public float descendFactor = 2.5f;
    public float ascendFactor = 2f;
    [HideInInspector] public bool isGrounded = true;
    public LayerMask groundLayer;
    
    // Ledge climb system
    public float ledgeClimbHeight = 1.5f;
    public float ledgeClimbSpeed = 2f;
    public float ledgeDetectionDistance = 0.6f;
    private bool isClimbing = false;
    
    // Camera
    public float mouseSensitivity = 3f;
    public float keyboardRotationSpeed = 180f;
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
        rb.angularDamping = 0.99f;
        rb.useGravity = true;
        rb.isKinematic = false;
        
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider>();
        }
        
        capsule.height = 2f;
        capsule.radius = 0.5f;
        capsule.center = new Vector3(0, 1f, 0);
        capsule.direction = 1;
        capsule.isTrigger = false;
        
        DisableCharacterColliders();
        
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        
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
        foreach (Transform child in transform)
        {
            Collider[] childColliders = child.GetComponentsInChildren<Collider>();
            foreach (Collider col in childColliders)
            {
                col.enabled = false;
                Debug.Log($"Disabled collider on: {col.gameObject.name}");
            }
            
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
        HandleRotation();
        
        // Ground check
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        float capsuleBottom = transform.position.y + capsule.center.y - (capsule.height / 2f);
        
        Vector3 center = new Vector3(transform.position.x, capsuleBottom + 0.05f, transform.position.z);
        float checkRadius = capsule.radius * 0.8f;
        float rayDistance = 0.15f;
        
        bool centerGrounded = Physics.Raycast(center, Vector3.down, rayDistance, groundLayer);
        bool frontGrounded = Physics.Raycast(center + transform.forward * checkRadius, Vector3.down, rayDistance, groundLayer);
        bool backGrounded = Physics.Raycast(center - transform.forward * checkRadius, Vector3.down, rayDistance, groundLayer);
        bool leftGrounded = Physics.Raycast(center - transform.right * checkRadius, Vector3.down, rayDistance, groundLayer);
        bool rightGrounded = Physics.Raycast(center + transform.right * checkRadius, Vector3.down, rayDistance, groundLayer);
        
        RaycastHit hit;
        
        isGrounded = centerGrounded || frontGrounded || backGrounded || leftGrounded || rightGrounded;
        
        if (isGrounded && Physics.Raycast(center, Vector3.down, out hit, rayDistance, groundLayer))
        {
            Debug.DrawRay(center, Vector3.down * rayDistance, Color.green);
            Debug.DrawLine(center, hit.point, Color.yellow);
        }
        else
        {
            Debug.DrawRay(center, Vector3.down * rayDistance, Color.red);
        }
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
        
        if (Input.GetKeyDown(KeyCode.P) && Time.time >= lastShootTime + shootCooldown)
        {
            ShootPillow();
        }
    }
    
    void HandleRotation()
    {
        float arrowHorizontal = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
            arrowHorizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            arrowHorizontal = 1f;
        
        if (Mathf.Abs(arrowHorizontal) > 0.1f)
        {
            float rotationAmount = arrowHorizontal * keyboardRotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);
        }
    }

    void FixedUpdate()
    {
        if (!isClimbing)
        {
            CheckLedgeClimb();
        }
        
        if (!isClimbing)
        {
            MovePlayer();
            ApplyJumpPhysics();
        }
        
        rb.angularVelocity = Vector3.zero;
    }

    void MovePlayer()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.D))
            horizontal = 1f;
        
        if (vertical == 0 && horizontal == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;
        
        Vector3 targetVelocity = moveDirection * speed;
        currentVelocity = rb.linearVelocity;
        currentVelocity.x = targetVelocity.x;
        currentVelocity.z = targetVelocity.z;
        
        rb.linearVelocity = currentVelocity;
    }

    void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }
    
    void ApplyJumpPhysics()
    {
        Vector3 velocity = rb.linearVelocity;
        
        if (velocity.y < 0)
        {
            velocity.y += Physics.gravity.y * (descendFactor - 1) * Time.fixedDeltaTime;
        }
        else if (velocity.y > 0)
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
    
    void CheckLedgeClimb()
    {
        if (isGrounded || rb.linearVelocity.y > 0)
            return;
        
        float vertical = Input.GetAxisRaw("Vertical");
        if (vertical <= 0)
            return;
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        
        Vector3 chestPos = transform.position + Vector3.up * (capsule.height * 0.6f);
        RaycastHit wallHit;
        
        if (Physics.Raycast(chestPos, transform.forward, out wallHit, ledgeDetectionDistance, groundLayer))
        {
            Debug.DrawRay(chestPos, transform.forward * ledgeDetectionDistance, Color.red);
            
            Vector3 aboveWallPos = wallHit.point + Vector3.up * ledgeClimbHeight;
            RaycastHit ledgeHit;
            
            if (Physics.Raycast(aboveWallPos, Vector3.down, out ledgeHit, ledgeClimbHeight, groundLayer))
            {
                Debug.DrawRay(aboveWallPos, Vector3.down * ledgeClimbHeight, Color.green);
                
                float ledgeHeight = ledgeHit.point.y - transform.position.y;
                
                if (ledgeHeight > capsule.height * 0.5f && ledgeHeight <= ledgeClimbHeight)
                {
                    if (Vector3.Dot(ledgeHit.normal, Vector3.up) > 0.8f)
                    {
                        Debug.Log($"Ledge detected! Height: {ledgeHeight:F2}m");
                        StartCoroutine(PerformLedgeClimb(ledgeHit.point));
                    }
                }
            }
        }
    }
    
    IEnumerator PerformLedgeClimb(Vector3 ledgeTop)
    {
        isClimbing = true;
        
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(ledgeTop.x, ledgeTop.y, ledgeTop.z) + transform.forward * 0.3f;
        
        float climbTime = 0f;
        float climbDuration = 0.5f;
        
        Debug.Log($"Climbing from {startPos.y:F2} to {targetPos.y:F2}");
        
        while (climbTime < climbDuration)
        {
            climbTime += Time.deltaTime;
            float t = climbTime / climbDuration;
            
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        transform.position = targetPos;
        
        rb.useGravity = true;
        isClimbing = false;
        
        Debug.Log("Climb complete!");
    }
}